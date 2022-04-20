using PKHeX.Core;
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
        private ISaveFileProvider SAV { get; }
        private IPKMView Editor { get; }

        private CancellationTokenSource tokenSource = new();

        private MethodType RNGMethod = MethodType.Method1;

        public Searcher(ISaveFileProvider sav, IPKMView editor)
        {
            SAV = sav;
            Editor = editor;

            InitializeComponent();
        
            BindingData();
        }

        private void BindingData()
        {
            this.methodTypeBox.DataSource = Enum.GetNames(typeof(MethodType));
            this.methodTypeBox.SelectedIndexChanged += (_, __) =>
            {
                RNGMethod = (MethodType)Enum.Parse(typeof(MethodType), this.methodTypeBox.SelectedItem.ToString(), false);
            };
            this.methodTypeBox.SelectedIndex = 0;

            CB_Species.InitializeBinding();
            CB_GameOrigin.InitializeBinding();

            var Any = new ComboItem(MessageStrings.MsgAny, -1);
            var DS_Species = new List<ComboItem>(GameInfo.SpeciesDataSource);
            DS_Species.RemoveAt(0);
            CB_Species.DataSource = DS_Species;

            var DS_Version = new List<ComboItem>(GameInfo.VersionDataSource);
            DS_Version.Insert(0, Any); CB_GameOrigin.DataSource = DS_Version;

            CB_Species.SelectedIndex = 0;
            CB_GameOrigin.SelectedIndex = 0;
        }

        private void show(PkmEntry pe)
        {
            int species = WinFormsUtil.GetIndex(CB_Species);
            var Version = WinFormsUtil.GetIndex(CB_GameOrigin);

            var encs = EncounterUtil.SearchEncounters(species, 0, SAV.SAV.BlankPKM, (GameVersion)Version);

            // skip egg
            var enc = encs.Where(z => z.EggEncounter == false).First();

            if (enc != null)
            {
                var criteria = EncounterUtil.GetCriteria(enc, Editor.Data);
                var pk = enc.ConvertToPKM(SAV.SAV, criteria);
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
        }

        private PkmEntry GenPkm(uint seed)
        {
            
            //MessageBox.Show($"{((ITrainerID)SAV.SAV).SID}, {((ITrainerID)SAV.SAV).TID}");
            return RNGMethod switch
            {
                MethodType.Method1 => Method1RNG.GenPkm(seed, SAV.SAV),
                MethodType.Roaming8b => Roaming8bRNG.GenPkm(seed, SAV.SAV),
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

        private int TypeXor()
        {
            return RNGMethod switch
            {
                MethodType.Method1 => 8,
                MethodType.Roaming8b => 16,
                _ => throw new NotSupportedException(),
            };
        }

        private GameVersion GetSearchVer()
        {
            return RNGMethod switch
            {
                MethodType.Method1 => GameVersion.E,
                MethodType.Roaming8b => (GameVersion)SAV.SAV.Game,
                _ => throw new NotSupportedException(),
            };
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

            conditionPKM1.ShinyXor = TypeXor();

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
                                show(pkm);
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
                show(pkm);
            }
        }
    }
}
