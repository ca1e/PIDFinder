using PKHeX.Core;
using PIDFinder.Lib;
using System;
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
}
