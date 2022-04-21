namespace PKHeX_Hunter_Plugin
{
    partial class Searcher
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BTN_Search = new System.Windows.Forms.Button();
            this.seedBox = new System.Windows.Forms.TextBox();
            this.checkBTN = new System.Windows.Forms.Button();
            this.cancelBTN = new System.Windows.Forms.Button();
            this.CB_Method = new System.Windows.Forms.ComboBox();
            this.CB_Species = new System.Windows.Forms.ComboBox();
            this.CB_GameOrigin = new System.Windows.Forms.ComboBox();
            this.conditionPKM1 = new PKHeX_Hunter_Plugin.ConditionPKM();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BTN_Encounter = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BTN_Search
            // 
            this.BTN_Search.Location = new System.Drawing.Point(355, 184);
            this.BTN_Search.Name = "BTN_Search";
            this.BTN_Search.Size = new System.Drawing.Size(117, 38);
            this.BTN_Search.TabIndex = 0;
            this.BTN_Search.Text = "狩猎开始！";
            this.BTN_Search.UseVisualStyleBackColor = true;
            this.BTN_Search.Click += new System.EventHandler(this.BTN_Search_Click);
            // 
            // seedBox
            // 
            this.seedBox.Location = new System.Drawing.Point(18, 21);
            this.seedBox.MaxLength = 64;
            this.seedBox.Name = "seedBox";
            this.seedBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.seedBox.Size = new System.Drawing.Size(175, 21);
            this.seedBox.TabIndex = 1;
            this.seedBox.Text = "00000000";
            // 
            // checkBTN
            // 
            this.checkBTN.Location = new System.Drawing.Point(94, 48);
            this.checkBTN.Name = "checkBTN";
            this.checkBTN.Size = new System.Drawing.Size(99, 23);
            this.checkBTN.TabIndex = 3;
            this.checkBTN.Text = "Lookup";
            this.checkBTN.UseVisualStyleBackColor = true;
            this.checkBTN.Click += new System.EventHandler(this.checkBTN_Click);
            // 
            // cancelBTN
            // 
            this.cancelBTN.Location = new System.Drawing.Point(478, 184);
            this.cancelBTN.Name = "cancelBTN";
            this.cancelBTN.Size = new System.Drawing.Size(52, 38);
            this.cancelBTN.TabIndex = 4;
            this.cancelBTN.Text = "Cancel";
            this.cancelBTN.UseVisualStyleBackColor = true;
            this.cancelBTN.Visible = false;
            this.cancelBTN.Click += new System.EventHandler(this.cancelBTN_Click);
            // 
            // CB_Method
            // 
            this.CB_Method.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_Method.FormattingEnabled = true;
            this.CB_Method.Location = new System.Drawing.Point(66, 88);
            this.CB_Method.Name = "CB_Method";
            this.CB_Method.Size = new System.Drawing.Size(127, 20);
            this.CB_Method.TabIndex = 5;
            // 
            // CB_Species
            // 
            this.CB_Species.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_Species.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_Species.FormattingEnabled = true;
            this.CB_Species.Location = new System.Drawing.Point(66, 118);
            this.CB_Species.Margin = new System.Windows.Forms.Padding(0);
            this.CB_Species.Name = "CB_Species";
            this.CB_Species.Size = new System.Drawing.Size(127, 20);
            this.CB_Species.TabIndex = 6;
            // 
            // CB_GameOrigin
            // 
            this.CB_GameOrigin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_GameOrigin.FormattingEnabled = true;
            this.CB_GameOrigin.Location = new System.Drawing.Point(66, 147);
            this.CB_GameOrigin.Margin = new System.Windows.Forms.Padding(0);
            this.CB_GameOrigin.Name = "CB_GameOrigin";
            this.CB_GameOrigin.Size = new System.Drawing.Size(127, 20);
            this.CB_GameOrigin.TabIndex = 7;
            // 
            // conditionPKM1
            // 
            this.conditionPKM1.Generation = 8;
            this.conditionPKM1.Location = new System.Drawing.Point(220, 21);
            this.conditionPKM1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.conditionPKM1.Name = "conditionPKM1";
            this.conditionPKM1.Size = new System.Drawing.Size(316, 161);
            this.conditionPKM1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 121);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "Species";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 150);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "Version";
            // 
            // BTN_Encounter
            // 
            this.BTN_Encounter.Location = new System.Drawing.Point(76, 184);
            this.BTN_Encounter.Name = "BTN_Encounter";
            this.BTN_Encounter.Size = new System.Drawing.Size(117, 38);
            this.BTN_Encounter.TabIndex = 10;
            this.BTN_Encounter.Text = "寻找猎物";
            this.BTN_Encounter.UseVisualStyleBackColor = true;
            this.BTN_Encounter.Click += new System.EventHandler(this.BTN_Encounter_Click);
            // 
            // Searcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 244);
            this.Controls.Add(this.BTN_Encounter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelBTN);
            this.Controls.Add(this.checkBTN);
            this.Controls.Add(this.conditionPKM1);
            this.Controls.Add(this.seedBox);
            this.Controls.Add(this.BTN_Search);
            this.Controls.Add(this.CB_Species);
            this.Controls.Add(this.CB_GameOrigin);
            this.Controls.Add(this.CB_Method);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Searcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "猎游者";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BTN_Search;
        private System.Windows.Forms.TextBox seedBox;
        private ConditionPKM conditionPKM1;
        private System.Windows.Forms.Button checkBTN;
        private System.Windows.Forms.Button cancelBTN;
        private System.Windows.Forms.ComboBox CB_Species;
        private System.Windows.Forms.ComboBox CB_Method;
        private System.Windows.Forms.ComboBox CB_GameOrigin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BTN_Encounter;
    }
}