using PKHeX.Core;
using PKHeX.Core.Searching;
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

        private IEnumerable<IEncounterInfo> SearchDatabase()
        {
            var settings = GetSearchSettings();
            var versions = settings.GetVersions(SAV);

            var pk = SAV.BlankPKM;

            var species = settings.Species;
            var results = EncounterUtil.GetAllSpeciesFormEncounters(species, SAV.Personal, versions, pk);
            results = results.Where(z => z.EggEncounter == settings.SearchEgg);

            // return filtered results
            var comparer = new ReferenceComparer<IEncounterInfo>();
            results = results.Distinct(comparer); // only distinct objects

            return results;
        }

        private void Show(PkmEntry pe)
        {
            if (Results.Count == 0)
            {
                WinFormsUtil.Alert(MessageStrings.MsgDBSearchNone);
                return;
            }

            var enc = Results[0];
            var criteria = EncounterUtil.GetCriteria(enc, Editor.Data);
            var pk = enc.ConvertToPKM(SAV, criteria);
            pk.SetAbilityIndex((int)pe.Ability);
            pk.EncryptionConstant = (uint)pe.EC;
            pk.PID = pe.PID;
            pk.IV_HP = (int)pe.HP;
            pk.IV_ATK = (int)pe.Atk;
            pk.IV_DEF = (int)pe.Def;
            pk.IV_SPA = (int)pe.SpA;
            pk.IV_SPD = (int)pe.SpD;
            pk.IV_SPE = (int)pe.Spe;
            if(pk is IScaledSize s) {
                s.HeightScalar = (byte)pe.Height;
                s.WeightScalar = (byte)pe.Weight;
            }

            pk.RefreshChecksum();
            Editor.PopulateFields(pk, false);
        }

        private PkmEntry GenPkm(uint seed)
        {
            return RNGMethod switch
            {
                MethodType.Method1 => Method1RNG.GenPkm(seed, SAV),
                MethodType.Roaming8b => Roaming8bRNG.GenPkm(seed, SAV),
                _ => throw new NotSupportedException(),
            };
        }

        private uint NextSeed(uint seed)
        {
            return RNGMethod switch
            {
                MethodType.Method1 => Method1RNG.Next(seed),
                MethodType.Roaming8b => Roaming8bRNG.Next(seed),
                _ => throw new NotSupportedException(),
            };
        }

        private int GetGeneration()
        {
            return RNGMethod switch
            {
                MethodType.Method1 => 3,
                MethodType.Roaming8b => 8,
                _ => throw new NotSupportedException(),
            };
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
            var search = SearchDatabase();
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
        }

        private void BTN_Search_Click(object sender, System.EventArgs e)
        {
            IsRunning(true);
            seedBox.Text = "searching...";

            conditionPKM1.Generation = GetGeneration();

            tokenSource = new();
            Task.Factory.StartNew(
                () =>
                {
                    var seed = Util.Rand32();
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                            return;

                        var pkm = GenPkm(seed);
                        if (conditionPKM1.Check(pkm))
                        {
                            this.Invoke(() =>
                            {
                                Show(pkm);
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
                var pkm = GenPkm(seed);
                Show(pkm);
            }
        }
    }
}
