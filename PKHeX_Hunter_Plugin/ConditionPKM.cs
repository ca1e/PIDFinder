using PKHeX.Core;
using System;
using System.Windows.Forms;

namespace PKHeX_Hunter_Plugin
{
    public partial class ConditionPKM : UserControl
    {
        private CheckRule rules = new();
        public int Generation { get; set; } = 8;

        public ConditionPKM()
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

            CB_Natures.MaxDropDownItems = GameInfo.NatureDataSource.Count;
            CB_Natures.Text = MessageStrings.MsgAny;
            CB_Natures.DefaultValue = MessageStrings.MsgAny;
            CB_Natures.Items.AddRange(GameInfo.Strings.natures);

            CB_Shiny.DataSource = Enum.GetNames(typeof(ShinyType));
            CB_Shiny.SelectedIndexChanged += (_, __) =>
            {
                rules.Shiny = (ShinyType)CB_Shiny.SelectedIndex;
            };

            CB_Shiny.SelectedIndex = 0;
        }

        public bool Check(PkmEntry pkm)
        {
            // check ivs
            if (! rules.CheckHP(pkm.HP))
                return false;
            if (! rules.CheckAtk(pkm.Atk))
                return false;
            if (! rules.CheckDef(pkm.Def))
                return false;
            if (! rules.CheckSpA(pkm.SpA))
                return false;
            if (! rules.CheckSpD(pkm.SpD))
                return false;
            if (! rules.CheckSpe(pkm.Spe))
                return false;
            // check shiny
            if (! rules.CheckShiny(pkm.ShinyStatus, Generation))
                return false;
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
        }
    }

    sealed class CheckRule
    {
        public uint minHP { get; set; }
        public uint maxHP { get; set; } = 31;
        public bool CheckHP(uint HP) => (HP >= minHP && HP <= maxHP);

        public uint minAtk { get; set; }
        public uint maxAtk { get; set; } = 31;
        public bool CheckAtk(uint Atk) => (Atk >= minAtk && Atk <= maxAtk);

        public uint minDef { get; set; }
        public uint maxDef { get; set; } = 31;
        public bool CheckDef(uint Def) => (Def >= minDef && Def <= maxDef);

        public uint minSpA { get; set; }
        public uint maxSpA { get; set; } = 31;
        public bool CheckSpA(uint SpA) => (SpA >= minSpA && SpA <= maxSpA);

        public uint minSpD { get; set; }
        public uint maxSpD { get; set; } = 31;
        public bool CheckSpD(uint SpD) => (SpD >= minSpD && SpD <= maxSpD);

        public uint minSpe { get; set; }
        public uint maxSpe { get; set; } = 31;
        public bool CheckSpe(uint Spe) => (Spe >= minSpe && Spe <= maxSpe);
        //
        public int Ability { get; set; }

        public int[] Natures { get; set; }

        public int Gender { get; set; }

        public ShinyType Shiny = ShinyType.Any;
        public bool CheckShiny(int shinyxor, int gen = 7)
        {
            var xornum = (gen >= 7 ? 16 : 8);
            return Shiny switch
            {
                ShinyType.Square => shinyxor == 0,
                ShinyType.Star => shinyxor < xornum && shinyxor != 0,
                ShinyType.Shiny => shinyxor < xornum,
                ShinyType.No => shinyxor >= xornum,
                _ => true,
            };
        }
    }

    enum ShinyType : byte
    {
        Any,
        No,
        Shiny,
        Star,
        Square,
    }
}
