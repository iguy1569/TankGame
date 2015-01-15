namespace Server
{
    partial class ServerForm
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
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.LB_Connected = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.LB_Log = new System.Windows.Forms.ListBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.TS_LBL_BytesR = new System.Windows.Forms.ToolStripLabel();
            this.TS_LB_FramesR = new System.Windows.Forms.ToolStripLabel();
            this.TS_LB_Fragment = new System.Windows.Forms.ToolStripLabel();
            this.TIM_Refresh = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(518, 512);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.LB_Connected);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(510, 486);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Connected";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // LB_Connected
            // 
            this.LB_Connected.FormattingEnabled = true;
            this.LB_Connected.Location = new System.Drawing.Point(7, 7);
            this.LB_Connected.Name = "LB_Connected";
            this.LB_Connected.Size = new System.Drawing.Size(496, 472);
            this.LB_Connected.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.LB_Log);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(510, 486);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // LB_Log
            // 
            this.LB_Log.FormattingEnabled = true;
            this.LB_Log.Location = new System.Drawing.Point(7, 7);
            this.LB_Log.Name = "LB_Log";
            this.LB_Log.Size = new System.Drawing.Size(496, 472);
            this.LB_Log.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TS_LBL_BytesR,
            this.TS_LB_FramesR,
            this.TS_LB_Fragment});
            this.toolStrip1.Location = new System.Drawing.Point(2, 527);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(264, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // TS_LBL_BytesR
            // 
            this.TS_LBL_BytesR.Name = "TS_LBL_BytesR";
            this.TS_LBL_BytesR.Size = new System.Drawing.Size(88, 22);
            this.TS_LBL_BytesR.Text = "Bytes Recieved:";
            // 
            // TS_LB_FramesR
            // 
            this.TS_LB_FramesR.Name = "TS_LB_FramesR";
            this.TS_LB_FramesR.Size = new System.Drawing.Size(98, 22);
            this.TS_LB_FramesR.Text = "Frames Recieved:";
            // 
            // TS_LB_Fragment
            // 
            this.TS_LB_Fragment.Name = "TS_LB_Fragment";
            this.TS_LB_Fragment.Size = new System.Drawing.Size(66, 22);
            this.TS_LB_Fragment.Text = "Fragments:";
            // 
            // TIM_Refresh
            // 
            this.TIM_Refresh.Enabled = true;
            this.TIM_Refresh.Interval = 25;
            this.TIM_Refresh.Tick += new System.EventHandler(this.TIM_Refresh_Tick);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 552);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tabControl1);
            this.Name = "ServerForm";
            this.Text = "Form1";
            this.Shown += new System.EventHandler(this.ServerForm_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox LB_Log;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel TS_LBL_BytesR;
        private System.Windows.Forms.ToolStripLabel TS_LB_FramesR;
        private System.Windows.Forms.ListBox LB_Connected;
        private System.Windows.Forms.Timer TIM_Refresh;
        private System.Windows.Forms.ToolStripLabel TS_LB_Fragment;

    }
}

