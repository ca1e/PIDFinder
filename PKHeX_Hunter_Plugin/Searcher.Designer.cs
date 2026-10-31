using System.Drawing;
using System.Windows.Forms;

namespace PKHeX_Hunter_Plugin
{
    partial class Searcher
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                activeCancellation?.Cancel();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            TB_Species = new TextBox { Name = "TB_Species", ReadOnly = true, Location = new Point(77, 17), Size = new Size(132, 21) };
            TB_Version = new TextBox { Name = "TB_Version", ReadOnly = true, Location = new Point(77, 47), Size = new Size(132, 21) };
            CB_Method = new ComboBox { Name = "CB_Method", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(77, 77), Size = new Size(132, 21) };
            NUD_Flawless = new NumericUpDown { Name = "NUD_Flawless", Minimum = 0, Maximum = 6, Value = 3, Location = new Point(125, 109), Size = new Size(84, 21) };
            NUD_Budget = new NumericUpDown { Name = "NUD_Budget", Minimum = 1, Maximum = 100000000, Value = 1000000, ThousandsSeparator = true, Location = new Point(77, 141), Size = new Size(132, 21) };
            conditionPKM1 = new ConditionPKM { Name = "conditionPKM1", Location = new Point(225, 17), Size = new Size(316, 161) };
            seedBox = new TextBox { Name = "seedBox", MaxLength = 8, Text = "00000000", Location = new Point(77, 194), Size = new Size(132, 21) };
            checkBTN = new Button { Name = "checkBTN", Text = "Lookup", Location = new Point(225, 192), Size = new Size(75, 26) };
            BTN_Search = new Button { Name = "BTN_Search", Text = "开始搜索", Location = new Point(350, 192), Size = new Size(100, 26) };
            cancelBTN = new Button { Name = "cancelBTN", Text = "取消", Visible = false, Location = new Point(460, 192), Size = new Size(75, 26) };
            L_Status = new Label { Name = "L_Status", Text = "就绪：使用 PKHeX 当前编辑模板。", Location = new Point(16, 230), Size = new Size(525, 42) };
            var help = new Label { Text = "种子关联模式；保证V数由遭遇决定，Method1 忽略此项。\nBDSP 使用同步分支并保留模板性格。结果仍需核对遭遇合法性。", Location = new Point(16, 275), Size = new Size(525, 35) };
            Controls.AddRange(new Control[]
            {
                new Label { Text = "模板种类", AutoSize = true, Location = new Point(16, 20) },
                new Label { Text = "来源版本", AutoSize = true, Location = new Point(16, 50) },
                new Label { Text = "算法", AutoSize = true, Location = new Point(16, 80) },
                new Label { Text = "保证V数（Gen8）", AutoSize = true, Location = new Point(16, 112) },
                new Label { Text = "扫描上限", AutoSize = true, Location = new Point(16, 144) },
                new Label { Text = "种子HEX", AutoSize = true, Location = new Point(16, 197) },
                TB_Species, TB_Version, CB_Method, NUD_Flawless, NUD_Budget, conditionPKM1,
                seedBox, checkBTN, BTN_Search, cancelBTN, L_Status, help,
            });
            BTN_Search.Click += BTN_Search_Click;
            checkBTN.Click += checkBTN_Click;
            cancelBTN.Click += cancelBTN_Click;
            AutoScaleDimensions = new SizeF(6F, 12F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(555, 322);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Searcher";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "猎游者";
            ResumeLayout(false);
            PerformLayout();
        }

        private Button BTN_Search;
        private TextBox seedBox;
        private ConditionPKM conditionPKM1;
        private Button checkBTN;
        private Button cancelBTN;
        private TextBox TB_Species;
        private ComboBox CB_Method;
        private TextBox TB_Version;
        private NumericUpDown NUD_Flawless;
        private NumericUpDown NUD_Budget;
        private Label L_Status;
    }
}
