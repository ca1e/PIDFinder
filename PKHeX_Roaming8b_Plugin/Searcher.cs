using PKHeX.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PKHeX_Roaming8b_Plugin
{
    public partial class Searcher : Form
    {
        private ISaveFileProvider SAV { get; }
        private IPKMView Editor { get; }

        private CancellationTokenSource tokenSource = new();

        public Searcher(ISaveFileProvider sav, IPKMView editor)
        {
            SAV = sav;
            Editor = editor;

            InitializeComponent();
        }

        private void show(PkmEntry pe)
        {
            var encs = EncounterUtil.SearchEncounters(SAV.SAV.BlankPKM, (GameVersion)SAV.SAV.Game);
            var criteria = EncounterUtil.GetCriteria(Editor.Data);
            var enc = encs.First();
            if (enc != null)
            {
                var pk = enc.ConvertToPKM(SAV.SAV, criteria);

                pk.EncryptionConstant = (uint)pe.EC;
                pk.PID = pe.PID;
                pk.SetAbilityIndex((int)pe.Ability);
                pk.IV_HP = (int)pe.HP;
                pk.IV_ATK = (int)pe.Atk;
                pk.IV_DEF = (int)pe.Def;
                pk.IV_SPA = (int)pe.SpA;
                pk.IV_SPD = (int)pe.SpD;
                pk.IV_SPE = (int)pe.Spe;
                var scale = (IScaledSize)pk;
                scale.HeightScalar = (int)pe.Height;
                scale.WeightScalar = (int)pe.Weight;

                Editor.PopulateFields(pk);
            }
        }

        private void IsRunning(bool running)
        {
            searchBTN.Enabled = !running;
            conditionPKM1.Enabled = !running;
            cancelBTN.Visible = running;
            checkBTN.Enabled = !running;
        }

        private void searchBTN_Click(object sender, System.EventArgs e)
        {
            IsRunning(true);
            seedBox.Text = "searching...";

            tokenSource = new();
            Task.Factory.StartNew(
                () =>
                {
                    var seed = Util.Rand32();
                    var xoro = new Xoroshiro128Plus8b(seed);
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                            return;

                        var pkm = Roaming8bRNG.GenPkm(seed, SAV.SAV);
                        if (conditionPKM1.Check(pkm))
                        {
                            this.Invoke(() =>
                            {
                                show(pkm);
                            });
                            break;
                        }
                        seed = (uint)xoro.Next();
                    }

                    this.Invoke(() =>
                    {
                        IsRunning(false);
                        seedBox.Text = $"{seed:X}";
                    });
                },
                tokenSource.Token);
        }

        private void cancelBTN_Click(object sender, System.EventArgs e)
        {
            tokenSource.Cancel();
            IsRunning(false);
        }

        private void checkBTN_Click(object sender, System.EventArgs e)
        {
            var result = uint.TryParse(seedBox.Text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var seed);
            if(result)
            {
                var pkm = Roaming8bRNG.GenPkm(seed, SAV.SAV);
                show(pkm);
            }
        }
    }
}
