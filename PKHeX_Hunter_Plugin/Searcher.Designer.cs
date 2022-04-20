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
            this.searchBTN = new System.Windows.Forms.Button();
            this.seedBox = new System.Windows.Forms.TextBox();
            this.conditionPKM1 = new PKHeX_Hunter_Plugin.ConditionPKM();
            this.checkBTN = new System.Windows.Forms.Button();
            this.cancelBTN = new System.Windows.Forms.Button();
            this.methodTypeBox = new System.Windows.Forms.ComboBox();
            this.CB_Species = new System.Windows.Forms.ComboBox();
            this.CB_GameOrigin = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // searchBTN
            // 
            this.searchBTN.Location = new System.Drawing.Point(210, 200);
            this.searchBTN.Name = "searchBTN";
            this.searchBTN.Size = new System.Drawing.Size(109, 46);
            this.searchBTN.TabIndex = 0;
            this.searchBTN.Text = "狩猎开始！";
            this.searchBTN.UseVisualStyleBackColor = true;
            this.searchBTN.Click += new System.EventHandler(this.searchBTN_Click);
            // 
            // seedBox
            // 
            this.seedBox.Location = new System.Drawing.Point(210, 34);
            this.seedBox.MaxLength = 64;
            this.seedBox.Name = "seedBox";
            this.seedBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.seedBox.Size = new System.Drawing.Size(175, 21);
            this.seedBox.TabIndex = 1;
            this.seedBox.Text = "00000000";
            // 
            // conditionPKM1
            // 
            this.conditionPKM1.Location = new System.Drawing.Point(27, 25);
            this.conditionPKM1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.conditionPKM1.Name = "conditionPKM1";
            this.conditionPKM1.Size = new System.Drawing.Size(153, 189);
            this.conditionPKM1.TabIndex = 2;
            // 
            // checkBTN
            // 
            this.checkBTN.Location = new System.Drawing.Point(286, 61);
            this.checkBTN.Name = "checkBTN";
            this.checkBTN.Size = new System.Drawing.Size(99, 23);
            this.checkBTN.TabIndex = 3;
            this.checkBTN.Text = "Lookup";
            this.checkBTN.UseVisualStyleBackColor = true;
            this.checkBTN.Click += new System.EventHandler(this.checkBTN_Click);
            // 
            // cancelBTN
            // 
            this.cancelBTN.Location = new System.Drawing.Point(333, 200);
            this.cancelBTN.Name = "cancelBTN";
            this.cancelBTN.Size = new System.Drawing.Size(52, 46);
            this.cancelBTN.TabIndex = 4;
            this.cancelBTN.Text = "Cancel";
            this.cancelBTN.UseVisualStyleBackColor = true;
            this.cancelBTN.Visible = false;
            this.cancelBTN.Click += new System.EventHandler(this.cancelBTN_Click);
            // 
            // methodTypeBox
            // 
            this.methodTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.methodTypeBox.FormattingEnabled = true;
            this.methodTypeBox.Location = new System.Drawing.Point(210, 100);
            this.methodTypeBox.Name = "methodTypeBox";
            this.methodTypeBox.Size = new System.Drawing.Size(127, 20);
            this.methodTypeBox.TabIndex = 5;
            // 
            // CB_Species
            // 
            this.CB_Species.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CB_Species.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CB_Species.FormattingEnabled = true;
            this.CB_Species.Location = new System.Drawing.Point(210, 120);
            this.CB_Species.Margin = new System.Windows.Forms.Padding(0);
            this.CB_Species.Name = "CB_Species";
            this.CB_Species.Size = new System.Drawing.Size(122, 21);
            this.CB_Species.TabIndex = 6;
            // 
            // CB_GameOrigin
            // 
            this.CB_GameOrigin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_GameOrigin.FormattingEnabled = true;
            this.CB_GameOrigin.Location = new System.Drawing.Point(210, 143);
            this.CB_GameOrigin.Margin = new System.Windows.Forms.Padding(0);
            this.CB_GameOrigin.Name = "CB_GameOrigin";
            this.CB_GameOrigin.Size = new System.Drawing.Size(122, 21);
            this.CB_GameOrigin.TabIndex = 7;
            // 
            // Searcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 275);
            this.Controls.Add(this.cancelBTN);
            this.Controls.Add(this.checkBTN);
            this.Controls.Add(this.conditionPKM1);
            this.Controls.Add(this.seedBox);
            this.Controls.Add(this.searchBTN);
            this.Controls.Add(this.CB_Species);
            this.Controls.Add(this.CB_GameOrigin);
            this.Controls.Add(this.methodTypeBox);
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

        private System.Windows.Forms.Button searchBTN;
        private System.Windows.Forms.TextBox seedBox;
        private ConditionPKM conditionPKM1;
        private System.Windows.Forms.Button checkBTN;
        private System.Windows.Forms.Button cancelBTN;
        private System.Windows.Forms.ComboBox CB_Species;
        private System.Windows.Forms.ComboBox methodTypeBox;
        private System.Windows.Forms.ComboBox CB_GameOrigin;
    }
}