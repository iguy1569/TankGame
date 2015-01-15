// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    3DClient
// Class File:  3DClientForm.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach and Dan Allen
//
// Discription: Responsible for rending all 3d animations, 
//              using irrlicht engine
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

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Video;
using IrrlichtLime.Scene;

namespace _3DClient
{
    public partial class _3DClientForm : Form
    {
        #region Origional values
        #region Tank_Directions
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

        bool _TogglePrime = false,
             _ToggleSecond = false,
             _ToggleSpecial = false;
        #endregion

        cServerFrame _sLastKnownInfo;       // Holds last frame sent from server
        Bitmap _MapTerrain = null;          // Terrain image (may be replaced)

        sTank _sPrimaryTank = new sTank();  // Main tank
        cDeathFrame _sCurrentStandings;     // Main standings
        int _PlayerID;                      // Assigned index from server
        // Holds obstacles (may be replaced)
        List<KeyValuePair<int, sObstacle>> _loObstacles = new List<KeyValuePair<int, sObstacle>>();
        List<sTank> sEnemies = new List<sTank>();   // enemy tanks

        Socket _Socket = null;  // Scoket used to connect
        string _Address;        // Holds server address
        string _Name;           // Holds player name
        int _Tank;              // Holds index of player's selected tank

        // Possible game states. Used for rendering
        enum GameState { Start = 1, Playing = 2, NoConnect = 3, Death = 4, Connect = 5 };

        //bool userWantExit = false;  // Used to stop the render thread
        Thread _Render = null;      // Thread used to render the game

        private GameState _Scene = GameState.Start; // start with a splash screen
        #endregion

        private bool _bSceneChange = false;      // Indicates to render loop that a scene change request is desired

        private bool userWantExit = false; // if "true", we shut down rendering thread and then exit app
        float _XCamOffset = -150; // offset of camera X
        float _YCamOffset = 50; // offset of camera Y ( up a bit )
        float _ZCamOffset = 0; // offset of camera Z

        float RotationOffset;

        public struct ModelInfo
        {
            public int iTankID;
            public int iYOffset;
            public Vector3Df v3dRotationOffset;
            public Vector3Df v3dScale;
            public string sTexture;
        }

        // scene setup animations and mesh information
        #region Scene_Items
        List<AnimatedMeshSceneNode> _Turrets = new List<AnimatedMeshSceneNode>();

        KeyValuePair<ModelInfo, AnimatedMeshSceneNode> _me;

        List<KeyValuePair<ModelInfo, IrrlichtLime.Scene.AnimatedMeshSceneNode>> _Enemies
            = new List<KeyValuePair<ModelInfo, AnimatedMeshSceneNode>>();

        List<KeyValuePair<ModelInfo, AnimatedMeshSceneNode>> _Ammo
            = new List<KeyValuePair<ModelInfo, AnimatedMeshSceneNode>>();

        List<KeyValuePair<ModelInfo, AnimatedMeshSceneNode>> _OilSlick
            = new List<KeyValuePair<ModelInfo, AnimatedMeshSceneNode>>();

        List<KeyValuePair<ModelInfo, AnimatedMeshSceneNode>> _LandMine
            = new List<KeyValuePair<ModelInfo, AnimatedMeshSceneNode>>();

        List<IrrlichtLime.Scene.AnimatedMeshSceneNode> _obstacles = new List<AnimatedMeshSceneNode>();
        #endregion

        public _3DClientForm()
        {
            InitializeComponent();

            // Initialize the render thread
            #region RenderThreadInit
            DeviceSettings s = new DeviceSettings(
                this.Handle, // the main form's handle, see demos for using an external : IntPtr.Zero ( new window is made )
                DriverType.Direct3D9, // what driver ? there are options for DX8 or OpenGL too
                0, // anti-alias value 0, 2, 4, 8
                IrrlichtLime.Video.Color.OpaqueCyan,
                false // VSync used ? 
            );
            userWantExit = false;
            _Render = new Thread(Renderer);
            _Render.IsBackground = true;
            _Render.Start(s);
            #endregion            
        }


        /// <summary>
        /// Handles enagaging tank controls 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _3DClientForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_Scene != GameState.Death)
            {
                // tank movement controls
                if (e.KeyCode == Keys.W && !_Up) { _Up = true; SendData(); }
                if (e.KeyCode == Keys.S && !_Down) { _Down = true; SendData(); }
                if (e.KeyCode == Keys.A && !_Left) { _Left = true; SendData(); }
                if (e.KeyCode == Keys.D && !_Right) { _Right = true; SendData(); }

                // tank turret controls
                if (e.KeyCode == Keys.Left && !_TurretLeft) { _TurretLeft = true; SendData(); }
                if (e.KeyCode == Keys.Right && !_TurretRight) { _TurretRight = true; SendData(); }

                // Tank firing controls
                if (e.KeyCode == Keys.Up && !_FireLight) { _FireLight = true; SendData(); }
                if (e.KeyCode == Keys.Down && !_FireSpecial) { _FireSpecial = true; SendData(); }
                if (e.KeyCode == Keys.Space && !_FireHeavy) { _FireHeavy = true; SendData(); }

                // tank camera controls
                if (e.KeyCode == Keys.U)
                    if (_YCamOffset <= 400) { _YCamOffset += 20; _XCamOffset -= 6; }
                if (e.KeyCode == Keys.I)
                    if (_YCamOffset >= 50) { _YCamOffset -= 20; _XCamOffset += 6; }

                // change primary weapon toggle
                if (e.KeyCode == Keys.D1 && !_TogglePrime)
                {
                    _TogglePrime = true;
                    _ToggleSecond = false;
                    _ToggleSpecial = false;
                    SendAmmoData();
                }

                // change secondary weapon toggle
                if (e.KeyCode == Keys.D2 && !_ToggleSecond)
                {
                    _ToggleSecond = true;
                    _TogglePrime = false;
                    _ToggleSpecial = false;
                    SendAmmoData();
                }

                // change Special weapon toggle
                if (e.KeyCode == Keys.D3 && !_ToggleSpecial)
                {
                    _ToggleSecond = false;
                    _TogglePrime = false;
                    _ToggleSpecial = true;
                    SendAmmoData();
                }
            }
            // escape sequence for death mode, escapes to play
            else
            {
                // respwan tank , resume play
                if (e.KeyCode == Keys.Space)
                {
                    SendRespawnData(_Socket, _sPrimaryTank.iTankId);
                    _Scene = GameState.Playing;
                }
            }
        }

