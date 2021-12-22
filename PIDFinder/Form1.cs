using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PIDFinder
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource tokenSource = new();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IsRunning(true);
            textBox3.Text = "searching...";

            tokenSource = new();
            Task.Factory.StartNew(
                () =>
                {
                    var seed = RandUtil.Rand32();
                    var xoro = new Xoroshiro128Plus8b(seed);
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                            return;

                        var pkm = Roaming8bRNG.GenPkm(seed, trainerid1.GetSIDTID());
                        if (pkmConsis1.Check(pkm))
                        {
                            this.Invoke(() =>
                            {
                                ShowPkm(pkm);

                                button1.Enabled = true;
                            });
                            break;
                        }
                        seed = (uint)xoro.Next();
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
            label3.Text = $"Ability: ({pkm.Ability + 1})";
            label4.Text = $"H:{pkm.Height}, W:{pkm.Weight}";
            label6.Text = pkm.GetShinyString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var seed = uint.Parse(textBox3.Text, System.Globalization.NumberStyles.HexNumber);

            var pkm = Roaming8bRNG.GenPkm(seed, trainerid1.GetSIDTID());
            ShowPkm(pkm);
        }
    }
}
