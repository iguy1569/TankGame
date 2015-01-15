namespace Client
{
    partial class ConnectDialog
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
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.tb_Address = new System.Windows.Forms.TextBox();
            this.tb_Name = new System.Windows.Forms.TextBox();
            this.lbl_Address = new System.Windows.Forms.Label();
            this.lbl_Name = new System.Windows.Forms.Label();
            this.gb_Tanks = new System.Windows.Forms.GroupBox();
            this.rb_Tank4 = new System.Windows.Forms.RadioButton();
            this.rb_Tank3 = new System.Windows.Forms.RadioButton();
            this.rb_Tank2 = new System.Windows.Forms.RadioButton();
            this.rb_Tank1 = new System.Windows.Forms.RadioButton();
            this.gb_Tanks.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(13, 13);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(95, 13);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // tb_Address
            // 
            this.tb_Address.Location = new System.Drawing.Point(172, 42);
            this.tb_Address.Name = "tb_Address";
            this.tb_Address.Size = new System.Drawing.Size(100, 20);
            this.tb_Address.TabIndex = 2;
            this.tb_Address.Text = "10.132.9.104";
            // 
            // tb_Name
            // 
            this.tb_Name.Location = new System.Drawing.Point(172, 68);
            this.tb_Name.MaxLength = 8;
            this.tb_Name.Name = "tb_Name";
            this.tb_Name.Size = new System.Drawing.Size(100, 20);
            this.tb_Name.TabIndex = 3;
            this.tb_Name.Text = "Default";
            // 
            // lbl_Address
            // 
            this.lbl_Address.AutoSize = true;
            this.lbl_Address.Location = new System.Drawing.Point(121, 45);
            this.lbl_Address.Name = "lbl_Address";
            this.lbl_Address.Size = new System.Drawing.Size(45, 13);
            this.lbl_Address.TabIndex = 4;
            this.lbl_Address.Text = "Address";
            // 
            // lbl_Name
            // 
            this.lbl_Name.AutoSize = true;
            this.lbl_Name.Location = new System.Drawing.Point(131, 71);
            this.lbl_Name.Name = "lbl_Name";
            this.lbl_Name.Size = new System.Drawing.Size(35, 13);
            this.lbl_Name.TabIndex = 5;
            this.lbl_Name.Text = "Name";
            // 
            // gb_Tanks
            // 
            this.gb_Tanks.BackColor = System.Drawing.Color.White;
            this.gb_Tanks.Controls.Add(this.rb_Tank4);
            this.gb_Tanks.Controls.Add(this.rb_Tank3);
            this.gb_Tanks.Controls.Add(this.rb_Tank2);
            this.gb_Tanks.Controls.Add(this.rb_Tank1);
            this.gb_Tanks.Location = new System.Drawing.Point(13, 95);
            this.gb_Tanks.Name = "gb_Tanks";
            this.gb_Tanks.Size = new System.Drawing.Size(563, 295);
            this.gb_Tanks.TabIndex = 6;
            this.gb_Tanks.TabStop = false;
            this.gb_Tanks.Text = "Tank Selection";
            // 
            // rb_Tank4
            // 
            this.rb_Tank4.AutoSize = true;
            this.rb_Tank4.BackColor = System.Drawing.Color.White;
            this.rb_Tank4.Location = new System.Drawing.Point(472, 19);
            this.rb_Tank4.Name = "rb_Tank4";
            this.rb_Tank4.Size = new System.Drawing.Size(59, 17);
            this.rb_Tank4.TabIndex = 3;
            this.rb_Tank4.Text = "Tank 4";
            this.rb_Tank4.UseVisualStyleBackColor = false;
            this.rb_Tank4.CheckedChanged += new System.EventHandler(this.rb_Tank4_CheckedChanged);
            // 
            // rb_Tank3
            // 
            this.rb_Tank3.AutoSize = true;
            this.rb_Tank3.BackColor = System.Drawing.Color.White;
            this.rb_Tank3.Location = new System.Drawing.Point(381, 19);
            this.rb_Tank3.Name = "rb_Tank3";
            this.rb_Tank3.Size = new System.Drawing.Size(59, 17);
            this.rb_Tank3.TabIndex = 2;
            this.rb_Tank3.Text = "Tank 3";
            this.rb_Tank3.UseVisualStyleBackColor = false;
            this.rb_Tank3.CheckedChanged += new System.EventHandler(this.rb_Tank3_CheckedChanged);
            // 
            // rb_Tank2
            // 
            this.rb_Tank2.AutoSize = true;
            this.rb_Tank2.BackColor = System.Drawing.Color.White;
            this.rb_Tank2.Location = new System.Drawing.Point(290, 19);
            this.rb_Tank2.Name = "rb_Tank2";
            this.rb_Tank2.Size = new System.Drawing.Size(59, 17);
            this.rb_Tank2.TabIndex = 1;
            this.rb_Tank2.Text = "Tank 2";
            this.rb_Tank2.UseVisualStyleBackColor = false;
            this.rb_Tank2.CheckedChanged += new System.EventHandler(this.rb_Tank2_CheckedChanged);
            // 
            // rb_Tank1
            // 
            this.rb_Tank1.AutoSize = true;
            this.rb_Tank1.BackColor = System.Drawing.Color.White;
            this.rb_Tank1.Checked = true;
            this.rb_Tank1.Location = new System.Drawing.Point(199, 19);
            this.rb_Tank1.Name = "rb_Tank1";
            this.rb_Tank1.Size = new System.Drawing.Size(59, 17);
            this.rb_Tank1.TabIndex = 0;
            this.rb_Tank1.TabStop = true;
            this.rb_Tank1.Text = "Tank 1";
            this.rb_Tank1.UseVisualStyleBackColor = false;
            this.rb_Tank1.CheckedChanged += new System.EventHandler(this.rb_Tank1_CheckedChanged);
            // 
            // ConnectDialog
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(588, 402);
            this.Controls.Add(this.gb_Tanks);
            this.Controls.Add(this.lbl_Name);
            this.Controls.Add(this.lbl_Address);
            this.Controls.Add(this.tb_Name);
            this.Controls.Add(this.tb_Address);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Name = "ConnectDialog";
            this.gb_Tanks.ResumeLayout(false);
            this.gb_Tanks.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.TextBox tb_Address;
        private System.Windows.Forms.TextBox tb_Name;
        private System.Windows.Forms.Label lbl_Address;
        private System.Windows.Forms.Label lbl_Name;
        private System.Windows.Forms.GroupBox gb_Tanks;
        private System.Windows.Forms.RadioButton rb_Tank4;
        private System.Windows.Forms.RadioButton rb_Tank3;
        private System.Windows.Forms.RadioButton rb_Tank2;
        private System.Windows.Forms.RadioButton rb_Tank1;
    }
}