        /// <summary>
        /// Handles disengaging tank controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _3DClientForm_KeyUp(object sender, KeyEventArgs e)
        {
            // tank movement controls
            if (e.KeyCode == Keys.W && _Up) { _Up = false; SendData(); }
            if (e.KeyCode == Keys.S && _Down) { _Down = false; SendData(); }
            if (e.KeyCode == Keys.A && _Left) { _Left = false; SendData(); }
            if (e.KeyCode == Keys.D && _Right) { _Right = false; SendData(); }

            // tank turret controls
            if (e.KeyCode == Keys.Left && _TurretLeft) { _TurretLeft = false; SendData(); }
            if (e.KeyCode == Keys.Right && _TurretRight) { _TurretRight = false; SendData(); }

            // Tank firing controls
            if (e.KeyCode == Keys.Up && _FireLight) { _FireLight = false; SendData(); }
            if (e.KeyCode == Keys.Down && _FireSpecial) { _FireSpecial = false; SendData(); }
            if (e.KeyCode == Keys.Space && _FireHeavy) { _FireHeavy = false; SendData(); }

            // turn off toggle switches
            if (e.KeyCode == Keys.D1) _TogglePrime = false;
            if (e.KeyCode == Keys.D2) _ToggleSecond = false;
            if (e.KeyCode == Keys.D3) _ToggleSpecial = false;
        }

