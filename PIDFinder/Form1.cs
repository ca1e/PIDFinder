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

            tokenSource = new();
            Task.Factory.StartNew(
                () =>
                {
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                            return;
                        var seed = GetSeed();
                        this.Invoke(() =>
                        {
                            textBox3.Text = $"{seed:X}";
                        });
                        

                        var pkm = Roaming8bRNG.GenPkm((uint)seed, trainerid1.GetSIDTID());
                        if (pkmConsis1.Check(pkm))
                        {
                            this.Invoke(() =>
                            {
                                ShowPkm(pkm);

                                button1.Enabled = true;
                            });
                            break;
                        }
                    }

                    this.Invoke(() =>
                    {
                        IsRunning(false);
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
        }

        private static ulong GetSeed()
        {
            var bytes = new byte[8];
            var rand = System.Security.Cryptography.RandomNumberGenerator.Create();
            rand.GetBytes(bytes);
            return BitConverter.ToUInt64(bytes);
        }

        private void ShowPkm(PKM pkm)
        {
            textBox1.Text = $"{pkm.PID:X8}";
            textBox2.Text = $"{pkm.EC:X8}";
            label2.Text = $"{pkm.HP}, {pkm.Atk}, {pkm.Def}, {pkm.SpA}, {pkm.SpD}, {pkm.Spe}";
            label3.Text = $"Ability: {pkm.Ability}";
            label4.Text = $"H:{pkm.Height}, W:{pkm.Weight}";
            label6.Text = pkm.GetShinyString();
        }
    }
}
