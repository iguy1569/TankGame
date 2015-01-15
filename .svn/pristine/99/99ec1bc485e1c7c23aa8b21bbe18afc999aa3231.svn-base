// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    ConnectDialog
// Class File:  ConnectDialog.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach, and Dan Allen
//
// Discription: Modal dialog used to get server ip, player name,
// and player tank type selection
// ///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class ConnectDialog : Form
    {
        int iTank = 0;  // Holds tank id

        public delegate void delVoidString(string str);
        public delegate void delVoidInt(int i);

        public delVoidString dAddress;  // delagate to return ip address
        public delVoidString dName;     // Delegate to return player name
        public delVoidInt dTank;        // Delegate to return player's selected tank

        public ConnectDialog()
        {
            InitializeComponent();
            tb_Address.Focus(); // focus textbox for ip address
            gb_Tanks.BackgroundImage = Properties.Resources.Tank1;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            dAddress(tb_Address.Text);  // Store ip address
            dName(tb_Name.Text);        // Store player name
            dTank(iTank);               // Store selected tank id
            DialogResult = DialogResult.OK; // set dialogresult to OK
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel; // Set dialogresult to Cancel
        }

        private void rb_Tank1_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Tank1.Checked)   // set tank id to 0
            {
                iTank = 0;
                gb_Tanks.BackgroundImage = Properties.Resources.Tank1;
            }
        }

        private void rb_Tank2_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Tank2.Checked)   // set tank id to 1
            {
                iTank = 1;
                gb_Tanks.BackgroundImage = Properties.Resources.Tank2;
            }
        }

        private void rb_Tank3_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Tank3.Checked)   // set tank id to 2
            {
                iTank = 2;
                gb_Tanks.BackgroundImage = Properties.Resources.Tank3;
            }
        }

        private void rb_Tank4_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_Tank4.Checked)   // set tank id to 3
            {
                iTank = 3;
                gb_Tanks.BackgroundImage = Properties.Resources.Tank4;
            }
        }
    }
}
