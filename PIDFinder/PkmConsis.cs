using System;
using System.Windows.Forms;
using PIDFinder.Lib;

namespace PIDFinder
{
    public partial class PkmConsis : UserControl
    {
        private CheckRule rules = new();

        /// <summary>UI 绑定的筛选规则（供 Form1 传给 Lib）。</summary>
        public CheckRule Rules => rules;

        /// <summary>当前选中的闪光类型。</summary>
        public ShinyType SelectedShiny { get; private set; } = ShinyType.Any;

        public int ShinyXor { get; set; } = 16;
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
                SelectedShiny = (ShinyType)Enum.Parse(typeof(ShinyType), this.comboBox1.SelectedItem.ToString(), false);
            };
            this.comboBox1.SelectedIndex = 0;
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
