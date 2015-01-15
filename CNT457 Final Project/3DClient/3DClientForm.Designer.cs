namespace _3DClient
{
    partial class _3DClientForm
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
            this.p_Init = new System.Windows.Forms.Panel();
            this.btn_Connect = new System.Windows.Forms.Button();
            this.p_Connect = new System.Windows.Forms.Panel();
            this.gb_Tanks = new System.Windows.Forms.GroupBox();
            this.rb_Tank4 = new System.Windows.Forms.RadioButton();
            this.rb_Tank3 = new System.Windows.Forms.RadioButton();
            this.rb_Tank2 = new System.Windows.Forms.RadioButton();
            this.rb_Tank1 = new System.Windows.Forms.RadioButton();
            this.lbl_Name = new System.Windows.Forms.Label();
            this.lbl_Address = new System.Windows.Forms.Label();
            this.tb_Name = new System.Windows.Forms.TextBox();
            this.tb_Addr = new System.Windows.Forms.TextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.p_Init.SuspendLayout();
            this.p_Connect.SuspendLayout();
            this.gb_Tanks.SuspendLayout();
            this.SuspendLayout();
            // 
            // p_Init
            // 
            this.p_Init.Controls.Add(this.btn_Connect);
            this.p_Init.Enabled = false;
            this.p_Init.Location = new System.Drawing.Point(13, 13);
            this.p_Init.Name = "p_Init";
            this.p_Init.Size = new System.Drawing.Size(614, 45);
            this.p_Init.TabIndex = 0;
            this.p_Init.Visible = false;
            // 
            // btn_Connect
            // 
            this.btn_Connect.Location = new System.Drawing.Point(4, 4);
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(75, 23);
            this.btn_Connect.TabIndex = 0;
            this.btn_Connect.Text = "Connect";
            this.btn_Connect.UseVisualStyleBackColor = true;
            this.btn_Connect.Click += new System.EventHandler(this.btn_Connect_Click);
            // 
            // p_Connect
            // 
            this.p_Connect.Controls.Add(this.gb_Tanks);
            this.p_Connect.Controls.Add(this.lbl_Name);
            this.p_Connect.Controls.Add(this.lbl_Address);
            this.p_Connect.Controls.Add(this.tb_Name);
            this.p_Connect.Controls.Add(this.tb_Addr);
            this.p_Connect.Controls.Add(this.btn_OK);
            this.p_Connect.Controls.Add(this.btn_Cancel);
            this.p_Connect.Enabled = false;
            this.p_Connect.Location = new System.Drawing.Point(13, 65);
            this.p_Connect.Name = "p_Connect";
            this.p_Connect.Size = new System.Drawing.Size(660, 345);
            this.p_Connect.TabIndex = 1;
            this.p_Connect.Visible = false;
            // 
            // gb_Tanks
            // 
            this.gb_Tanks.Controls.Add(this.rb_Tank4);
            this.gb_Tanks.Controls.Add(this.rb_Tank3);
            this.gb_Tanks.Controls.Add(this.rb_Tank2);
            this.gb_Tanks.Controls.Add(this.rb_Tank1);
            this.gb_Tanks.Location = new System.Drawing.Point(49, 78);
            this.gb_Tanks.Name = "gb_Tanks";
            this.gb_Tanks.Size = new System.Drawing.Size(563, 188);
            this.gb_Tanks.TabIndex = 8;
            this.gb_Tanks.TabStop = false;
            this.gb_Tanks.Text = "Tank Selection";
            // 
            // rb_Tank4
            // 
            this.rb_Tank4.AutoSize = true;
            this.rb_Tank4.Location = new System.Drawing.Point(472, 19);
            this.rb_Tank4.Name = "rb_Tank4";
            this.rb_Tank4.Size = new System.Drawing.Size(59, 17);
            this.rb_Tank4.TabIndex = 3;
            this.rb_Tank4.Text = "Tank 4";
            this.rb_Tank4.UseVisualStyleBackColor = true;
            // 
            // rb_Tank3
            // 
            this.rb_Tank3.AutoSize = true;
            this.rb_Tank3.Location = new System.Drawing.Point(381, 19);
            this.rb_Tank3.Name = "rb_Tank3";
            this.rb_Tank3.Size = new System.Drawing.Size(59, 17);
            this.rb_Tank3.TabIndex = 2;
            this.rb_Tank3.Text = "Tank 3";
            this.rb_Tank3.UseVisualStyleBackColor = true;
            // 
            // rb_Tank2
            // 
            this.rb_Tank2.AutoSize = true;
            this.rb_Tank2.Location = new System.Drawing.Point(290, 19);
            this.rb_Tank2.Name = "rb_Tank2";
            this.rb_Tank2.Size = new System.Drawing.Size(59, 17);
            this.rb_Tank2.TabIndex = 1;
            this.rb_Tank2.Text = "Tank 2";
            this.rb_Tank2.UseVisualStyleBackColor = true;
            // 
            // rb_Tank1
            // 
            this.rb_Tank1.AutoSize = true;
            this.rb_Tank1.Checked = true;
            this.rb_Tank1.Location = new System.Drawing.Point(199, 19);
            this.rb_Tank1.Name = "rb_Tank1";
            this.rb_Tank1.Size = new System.Drawing.Size(59, 17);
            this.rb_Tank1.TabIndex = 0;
            this.rb_Tank1.TabStop = true;
            this.rb_Tank1.Text = "Tank 1";
            this.rb_Tank1.UseVisualStyleBackColor = true;
            // 
            // lbl_Name
            // 
            this.lbl_Name.AutoSize = true;
            this.lbl_Name.Location = new System.Drawing.Point(44, 32);
            this.lbl_Name.Name = "lbl_Name";
            this.lbl_Name.Size = new System.Drawing.Size(35, 13);
            this.lbl_Name.TabIndex = 7;
            this.lbl_Name.Text = "Name";
            // 
            // lbl_Address
            // 
            this.lbl_Address.AutoSize = true;
            this.lbl_Address.Location = new System.Drawing.Point(34, 6);
            this.lbl_Address.Name = "lbl_Address";
            this.lbl_Address.Size = new System.Drawing.Size(45, 13);
            this.lbl_Address.TabIndex = 6;
            this.lbl_Address.Text = "Address";
            // 
            // tb_Name
            // 
            this.tb_Name.Location = new System.Drawing.Point(85, 30);
            this.tb_Name.Name = "tb_Name";
            this.tb_Name.Size = new System.Drawing.Size(100, 20);
            this.tb_Name.TabIndex = 3;
            this.tb_Name.Text = "Default";
            // 
            // tb_Addr
            // 
            this.tb_Addr.Location = new System.Drawing.Point(85, 3);
            this.tb_Addr.Name = "tb_Addr";
            this.tb_Addr.Size = new System.Drawing.Size(100, 20);
            this.tb_Addr.TabIndex = 2;
            this.tb_Addr.Text = "localhost";
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(582, 319);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 1;
            this.btn_OK.Text = "OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(500, 319);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 0;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // _3DClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 479);
            this.Controls.Add(this.p_Connect);
            this.Controls.Add(this.p_Init);
            this.Name = "_3DClientForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this._3DClientForm_FormClosing);
            this.Shown += new System.EventHandler(this._3DClientForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this._3DClientForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this._3DClientForm_KeyUp);
            this.p_Init.ResumeLayout(false);
            this.p_Connect.ResumeLayout(false);
            this.p_Connect.PerformLayout();
            this.gb_Tanks.ResumeLayout(false);
            this.gb_Tanks.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel p_Init;
        private System.Windows.Forms.Button btn_Connect;
        private System.Windows.Forms.Panel p_Connect;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.TextBox tb_Addr;
        private System.Windows.Forms.TextBox tb_Name;
        private System.Windows.Forms.Label lbl_Name;
        private System.Windows.Forms.Label lbl_Address;
        private System.Windows.Forms.GroupBox gb_Tanks;
        private System.Windows.Forms.RadioButton rb_Tank4;
        private System.Windows.Forms.RadioButton rb_Tank3;
        private System.Windows.Forms.RadioButton rb_Tank2;
        private System.Windows.Forms.RadioButton rb_Tank1;
    }
}

