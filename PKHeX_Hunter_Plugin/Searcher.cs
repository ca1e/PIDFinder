using PKHeX.Core;
using PKHeX.Core.Searching;
using PIDFinder.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PKHeX_Hunter_Plugin
{
    public partial class Searcher : Form
    {
        enum MethodType
        {
            Method1,
            Roaming8b,
            Overworld8,
        }
        private SaveFile SAV { get; }
        private IPKMView Editor { get; }
        private CancellationTokenSource tokenSource = new();
        private List<IEncounterInfo> Results = new();
        private MethodType RNGMethod = MethodType.Method1;

        public Searcher(ISaveFileProvider sav, IPKMView editor)
        {
            SAV = sav.SAV;
            Editor = editor;

            InitializeComponent();
        
            BindingData();
        }

        private void BindingData()
        {
            this.CB_Method.DataSource = Enum.GetNames(typeof(MethodType));
            this.CB_Method.SelectedIndexChanged += (_, __) =>
            {
                RNGMethod = (MethodType)Enum.Parse(typeof(MethodType), this.CB_Method.SelectedItem.ToString(), false);
            };
            this.CB_Method.SelectedIndex = 0;

            CB_Species.InitializeBinding();
            CB_GameOrigin.InitializeBinding();

            var DS_Species = new List<ComboItem>(GameInfo.SpeciesDataSource);
            DS_Species.RemoveAt(0);
            CB_Species.DataSource = DS_Species;

            var Any = new ComboItem(MessageStrings.MsgAny, -1);
            var DS_Version = new List<ComboItem>(GameInfo.VersionDataSource);
            DS_Version.Insert(0, Any); CB_GameOrigin.DataSource = DS_Version;

            CB_Species.SelectedIndex = 0;
            CB_GameOrigin.SelectedIndex = 0;
        }

        private SearchSettings GetSearchSettings()
        {
            var settings = new SearchSettings
            {
                Format = SAV.Generation, // 0->(n-1) => 1->n
                Generation = SAV.Generation,

                Species = WinFormsUtil.GetIndex(CB_Species),

                Version = WinFormsUtil.GetIndex(CB_GameOrigin),

                SearchEgg = false, // skip egg
            };

            return settings;
        }

        private PKM GenEntity()
        {
            var enc = Results[0];
            var criteria = EncounterUtil.GetCriteria(enc, Editor.Data);
            return enc.ConvertToPKM(SAV, criteria);
        }

        private bool CheckEntity(PKM pk, uint seed)
        {
            return RNGMethod switch
            {
                MethodType.Method1 => PIDFinder.Lib.Method1RNG.TryApplyFromSeed(ref pk, SAV, conditionPKM1.MyRules, seed),
                MethodType.Roaming8b => PIDFinder.Lib.Roaming8bRNG.TryApplyFromSeed(ref pk, SAV, conditionPKM1.MyRules, seed),
                MethodType.Overworld8 => PIDFinder.Lib.Overworld8RNG.TryApplyFromSeed(ref pk, SAV, conditionPKM1.MyRules, seed),
                _ => throw new NotSupportedException(),
            };
        }

        private uint NextSeed(uint seed)
        {
            return RNGMethod switch
            {
                MethodType.Method1 => PIDFinder.Lib.Method1RNG.Next(seed),
                MethodType.Roaming8b => PIDFinder.Lib.Roaming8bRNG.Next(seed),
                MethodType.Overworld8 => PIDFinder.Lib.Overworld8RNG.Next(seed),
                _ => throw new NotSupportedException(),
            };
        }

        private void ShowEntity(PKM pk)
        {
            pk.RefreshChecksum();

            Editor.PopulateFields(pk, false);
        }

        private void IsRunning(bool running)
        {
            BTN_Search.Enabled = !running;
            conditionPKM1.Enabled = !running;
            cancelBTN.Visible = running;
            checkBTN.Enabled = !running;
        }

        private async void BTN_Encounter_Click(object sender, EventArgs e)
        {
            var search = EncounterUtil.SearchDatabase(GetSearchSettings(), SAV);
            var results = await Task.Run(() => search.ToList()).ConfigureAwait(true);
            if (results.Count == 0)
            {
                WinFormsUtil.Alert(MessageStrings.MsgDBSearchNone);
            }
            else
            {
                WinFormsUtil.Alert("可以狩猎！");
            }
            Results = results;

            if (Results.Count == 0)
            {
                WinFormsUtil.Alert(MessageStrings.MsgDBSearchNone);
            }
        }

        private void BTN_Search_Click(object sender, EventArgs e)
        {
            IsRunning(true);
            seedBox.Text = "searching...";

            tokenSource = new();
            Task.Factory.StartNew(
                () =>
                {
                    if (Results.Count == 0)
                    {
                        WinFormsUtil.Alert(MessageStrings.MsgDBSearchNone);
                        return;
                    }

                    var pk = GenEntity();

                    var seed = Util.Rand32();
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                            return;

                        if (CheckEntity(pk, seed))
                        {
                            this.Invoke(() =>
                            {
                                ShowEntity(pk);
                            });
                            break;
                        }
                        seed = NextSeed(seed);
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
                var pk = GenEntity();
                CheckEntity(pk, seed);
                ShowEntity(pk);
            }
        }
    }
}
