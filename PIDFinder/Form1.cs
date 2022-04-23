using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            //checkBoxComboBox1.ClearSelection();
            var CellStyle = new DataGridViewCellStyle();
            checkBoxComboBox1.Font = CellStyle.Font;
            for (int checkBoxIndex = 1; checkBoxIndex < checkBoxComboBox1.Items.Count; checkBoxIndex++)
            {
                checkBoxComboBox1.CheckBoxItems[checkBoxIndex].Text =
                    (checkBoxComboBox1.CheckBoxItems[checkBoxIndex].ComboBoxItem).ToString();
                checkBoxComboBox1.CheckBoxItems[checkBoxIndex].Font = CellStyle.Font;
            }

            checkBoxComboBox1.CheckBoxItems[0].Checked = true;
            checkBoxComboBox1.CheckBoxItems[0].Checked = false;
        }

        private PKM GenPkm(uint seed)
        {
            return RNGMethod switch
            {
                MethodType.Method1 => Method1RNG.GenPkm(seed, trainerid1.GetSIDTID()),
                MethodType.Roaming8b => Roaming8bRNG.GenPkm(seed, trainerid1.GetSIDTID()),
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
                        if (pkmConsis1.Check(pkm))
                        {
                            this.Invoke(() =>
                            {
                                ShowPkm(pkm);

                                button1.Enabled = true;
                            });
                            break;
                        }
                        seed = NextSeed(seed);
                        //System.Diagnostics.Debug.Print($"{seed:X}");
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

        private void ShowPkm(PKM pkm)
        {
            textBox1.Text = $"{pkm.PID:X8}";
            textBox2.Text = $"{pkm.EC:X8}";
            label2.Text = $"{pkm.HP}, {pkm.Atk}, {pkm.Def}, {pkm.SpA}, {pkm.SpD}, {pkm.Spe}";
            label3.Text = $"Ability: ({pkm.Ability + 1}), {pkm.Nature}";
            label4.Text = $"H:{pkm.Height}, W:{pkm.Weight}";
            label6.Text = pkm.Gen3ShinyString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var seed = uint.Parse(textBox3.Text, System.Globalization.NumberStyles.HexNumber);

            var pkm = GenPkm(seed);
            ShowPkm(pkm);
        }
    }
}