        /// <summary>
        /// Used to take user input and send all current states to the server for update
        /// </summary>
        private void SendData()
        {
            // if a active connection is open
            if (_Socket != null && _Socket.Connected)
            {
                try
                {
                    // package up client information and send to server
                    cClientFrame cCF = new cClientFrame(_Up, _Down, _Left, _Right, _TurretLeft, _TurretRight, _FireHeavy,
                                                            _FireLight, _FireSpecial, _TogglePrime, _ToggleSecond, _ToggleSpecial);  // Create new ClientFrame

                    BinaryFormatter BF = new BinaryFormatter();                     // Binary Formatter for sending
                    MemoryStream MS = new MemoryStream();                           // Memory Stream for sending
                    BF.Serialize(MS, cCF);                                          // Serialize the ClientFrame
                    _Socket.Send(MS.GetBuffer(), (int)MS.Length, SocketFlags.None); // Send the serialized ClientFrame
                }
                catch (FormatException ex)
                {
                    Console.WriteLine("Error: " + ex.ToString());
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Error: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Used to send single state updates of weapon toggles
        /// seperate from primary updates to prevent misfires
        /// </summary>
        private void SendAmmoData()
        {
            // if active conection open
            if (_Socket != null && _Socket.Connected)
            {
                try
                {
                    // package up toggle information and send to server
                    cClientToggle cCT = new cClientToggle();
                    cCT.bPrimary = _TogglePrime;
                    cCT.bSecondary = _ToggleSecond;
                    cCT.bSpecial = _ToggleSpecial;

                    BinaryFormatter BF = new BinaryFormatter();                     // Binary Formatter for sending
                    MemoryStream MS = new MemoryStream();                           // Memory Stream for sending
                    BF.Serialize(MS, cCT);                                          // Serialize the ClientFrame
                    _Socket.Send(MS.GetBuffer(), (int)MS.Length, SocketFlags.None); // Send the serialized ClientFrame
                }
                catch (FormatException ex)
                {
                    Console.WriteLine("Error: " + ex.ToString());
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Error: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Used to inform server that a user needs a map generated, with all user data
        /// or if user request new placement after a death
        /// </summary>
        /// <param name="sSend">user socket location</param>
        /// <param name="iTank">tank server assigned index</param>
        private void SendRespawnData(Socket sSend, int iTank)
        {
            MemoryStream mssend = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            cRespawn crTemp = new cRespawn();

            // package up tank information and send to server
            crTemp.iTankType = iTank;
            bf.Serialize(mssend, crTemp);

            sSend.Send(mssend.GetBuffer(), (int)mssend.Length, SocketFlags.None);
        }

        /// <summary>
        /// Initiates program shutdown requirements on close
        /// used to help server disconect properly, uninterupted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _3DClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_Socket != null)
                _Socket.Shutdown(SocketShutdown.Both);
        }

        /// <summary>
        /// Initates start menu controls to connect to a game and select a tank
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _3DClientForm_Shown(object sender, EventArgs e)
        {
            p_Init.Enabled = true;  // Enable panel on form shown (will hold connect button)
            p_Init.Show();          // Show panel on form load (will hold connect button)
        }
 
        #region Init_Panel
        private void btn_Connect_Click(object sender, EventArgs e)
        {
            p_Init.Enabled = false;     // Disable init panel
            p_Init.Hide();              // Hide init panel
            p_Connect.Enabled = true;   // Enable connect panel
            p_Connect.Show();           // Show connect panel
        }
        #endregion

        #region Connect_Panel
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            p_Connect.Enabled = false;  // Disable connect panel
            p_Connect.Hide();           // Hide connect panel
            p_Init.Enabled = true;      // Enable init panel
            p_Init.Show();              // Show init panel
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            p_Connect.Enabled = false;  // Disable connect panel
            p_Connect.Hide();           // Hide connect panel

            _Address = tb_Addr.Text;    // Store provided server address
            _Name = tb_Name.Text;       // Store provided player name

            // Test for and store provided tank
            if (rb_Tank1.Checked)
                _Tank = 0;
            else if (rb_Tank2.Checked)
                _Tank = 1;
            else if (rb_Tank3.Checked)
                _Tank = 2;
            else if (rb_Tank4.Checked)
                _Tank = 3;

            if (_Socket == null)    // If Socket is not null
            {
                _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  // Initialize new Socket for connection

                _Socket.NoDelay = true; // Necessary?

                try
                {
                    _Socket.BeginConnect(_Address, 1666, EndConnect, _Socket);  // Begin connection
                }
                catch (SocketException)
                {
                    _Socket = null; // Reset Socket to null
                    p_Init.Enabled = true;  // Enable init panel
                    p_Init.Show();          // Show init panel
                }
                catch (Exception exc)
                {
                    string s = exc.Message;
                    _Socket = null; // Reset Socket to null
                    p_Init.Enabled = true;  // Enable init panel
                    p_Init.Show();          // Show init panel
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
                //_bSceneChange = true;

                // Possible message back displaying "Connected"

                Thread ReceiveThread = new Thread(Receive); // Create new ReceiveThread
                ReceiveThread.IsBackground = true;  // Set ReceiveThread to Background
                ReceiveThread.Start(_Socket);   // Start ReceiveThread
            }
            catch (SocketException)
            {
                _Socket = null; // Reset Socket to null
                p_Init.Enabled = true;  // Enable init panel
                p_Init.Show();          // Show init panel

            }
            catch (Exception)
            {
                _Socket = null; // Reset Socket to null
                p_Init.Enabled = true;  // Enable init panel
                p_Init.Show();          // Show init panel
            }
            finally
            {

            }
        }
        #endregion

        /// <summary>
        /// Running thread connection to server that is used to send game updates
        /// </summary>
        /// <param name="obj"></param>
        private void Receive(object obj)
        {
            Socket sok = (Socket)obj;   // Local Socket for receives
            byte[] Buffer = new byte[1000000];  // Receive Buffer
            int iBytesReceived; // Variable for number of Bytes Received
            MemoryStream Stream = new MemoryStream();   // Memory Stream used for destacking

            try
            {
                // On Connect, send Player Name and Tank ID to server
                #region InitialConnect
                if (sok != null)    // If Socket is not null
                {
                    try
                    {
                        cClientConnect cConnect = new cClientConnect(_Name, _Tank); // Create new ClientConnect frame with Player Name and Tank ID
                        BinaryFormatter tBF = new BinaryFormatter();    // Temporary Binary Formatter
                        MemoryStream tMS = new MemoryStream();  // Temporary Memory Stream
                        tBF.Serialize(tMS, cConnect);   // Serialize the ClientConnect frame
                        sok.Send(tMS.GetBuffer(), (int)tMS.Length, SocketFlags.None);   // Send the serialized ClientConnect frame to the server
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

                while (true)
                {
                    iBytesReceived = sok.Receive(Buffer, Buffer.Length, SocketFlags.None);  // Store number of Bytes Received
                    if (iBytesReceived == 0)    // If 0 Bytes Received
                    {
                        // Message back with "Connection Lost"

                        _Socket = null; // Reset Socket to null
                        p_Init.Enabled = true;  // Enable init panel
                        p_Init.Show();          // Show init panel
                        return; // Return from method
                    }

                    long lPosition = Stream.Position;   // Present Stream position
                    Stream.Seek(0, SeekOrigin.End); // Seek to beginning of Stream
                    Stream.Write(Buffer, 0, iBytesReceived);    // Write Bytes Received to Stream
                    Stream.Position = lPosition;    // Go back to Present Stream position

                    BinaryFormatter bf = new BinaryFormatter(); // Binary Formatter for Receives

                    do  // Do
                    {
                        long lStartPosition = Stream.Position;  // Store Stream start position
                        try
                        {
                            // Deserialize the Received Frame
                            object Frame = bf.Deserialize(Stream);

                            // if frame is servr frame update screen
                            if (Frame is cServerFrame)
                            {
                                //lock(_sLastKnownInfo)
                                _sLastKnownInfo = (cServerFrame)Frame;
                            }

                            // if frame is terrain frame save to background image
                            else if (Frame is cInitialFrame)
                            {
                                // if frame contains initial game information
                                if (((cInitialFrame)Frame).loObstacles != null)
                                {
                                    _MapTerrain = ((cInitialFrame)Frame).bmTerrainMap;
                                    _PlayerID = ((cInitialFrame)Frame).iIndex;
                                    _loObstacles = ((cInitialFrame)Frame).loObstacles;
                                    _Scene = GameState.Start;
                                    _bSceneChange = true;
                                }
                                // otherwise send respawn cue
                                else
                                    _Scene = GameState.Playing;
                            }
                            // if no connection frame recieved, reshow main menu
                            else if (Frame is NoConnection)
                            {
                                _Scene = GameState.NoConnect;    // toggle no connect screen
                                _Socket = null;
                                p_Init.Enabled = true;
                                p_Init.Show();
                                return;
                            }
                            // show current game standings and run gamestate options
                            else if (Frame is cDeathFrame)
                            {
                                _sCurrentStandings = (cDeathFrame)Frame;
                                _Scene = GameState.Death;
                            }
                        }
                        catch (SerializationException)
                        {
                            Stream.Position = lStartPosition;   // Set Stream position to Stream start position
                            break;  // Break out of the loop
                        }
                    }
                    while (Stream.Position < iBytesReceived);   // While Stream position is less than Bytes Received

                    if (Stream.Position == Stream.Length)   // If Stream position is equal to Stream length
                    {
                        Stream.Position = 0;    // Set Stream position to 0
                        Stream.SetLength(0);    // Set Stream length to 0
                    }

                    Thread.Sleep(0);    // Prevent thread saturation
                }
            }
            catch (SocketException)
            {
                _Socket = null; // Reset socket to null
            }
            catch (Exception)
            {
                _Socket = null; // Reset socket to null
            }
        }

        /// <summary>
        /// Called on the initialization of game after connection to server
        /// Run only when Start state is current gamestate
        /// </summary>
        /// <param name="sMe">current player information</param>
        /// <param name="sEnemies">current oponent information</param>
        private void SetupGame(SceneManager sm, VideoDriver dr, IrrlichtDevice dv, sTank sMe, List<sTank> sEnemies)
        {
            sm.Clear(); // new scene refresh
            sm.AmbientLight.Set(0.3f, 0.3f, 0.3f);

            #region Add_Floor
            // Add the floor for the play area, requires a bitmap
            TerrainSceneNode terrain = sm.AddTerrainSceneNode("terrain.bmp",                    // 1 pixel = 1 unit, so 2500x2500
                                                              null,                             // parent, nobody
                                                              -1,                               // doesn't matter
                                                              new Vector3Df(0, 0, -2500),       // position offset -2500 Y to offset negative y coordinates from server
                                                              new Vector3Df(0, 0, 0),           // rotation
                                                              new Vector3Df(5.0f, 5.0f, 5.0f)); // scale

            terrain.SetMaterialTexture(0, dr.GetTexture("Forest _Floor.jpg"));                  // terrain texture
            terrain.SetMaterialType(MaterialType.Solid);                                        
            terrain.ScaleTexture(100f);                                                         // texture multiplier
            terrain.SetMaterialFlag(MaterialFlag.Lighting, true);
            #endregion

            #region Add_SkyMesh
            sm.AddSkyBoxSceneNode("irrlicht2_up.jpg", "irrlicht2_dn.jpg", "irrlicht2_lf.jpg", "irrlicht2_rt.jpg", "irrlicht2_ft.jpg", "irrlicht2_bk.jpg");
            #endregion

            #region Add_Obstacles
            // Clear current _obstacles
            _obstacles.Clear();
            foreach (KeyValuePair<int, sObstacle> cOb in _loObstacles)
            {
                // setup default information containers
                AnimatedMesh obstacleMesh = null;
                Vector3Df vNewScale = null;
                string sTextureName = "";

                #region Determine_Object_Meshes
                if (cOb.Key == 1)       // if tankstopper
                {
                    obstacleMesh = sm.GetMesh("rusty_wall.ms3d"); // retrieve our model, supports lots o types of meshes, ONLY ONCE
                    vNewScale = new Vector3Df(1.0f, 2.0f, 1.0f); // You shouldn't need to do this, you should make your obstacles the size you want
                    sTextureName = "stones.jpg";
                }

                if (cOb.Key == 2)       //  if tree
                {
                    obstacleMesh = sm.GetMesh("firtree3.3ds"); // retrieve our model, supports lots o types of meshes, ONLY ONCE
                    vNewScale = new Vector3Df(25.0f, 35.0f, 20.0f); // You shouldn't need to do this, you should make your obstacles the size you want
                    sTextureName = "tree_texture.jpg";
                }

                if (cOb.Key == 3)       // if wall
                {
                    obstacleMesh = sm.GetMesh("rusty_wall.ms3d"); // retrieve our model, supports lots o types of meshes, ONLY ONCE
                    vNewScale = new Vector3Df(1.0f, 2.5f, 3.0f); // You shouldn't need to do this, you should make your obstacles the size you want
                    sTextureName = "wall_128.bmp";
                }
                #endregion

                AnimatedMeshSceneNode obstacle = sm.AddAnimatedMeshSceneNode(obstacleMesh); // make a node and Add it, save return to "tweak" it
                _obstacles.Add(obstacle);

                // apply texture property
                obstacle.SetMaterialTexture(0, dr.GetTexture(sTextureName));
                obstacle.SetMaterialType(MaterialType.Solid);
                obstacle.SetMaterialFlag(MaterialFlag.Lighting, true);

                obstacle.Position = new Vector3Df(cOb.Value.pLocation.X, 0, -cOb.Value.pLocation.Y); // start at 10, 0 ( ground ), 10  - remember x is x, y is 0, z is y
                obstacle.Rotation = new Vector3Df(0, cOb.Value.fRotation + 90, 0);
                obstacle.Scale = vNewScale;
            }
            #endregion

            #region Setup_Enemies
            // for all potential enemies
            for (int i = 0; i < 4; i++)
            {
                // setup containers 
                AnimatedMeshSceneNode _Enemy = null;
                sTank sTemp = new sTank();
                sTemp.iTankId = -1;

                // get predetermined ModelInfo and mesh type 
                KeyValuePair<ModelInfo, AnimatedMesh> kvpEnemy = GetTankModel(sm, sTemp);
                _Enemy = sm.AddAnimatedMeshSceneNode(kvpEnemy.Value); // make a node and Add it, save return to "tweak" it
                _Enemies.Add(new KeyValuePair<ModelInfo, AnimatedMeshSceneNode>(kvpEnemy.Key, _Enemy));
                _Enemy.Position = new Vector3Df(0, -100, 0); // set default under terrain, hidden
                _Enemy.Rotation = new Vector3Df(0, 0, 0);    // no rotation required
                _Enemy.SetMaterialFlag(MaterialFlag.Lighting, true);

                // turret editing for enemies
                AnimatedMesh smTurret = sm.GetMesh("Turret.ms3d");                      // create enemy turret
                AnimatedMeshSceneNode Turret = sm.AddAnimatedMeshSceneNode(smTurret);
                _Turrets.Add(Turret);
                Turret.Position = new Vector3Df(0, -150, 0); // set default under terrain, hidden
                RotationOffset = 0;
                Turret.Rotation = new Vector3Df(0, 0, 0);
                Turret.SetMaterialFlag(MaterialFlag.Lighting, true);
                Turret.SetMaterialTexture(1, dr.GetTexture("tank1.jpg"));
                Turret.SetMaterialType(MaterialType.Solid);
            }
            #endregion

            #region Setup_Special
            //Create 100 premade moedels of both mines and oilslicks, should not matter
            for (int i = 0; i < 100; i++)
            {
                // create oilslick mesh and hide it under terrain
                AnimatedMeshSceneNode Oil = sm.AddAnimatedMeshSceneNode(sm.GetMesh("Oil.ms3d")); 
                _OilSlick.Add(new KeyValuePair<ModelInfo, AnimatedMeshSceneNode>(new ModelInfo(), Oil));

                // apply texture property
                Oil.SetMaterialTexture(0, dr.GetTexture("terrain.bmp"));
                Oil.SetMaterialType(MaterialType.Solid);
                Oil.SetMaterialFlag(MaterialFlag.Lighting, true);
                Oil.Scale = new Vector3Df(1f, 1f, 1f);

                // hide under terrain
                Oil.Position = new Vector3Df(0, -100, 0);
                Oil.Rotation = new Vector3Df(0, 0, 0);

                // create landmine mesh and hide it under terrain
                AnimatedMeshSceneNode Mine = sm.AddAnimatedMeshSceneNode(sm.GetMesh("Mine.ms3d"));
                _LandMine.Add(new KeyValuePair<ModelInfo, AnimatedMeshSceneNode>(new ModelInfo(), Mine));

                // apply texture property
                Mine.SetMaterialTexture(0, dr.GetTexture("wall_128.bmp"));
                Mine.SetMaterialType(MaterialType.Solid);
                Mine.SetMaterialFlag(MaterialFlag.Lighting, true);
                Mine.Scale = new Vector3Df(0.8f, 0.8f, 0.8f);

                // hide under terrain
                Mine.Position = new Vector3Df(0, -100, 0);
                Mine.Rotation = new Vector3Df(0, 0, 0);
            }
            #endregion

            #region My_Tank_Addition
            
            // get tank mesh from predetermined measurements
            KeyValuePair<ModelInfo, AnimatedMesh> kvpMe = GetTankModel(sm, _sPrimaryTank);
            AnimatedMesh am = kvpMe.Value;

            _me = new KeyValuePair<ModelInfo, AnimatedMeshSceneNode>(kvpMe.Key, sm.AddAnimatedMeshSceneNode(am)); 
            _me.Value.Position = new Vector3Df(sMe.pLocation.X, 0, -sMe.pLocation.Y); // Position at location from server
            _me.Value.Rotation = kvpMe.Key.v3dRotationOffset;

            // if a texture image name was given assign to mesh
            if (kvpMe.Key.sTexture.Length > 0)
            {
                _me.Value.SetMaterialTexture(0, dr.GetTexture(kvpMe.Key.sTexture));
                _me.Value.SetMaterialType(MaterialType.Solid);
            }
            _me.Value.Scale = kvpMe.Key.v3dScale;       // set scale from predetermined measurement
            _me.Value.SetMaterialFlag(MaterialFlag.Lighting, true);
            

            CameraSceneNode carCam = sm.AddCameraSceneNode(
              _me.Value, // parent node, will attach to it
              new Vector3Df(_XCamOffset, _YCamOffset, _ZCamOffset), // Position, relativeE to tank
              _me.Value.Position); // Looking tank

            // turret editing for enemies
            AnimatedMesh smTurretMe = sm.GetMesh("Turret.ms3d");
            AnimatedMeshSceneNode TurretMe = sm.AddAnimatedMeshSceneNode(smTurretMe);
            _Turrets.Insert(0, TurretMe);                                               // insert first to make finding faster
            TurretMe.Position = new Vector3Df(sMe.pLocation.X, 45, -sMe.pLocation.Y);   // Place at server based position
            RotationOffset = 0;
            TurretMe.Rotation = new Vector3Df(0, 0, 0);                                 // no rotation
            TurretMe.Scale = _me.Value.Scale;
            TurretMe.SetMaterialFlag(MaterialFlag.Lighting, true);
            TurretMe.SetMaterialTexture(1, dr.GetTexture("tank1.jpg"));
            TurretMe.SetMaterialType(MaterialType.Solid);
            #endregion

            SceneNode light = sm.AddLightSceneNode(null,    // parent node, here none
              new Vector3Df(0, 500, 0),                     // where is the light
              new Colorf(0.5f, 0.5f, 0.5f, 1f),             // what color is the light, light gray, not too bright ?
              10000);                                       // travel distnace
            _Scene = GameState.Playing;                     // start game play options
        }

        /// <summary>
        /// Predetermined inputs for tank information, 
        /// takes in tank information from server and converts to animation information
        /// </summary>
        /// <param name="sm">current environement</param>
        /// <param name="sT">tank information to convert</param>
        /// <returns></returns>
        private KeyValuePair<ModelInfo, AnimatedMesh> GetTankModel(SceneManager sm, sTank sT)
        {
            ModelInfo miTemp = new ModelInfo();
            AnimatedMesh am;
            miTemp.iTankID = sT.iTankId;
            if (sT.iTankId == 1)                // cTankType1 equivalent
            {
                am = sm.GetMesh("Tank1.ms3d");
                miTemp.sTexture = "tank1.jpg";
                miTemp.v3dRotationOffset = new Vector3Df(0, 0, 0);
                miTemp.v3dScale = new Vector3Df(1.4f, 1.9f, 2f);
                miTemp.iYOffset = 15;
            }
            else if (sT.iTankId == 2)           // cTankType1 equivalent
            {
                am = sm.GetMesh("Tank2.ms3d");
                miTemp.sTexture = "H2.jpg";
                miTemp.v3dRotationOffset = new Vector3Df(0, 0, 0);
                miTemp.v3dScale = new Vector3Df(1f, 1f, 1f);
                miTemp.iYOffset = 0;
            }
            else if (sT.iTankId == 3)           // cTankType2 equivalent
            {
                am = sm.GetMesh("Tank1.ms3d");
                miTemp.sTexture = "Tank3.jpg";
                miTemp.v3dRotationOffset = new Vector3Df(0, 0, 0);
                miTemp.v3dScale = new Vector3Df(1.4f, 2.1f, 2f);
                miTemp.iYOffset = 15;
            }
            else                                // cTankType4 equivalent, default
            {
                am = sm.GetMesh("UFO.ms3d");
                miTemp.sTexture = "wall.bmp";
                miTemp.v3dRotationOffset = new Vector3Df(0, 0, 0);
                miTemp.v3dScale = new Vector3Df(1f, 1f, 1f);
                miTemp.iYOffset = -5;
            }
            return new KeyValuePair<ModelInfo, AnimatedMesh>(miTemp, am);   // return animation information
        }

        /// <summary>
        /// Predetermined inputs for ammo information, 
        /// takes in ammo information from server and converts to animation information
        /// </summary>
        /// <param name="sm">current environment</param>
        /// <param name="iType">ammo int type</param>
        /// <returns>animation information</returns>
        private KeyValuePair<ModelInfo, AnimatedMesh> GetAmmoType(SceneManager sm, int iType)
        {
            // setup default containers
            ModelInfo miTemp = new ModelInfo();
            AnimatedMesh am = null;
            miTemp.iTankID = iType;
            const int ciDefaultHeight = 50;

            if (iType == 1)         // machine gun
            {
                am = sm.GetMesh("lowpolybullet.3ds");
                miTemp.sTexture = "Bullet_U.bmp";
                miTemp.v3dRotationOffset = new Vector3Df(-90, -90, 0);
                miTemp.v3dScale = new Vector3Df(1f, 1f, 1f);
                miTemp.iYOffset = ciDefaultHeight;
            }
            if (iType == 2)         // flame gun
            {
                am = sm.GetMesh("Flame.ms3d");
                miTemp.sTexture = "texture_torch.bmp";
                miTemp.v3dRotationOffset = new Vector3Df(0, 0, 0);
                miTemp.v3dScale = new Vector3Df(0.8f, 1f, 1f);
                miTemp.iYOffset = ciDefaultHeight;
            }

            if (iType == 11)        // shell normal
            {
                am = sm.GetMesh("lowpolybullet.3ds");
                miTemp.sTexture = "Bullet_P.jpg";
                miTemp.v3dRotationOffset = new Vector3Df(-90, -90, 0);
                miTemp.v3dScale = new Vector3Df(2f, 2f, 2f);
                miTemp.iYOffset = ciDefaultHeight;
            }
            if (iType == 12)        // shell heavy
            {
                am = sm.GetMesh("lowpolybullet.3ds");
                miTemp.sTexture = "Bullet_S.jpg";
                miTemp.v3dRotationOffset = new Vector3Df(-90, -90, 0);
                miTemp.v3dScale = new Vector3Df(2.5f, 2.5f, 2.5f);
                miTemp.iYOffset = ciDefaultHeight;
            }
            return new KeyValuePair<ModelInfo, AnimatedMesh>(miTemp, am);   // return animation information
        }

        /// <summary>
        /// Renders all on screen items, including tanks, obstacles, and ammo
        /// </summary>
        /// <param name="e"></param>
        private void Renderer(object e)
        {
            DeviceSettings settings = e as DeviceSettings; // Save our settings to make our rendering device

            // create irrlicht device using provided settings
            IrrlichtDevice dev = IrrlichtDevice.CreateDevice(settings);

            if (dev == null)
                throw new Exception("Failed to create Irrlicht device."); // something bad happened

            VideoDriver drv = dev.VideoDriver;
            SceneManager smgr = dev.SceneManager;
            smgr.FileSystem.WorkingDirectory = @"../../../3dMedia"; // Where is our media at ? Use relative so it ports easily

            // Font Drawing is done every loop, it is not a "node" in the scene which is fully traversed and displayed via smgr.DrawAll()
            IrrlichtLime.GUI.GUIFont f = dev.GUIEnvironment.BuiltInFont;

            // Bitmap Font
            f = dev.GUIEnvironment.GetFont("font_lucida.png");

            Random rnd = new Random(); // just for demo purposes
            while (dev.Run()) // Main rendering loop, stays here until thread is requested to quit
            {
                // add background to drive over
                List<sTank> sEnemies = new List<sTank>();   // enemy tanks

                #region Determine_Primary_Tank
                // sort out all enemy tanks from my tank
                if (_sLastKnownInfo != null)
                {
                    List<KeyValuePair<int, sTank>> kvpTemp;
                    lock (_sLastKnownInfo)
                        kvpTemp = _sLastKnownInfo.TanksInPlay;

                    foreach (KeyValuePair<int, sTank> kvpTanks in kvpTemp)
                        if (kvpTanks.Key.Equals(_PlayerID))
                            _sPrimaryTank = kvpTanks.Value;
                        else
                            sEnemies.Add(kvpTanks.Value);
                }
                #endregion

                #region Check_For_Menu_Change
                if (_bSceneChange) // main form change of scene 
                {
                    switch (_Scene)
                    {
                        case GameState.Start:
                            SetupGame(smgr, drv, dev, _sPrimaryTank, sEnemies);
                            break;
                        default:
                            break;
                    }
                    _bSceneChange = false; // Done scene setup, revert to normal mode again
                }
                #endregion

                drv.BeginScene(true, true, settings.BackColor);

                // Game Scene processing Main processing (playing)
                if (_Scene == GameState.Playing)
                {
                    #region Render_Enemies
                    int i = 0;
                    // re add all enemies from server packet to screen
                    foreach (sTank sT in sEnemies)
                    {
                        AnimatedMeshSceneNode _Enemy = null;        // set new empty enemy mesh

                        // if enemy in current storage space does not equal the current update position
                        // re add mesh with new tank information
                        if (sT.iTankId != _Enemies[i].Key.iTankID)
                        {
                            KeyValuePair<ModelInfo, AnimatedMesh> kvpEnemy = GetTankModel(smgr, sT);    // get tank animation info
                            _Enemy = smgr.AddAnimatedMeshSceneNode(kvpEnemy.Value);                    
                            _Enemies[i] = new KeyValuePair<ModelInfo, AnimatedMeshSceneNode>(kvpEnemy.Key, _Enemy);
                            _Enemies[i].Value.Scale = _Enemies[i].Key.v3dScale;
                            //  if texture name available
                            if (_Enemies[i].Key.sTexture.Length > 0)
                            {
                                _Enemies[i].Value.SetMaterialTexture(0, drv.GetTexture(_Enemies[i].Key.sTexture));
                                _Enemies[i].Value.SetMaterialType(MaterialType.Solid);
                            }
                        }
                        // reset enemy position to server update location
                        _Enemies[i].Value.Position = new Vector3Df(sT.pLocation.X, _Enemies[i].Key.iYOffset, -sT.pLocation.Y); 
                        RotationOffset = 0;
                        // reset enemy rotation to server update location
                        _Enemies[i].Value.Rotation = new Vector3Df(_Enemies[i].Key.v3dRotationOffset.X,
                        _Enemies[i].Key.v3dRotationOffset.Y + sT.fTankRotation, _Enemies[i].Key.v3dRotationOffset.Z);

                        //  add enemy turret
                        _Turrets[i + 1].Position = new Vector3Df(sT.pLocation.X, 45, -sT.pLocation.Y);
                        _Turrets[i + 1].Rotation = new Vector3Df(0, sT.fTurretRotation, 0);
                        i++;
                    }
                    #endregion

                    #region Dynamic_Ammo_Creation
                    // for all ammo meshes in play, clear them
                    foreach (KeyValuePair<ModelInfo, AnimatedMeshSceneNode> kvpTemp in _Ammo)
                        kvpTemp.Value.Remove();

                    // initiate new ammo list
                    _Ammo = new List<KeyValuePair<ModelInfo, AnimatedMeshSceneNode>>();

                    // for all ammo in server update list
                    foreach (KeyValuePair<byte, sAmmo> sA in _sLastKnownInfo.AmmoPoints)
                    {
                        // create new ammo mesh
                        KeyValuePair<ModelInfo, AnimatedMesh> kvpTemp = GetAmmoType(smgr, sA.Key);
                        AnimatedMeshSceneNode Ammo = smgr.AddAnimatedMeshSceneNode(kvpTemp.Value); // make a node and Add it, save return to "tweak" it
                        _Ammo.Add(new KeyValuePair<ModelInfo, AnimatedMeshSceneNode>(kvpTemp.Key, Ammo));

                        // apply texture property
                        Ammo.SetMaterialTexture(0, drv.GetTexture(kvpTemp.Key.sTexture));
                        Ammo.SetMaterialType(MaterialType.Solid);
                        Ammo.SetMaterialFlag(MaterialFlag.Lighting, true);

                        // apply ammunition position and rotation
                        Ammo.Position = new Vector3Df(sA.Value.pLocation.X, kvpTemp.Key.iYOffset, -sA.Value.pLocation.Y); // start at 10, 0 ( ground ), 10  - remember x is x, y is 0, z is y
                        Ammo.Rotation = new Vector3Df(kvpTemp.Key.v3dRotationOffset.X, sA.Value.fRotation + kvpTemp.Key.v3dRotationOffset.Y, kvpTemp.Key.v3dRotationOffset.Z);
                        Ammo.Scale = kvpTemp.Key.v3dScale;
                        Ammo.SetMaterialFlag(MaterialFlag.Lighting, true);
                    }
                    #endregion

                    #region My_Tank_Render
                    RotationOffset = _sPrimaryTank.fTankRotation + _me.Key.v3dRotationOffset.Y;

                    _me.Value.Position = new Vector3Df(_sPrimaryTank.pLocation.X, _me.Key.iYOffset, -_sPrimaryTank.pLocation.Y);
                    _me.Value.Rotation = new Vector3Df(0, RotationOffset, 0); // only about the Y, we are not a plane, or are we ?

                    _Turrets[0].Position = new Vector3Df(_sPrimaryTank.pLocation.X, 45, -_sPrimaryTank.pLocation.Y);
                    _Turrets[0].Rotation = new Vector3Df(0, _sPrimaryTank.fTurretRotation, 0);

                    smgr.ActiveCamera.Target =
                        new Vector3Df(_me.Value.Position.X, _me.Value.Position.Y + _me.Key.iYOffset, _me.Value.Position.Z);
                    float fScale = _me.Key.v3dScale.X;
                    smgr.ActiveCamera.Rotation = _me.Key.v3dRotationOffset;
                    smgr.ActiveCamera.Position = new Vector3Df(_XCamOffset, _YCamOffset, _ZCamOffset);
                    #endregion

                    #region Dynamic_Special_Creation
                    // set all oilslicks back to hidden position
                    foreach (KeyValuePair<ModelInfo, AnimatedMeshSceneNode> kvpSp in _OilSlick)
                        kvpSp.Value.Position = new Vector3Df(0, -100, 0);

                    // set all landmines back to hidden position
                    foreach (KeyValuePair<ModelInfo, AnimatedMeshSceneNode> kvpSp in _LandMine)
                        kvpSp.Value.Position = new Vector3Df(0, -100, 0);

                    int iCurrentO = 0,  // oilslick counter
                        iCurrentM = 0;  // landmine counter

                    // for all special weapons on server update
                    foreach (KeyValuePair<byte, sSpecial> kvpTemp in _sLastKnownInfo.SpecialItems)
                    {
                        if (kvpTemp.Key == 21)  // is oilslick
                        {
                            _OilSlick[iCurrentO].Value.Rotation = new Vector3Df(0, kvpTemp.Value.fRotation, 0);
                            _OilSlick[iCurrentO].Value.Position = new Vector3Df(kvpTemp.Value.pLocation.X, 0, -kvpTemp.Value.pLocation.Y);
                        }
                        else if (kvpTemp.Key == 22) // is landmine
                        {
                            _LandMine[iCurrentM].Value.Rotation = new Vector3Df(0, kvpTemp.Value.fRotation, 0);
                            _LandMine[iCurrentM].Value.Position = new Vector3Df(kvpTemp.Value.pLocation.X, 0, -kvpTemp.Value.pLocation.Y);
                        }
                        // increment counters
                        ++iCurrentM;
                        ++iCurrentO;
                    }
                    #endregion
                }

                #region Hud_Functionality
                smgr.DrawAll(); // Render all 3D elements, still have to overlay GUI elements

                // draw hud for player if currently in playing state and player exists
                if (_Scene == GameState.Playing && _me.Value != null)
                {
                    f.Draw("Current Life: " + _sPrimaryTank.iLife + "  Primary Reload: " + _sLastKnownInfo.cShotInfo.iSlowAmmoCurrent +
                        "  Special Reload: " + _sLastKnownInfo.cShotInfo.iSpecialAmmoCurrent, new Vector2Di(0, 0), IrrlichtLime.Video.Color.OpaqueRed);
                    RenderPrimaryRadar(drv, _sPrimaryTank, sEnemies);       // add radar to screen
                }

                if (_Scene == GameState.Death) // Display overhead view of math and current player standings
                {
                    // move camera
                    smgr.ActiveCamera.Target = new Vector3Df(1250, 0, -1250);
                    smgr.ActiveCamera.Rotation = new Vector3Df(0, 0, 0);
                    smgr.ActiveCamera.Position = new Vector3Df(0, 510, 0);

                    // create standings table
                    Stats[] sSt = _sCurrentStandings.sCurrentStandings;
                    int iSCount = 2;
                    f.Draw(string.Format("You were killed by {0}", _sCurrentStandings.sKilledBy),
                        0, 0, IrrlichtLime.Video.Color.OpaqueRed);
                    f.Draw("=======================================",
                        0, 15, IrrlichtLime.Video.Color.OpaqueRed);
                    f.Draw(string.Format("||{0}|{1}|{2}|{3}||",  
                        PadString("Name", 30),  PadString("Kills", 12),  PadString("Deaths",12),  PadString("Kill Streak", 22)),
                        0, 30, IrrlichtLime.Video.Color.OpaqueRed);
                    f.Draw("---------------------------------------------------------",
                        0, 45, IrrlichtLime.Video.Color.OpaqueRed);
                    foreach (Stats s in sSt)
                    {
                        f.Draw(string.Format("||{0}|{1}|{2}|{3}||",
                            PadString(s.sTankName, 30), PadString(s.ideaths, 15), PadString(s.iKills, 15), PadString(s.iKillStreak, 25)),
                            0, 30 * iSCount , IrrlichtLime.Video.Color.OpaqueRed);
                        if (iSCount < sSt.Length)
                            f.Draw("-------------------------------------------------------",
                                0, 30 * iSCount + 15, IrrlichtLime.Video.Color.OpaqueRed);
                        iSCount++;
                    }
                    f.Draw("=======================================",
                        0, 30 * (iSCount - 1) + 15, IrrlichtLime.Video.Color.OpaqueRed);
                    f.Draw("Press [Space] to Respawn", 0, 30 * iSCount, IrrlichtLime.Video.Color.OpaqueRed);
                }
                #endregion

                dev.GUIEnvironment.DrawAll(); // Render all the GUI overlay elements ( like radar etc )

                drv.EndScene(); // Flag that everything is ready to "flip" to the display

                // if we requested to stop, we close the device, exit loop and bail
                if (userWantExit)
                {
                    dev.Close();
                    break;
                }
                System.Threading.Thread.Sleep(10); // play nice
            } // end-while - end of Render loop

            // drop device
            dev.Drop();
            _Render = null;
        }

        /// <summary>
        /// Used to render primary radar for usr interface
        /// </summary>
        /// <param name="dv">current screen</param>
        /// <param name="sPrimary">priamry tank info</param>
        /// <param name="sEnemies">enemy tank info</param>
        private void RenderPrimaryRadar(VideoDriver dv, sTank sPrimary, List<sTank> sEnemies)
        {
            // draw inital radar map, at a reduced opacity
            dv.Draw2DPolygon(new Vector2Di(dv.ScreenSize.Width - 50, dv.ScreenSize.Height - 50), 50, IrrlichtLime.Video.Color.OpaqueRed, 20);
            dv.Draw2DRectangle(new Recti(new Vector2Di(dv.ScreenSize.Width - 50,dv.ScreenSize.Height - 50), 
                new Dimension2Di(4,4)), IrrlichtLime.Video.Color.OpaqueGreen);

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
                    dv.Draw2DRectangle(new Recti(new Vector2Di(dv.ScreenSize.Width - (50 + (int)fXOffset), 
                            dv.ScreenSize.Height - (50 + (int)fYOffset)),
                        new Dimension2Di(4, 4)), IrrlichtLime.Video.Color.OpaqueRed);
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

        /// <summary>
        /// Pads string out to required witdth with text centered in the middle
        /// takes in either string or int
        /// </summary>
        /// <param name="input">input information</param>
        /// <param name="totalBuff">width of spacing</param>
        /// <returns>padded string</returns>
        private string PadString(object input, int totalBuff)
        {
            string output = "";
            string sInput = "";
            if (input is string)
                sInput = (string)input;
            else if (input is int)
                sInput = string.Format("{0}", ((int)input));

            int iCountMid = (totalBuff - sInput.Length) / 2;
            int totalOffset = totalBuff - sInput.Length;

            for (int i = 0; i < totalOffset; i++)
            {
                if (i == iCountMid)
                    output += input;
                else
                    output += " ";
            }
            return output;
        }
    }
}
