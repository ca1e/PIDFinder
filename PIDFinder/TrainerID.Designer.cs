namespace PIDFinder
{
    partial class TrainerID
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SID_TXT = new System.Windows.Forms.TextBox();
            this.TID_TXT = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SID_TXT
            // 
            this.SID_TXT.Location = new System.Drawing.Point(39, 2);
            this.SID_TXT.MaxLength = 4;
            this.SID_TXT.Name = "SID_TXT";
            this.SID_TXT.Size = new System.Drawing.Size(37, 23);
            this.SID_TXT.TabIndex = 0;
            this.SID_TXT.Text = "0000";
            this.SID_TXT.TextChanged += new System.EventHandler(this.SID_TXT_TextChanged);
            // 
            // TID_TXT
            // 
            this.TID_TXT.Location = new System.Drawing.Point(125, 2);
            this.TID_TXT.MaxLength = 6;
            this.TID_TXT.Name = "TID_TXT";
            this.TID_TXT.Size = new System.Drawing.Size(51, 23);
            this.TID_TXT.TabIndex = 1;
            this.TID_TXT.Text = "999999";
            this.TID_TXT.TextChanged += new System.EventHandler(this.TID_TXT_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "SID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(88, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "TID:";
            // 
            // TrainerID
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TID_TXT);
            this.Controls.Add(this.SID_TXT);
            this.Name = "TrainerID";
            this.Size = new System.Drawing.Size(180, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox SID_TXT;
        private System.Windows.Forms.TextBox TID_TXT;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
