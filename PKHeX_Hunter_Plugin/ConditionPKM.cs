using PKHeX.Core;
using PIDFinder.Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PKHeX_Hunter_Plugin
{
    public partial class ConditionPKM : UserControl
    {
        public ConditionPKM()
        {
            InitializeComponent();
            CB_Natures.MaxDropDownItems = GameInfo.Sources.NatureDataSource.Count;
            CB_Natures.Text = MessageStrings.MsgAny;
            CB_Natures.DefaultValue = MessageStrings.MsgAny;
            CB_Natures.Items.AddRange(GameInfo.Strings.natures);
            CB_Ability.DataSource = new[] { "Any", "Ability 1", "Ability 2", "Hidden" };
            CB_Gender.DataSource = new[] { "Any", "Male", "Female", "Genderless" };
            CB_Shiny.DataSource = Enum.GetNames(typeof(ShinyType));
        }

        // Read controls exactly once on the UI thread. No data binding or shared rule object.
        public CheckRule ReadRulesSnapshot()
        {
            if (InvokeRequired)
                throw new InvalidOperationException("筛选条件必须在界面线程读取。");
            var rule = new CheckRule
            {
                minHP = ReadIV(HPMin), maxHP = ReadIV(HPMax),
                minAtk = ReadIV(AtkMin), maxAtk = ReadIV(AtkMax),
                minDef = ReadIV(DefMin), maxDef = ReadIV(DefMax),
                minSpA = ReadIV(SpAMin), maxSpA = ReadIV(SpAMax),
                minSpD = ReadIV(SpDMin), maxSpD = ReadIV(SpDMax),
                minSpe = ReadIV(SpeMin), maxSpe = ReadIV(SpeMax),
                Ability = CB_Ability.SelectedIndex switch { 0 => -1, 1 => 0, 2 => 1, 3 => 2, _ => throw new ArgumentException("请选择特性。") },
                Gender = CB_Gender.SelectedIndex switch { 0 => -1, 1 => 0, 2 => 1, 3 => 2, _ => throw new ArgumentException("请选择性别。") },
                Shiny = (ShinyType)CB_Shiny.SelectedIndex,
                Natures = new List<int>(),
            };
            for (var i = 0; i < CB_Natures.Items.Count; i++)
                if (CB_Natures.GetItemChecked(i)) rule.Natures.Add(i);
            return rule.Snapshot();
        }

        private static uint ReadIV(TextBox box)
        {
            if (!uint.TryParse(box.Text, out var iv) || iv > 31)
                throw new ArgumentException($"{box.Name}：个体值必须为 0 至 31。");
            return iv;
        }

        private void IVS_TextChanged(object sender, EventArgs e)
        {
            var box = (TextBox)sender;
            box.BackColor = uint.TryParse(box.Text, out var iv) && iv <= 31 ? SystemColors.Window : Color.MistyRose;
        }
    }
}
