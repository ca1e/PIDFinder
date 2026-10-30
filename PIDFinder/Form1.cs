using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PIDFinder.Lib;

namespace PIDFinder
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource tokenSource = new();
        private MethodType RNGMethod = MethodType.Method1;
        public Form1()
        {
            InitializeComponent();

            BindingData();
        }

        private void BindingData()
        {
            this.comboBox1.DataSource = Enum.GetNames(typeof(MethodType));
            this.comboBox1.SelectedIndexChanged += (_, __) =>
            {
                RNGMethod = (MethodType)Enum.Parse(typeof(MethodType), this.comboBox1.SelectedItem.ToString(), false);
            };
            this.comboBox1.SelectedIndex = 0;

            checkBoxComboBox1.Items.AddRange(Enum.GetNames(typeof(MethodType)));
            checkBoxComboBox1.SelectedIndex = 0;
            checkBoxComboBox1.ClearSelection();

            checkBoxComboBox1.CheckBoxItems[0].Checked = true;
            checkBoxComboBox1.CheckBoxItems[0].Checked = false;
        }

        private PkmResult? GenPkm(uint seed)
        {
            var rules = pkmConsis1.Rules;
            rules.Shiny = pkmConsis1.SelectedShiny;
            return RNGMethod switch
            {
                MethodType.Method1 => Method1RNG.Generate(seed, trainerid1.GetSIDTID().TID16, trainerid1.GetSIDTID().SID16, rules),
                MethodType.Roaming8b => Roaming8bRNG.Generate(seed, trainerid1.GetSIDTID().TID16, trainerid1.GetSIDTID().SID16, rules),
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

        private void button1_Click(object sender, EventArgs e)
        {
            IsRunning(true);
            textBox3.Text = "searching...";

            pkmConsis1.ShinyXor = TypeXor();

            tokenSource = new();
            Task.Factory.StartNew(
                () =>
                {
                    var seed = RandUtil.Rand32();
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                            return;

                        var pkm = GenPkm(seed);
                        if (pkm is not null)
                        {
                            var result = pkm;
                            this.Invoke(() =>
                            {
                                ShowPkm(result);

                                button1.Enabled = true;
                            });
                            break;
                        }
                        seed = NextSeed(seed);
                    }

                    this.Invoke(() =>
                    {
                        IsRunning(false);
                        textBox3.Text = $"{seed:X}";
                    });
                },
                tokenSource.Token);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
            IsRunning(false);
        }

        private void IsRunning(bool running)
        {
            button1.Enabled = !running;
            pkmConsis1.Enabled = !running;
            button2.Visible = running;
            button3.Enabled = !running;
        }

        private void ShowPkm(PkmResult pkm)
        {
            textBox1.Text = $"{pkm.PID:X8}";
            textBox2.Text = $"{pkm.EC:X8}";
            label2.Text = $"{pkm.IVs[0]}, {pkm.IVs[1]}, {pkm.IVs[2]}, {pkm.IVs[3]}, {pkm.IVs[4]}, {pkm.IVs[5]}";
            label3.Text = $"Ability: ({pkm.Ability + 1}), {pkm.Nature}";
            label4.Text = $"H:{pkm.HeightScalar}, W:{pkm.WeightScalar}";
            label6.Text = pkm.ShinyXor switch
            {
                0 => "Square!",
                < 8 => "Star!",
                _ => "No Shiny",
            };
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var seed = uint.Parse(textBox3.Text, System.Globalization.NumberStyles.HexNumber);

            var pkm = GenPkm(seed);
            if (pkm is null)
                return;
            ShowPkm(pkm);
        }
    }
}
