// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    Clientform
// Class File:  ClientForm.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach, and Dan Allen
//
// Discription: Performs all client rendering and calculations
// 
// ///////////////////////////////////////////////////////////////////////////

using Ammo;
using ClientFrame;
using ServerFrame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Tanks;
using Obstacles;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class ClientForm : Form
    {
        Socket _Socket = null;  // Connection Socket
        ConnectDialog _ConnectDialog;    // Connection Dialog. Gets Server Address, Player Name, and Selected Tank
        string _Address;    // Server Address
        string _Name;       // This Player's Name
        int _Tank;          // This Player's Tank ID
        int _PlayerID;      // assigned index from server

        int m_iBytesReceived = 0,        // Total bytes received. Required to update the UI.
            m_iFramesReceived = 0,       // Total frames received. Required to update the UI.
            m_iFragments = 0;            // Total fragments

        cServerFrame _sLastKnownInfo;   // Holds the last packet sent from the server
        cShotCounts _ShotCounts = null; // Holds the shot timers for the various ammo types
        Bitmap _MapTerrain = null;      // Bitmap of the terrain

        enum GameState { Start = 1, Playing = 2, NoConnect = 3, Death = 4 };    // Game states
        GameState gsCurrent = GameState.Start;  // Holds current game state

        sTank _sPrimaryTank = new sTank();               // main tank
        cDeathFrame _sCurrentStandings;                  // main standings

        List<KeyValuePair<int, sObstacle>> _loObstacles = new List<KeyValuePair<int, sObstacle>>(); // Holds the obstacles in game

        public delegate void delVoidVoid();
        public delVoidVoid m_tool;  // Toolstrip delegate


        // State of tank movement
        bool _Up = false,
             _Down = false,
             _Left = false,
             _Right = false;

        // State of tank turret movement
        bool _TurretLeft = false,
             _TurretRight = false;

        // State of tank firing
        bool _FireHeavy = false,
             _FireLight = false,
             _FireSpecial = false;

        // State of ammo toggles
        bool _TogglePrime = false,
             _ToggleSecond = false,
             _ToggleSpecial = false;

        // State of help menu toggle
        bool _ToggleHelpMenu = false;

        public delegate void delVoidSF(cServerFrame SF);    // Delegate receiving ServerFrame from ReceiveThread

        public delVoidSF dRender = null;
        public delVoidVoid dHideButton = null;
        public delVoidVoid dFocusScreen = null;
        public delVoidVoid dRenderMap = null;
        public delVoidVoid dButtonVisible = null;

        public ClientForm()
        {
            InitializeComponent();

            dHideButton = HideButton;   // Links dHideButon to HideButton method
            dFocusScreen = ReFocusScreen;   // Links dFocusScreen to ReFocusScreen method
            dRenderMap = RenderMap; // Links dRenderMap to RenderMap method
            dButtonVisible = VisibleConnectButton;  // Links dButtonVisible to VisibleConnectButton method
            m_tool = FormLog;   // Links m_tool to FormLog method
        }

        private void FormLog()
        {
            //This is used to update the UI in proper k/m/g notation
            string sOut;
            if (m_iBytesReceived / 1073741824 != 0) // Display in GB if size exceeds 1GB
                sOut = string.Format("{0:F3}G", (double)m_iBytesReceived / (double)1073741824);
            else if (m_iBytesReceived / 1048576 != 0)   // Display in MB if size exceeds 1MB
                sOut = string.Format("{0:F3}M", (double)m_iBytesReceived / (double)1048576);
            else if (m_iBytesReceived / 1024 != 0)  // Display in KB if size exceeds 1KB
                sOut = string.Format("{0:F3}K", (double)m_iBytesReceived / (double)1024);
            else    // Display raw value if less than 1KB
                sOut = string.Format("{0}", m_iBytesReceived);

            Text = string.Format("Bytes Rec: {0:F2}", sOut);    // Display bytes received
            Text += string.Format("| Frames Rec: {0:F2}", m_iFramesReceived);   // Append frames received
            Text += string.Format("| Frags Rec: {0:F2}", m_iFragments); // Append fragment count
        }

        #region Other_Support_Functions
        /// <summary>
        /// Refocuses screen on game start, compensates for modal dialog
        /// </summary>
        private void ReFocusScreen()
        {
            Focus();
        }   //

        /// <summary>
        /// The render screen for no connection made, enum NoConnect
        /// </summary>
        /// <param name="gr"></param>
        private void DrawConnectionLimitScreen(Graphics gr)
        {
            gr.Clear(System.Drawing.Color.Black);   // clear form with black
            gr.DrawImage(Properties.Resources.Tank, 0, 0, 500, 500);    // Display background tank image
            gr.DrawString("Connection limit has \nbeen reached.\nPlease wait and \ntry again",  // Display "try again" text
                new Font(FontFamily.GenericSansSerif, 30, FontStyle.Bold), new SolidBrush(System.Drawing.Color.Red),
                new Point(5, 210));
        }   //

        private void DrawStartScreen(Graphics gr)
        {
            gr.Clear(System.Drawing.Color.Black);   // Clear form with black
            gr.DrawImage(Properties.Resources.Tank, 0, 0, 500, 500);    // Display tank image background
            gr.DrawString("Welcome to tank game",   // Display "welcome" text
                new Font(FontFamily.GenericSansSerif, 30, FontStyle.Bold), new SolidBrush(System.Drawing.Color.Green),
                new Point(5, 210));
        }   //

        /// <summary>
        /// Re adds the connect button to screen for attempt at game
        /// </summary>
        private void VisibleConnectButton()
        {
            btn_Connect.Visible = true; // Enable Connect Button visibility
        }   //
        #endregion  //  //

        //Connection stuff
        #region Connect
        private void HideButton()
        {
            btn_Connect.Visible = false;    // Disable Connect Button visibility
        }

        private void ReturnAddress(string str)
        {
            _Address = str; // Stores Server Address from ConnectDialog
        }

        private void ReturnName(string str)
        {
            _Name = str;    // Stores Player Name from ConnectDialog
        }

        private void ReturnTank(int i)
        {
            _Tank = i;  // Stores Player Tank ID from ConnectDialog
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            if (_Socket == null)    // If Socket is not null
            {
                _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  // Initialize new Socket for connection

                _Socket.NoDelay = true; // Necessary?

                try
                {
                    _ConnectDialog = new ConnectDialog();
                    _ConnectDialog.dAddress = new ConnectDialog.delVoidString(ReturnAddress);   // Get Server Address from ConnectDialog
                    _ConnectDialog.dName = new ConnectDialog.delVoidString(ReturnName);         // Get Player Name from ConnectDialog
                    _ConnectDialog.dTank = new ConnectDialog.delVoidInt(ReturnTank);            // Get Player Tank ID from ConnectDialog
                    if (_ConnectDialog.ShowDialog() == DialogResult.OK) // If ConnectDialog result is OK
                    {
                        _Socket.BeginConnect(_Address, 1666, EndConnect, _Socket);  // Begin the connection to the server
                    }
                    else    // If ConnectDialog result is not OK
                    {
                        _Socket = null; // Reset Socket to null
                        btn_Connect.Visible = true; // Enable Connect Button visibility
                    }
                }
                catch (SocketException)
                {
                    _Socket = null; // Reset Socket to null
                    btn_Connect.Visible = true; // Enable Connect Button visibility
                }
                catch (Exception exc)
                {
                    string s = exc.Message;
                    _Socket = null; // Reset Socket to null
                    btn_Connect.Visible = true; // Enable Connect Button visibility
                }
                finally
                {

                }
            }
        }

        private void EndConnect(IAsyncResult ar)
        {
            try
            {
                _Socket.EndConnect(ar); // End server connection phase

                Invoke(dHideButton);

                // On Connect, send Player Name and Tank ID to server
                #region InitialConnect
                if (_Socket != null)    // If Socket is not null
                {
                    try
                    {
                        cClientConnect cConnect = new cClientConnect(_Name, _Tank); // Create new ClientConnect frame with Player Name and Tank ID
                        BinaryFormatter tBF = new BinaryFormatter();    // Temporary Binary Formatter
                        MemoryStream tMS = new MemoryStream();  // Temporary Memory Stream
                        tBF.Serialize(tMS, cConnect);   // Serialize the ClientConnect frame
                        _Socket.Send(tMS.GetBuffer(), (int)tMS.Length, SocketFlags.None);   // Send the serialized ClientConnect frame to the server
                    }
                    catch (SerializationException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {

                    }
                }
                #endregion

                Thread ReceiveThread = new Thread(ReceivingConnection); // Create new ReceiveThread
                ReceiveThread.IsBackground = true;  // Set ReceiveThread to Background
                ReceiveThread.Start(_Socket);   // Start ReceiveThread
            }
            catch (SocketException)
            {
                _Socket = null; // Reset Socket to null
                BeginInvoke(dButtonVisible); // Enable Connect Button visibility
            }
            catch (Exception)
            {
                _Socket = null; // Reset Socket to null
                BeginInvoke(dButtonVisible); // Enable Connect Button visibility
            }
            finally
            {

            }
        }
        #endregion

        private void ReceivingConnection(object obj)
        {
            Socket Sok = (Socket)obj;   // local socket object
            Sok.NoDelay = true;
            byte[] buffer = new byte[50000];    // buffer for receives

            MemoryStream msreceive = new MemoryStream();    // memory stream or receives
            int iReceivesFired = 0; // holds frames received

            try
            {
                while (true)    // Receive loop
                {
                    int iBytesReceivedCheck = Sok.Receive(buffer, buffer.Length, SocketFlags.None); // Get bytes received count
                    iReceivesFired++;   // Increment receive count
                    if (iBytesReceivedCheck == 0)   // If 0 bytes (server closed)
                    {
                        Sok = null; // Set socket to null
                    }

                    //save the current read position
                    long lPos = msreceive.Position;
                    //seek to the end to append the data
                    msreceive.Seek(0, SeekOrigin.End);
                    //append the new data to the end of the stream
                    msreceive.Write(buffer, 0, iBytesReceivedCheck);
                    //restore the read position back to where it started
                    msreceive.Position = lPos;

                    m_iBytesReceived += iBytesReceivedCheck;    // Append global bytes received count
                    BinaryFormatter bf = new BinaryFormatter(); // binary formatter for receives

                    while (msreceive.Position < msreceive.Length) // until memory stream is empty
                    {
                        //save the stream position in case the deserialization fails, and it has to be reset
                        long lStartPos = msreceive.Position;
                        try
                        {
                            object oIncomingInstruction = bf.Deserialize(msreceive);

                            // if frame is servr frame update screen
                            if (oIncomingInstruction is cServerFrame)
                            {
                                _sLastKnownInfo = (cServerFrame)oIncomingInstruction;   // store most recent data received
                                _ShotCounts = _sLastKnownInfo.cShotInfo;    // store most recent shot count
                                BeginInvoke(dRenderMap);    // render the map
                            }

                            else if (oIncomingInstruction is cInitialFrame) // test for cInitialFrame
                            {
                                if (((cInitialFrame)oIncomingInstruction).bmTerrainMap != null) // If a map was sent
                                {
                                    _MapTerrain = ((cInitialFrame)oIncomingInstruction).bmTerrainMap;   // Stoe terrain image
                                    _PlayerID = ((cInitialFrame)oIncomingInstruction).iIndex;   // Store player id
                                    _loObstacles = ((cInitialFrame)oIncomingInstruction).loObstacles;   // Store all obstacles
                                    BeginInvoke(dRenderMap);    // render the map
                                    BeginInvoke(dFocusScreen);  // Focus the screen
                                }
                                gsCurrent = GameState.Playing;  // Set game state to "playing"
                            }
                            else if (oIncomingInstruction is NoConnection)  // test for NoConnection frame
                            {
                                gsCurrent = GameState.NoConnect;    // toggle no connect screen
                                BeginInvoke(dButtonVisible);        // Enable Connect Button visibility
                                BeginInvoke(dRenderMap);    // Render map
                                _Socket = null;
                                return;
                            }

                            else if (oIncomingInstruction is cDeathFrame)   // Test for cDeathFrame
                            {
                                _sCurrentStandings = (cDeathFrame)oIncomingInstruction; // Store current standings

                                gsCurrent = GameState.Death;    // change gamestate to "death"
                                BeginInvoke(dRenderMap);    // render the map
                            }

                            m_iFramesReceived += 1; // increment frames received count

                            //Update the overall stats
                            BeginInvoke(m_tool);
                        }

                        catch (SerializationException ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            //deserialize failed, so move the read position back assume 
                            //more data will show up that this item needs to be deserialized
                            msreceive.Position = lStartPos;
                            m_iFragments += 1;

                            //Update the overall stats
                            BeginInvoke(m_tool);

                            break;
                        }
                    }

                    //if all data has been read from the rx memorystream, reset it otherwise 
                    //it will continue to hold all data EVER received
                    if (msreceive.Position == msreceive.Length)
                    {
                        msreceive.Position = 0;
                        msreceive.SetLength(0);
                    }
                    Thread.Sleep(0);
                }
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Trace.WriteLine("Socket Exception : " + ex.Message);
            }

            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Generic Exception : " + ex.Message);
            }
            finally
            {
                Sok = null;
            }
        }

        private void DrawStandingsScreen(Graphics gr)
        {
            gr.Clear(System.Drawing.Color.Black);   // Clear form with black
            gr.DrawImage(Properties.Resources.Tank, 0, 0, 500, 500);    // Draw tank image background
            gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(200, System.Drawing.Color.Black)), new Rectangle(0, 0, 500, 500));

            gr.DrawString("You were killed by " + _sCurrentStandings.sKilledBy, // Display death message
                            new Font(FontFamily.GenericSansSerif, 20),
                            new SolidBrush(System.Drawing.Color.Red),
                            new PointF(5, 10));

            // Sets up box frame
            gr.FillRectangles(new SolidBrush(System.Drawing.Color.Gold), new Rectangle[] { new Rectangle(  5,  40, 470,   5),
                                                                                                new Rectangle(  5,  45,   5, 250),
                                                                                                new Rectangle(150,  45,   5, 250),
                                                                                                new Rectangle(270,  45,   5, 250),
                                                                                                new Rectangle(350,  45,   5, 250),
                                                                                                new Rectangle(470,  45,   5, 250),
                                                                                           new Rectangle(  5, 295, 470,   5)});

            // Display stat headers
            gr.DrawString("Name", new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(System.Drawing.Color.Green), new PointF(20, 45));
            gr.DrawString("Kills", new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(System.Drawing.Color.Green), new PointF(280, 45));
            gr.DrawString("Deaths", new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(System.Drawing.Color.Green), new PointF(160, 45));
            gr.DrawString("Streak", new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(System.Drawing.Color.Green), new PointF(360, 45));

            int iOffset = 75;   // Offset for stat lines
            
            foreach (Stats s in _sCurrentStandings.sCurrentStandings)   // Display all stats
            {
                gr.DrawString(s.sTankName, new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(System.Drawing.Color.Blue), new PointF(20, iOffset));
                gr.DrawString(s.iKills.ToString(), new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(System.Drawing.Color.Yellow), new PointF(160, iOffset));
                gr.DrawString(s.ideaths.ToString(), new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(System.Drawing.Color.Yellow), new PointF(280, iOffset));
                gr.DrawString(s.iKillStreak.ToString(), new Font(FontFamily.GenericSansSerif, 20), new SolidBrush(System.Drawing.Color.Yellow), new PointF(360, iOffset));
                iOffset += 30;
            }

            // Display instructions to respawn
            gr.DrawString("Press [space] to respawn", new Font(FontFamily.GenericSansSerif, 20),
                            new SolidBrush(System.Drawing.Color.White), new PointF(25, 310));
        }

        /// <summary>
        /// Renders primary tank life meter, housed just below enemy unit
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        /// <param name="iLife">life amount remaining</param>
        /// <param name="iLimit">life total possible</param>
        private void RenderEnemyLifeMeter(Graphics gr, PointF pPos, float iLife, float iLimit)
        {
            // Outer rectangle representing full health meter
            gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(150, 0, 0, 0)),
                                new Rectangle((int)pPos.X - 20, (int)pPos.Y + 20, 40, 8));

            // Inner rectangle representing current helath
            gr.FillRectangle(new SolidBrush(iLife > iLimit / 4 ? System.Drawing.Color.FromArgb(150, 0, 255, 0) : System.Drawing.Color.FromArgb(150, 255, 0, 0)),
                                new Rectangle((int)pPos.X - 18, (int)pPos.Y + 22, (int)((double)(iLife / iLimit) * 36), 4));
        }

        private void RenderHeavyTimer(Graphics gr, PointF pPos, int iHeavy1Cur, int iHeavy2Cur, int iHeavy1Tim, int iHeavy2Tim)
        {
            // Heavy ammo timer rectangle
            gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(150, 0, 0, 0)),
                new Rectangle((int)pPos.X - 20, (int)pPos.Y, 40, 8));

            // Special ammo timer rectangle
            gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(150, 0, 0, 0)),
                new Rectangle((int)pPos.X + 25, (int)pPos.Y, 40, 8));

            // Heavy ammo current time rectangle
            gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(150, System.Drawing.Color.Aqua)),
                new Rectangle((int)pPos.X - 18, (int)pPos.Y + 2, (int)(((double)iHeavy1Cur / (iHeavy1Tim + 1.0)) * 36), 4));

            // Special ammo current time
            gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(150, System.Drawing.Color.Aqua)),
                new Rectangle((int)pPos.X + 27, (int)pPos.Y + 2, (int)(((double)iHeavy2Cur / (iHeavy2Tim + 1.0)) * 36), 4));
        }

        /// <summary>
        /// Renders primary tank life meter, housed up in the upper left corner
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        /// <param name="iLife">life amount remaining</param>
        /// <param name="iLimit">life total possible</param>
        private void RenderPrimaryLifeMeter(Graphics gr, float iLife, float iLimit)
        {
            // Rectangle for this tank's health meter
            gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(150, 0, 0, 0)),
                    new Rectangle(5, 5, 80, 8));

            // Rectangle for this tank's current life
            gr.FillRectangle(new SolidBrush(iLife > iLimit / 4 ? System.Drawing.Color.FromArgb(150, 0, 255, 0) : System.Drawing.Color.FromArgb(150, 255, 0, 0)),
                                new Rectangle(7, 7, (int)((double)(iLife / iLimit) * 76), 4));
        }

        private void RenderHelpMenu(Graphics gr)
        {
            // Background for help meter
            gr.FillRectangle(new SolidBrush(Color.FromArgb(200, Color.Black)), new Rectangle(new Point(DisplayRectangle.Width / 2 - 175, DisplayRectangle.Height / 2 - 175),new Size(280, 270)));
            
            // Display all help
            gr.DrawString("**HELP MENU - CONTROLS**\n\n 1 - Toggle Primary Weapon Amo\n 2 - Toggle Secondary Weapon Amo\n 3 - Toggle Special Items \n w - Move Forwards \n s - Move Backwards\n a - Move Left\n d - Move Right\n Space - Fire\n Up Arrow - Fire Secondary\n Down Arrow - Set/drop Special Item\n Left Arrow - Rotate Turret Left\n Right Arrow - Rotate Turret Right",
                new Font(FontFamily.GenericSansSerif, 12), new SolidBrush(Color.White), 
                new PointF(DisplayRectangle.Width / 2 -175, DisplayRectangle.Height / 2 -175 ));
        }

        /// <summary>
        /// Converts all enemy positions to a radar map which covers 1/3 of the map
        /// Primary unit respresented as green enemies as red
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        /// <param name="sPrimary">primary tank</param>
        /// <param name="sEnemies">enemy tank list</param>
        private void RenderPrimaryRadar(Graphics gr, sTank sPrimary, List<sTank> sEnemies)
        {
            // draw inital radar map, at a reduced opacity
            gr.FillEllipse(new SolidBrush(System.Drawing.Color.FromArgb(150, System.Drawing.Color.White)), new Rectangle(400, 380, 80, 80));
            gr.DrawEllipse(new Pen(System.Drawing.Color.FromArgb(150, System.Drawing.Color.Black)), new Rectangle(400, 380, 80, 80));
            gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(150, System.Drawing.Color.Green)), new Rectangle(440, 420, 4, 4));

            // foreach enemy determine if in bounds of radar map
            foreach (sTank sEnemyTank in sEnemies)
            {
                const int ciConvertion = 835;   // 1/3 map pixels
                // determine distance for drawing enemy blips
                if (GetDistance(sPrimary.pLocation, sEnemyTank.pLocation) < ciConvertion)
                {
                    // set initial values to offset
                    float fXOffset = sPrimary.pLocation.X - sEnemyTank.pLocation.X,
                          fYOffset = sPrimary.pLocation.Y - sEnemyTank.pLocation.Y,
                          fConvertion = (40.0f / ciConvertion);     // radius / scaled area

                    // apply convertions
                    fXOffset *= fConvertion;
                    fYOffset *= fConvertion;

                    // draw enemy blip
                    gr.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(100, System.Drawing.Color.Red)),
                                        new Rectangle(440 - (int)fXOffset, 420 - (int)fYOffset, 4, 4));
                }
            }
        }

        /// <summary>
        /// Distance formula for determining the distance between 2 points  
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns>distance between points</returns>
        private double GetDistance(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));  // Returns distance between given points
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (gsCurrent != GameState.Death)   // If game state is not "Death"
            {
                if (e.KeyCode == Keys.W && !_Up)
                {
                    _Up = true; // Set forward state to true
                    SendData();
                }
                if (e.KeyCode == Keys.S && !_Down)
                {
                    _Down = true;   // Set backward state to true
                    SendData();
                }
                if (e.KeyCode == Keys.A && !_Left)
                {
                    _Left = true;   // Set turn left state to true
                    SendData();
                }
                if (e.KeyCode == Keys.D && !_Right)
                {
                    _Right = true;  // Set turn right state to true
                    SendData();
                }

                if (e.KeyCode == Keys.Left && !_TurretLeft)
                {
                    _TurretLeft = true; // Set turret left rotation to true
                    SendData();
                }
                if (e.KeyCode == Keys.Right && !_TurretRight)
                {
                    _TurretRight = true;    // Set turret right rotation to true
                    SendData();
                }

                if (e.KeyCode == Keys.Up && !_FireLight)
                {
                    _FireLight = true;    // Set turret right rotation to true
                    SendData();
                }

                if (e.KeyCode == Keys.Down && !_FireSpecial)
                {
                    _FireSpecial = true;    // Set turret right rotation to true
                    SendData();
                }

                if (e.KeyCode == Keys.Space && !_FireHeavy)
                {
                    _FireHeavy = true;    // Set turret right rotation to true
                    SendData();
                }

                if (e.KeyCode == Keys.D1 && !_TogglePrime)  // Send toggle data for heavy ammo only
                {
                    _TogglePrime = true;
                    _ToggleSecond = false;
                    _ToggleSpecial = false;
                    SendAmmoData();
                }

                if (e.KeyCode == Keys.D2 && !_ToggleSecond) // send toggle data for fast ammo only
                {
                    _ToggleSecond = true;
                    _TogglePrime = false;
                    _ToggleSpecial = false;
                    SendAmmoData();
                }

                if (e.KeyCode == Keys.D3 && !_ToggleSpecial)    // Send toggle data for special ammo only
                {
                    _ToggleSecond = false;
                    _TogglePrime = false;
                    _ToggleSpecial = true;
                    SendAmmoData();
                }
                if (e.KeyCode == Keys.H)    // Set _ToggleHelpMenu to display help menu
                    _ToggleHelpMenu = true;
            }
            else
            {
                if (e.KeyCode == Keys.Space)    // Send respawn data to server to request spawn location
                {
                    SendRespawnData(_Socket, _sPrimaryTank.iTankId);
                    gsCurrent = GameState.Playing;
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && _Up)
            {
                _Up = false;    // Set forward state to false
                SendData();
            }
            if (e.KeyCode == Keys.S && _Down)
            {
                _Down = false;  // Set backward state to false
                SendData();
            }
            if (e.KeyCode == Keys.A && _Left)
            {
                _Left = false;  // Set turn left state to false
                SendData();
            }
            if (e.KeyCode == Keys.D && _Right)
            {
                _Right = false; // Set turn right state to false
                SendData();
            }
            if (e.KeyCode == Keys.Left && _TurretLeft)
            {
                _TurretLeft = false;    // Set turret left rotation to false
                SendData();
            }
            if (e.KeyCode == Keys.Right && _TurretRight)
            {
                _TurretRight = false;   // Set turret right rotation to false
                SendData();
            }

            if (e.KeyCode == Keys.Up && _FireLight)
            {
                _FireLight = false;    // Set turret right rotation to true
                SendData();
            }

            if (e.KeyCode == Keys.Down && _FireSpecial)
            {
                _FireSpecial = false;    // Set turret right rotation to true
                SendData();
            }

            if (e.KeyCode == Keys.Space && _FireHeavy)
            {
                _FireHeavy = false;    // Set turret right rotation to true
                SendData();
            }

            if (e.KeyCode == Keys.D1)   // Disable toggle primary
                _TogglePrime = false;

            if (e.KeyCode == Keys.D2)   // disable togle fast
                _ToggleSecond = false;

            if (e.KeyCode == Keys.D3)   // Disable toggle special
                _ToggleSpecial = false;

            if (e.KeyCode == Keys.H)    // Disable toggle help
                _ToggleHelpMenu = false;
        }

        public void SendData()
        {
            if (_Socket != null && _Socket.Connected)
            {
                try
                {
                    cClientFrame cCF = new cClientFrame(_Up, _Down, _Left, _Right, _TurretLeft, _TurretRight, _FireHeavy,
                                                            _FireLight, _FireSpecial, _TogglePrime, _ToggleSecond, _ToggleSpecial);  // Create new ClientFrame

                    BinaryFormatter BF = new BinaryFormatter(); // Binary Formatter for sending
                    MemoryStream MS = new MemoryStream();   // Memory Stream for sending
                    BF.Serialize(MS, cCF);  // Serialize the ClientFrame
                    _Socket.Send(MS.GetBuffer(), (int)MS.Length, SocketFlags.None); // Send the serialized ClientFrame
                }
                catch (FormatException)
                {

                }
                catch (SocketException)
                {

                }
                finally
                {

                }
            }
        }

        public void SendAmmoData()
        {
            if (_Socket != null && _Socket.Connected)
            {
                try
                {
                    cClientToggle cCT = new cClientToggle();    // Package toggle data
                    cCT.bPrimary = _TogglePrime;
                    cCT.bSecondary = _ToggleSecond;
                    cCT.bSpecial = _ToggleSpecial;

                    BinaryFormatter BF = new BinaryFormatter(); // Binary Formatter for sending
                    MemoryStream MS = new MemoryStream();   // Memory Stream for sending
                    BF.Serialize(MS, cCT);  // Serialize the ClientFrame
                    _Socket.Send(MS.GetBuffer(), (int)MS.Length, SocketFlags.None); // Send the serialized ClientFrame
                }
                catch (FormatException)
                {

                }
                catch (SocketException)
                {

                }
                finally
                {

                }
            }
        }

        private void SendRespawnData(Socket sSend, int iTank)
        {
            MemoryStream mssend = new MemoryStream();   // stream for respawn data
            BinaryFormatter bf = new BinaryFormatter(); // binary formatter for respawn data
            
            cRespawn crTemp = new cRespawn();   // Package respawn data
            crTemp.iTankType = iTank;

            bf.Serialize(mssend, crTemp);   // serialize respawn data

            sSend.Send(mssend.GetBuffer(), (int)mssend.Length, SocketFlags.None);   // Send respawn data
        }

        private void RenderMap()
        {
            using (Graphics gr = CreateGraphics())  // graphics object
            {
                BufferedGraphicsContext bgc = new BufferedGraphicsContext();    // buffered graphics
                using (BufferedGraphics bg = bgc.Allocate(gr, ClientRectangle)) // allocate buffered graphics
                {
                    bg.Graphics.Clear(System.Drawing.Color.Black);  // Clear form with black
                    
                    List<sTank> sEnemies = new List<sTank>();   // enemy tanks

                    #region Determine_Primary_Tank
                    if (_sLastKnownInfo != null)
                    {
                        List<KeyValuePair<int, sTank>> kvpTemp;
                        lock (_sLastKnownInfo)
                            kvpTemp = _sLastKnownInfo.TanksInPlay;  // Store all tanks in temporary list

                        foreach (KeyValuePair<int, sTank> kvpTanks in kvpTemp)
                            if (kvpTanks.Key.Equals(_PlayerID)) // if tank is player's tank
                                _sPrimaryTank = kvpTanks.Value; // Store tank data to _sPrimaryTank
                            else    // If tank is enemy
                                sEnemies.Add(kvpTanks.Value);   // Add to enemies list
                    }
                    #endregion

                    #region Render_GameMap
                    if (gsCurrent == GameState.Playing)
                    {
                        // add map terrain background to show tank traveling over ground, offset from sPrimary
                        if (_MapTerrain != null)
                            bg.Graphics.DrawImage(_MapTerrain,
                                new PointF(250 - _sPrimaryTank.pLocation.X, 250 - _sPrimaryTank.pLocation.Y));


                        int iLifeLimitPrimary = 0;  // holder for primary tank life max, used later for render

                        // if a server frame has been recieved, update all screen locations
                        if (_sLastKnownInfo != null)
                        {
                            #region Render_All_Ammo
                            foreach (KeyValuePair<byte, sAmmo> kvpTemp in _sLastKnownInfo.AmmoPoints)
                            {
                                PointF pAmmoOffset = new PointF(250 - (_sPrimaryTank.pLocation.X - kvpTemp.Value.pLocation.X),  // Store ammo location
                                                                250 - (_sPrimaryTank.pLocation.Y - kvpTemp.Value.pLocation.Y));
                                cAmmo cA = WhatAmmo.AmmoConvertByteToType(kvpTemp.Key, pAmmoOffset, kvpTemp.Value.fRotation);   // store ammo type

                                if (cA is cFastAmmo)
                                    ((cFastAmmo)cA).Render(bg.Graphics);    // If ammo is fast, render as fast

                                if (cA is cSlowAmmo)
                                    ((cSlowAmmo)cA).Render(bg.Graphics);    // If ammo is slow, render as slow
                            }

                            foreach (KeyValuePair<byte, sSpecial> kvpTemp in _sLastKnownInfo.SpecialItems)
                            {
                                PointF pSpecialOffset = new PointF(250 - (_sPrimaryTank.pLocation.X - kvpTemp.Value.pLocation.X),   // Store special ammo location
                                250 - (_sPrimaryTank.pLocation.Y - kvpTemp.Value.pLocation.Y));
                                cSpecialItem cA = WhatAmmo.ConvertIntToType(kvpTemp.Key, kvpTemp.Value.fRotation,   // Store special ammo type
                                                pSpecialOffset, kvpTemp.Value.bArmed);

                                cA.Render(bg.Graphics); // Render special ammo
                            }
                            #endregion

                            #region Render_All_Tanks
                            cTank cT;

                            foreach (sTank kvpTanks in sEnemies)
                            {
                                // Math required to find enemy offset from primary tank
                                cT = WhatTank.TankConvertByteToType(kvpTanks.iTankId,
                                                                    new PointF(250 - (_sPrimaryTank.pLocation.X - kvpTanks.pLocation.X),
                                                                               250 - (_sPrimaryTank.pLocation.Y - kvpTanks.pLocation.Y)),
                                                                    kvpTanks.fTankRotation,
                                                                    kvpTanks.fTurretRotation,
                                                                    kvpTanks.iLife,
                                                                    kvpTanks.bHit);
                                cT.Render(bg.Graphics); // render enemy tank

                                // render all enemy life meters
                                RenderEnemyLifeMeter(bg.Graphics, cT.CurrentTankPosition, cT.Life, cT.LifeLimit);
                            }

                            cT = WhatTank.TankConvertByteToType(_sPrimaryTank.iTankId,  // convert primary tank to tank type
                                                                       new PointF(250, 250),
                                                                       _sPrimaryTank.fTankRotation,
                                                                       _sPrimaryTank.fTurretRotation,
                                                                       _sPrimaryTank.iLife,
                                                                       _sPrimaryTank.bHit);
                            cT.Render(bg.Graphics); // render primary tank

                            iLifeLimitPrimary = cT.LifeLimit;       // save primary tank life limit meter

                            #endregion

                            #region Render_All_Obstcales

                            foreach (KeyValuePair<int, sObstacle> kvpTemp in _loObstacles)
                            {
                                PointF pPlace = new PointF(250 - (_sPrimaryTank.pLocation.X - kvpTemp.Value.pLocation.X),   // Store obstacle location
                                                            250 - (_sPrimaryTank.pLocation.Y - kvpTemp.Value.pLocation.Y));

                                cObstacle oTemp = WhatObstacle.ObstacleConvertByteToType(kvpTemp.Key,   // convert to obstacle type
                                                                                         pPlace,
                                                                                         kvpTemp.Value.fRotation);
                                oTemp.Render(bg.Graphics);  // Render obstacle
                            }
                            #endregion

                            // render primary tank life meter
                            RenderPrimaryLifeMeter(bg.Graphics, _sPrimaryTank.iLife, iLifeLimitPrimary);
                            RenderPrimaryRadar(bg.Graphics, _sPrimaryTank, sEnemies);
                            if (_ShotCounts != null)    // Render shot timers if not null
                                RenderHeavyTimer(bg.Graphics, new PointF(120, 5), _ShotCounts.iSlowAmmoCurrent,
                                    _ShotCounts.iSpecialAmmoCurrent, _ShotCounts.iSlowAmmoTimeout,
                                    _ShotCounts.iSpecialAmmoTimeout);

                            if (_ToggleHelpMenu)    // rende help menu if toggled
                                RenderHelpMenu(bg.Graphics);
                            else    // Render text instruction to toggle help menu if not toggled
                            {
                                bg.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Black)), 5, 435, 140, 30); 
                                bg.Graphics.DrawString("Press 'H' for help", new Font(FontFamily.GenericSansSerif, 12),
                                    new SolidBrush(Color.Orange), 10, 440);
                            }
                        }
                    #endregion
                    }
                            
                    else if (gsCurrent == GameState.NoConnect)  // If gamestate is "Noconnect"
                        DrawConnectionLimitScreen(bg.Graphics); // display no connection screen

                    else if (gsCurrent == GameState.Death)  // If gamestate is "death"
                    {
                        DrawStandingsScreen(bg.Graphics);   // Display stats screen
                    }

                    bg.Render();    // Render all graphics
                }
            }
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_Socket != null && _Socket.Connected)
            {
                //Nicely shutdown the socket
                _Socket.Shutdown(SocketShutdown.Both);
                _Socket.Close();
            }
        }
    }
}
