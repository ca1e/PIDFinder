using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PKHeX_Hunter_Plugin
{
    public partial class ConditionPKM : UserControl
    {
        private CheckRule rules = new();

        public CheckRule MyRules
        {
            get
            {
                rules.Natures.Clear();
                for (var i = 0; i < CB_Natures.Items.Count; i++)
                {
                    if (CB_Natures.GetItemChecked(i))
                    {
                        rules.Natures.Add(i);
                    }
                }
                return rules;
            }
        }

        public ConditionPKM()
        {
            InitializeComponent();

            BindingData();

            InitRule();
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

            CB_Ability.DataSource = Enum.GetNames(typeof(AbilityType));
            CB_Gender.DataSource = Enum.GetNames(typeof(GenderType));
            CB_Shiny.DataSource = Enum.GetNames(typeof(ShinyType));

            CB_Shiny.SelectedIndexChanged += (_, __) =>
            {
                rules.Shiny = (ShinyType)CB_Shiny.SelectedIndex;
            };

            CB_Ability.SelectedIndex = 0;
            CB_Gender.SelectedIndex = 0;
            CB_Shiny.SelectedIndex = 0;
        }

        private void InitRule()
        {
            rules.Ability = -1;
            rules.Gender = -1;
            rules.Natures = new();
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

    public sealed class CheckRule
    {
        public uint minHP { get; set; }
        public uint maxHP { get; set; } = 31;
        public bool CheckHP(int HP) => (HP >= minHP && HP <= maxHP);

        public uint minAtk { get; set; }
        public uint maxAtk { get; set; } = 31;
        public bool CheckAtk(int Atk) => (Atk >= minAtk && Atk <= maxAtk);

        public uint minDef { get; set; }
        public uint maxDef { get; set; } = 31;
        public bool CheckDef(int Def) => (Def >= minDef && Def <= maxDef);

        public uint minSpA { get; set; }
        public uint maxSpA { get; set; } = 31;
        public bool CheckSpA(int SpA) => (SpA >= minSpA && SpA <= maxSpA);

        public uint minSpD { get; set; }
        public uint maxSpD { get; set; } = 31;
        public bool CheckSpD(int SpD) => (SpD >= minSpD && SpD <= maxSpD);

        public uint minSpe { get; set; }
        public uint maxSpe { get; set; } = 31;
        public bool CheckSpe(int Spe) => (Spe >= minSpe && Spe <= maxSpe);
        
        /// <summary>
        /// TODO 1/2,not hidden
        /// </summary>
        public int Ability { get; set; }
        public bool CheckAbility(int ability)
        {
            if (Ability == -1) return true;
            if (ability == Ability) return true;
            return false;
        }

        public List<int> Natures { get; set; }
        private bool AnyNature => Natures == null || Natures.Count == 0;
        public bool CheckNature(int nature)
        {
            if(AnyNature)return true;
            if (Natures.Contains(nature)) return true;
            return false;
        }

        public int Gender { get; set; }
        public bool CheckGender(int gender)
        {
            if (Ability == -1) return true;
            if(gender == Gender) return true;
            return false;
        }

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

    public enum AbilityType : byte
    {
        Any,
        Ability1,
        Ability2,
        AbilityHidden,
    }

    public enum GenderType : byte
    {
        Any,
        Male,
        Female,
    }

    public enum ShinyType : byte
    {
        Any,
        No,
        Shiny,
        Star,
        Square,
    }
}
