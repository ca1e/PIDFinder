using PKHeX.Core;
using PIDFinder.Lib;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PKHeX_Hunter_Plugin
{
    public partial class Searcher : Form
    {
        private IPKMView Editor { get; }
        private CancellationTokenSource activeCancellation;
        private Task activeTask;
        private bool closeRequested;

        public Searcher(ISaveFileProvider sav, IPKMView editor)
        {
            Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            InitializeComponent();
            CB_Method.DataSource = Enum.GetNames(typeof(RngKind));
            RefreshTemplateDisplay();
            FormClosing += Searcher_FormClosing;
        }

        private void RefreshTemplateDisplay()
        {
            var pk = Editor.Data;
            TB_Species.Text = $"{pk.Species} / {pk.GetType().Name}";
            TB_Version.Text = pk.Version.ToString();
        }

        private GenerationContext CaptureContext()
        {
            RefreshTemplateDisplay();
            var template = (PKM)Editor.Data.Clone();
            var kind = Enum.Parse<RngKind>((string)CB_Method.SelectedItem);
            return new GenerationContext(template, kind, conditionPKM1.ReadRulesSnapshot(), (int)NUD_Flawless.Value);
        }

        private uint ReadSeed()
        {
            if (!uint.TryParse(seedBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var seed))
                throw new ArgumentException("种子必须为 1 至 8 位十六进制数字。");
            return seed;
        }

        private void ShowEntity(PKM pk)
        {
            pk.RefreshChecksum();
            Editor.PopulateFields(pk, false);
        }

        private void SetRunning(bool running)
        {
            BTN_Search.Enabled = !running;
            checkBTN.Enabled = !running;
            CB_Method.Enabled = !running;
            conditionPKM1.Enabled = !running;
            seedBox.Enabled = !running;
            NUD_Flawless.Enabled = !running;
            NUD_Budget.Enabled = !running;
            cancelBTN.Visible = running;
            cancelBTN.Enabled = running;
        }

        private async void BTN_Search_Click(object sender, EventArgs e)
        {
            if (activeTask != null || closeRequested) return;
            GenerationContext context;
            uint start;
            uint count;
            try
            {
                context = CaptureContext();
                start = ReadSeed();
                count = (uint)NUD_Budget.Value;
            }
            catch (Exception ex)
            {
                L_Status.Text = ex.Message;
                return;
            }

            // Each run owns its cancellation source. Nothing in the worker reads controls.
            using var cancellation = new CancellationTokenSource();
            activeCancellation = cancellation;
            SetRunning(true);
            L_Status.Text = $"搜索中：最多 {count:N0} 个种子（算法步进）…";
            activeTask = RunSearchAsync(context, start, count, cancellation.Token);
            try { await activeTask; }
            finally
            {
                activeTask = null;
                activeCancellation = null;
                if (!IsDisposed)
                {
                    SetRunning(false);
                    if (closeRequested) Close();
                }
            }
        }

        private async Task RunSearchAsync(GenerationContext context, uint start, uint count, CancellationToken token)
        {
            try
            {
                var found = await Task.Run(() => Search(context, start, count, token), token);
                token.ThrowIfCancellationRequested();
                if (closeRequested || IsDisposed) return;
                if (found.Entity == null)
                    L_Status.Text = $"已检查 {count:N0} 个种子，未找到匹配。";
                else
                {
                    ShowEntity(found.Entity);
                    seedBox.Text = $"{found.Seed:X8}";
                    L_Status.Text = "已应用匹配结果；请在 PKHeX 中核对遭遇合法性。";
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                if (!IsDisposed) L_Status.Text = "搜索已取消。";
            }
            catch (Exception ex)
            {
                if (!IsDisposed) L_Status.Text = $"搜索失败：{ex.Message}";
            }
        }

        private static (uint Seed, PKM Entity) Search(GenerationContext context, uint start, uint count, CancellationToken token)
        {
            var seed = start;
            for (uint i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();
                if (context.TryGenerate(seed, out var pk)) return (seed, pk);
                seed = context.Kind switch
                {
                    RngKind.Method1 => PIDFinder.Lib.Method1RNG.Next(seed),
                    RngKind.Overworld8 => PIDFinder.Lib.Overworld8RNG.Next(seed),
                    RngKind.Roaming8b => PIDFinder.Lib.Roaming8bRNG.Next(seed),
                    _ => throw new NotSupportedException(),
                };
                if (seed == start) break;
            }
            return (seed, null);
        }

        private void RequestCancellation()
        {
            if (activeTask == null) return;
            cancelBTN.Enabled = false;
            L_Status.Text = "正在取消…";
            activeCancellation.Cancel();
        }

        private void cancelBTN_Click(object sender, EventArgs e) => RequestCancellation();

        private void Searcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (activeTask == null) return;
            e.Cancel = true;
            closeRequested = true;
            RequestCancellation();
        }

        private void checkBTN_Click(object sender, EventArgs e)
        {
            if (activeTask != null || closeRequested) return;
            try
            {
                var context = CaptureContext();
                var seed = ReadSeed();
                if (!context.TryGenerate(seed, out var pk))
                {
                    L_Status.Text = "该种子不符合当前条件，编辑器保持原样。";
                    return;
                }
                ShowEntity(pk);
                L_Status.Text = "已应用匹配结果；请在 PKHeX 中核对遭遇合法性。";
            }
            catch (Exception ex) { L_Status.Text = ex.Message; }
        }
    }
}
