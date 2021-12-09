using System;
using System.Windows.Forms;

namespace PIDFinder
{
    public partial class PkmConsis : UserControl
    {
        private CheckRule rules = new();
        private ShinyType selectedShiny = ShinyType.None;
        public PkmConsis()
        {
            InitializeComponent();

            BindingData();
        }

        private void BindingData()
        {
            this.HPMin.DataBindings.Add("Text", rules, "minHP");
            this.HPMax.DataBindings.Add("Text", rules, "maxHP");
            this.AtkMin.DataBindings.Add("Text", rules, "minAtk");
            this.AtkMax.DataBindings.Add("Text", rules, "maxAtk");
            this.DefMin.DataBindings.Add("Text", rules, "minDef");
            this.DefMax.DataBindings.Add("Text", rules, "maxDef");
            this.SpAMin.DataBindings.Add("Text", rules, "minSpA");
            this.SpAMax.DataBindings.Add("Text", rules, "maxSpA");
            this.SpDMin.DataBindings.Add("Text", rules, "minSpD");
            this.SpDMax.DataBindings.Add("Text", rules, "maxSpD");
            this.SpeMin.DataBindings.Add("Text", rules, "minSpe");
            this.SpeMax.DataBindings.Add("Text", rules, "maxSpe");

            this.comboBox1.DataSource = Enum.GetNames(typeof(ShinyType));
            this.comboBox1.SelectedIndexChanged += (_, __) =>
            {
                selectedShiny = (ShinyType)Enum.Parse(typeof(ShinyType), this.comboBox1.SelectedItem.ToString(), false);
            };
            this.comboBox1.SelectedIndex = 0;
        }

        public bool Check(PKM pkm)
        {
            // check ivs
            if (pkm.HP < rules.minHP || pkm.HP > rules.maxHP)
                return false;
            if (pkm.Atk < rules.minAtk || pkm.Atk > rules.maxAtk)
                return false;
            if (pkm.Def < rules.minDef || pkm.Def > rules.maxDef)
                return false;
            if (pkm.SpA < rules.minSpA || pkm.SpA > rules.maxSpA)
                return false;
            if (pkm.SpD < rules.minSpD || pkm.SpD > rules.maxSpD)
                return false;
            if (pkm.Spe < rules.minSpe || pkm.Spe > rules.maxSpe)
                return false;
            // check shiny
            var matchShiny = selectedShiny switch
            {
                ShinyType.None => pkm.ShinyStatus == Shiny.Never,
                ShinyType.Shiny => pkm.ShinyStatus != Shiny.Never,
                ShinyType.Star => pkm.ShinyStatus == Shiny.AlwaysStar,
                ShinyType.Square => pkm.ShinyStatus == Shiny.AlwaysSquare,
                _ => true,
            };
            if (!matchShiny) return false;
            return true;
        }

        private void IVS_TextChanged(object sender, EventArgs e)
        {
            var txtbox = (TextBox)sender;

            if (!uint.TryParse(txtbox.Text, out var iv))
                iv = 0;
            if (iv < 0 || iv > 31)
            {
                iv = 0;
                txtbox.Text = "0";
            }

            //System.Diagnostics.Debug.Print(iv.ToString());
        }
    }

    class CheckRule
    {
        public uint minHP { get; set; }
        public uint maxHP { get; set; } = 31;
        //
        public uint minAtk { get; set; }
        public uint maxAtk { get; set; } = 31;
        //
        public uint minDef { get; set; }
        public uint maxDef { get; set; } = 31;
        //
        public uint minSpA { get; set; }
        public uint maxSpA { get; set; } = 31;
        //
        public uint minSpD { get; set; }
        public uint maxSpD { get; set; } = 31;
        //
        public uint minSpe { get; set; }
        public uint maxSpe { get; set; } = 31;
    }

    enum ShinyType : byte
    {
        None,
        Shiny,
        Star,
        Square,
    }
}
