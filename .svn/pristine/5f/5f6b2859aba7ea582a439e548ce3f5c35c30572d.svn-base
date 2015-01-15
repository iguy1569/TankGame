// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    Server
// Class File:  ServerForm.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach and Dan Allen
//
// Discription: Runs connection for clients to connect to the game.
//              Responsible for running all of the game states as well as
//              processing the appropiate commands revieved by the user.
// ///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using ClientFrame;
using ServerFrame;
using Tanks;

using Ammo;
using Terrain;
using Obstacles;

namespace Server
{
    public partial class ServerForm : Form
    {

        /// <summary>
        /// Make sure that the server is able to keep track of all the information it has assigned
        /// to each player that has connected for their individual information.
        /// </summary>
        public struct TankInformation
        {
            public cTank tPlayerTank;
            public cClientFrame cPlayerInsructions;
            public int iAssignedIndex;
            public int iPrimaryAmmoType;
            public int iSecondaryAmmoType;
            public int iSpecialAmmoType;

            public TankInformation(cTank ct, cClientFrame cc, int iA, int iPAT, int iSAT, int iSpAT)
            {
                tPlayerTank = ct;
                cPlayerInsructions = cc;
                iAssignedIndex = iA;
                iPrimaryAmmoType = iPAT;
                iSecondaryAmmoType = iSAT;
                iSpecialAmmoType = iSpAT;               
            }
        }

        Socket m_MainSock = null;                                                                               // This is essentially the listening socket.
        Dictionary<Socket, TankInformation> m_AllPlayerStats = new Dictionary<Socket, TankInformation>();       // Keep track of every player stats in relation to his socket.
        List<KeyValuePair<Socket, cClientFrame>> m_lkvpClientInstruction =
            new List<KeyValuePair<Socket, cClientFrame>>();                                                     // queue for all client instructions recieved

        List<cAmmo> m_lcAmmo = new List<cAmmo>();                    // list of all live ammo     
        List<cSpecialItem> m_lcSpecial = new List<cSpecialItem>();     // list of all special items
        List<cObstacle> m_loObstacles = new List<cObstacle>();

        int m_iBytesReceived = 0,                                                           // Total bytes received. Required to update the UI.
            m_iFramesReceived = 0,                                                          // Total frames received. Required to update the UI.
            m_iFragments = 0,                                                               // Total fragments received. Required to update the UI.
            m_iPlayerIndex = 0;                                                             // Servers' holder for indexing the players connected

        // calback delegates
        public delegate void delVoidString(string s);
        public delegate void delVoidVoid();

        public delVoidString m_Log;                                                         // Used to update the administrator with critical updates
        public delVoidString m_con;                                                         // Used to update the connection tab with critical updates 
        public delVoidVoid m_tool;                                                          // Used to update the tool strip when new information is received

        #region Main UI Thread
        public ServerForm()
        {
            InitializeComponent();
            m_MainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_MainSock.NoDelay = true;
            m_Log = LogMessage;
            m_con = ConnectionMessage;
            m_tool = ToolStripLog;
        }

        /// <summary>
        /// Connection screen message log
        /// </summary>
        /// <param name="s">message</param>
        private void ConnectionMessage(string s)
        {
            LB_Connected.Items.Insert(0, s);
        }

        /// <summary>
        /// log screen message log
        /// </summary>
        /// <param name="s">message</param>
        private void LogMessage(string s)
        {
            LB_Log.Items.Insert(0, s);
        }
    
        /// <summary>
        /// Updates all data in the toolstrip bar 
        /// basic server information, no player stats
        /// </summary>
        private void ToolStripLog()
        {
            //This is used to update the UI in proper k/m/g notation
            string sOut;
            if (m_iBytesReceived / 1073741824 != 0)
                sOut = string.Format("{0:F3}G", (double)m_iBytesReceived / (double)1073741824);
            else if (m_iBytesReceived / 1048576 != 0)
                sOut = string.Format("{0:F3}M", (double)m_iBytesReceived / (double)1048576);
            else if (m_iBytesReceived / 1024 != 0)
                sOut = string.Format("{0:F3}K", (double)m_iBytesReceived / (double)1024);
            else
                sOut = string.Format("{0}", m_iBytesReceived);

            TS_LBL_BytesR.Text = string.Format("Bytes Received: {0:F2}", sOut);
            TS_LB_FramesR.Text = string.Format("Frames Recieved: {0:F2}", m_iFramesReceived);
            TS_LB_Fragment.Text = string.Format("Fragments Recieved: {0:F2}", m_iFragments);
        }

        /// <summary>
        /// Finishing initial components
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerForm_Shown(object sender, EventArgs e)
        {
            TIM_Refresh.Enabled = true;
            m_MainSock.Bind(new IPEndPoint(IPAddress.Any, 1666));
            m_MainSock.Listen(10);      //Maximum length of pending connections is 10 (limited players 4)

            //The server uses the threaded model for connections
            Thread ThAccept = new Thread(AcceptingConnection);
            ThAccept.IsBackground = true;
            ThAccept.Start(m_MainSock);

            ConnectionMessage("Listening to connections");
            m_loObstacles = CreateObstacleInitials();
        }

        /// <summary>
        /// Does safe disconnect when the server gets closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            //When the form is closing, we say goodbyte to everyone nicely
            lock (m_AllPlayerStats)
            {
                foreach (Socket sok in m_AllPlayerStats.Keys)
                {
                    if (sok != null && sok.Connected)
                    {
                        sok.Shutdown(SocketShutdown.Both); 
                        sok.Close();
                    }
                }
            }
        }
        #endregion

        #region Connection_Threads_For_Data_Retrieval
        /// <summary>
        /// Server listening for new players to join, ensures that while server is running
        /// if the game connection limit is not reached another player will be able to join
        /// the game while it is in progress.
        /// </summary>
        /// <param name="obj"></param>
        private void AcceptingConnection(object obj)
        {
            Socket Sok = (Socket)obj;

            try
            {
                //We're in a constant listening state
                while (true)
                {
                    Socket temp = Sok.Accept();  
                    int iPlayerCount;

                    lock(m_AllPlayerStats)
                        iPlayerCount = m_AllPlayerStats.Count;
                    
                    // if connection limit is reached it will not allow another connection to be made
                    if (iPlayerCount >= 4)
                    {
                        BeginInvoke(m_con, "Connection Limit Reached: " + m_AllPlayerStats.Count);

                        MemoryStream ms = new MemoryStream();
                        BinaryFormatter bf = new BinaryFormatter();
                        NoConnection ncInform = new NoConnection();
                        ncInform.iNoConnect = 5;              
                        bf.Serialize(ms, ncInform);
                        temp.Send(ms.GetBuffer(), (int)ms.Length, SocketFlags.None); //send update status
                        continue;
                    }
                    // Connection gets made and thread gets started
                    else
                    {
                        Thread ThReceive = new Thread(ReceivingConnection);
                        ThReceive.IsBackground = true;
                        ThReceive.Start(temp);
                        BeginInvoke(m_con, "Connection Made");
                    }
                    Thread.Sleep(0);
                }
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Trace.WriteLine("Socket Exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Generic Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Processes all information received from the client
        /// </summary>
        /// <param name="obj"></param>
        private void ReceivingConnection(object obj)
        {
            Socket Sok = (Socket)obj;
            Sok.NoDelay = true;
            byte[] buffer = new byte[50000];

            MemoryStream msreceive = new MemoryStream();
            int iReceivesFired = 0;

            try
            {
                while (true)
                {
                    int iBytesReceivedCheck = Sok.Receive(buffer, buffer.Length, SocketFlags.None);
                    iReceivesFired++;
                    if (iBytesReceivedCheck == 0)
                    {
                        m_Log("0 bytes were received from " + Sok.RemoteEndPoint.ToString() + ". Closing connection on " + Sok.RemoteEndPoint.ToString());
                        Sok.Shutdown(SocketShutdown.Both);
                        lock (m_AllPlayerStats)
                            m_AllPlayerStats.Remove(Sok);
                        return;
                    }

                    //save the current read position
                    long lPos = msreceive.Position;
                    //seek to the end to append the data
                    msreceive.Seek(0, SeekOrigin.End);
                    //append the new data to the end of the stream
                    msreceive.Write(buffer, 0, iBytesReceivedCheck);
                    //restore the read position back to where it started
                    msreceive.Position = lPos;

                    m_iBytesReceived += iBytesReceivedCheck;
                    BinaryFormatter bf = new BinaryFormatter();      

                    while (msreceive.Position < msreceive.Length)
                    {
                        //save the stream position in case the deserialization fails, and it has to be reset
                        long lStartPos = msreceive.Position;
                        try
                        {
                            object oIncomingInstruction = bf.Deserialize(msreceive);

                            #region Client_Connecting
                            // all initial connection data received from the client and stores appropriate data
                            // into their corresponding lists
                            if (oIncomingInstruction is cClientConnect)
                            {
                                cTank tTemp = SpawnNewTank((cClientConnect)oIncomingInstruction);

                                m_iPlayerIndex += 5; // change default index
                                // add to playerlist
                                m_AllPlayerStats.Add(Sok, new TankInformation(tTemp, new cClientFrame(), m_iPlayerIndex, 0, 0, 0));

                                // create obstacles and border 
                                
                                List<KeyValuePair<int, sObstacle>> lisTemp = new List<KeyValuePair<int,sObstacle>>();
                                foreach (cObstacle o in m_loObstacles)
                                {
                                    sObstacle sTemp = new sObstacle();
                                    sTemp.pLocation = o.Location;
                                    sTemp.fRotation = o.Rotation;
                                    lisTemp.Add(new KeyValuePair<int,sObstacle>(WhatObstacle.ObstacleConvertTypeToByte(o), sTemp));
                                }

                                // create a package to send player map choosen, obstacles and their assigned index
                                cTerrain cTemp = new cTerrain();
                                MemoryStream msSend = new MemoryStream();
                                cInitialFrame ctftemp = new cInitialFrame();
                                ctftemp.bmTerrainMap = cTemp.GetMapsList[1];        // set map
                                ctftemp.iIndex = m_iPlayerIndex;                    // set player index
                                ctftemp.loObstacles = lisTemp;

                                // once packages are built send to clients
                                bf.Serialize(msSend, ctftemp);
                                Sok.Send(msSend.GetBuffer(), (int)msSend.Length, SocketFlags.None);                                

                                BeginInvoke(m_con, "Player " + tTemp.TankName + " Added");
                                BeginInvoke(m_Log, "Player Count: " + m_AllPlayerStats.Count);
                            }
                            #endregion

                            #region Client_Instruction

                            //stores and determines the client instruction for the tank that is received from the client
                            else if (oIncomingInstruction is cClientFrame)
                            {
                                lock (m_lkvpClientInstruction)
                                    m_lkvpClientInstruction.Add(new KeyValuePair<Socket, cClientFrame>(Sok, (cClientFrame)oIncomingInstruction));

                                bool[] bTemp = ((cClientFrame)oIncomingInstruction).GetAllInfo;
                                string sTemp = "";
                                // determines display for the log in the ServerForm UI
                                foreach (bool b in bTemp)
                                    sTemp += b ? "X," : "O,";
                                lock(m_AllPlayerStats)
                                    BeginInvoke(m_Log, "Command recieved from " + m_AllPlayerStats[Sok].tPlayerTank.TankName 
                                        + "ID: " + m_AllPlayerStats[Sok].iAssignedIndex + " Command: " + sTemp);
                            }
                            #endregion

                            #region Client_Weapon_Toggle

                            // determines the weapon toggle, figuring out which weapon type to change.
                            else if (oIncomingInstruction is cClientToggle)
                            {
                                bool bP = ((cClientToggle)oIncomingInstruction).bPrimary,
                                     bS = ((cClientToggle)oIncomingInstruction).bSecondary,
                                     bSp = ((cClientToggle)oIncomingInstruction).bSpecial;
                                TankInformation Tank;
                                lock (m_AllPlayerStats)
                                    Tank = m_AllPlayerStats[Sok];
                                UpdateToggleSwitches(new KeyValuePair<Socket,TankInformation>(Sok, Tank), bP, bS, bSp);
                            }
                            #endregion
                            //determines if a tank is destroyed and starts the respawn process    
                            else if (oIncomingInstruction is cRespawn)
                            {
                                MemoryStream msSend = new MemoryStream();

                                TankInformation ti;
                                lock(m_AllPlayerStats)
                                    ti = m_AllPlayerStats[Sok];
                                cClientConnect ccTemp = new cClientConnect(ti.tPlayerTank.TankName,
                                                                            WhatTank.TankConvertTypeToByte(ti.tPlayerTank) -1);
                                cTank tTemp = SpawnNewTank(ccTemp);
                                tTemp.DeathCount = ti.tPlayerTank.DeathCount;
                                tTemp.KillCount = ti.tPlayerTank.KillCount;
                                tTemp.KillStreak = 0;

                                lock (m_AllPlayerStats)
                                    m_AllPlayerStats[Sok] = new TankInformation(tTemp,
                                                                                ti.cPlayerInsructions,
                                                                                ti.iAssignedIndex,
                                                                                ti.iPrimaryAmmoType,
                                                                                ti.iSecondaryAmmoType, 
                                                                                ti.iSpecialAmmoType);
                                cInitialFrame cTemp = new cInitialFrame();
                                cTemp.bmTerrainMap = null;
                                cTemp.iIndex = -1;
                                cTemp.loObstacles = null;

                                // once packages are built send to clients
                                bf.Serialize(msSend, cTemp);
                                Sok.Send(msSend.GetBuffer(), (int)msSend.Length, SocketFlags.None);
                            }

                            m_iFramesReceived += 1;
                            
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
                Sok.Shutdown(SocketShutdown.Both);
                lock (m_AllPlayerStats)                   
                    m_AllPlayerStats.Remove(Sok);
            }
        }
        #endregion

        /// <summary>
        /// Dictates the respawn of of every tank for the beginning of the game
        /// as well as after a death.
        /// </summary>
        /// <param name="cIncomingConnect"></param>
        /// <returns></returns>
        private cTank SpawnNewTank(cClientConnect cIncomingConnect)
        {
            Random rnd = new Random();
            cTank tTemp = null;
            // create new tank of derived class indicated by user
            PointF pSpawnPoint = new PointF(0, 0);
            bool bSafe = false;

            // ensure that no tank respawns inside of an object or another tank
            while (!bSafe)
            {
                bSafe = true;
                pSpawnPoint = new PointF(rnd.Next(260, 2240), rnd.Next(260, 2240));
                foreach (cObstacle cO in m_loObstacles)
                    if (GetDistance(pSpawnPoint, cO.Location) <= 90)
                        bSafe = false;

                foreach (TankInformation ti in m_AllPlayerStats.Values)
                    if (GetDistance(pSpawnPoint, ti.tPlayerTank.CurrentTankPosition) <= 200)
                        bSafe = false;

            }

            //determines tank type dependant upon the idex that is passed in
            switch (cIncomingConnect.TankIndex)
            {
                case 0:
                    tTemp = new cTankType1(pSpawnPoint, rnd.Next(0,90));
                    break;

                case 1:
                    tTemp = new cTankType2(pSpawnPoint, rnd.Next(0,90));
                    break;

                case 2:
                    tTemp = new cTankType3(pSpawnPoint, rnd.Next(0, 90));
                    break;

                case 3:
                    tTemp = new cTankType4(pSpawnPoint, rnd.Next(0, 90));
                    break;

                default:
                    tTemp = new cTankType1(pSpawnPoint, rnd.Next(0, 90));
                    break;
            }
            // Set Name of player tank
            tTemp.TankName = cIncomingConnect.Name;

            return tTemp;
        }

        #region Send_Code_For_Game_Update
        /// <summary>
        /// Used to update and reload all game information every 25 ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TIM_Refresh_Tick(object sender, EventArgs e)
        {
            // recieve and update new tank instructions
            UpdateClientInstructions();

            // move tanks accordingly in regards to current angle and position
            UpdateTanksFromInstructions();

            // move bullets accordingly in regards to current angle and position
            UpdateBulletFromInstructions();

            UpdateBulletContactsWithObstacles();

            // tick all special items for arming functionality or disabling
            foreach (cSpecialItem cS in m_lcSpecial)
                cS.TickDuration();

            try
            {
                lock (m_AllPlayerStats)
                {
                    foreach (KeyValuePair<Socket, TankInformation> TI in m_AllPlayerStats)
                        UpdateBulletContactsWithTanks(TI.Key, TI.Value);
                }

                lock (m_AllPlayerStats)
                {
                    foreach (KeyValuePair<Socket, TankInformation> TI in m_AllPlayerStats)
                        UpdateBulletContactsWithTanks(TI.Key, TI.Value);
                }

                lock (m_AllPlayerStats)
                {
                    foreach (KeyValuePair<Socket, TankInformation> TI in m_AllPlayerStats)
                        UpdateSpecialContactsWithTanks(TI);
                }
                lock (m_AllPlayerStats)
                {
                    foreach (KeyValuePair<Socket, TankInformation> TI in m_AllPlayerStats)
                        UpdateSpecialContactsWithTanks(TI);
                }
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Collection issue");
            }

            // move bullets along current route
            foreach (cAmmo cA in m_lcAmmo)
                cA.MoveBullet();

            m_lcAmmo.RemoveAll(RemoveContactedBullets);
            m_lcSpecial.RemoveAll(RemoveContactedSpecialItems);

            // player stat additions will go here as well as player objectives
            lock (m_AllPlayerStats)
                SendToClient(new List<KeyValuePair<Socket, TankInformation>>(m_AllPlayerStats));

            //********************************************************************\\

        }
        #endregion

        #region Support_Functions
        #region Removal_Predicates
        private bool RemoveContactedBullets(cAmmo Bullet)
        {
            if (Bullet.Hit)
                return true;
            return false;
        }

        private bool RemoveContactedTanks(cTank Tank)
        {
            if (Tank.Hit)
                return true;
            return false;
        }

        private bool RemoveContactedSpecialItems(cSpecialItem Item)
        {
            if (Item.Hit)
                return true;
            return false;
        }

        #endregion

        /// <summary>
        /// Generates a obstacle border along the outside of the map by creating a series of
        /// TankStoppers, placed 50px apart offset 250 pixels towards center
        /// Returns a list of obstacles
        /// </summary>
        /// <returns>List of obstacles</returns>
        private List<cObstacle> CreateObstacleInitials()
        {
            List<cObstacle> loOutput = new List<cObstacle>();
            Random rnd = new Random();
            for (int i = 230; i < 2276; i += 30)
            {
                loOutput.Add(new TankStoppers(new PointF(i, 230), rnd.Next(0, 90)));
                loOutput.Add(new TankStoppers(new PointF(i, 2275), rnd.Next(0, 90)));
                loOutput.Add(new TankStoppers(new PointF(235, i), rnd.Next(0, 90)));
                loOutput.Add(new TankStoppers(new PointF(2275, i), rnd.Next(0, 90)));
            }

            for (int i = 0; i < 30; i++)
                loOutput.Add(new Walls(CheckForOverlap(loOutput), rnd.Next(0, 4) * 90));

            for (int i = 0; i < 70; i++)
                loOutput.Add(new Trees(CheckForOverlap(loOutput), rnd.Next(0, 180)));

            return loOutput;
        }

        /// <summary>
        /// Ensures that rendered obstacles are not rendered over top of each other
        /// </summary>
        /// <param name="loCurrent"></param>
        /// <returns></returns>
        private PointF CheckForOverlap(List<cObstacle> loCurrent)
        {
            Random rRandomOffset = new Random();
            bool bTemp = false;
            PointF pTemp = new PointF(0, 0);
            do
            {
                bTemp = false;
                pTemp = new PointF(rRandomOffset.Next(0, 50) * 50, rRandomOffset.Next(0, 50) * 50);
                foreach (cObstacle ob in loCurrent)
                    if (GetDistance(ob.Location, pTemp) <= 120)
                        bTemp = true;
            }
            while (bTemp);

            return pTemp;
        }

        /// <summary>
        /// Updates all client instructions based off of the instructions in the recieving queue
        /// the last instructions recieved from client will be used (currently, will probably change)
        /// clears the instruction queue upon finishing updates
        /// </summary>
        private void UpdateClientInstructions()
        {
            if (m_lkvpClientInstruction.Count > 0)
            {
                lock (m_lkvpClientInstruction)
                {
                    // for all instructions found in client frame update the current tank values
                    foreach (KeyValuePair<Socket, cClientFrame> kvpTemp in m_lkvpClientInstruction)
                    {
                        lock (m_AllPlayerStats)
                        {
                            // unpack
                            TankInformation tTemp;
                            if (m_AllPlayerStats.TryGetValue(kvpTemp.Key, out tTemp))
                            {
                                tTemp.cPlayerInsructions = kvpTemp.Value;
                                //repack
                                m_AllPlayerStats[kvpTemp.Key] = tTemp;
                            }
                        }
                    }
                    m_lkvpClientInstruction.Clear();
                }
            }
        }

        /// <summary>
        /// Updates all tank related changes based off of the current recieved instructions 
        /// relies on instruction set of 9 bools, uses first 6, console message on failure
        /// </summary>
        private void UpdateTanksFromInstructions()
        {
            List<KeyValuePair<Socket, TankInformation>> lkvpTemp;
            lock (m_AllPlayerStats)
                lkvpTemp = new List<KeyValuePair<Socket, TankInformation>>(m_AllPlayerStats);

            foreach (KeyValuePair<Socket, TankInformation> kvpPlayer in lkvpTemp)
            {
                // unpack for updating
                TankInformation tTemp = kvpPlayer.Value;    // point to tank item
                    
                bool[] bInstructions = tTemp.cPlayerInsructions.GetAllInfo;    // 0:up, 1:down, 2: left, 3: right
           
                // if instructions are valid
                if (bInstructions != null)
                {
                    #region Check_Left_Turn
                    if (bInstructions[2])       // left
                    {
                        cTank cTemp;
                        float fTemp = 1;
                        do
                        {
                            cTemp = TankClone.DeepCopy(tTemp.tPlayerTank);           // make a clone of the tank object
                            cTemp.TankTurn(-fTemp);
             
                            if (CheckMoveForTankValidation(cTemp) && CheckMoveForTankContact(cTemp, kvpPlayer.Key))
                                break;
                            else
                                fTemp -= 0.2f;
                        }
                        while (fTemp >= 0.1f);
                        if (fTemp >= 0.1f)
                            tTemp.tPlayerTank = cTemp;
                    }
                    #endregion

                    #region Check_Right_Turn
                    if (bInstructions[3])       // right
                    {
                        cTank cTemp;
                        float fTemp = 1;
                        do
                        {
                            cTemp = TankClone.DeepCopy(tTemp.tPlayerTank);           // make a clone of the tank object
                            cTemp.TankTurn(fTemp);
                            if (CheckMoveForTankValidation(cTemp) && CheckMoveForTankContact(cTemp, kvpPlayer.Key))
                                break;
                            else
                                fTemp -= 0.2f;
                        }
                        while (fTemp >= 0.2f);
                        if (fTemp >= 0.1)
                            tTemp.tPlayerTank = cTemp;
                    }
                    #endregion

                    #region Check_Forward_Move
                    if (bInstructions[0])       // forwards
                    {
                        cTank cTemp;
                        float fTemp = 1;
                        do
                        {
                            cTemp = TankClone.DeepCopy(tTemp.tPlayerTank);           // make a clone of the tank object
                            cTemp.MoveTank(fTemp);
                            bool bObstacle = CheckMoveForTankValidation(cTemp);
                            bool bTank = CheckMoveForTankContact(cTemp, kvpPlayer.Key);
                            if (bObstacle && bTank)
                                break;
                            else
                                fTemp -= 0.2f;
                        }
                        while (fTemp >= 0.2f);
                        if (fTemp >= 0.1f)
                            tTemp.tPlayerTank = cTemp;
                    }
                    #endregion

                    #region Check_Backwards_Move
                    if (bInstructions[1])       // backwards 
                    {
                        cTank cTemp;
                        float fTemp = 1;
                        do
                        {
                            cTemp = TankClone.DeepCopy(tTemp.tPlayerTank);           // make a clone of the tank object
                            cTemp.MoveTank(-fTemp);
                            if (CheckMoveForTankValidation(cTemp) && CheckMoveForTankContact(cTemp, kvpPlayer.Key))
                                break;
                            else
                                fTemp -= 0.2f;
                        }
                        while (fTemp >= 0.1f);
                        if (fTemp >= 0.1f)
                            tTemp.tPlayerTank = cTemp;
                    }
                    #endregion

                    #region Check_Turret_Right
                    if (bInstructions[4])
                    {
                        cTank cTemp;
                        float fTemp = 1;
                        do
                        {
                            cTemp = TankClone.DeepCopy(tTemp.tPlayerTank);           // make a clone of the tank object
                            cTemp.TurretTurn(-fTemp);
                            if (CheckMoveForTurretValidation(cTemp))
                                break;
                            else
                                fTemp -= 0.2f;
                        }
                        while (fTemp >= 0.1f);
                        if (fTemp >= 0.1f)
                            tTemp.tPlayerTank = cTemp;
                    }
                    #endregion

                    #region Check_Turret_Left
                    if (bInstructions[5])
                    {
                        cTank cTemp;
                        float fTemp = 1;
                        do
                        {
                            cTemp = TankClone.DeepCopy(tTemp.tPlayerTank);           // make a clone of the tank object
                            cTemp.TurretTurn(fTemp);
                            if (CheckMoveForTurretValidation(cTemp))
                                break;
                            else
                                fTemp -= 0.2f;
                        }
                        while (fTemp >= 0.1f);
                        if (fTemp >= 0.1f)
                            tTemp.tPlayerTank = cTemp;
                    }
                    #endregion

                    lock (m_AllPlayerStats)
                        m_AllPlayerStats[kvpPlayer.Key] = tTemp;
                }
                else
                    Console.WriteLine("failed to read tank movement instructions");
            }
        }

        /// <summary>
        /// Compares tank turret to all obstacles on map to see if a region conflict occurs 
        /// if a region crosses over the tank, return false, else return true
        /// </summary>
        /// <param name="Tank">tank to compare objects to</param>
        /// <returns>bool collision condition</returns>
        private bool CheckMoveForTurretValidation(cTank Tank)
        {
            using (Graphics gr = CreateGraphics())
            {
                // get tank region
                Region RegA = Tank.GetTurretRegion();
                foreach (cObstacle sO in m_loObstacles)
                {
                    // if the tank center is within 40 pixels of obstacle center
                    if (GetDistance(sO.Location, Tank.CurrentTankPosition) < 100)
                    {
                        Region RegB = sO.GetRegion();   // create obstacle region for comparision
                        RegB.Intersect(RegA);
                        // if comparision returns an intersection return false
                        if (!RegB.IsEmpty(gr))
                            return false;
                    }
                }
                return true;
            }     
        }

        /// <summary>
        /// Compares tank turret to all obstacles on map to see if a region conflict occurs 
        /// if a region crosses over the tank, return false, else return true
        /// </summary>
        /// <param name="Tank">tank to compare objects to</param>
        /// <returns>bool collision condition</returns>
        private bool CheckMoveForTankValidation(cTank Tank)
        {
            using (Graphics gr = CreateGraphics())
            {
                // get tank region
                Region RegA = Tank.GetRegion();
                foreach (cObstacle sO in m_loObstacles)
                {
                    // if the tank center is within 40 pixels of obstacle center
                    if (GetDistance(sO.Location, Tank.CurrentTankPosition) < 100)
                    {
                       // Region RegC = RegA.Clone();     // create region clone for A
                        Region RegB = sO.GetRegion();   // create obstacle region for comparision
                        RegB.Intersect(RegA);
                        // if comparision returns an intersection return false
                        if (!RegB.IsEmpty(gr))
                            return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Region check to determine if tank currently occupying area
        /// if so return false, if not return true
        /// </summary>
        /// <param name="Tank">Tank to compare</param>
        /// <param name="mySocket">Identity of tank to compare</param>
        /// <returns>bool condition on contact</returns>
        private bool CheckMoveForTankContact(cTank Tank, Socket mySocket)
        {
            using (Graphics gr = CreateGraphics())
            {
                // get tank region
                Region RegA = Tank.GetRegion();
                // create a temporary list of all items required
                List<KeyValuePair<Socket, TankInformation>> lkvpTemp;
                lock (m_AllPlayerStats)
                    lkvpTemp = new List<KeyValuePair<Socket, TankInformation>>(m_AllPlayerStats);

                // check all tanks against main pass in for region overlap
                foreach (KeyValuePair<Socket, TankInformation> kvpTemp in lkvpTemp)
                {
                    cTank cT = kvpTemp.Value.tPlayerTank;
                    if (!mySocket.Equals(kvpTemp.Key))  // if not self
                    {
                        // if the tank center is within 40 pixels of obstacle center
                        if (GetDistance(cT.CurrentTankPosition, Tank.CurrentTankPosition) < 80)
                        {
                           // Region RegC = RegA.Clone();                 // create region clone for A
                            Region RegB = cT.GetRegion();   // create obstacle region for comparision
                            RegB.Intersect(RegA);
                            // if comparision returns an intersection return false
                            if (!RegB.IsEmpty(gr))
                                return false;
                        }
                    }
                }
                return true;
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
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// Updates all bullets fired by checking to see if a neccessary amount of time has elapsed
        /// If a shot is fired the delay timer is reset.
        /// </summary>
        private void UpdateBulletFromInstructions()
        {
            List<KeyValuePair<Socket, TankInformation>> lkvpTemp;
            lock (m_AllPlayerStats)
                lkvpTemp = new List<KeyValuePair<Socket, TankInformation>>(m_AllPlayerStats);

            foreach (KeyValuePair<Socket, TankInformation> kvpPlayer in lkvpTemp)
            {
                // unpack for updating
                TankInformation tTemp = kvpPlayer.Value;

                bool[] bInstructions = tTemp.cPlayerInsructions.GetAllInfo;    // 6: Heavy, 7: Light, 8: Special

                #region Fast_Ammo_Instructions
                // if player fires fast ammo type add to ammo list
                if (bInstructions[7])
                {
                    // based on the FireDelay rate of the FastAmmo, will determine how fast the player can fire the ammo type
                    if (tTemp.tPlayerTank.FastFireRate <= 0)
                    {
                        cFastAmmo cfa = null;
                        if (tTemp.iSecondaryAmmoType == 0)
                            cfa = new cMachineGun(tTemp.tPlayerTank.CurrentTurretAngle,
                                                    AmmoOffset.ApplyTankOffset(tTemp.tPlayerTank.CurrentTankPosition,
                                                                               tTemp.tPlayerTank.CurrentTurretAngle,
                                                                               18));
                        if (tTemp.iSecondaryAmmoType >= 1)
                            cfa = new cFlameGun(tTemp.tPlayerTank.CurrentTurretAngle,
                                                    AmmoOffset.ApplyTankOffset(tTemp.tPlayerTank.CurrentTankPosition,
                                                                               tTemp.tPlayerTank.CurrentTurretAngle,
                                                                               18));

                        // force bullets to accurately track with tanks if in motion
                        if (bInstructions[0])
                            cfa.SpeedOffset = tTemp.tPlayerTank.Speed;
                        if (bInstructions[1])
                            cfa.SpeedOffset = -1 * tTemp.tPlayerTank.Speed;

                        cfa.FiredByIndex = tTemp.iAssignedIndex;    // assign shooter

                        m_lcAmmo.Add(cfa);
                        tTemp.tPlayerTank.FastFireRate = cfa.FireDelayRate;     // reset timer delay
                    }
                }
                #endregion

                #region Slow_Ammo_Instructions
                // if player fires slow ammo type add to ammo list
                if (bInstructions[6])
                {
                    // based on the FireDelay rate of the SlowAmmo, will determine how fast the player can fire the ammo type
                    if (tTemp.tPlayerTank.SlowReloadSpeed <= 0)
                    {
                        cSlowAmmo csa = null;
                        if (tTemp.iPrimaryAmmoType == 0)
                                csa = new cShell(tTemp.tPlayerTank.CurrentTurretAngle,
                                                        AmmoOffset.ApplyTankOffset(tTemp.tPlayerTank.CurrentTankPosition,
                                                                                   tTemp.tPlayerTank.CurrentTurretAngle,
                                                                                   22));
                        if (tTemp.iPrimaryAmmoType >= 1)
                                csa = new cHeavyShell(tTemp.tPlayerTank.CurrentTurretAngle,
                                                        AmmoOffset.ApplyTankOffset(tTemp.tPlayerTank.CurrentTankPosition,
                                                                                   tTemp.tPlayerTank.CurrentTurretAngle,
                                                                                   22));

                        // force bullets to accurately track with tanks if in motion
                        if (bInstructions[0])
                            csa.SpeedOffset = tTemp.tPlayerTank.Speed;
                        if (bInstructions[1])
                            csa.SpeedOffset = -1 * tTemp.tPlayerTank.Speed;

                        csa.FiredByIndex = tTemp.iAssignedIndex;    // assign shooter

                        m_lcAmmo.Add(csa);
                        tTemp.tPlayerTank.SlowReloadSpeed = csa.FireDelayRate;  // reset timer delay  
                        tTemp.tPlayerTank.SlowTimeout = csa.FireDelayRate;
                    }
                }
                #endregion

                #region Special_Ammo_Instructions
                // if player fires special item type add to items list
                if (bInstructions[8])
                {
                    // based on the FireDelay rate of the specialAmmo, will determine how fast the player can fire the ammo type
                    if (tTemp.tPlayerTank.SpecialFireRate <= 0)
                    {
                        cSpecialItem csi = null;
                        if (tTemp.iSpecialAmmoType == 0)
                        {
                            csi = new cOilSlick(tTemp.tPlayerTank.CurrentTurretAngle,
                                                    tTemp.tPlayerTank.CurrentTankPosition);
                            tTemp.tPlayerTank.SpecialFireRate = 100;
                            tTemp.tPlayerTank.SpecialTimeout = 100;
                        }

                        else if (tTemp.iSpecialAmmoType == 1)
                        {
                            csi = new cLandMine(tTemp.tPlayerTank.CurrentTurretAngle,
                                                    tTemp.tPlayerTank.CurrentTankPosition, false);
                            tTemp.tPlayerTank.SpecialFireRate = 750;
                            tTemp.tPlayerTank.SpecialTimeout = 750;
                        }
                        csi.FiredByIndex = tTemp.iAssignedIndex;
                        m_lcSpecial.Add(csi);
                    }
                #endregion
                }

                // decrement both of the tank shot timers
                tTemp.tPlayerTank.FastFireRate--;
                tTemp.tPlayerTank.SlowReloadSpeed--;
                tTemp.tPlayerTank.SpecialFireRate--;
            }
        }

        private void UpdateToggleSwitches(KeyValuePair<Socket,TankInformation> stiTank, bool bPrimary, bool bSecondary, bool bSpecial)
        {
            TankInformation tiTank = stiTank.Value;
            // unpack for updating
            bool[] bInstructions = tiTank.cPlayerInsructions.GetAllInfo;    // 4: Turret Left, 5: turret Right

            // if instructions are valid
            if (bInstructions != null)
            {
                if (bPrimary)
                    tiTank.iPrimaryAmmoType = (tiTank.iPrimaryAmmoType + 1) % 2;    

                if (bSecondary)
                    tiTank.iSecondaryAmmoType = (tiTank.iSecondaryAmmoType + 1) % 2;

                if (bSpecial)
                    tiTank.iSpecialAmmoType = (tiTank.iSpecialAmmoType + 1) % 2;

                lock (m_AllPlayerStats)
                    m_AllPlayerStats[stiTank.Key] = tiTank;
            }
            else
                Console.WriteLine("failed to read turret instructions");
        }

        /// <summary>
        /// Compares all tanks with bullets currently floating on map,
        /// if an intersection occurs the bullet is removed and tank life is updated
        /// </summary>
        /// <param name="Tank">tank to compare against</param>
        private void UpdateBulletContactsWithTanks(Socket sSok, TankInformation Tank)
        {
            using (Graphics gr = CreateGraphics())
            {
                Region RegA = Tank.tPlayerTank.GetRegion();
                foreach (cAmmo Ammo in m_lcAmmo)
                {
                    if (GetDistance(Ammo.AmmoPosition, Tank.tPlayerTank.CurrentTankPosition) < 50
                        && Ammo.FiredByIndex != Tank.iAssignedIndex)
                    {
                        Region RegB = null;

                        if (Ammo is cFastAmmo)
                            RegB = ((cFastAmmo)Ammo).GetRegion();      // create obstacle region for comparision

                        else if (Ammo is cSlowAmmo)
                            RegB = ((cSlowAmmo)Ammo).GetRegion();           // create obstacle region for comparision

                        RegB.Intersect(RegA);
                        // if comparision returns an intersection return false
                        if (!RegB.IsEmpty(gr))
                        {
                            Ammo.Hit = true;
                            // if tank has not yet been destroyed
                            if (!Tank.tPlayerTank.Hit)
                            {
                                Tank.tPlayerTank.TankHit(Ammo.DamageAmount);    // add to damage

                                // if tank is dead after damage taken
                                if (Tank.tPlayerTank.Hit)
                                {
                                    Tank.tPlayerTank.DeathCount += 1;        // increase death amount  

                                    Socket soTemp = FindFiredByIndex(Ammo.FiredByIndex);
                                    string sKilledBy;
                                    lock (m_AllPlayerStats)
                                    {
                                        m_AllPlayerStats[soTemp].tPlayerTank.KillCount += 1;
                                        m_AllPlayerStats[soTemp].tPlayerTank.KillStreak += 1;
                                        sKilledBy = m_AllPlayerStats[soTemp].tPlayerTank.TankName;
                                    }

                                    // send death frame to particular client
                                    SendDeathMessage(sSok, sKilledBy);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares all obstacles with bullets currently floating on map,
        /// if an intersection occurs the bullet is removed
        /// </summary>
        /// <param name="Tank">tank to compare against</param>
        private void UpdateBulletContactsWithObstacles()
        {
            using (Graphics gr = CreateGraphics())
            {
                foreach (cObstacle Obstacle in m_loObstacles)
                {
                    Region RegA = Obstacle.GetRegion();
                    foreach (cAmmo Ammo in m_lcAmmo)
                    {
                        if (GetDistance(Ammo.AmmoPosition, Obstacle.Location) < 100)
                        {
                            Region RegB = null;

                            if (Ammo is cFastAmmo)
                                RegB = ((cFastAmmo)Ammo).GetRegion();      // create obstacle region for comparision
                            else if (Ammo is cSlowAmmo)
                                RegB = ((cSlowAmmo)Ammo).GetRegion();           // create obstacle region for comparision

                            RegB.Intersect(RegA);
                            // if comparision returns an intersection return false
                            if (!RegB.IsEmpty(gr))
                                Ammo.Hit = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares all tanks with SpecialItems currently floating on map,
        /// if an intersection occurs the SpecialItem is removed or timer expires and tank life is updated
        /// </summary>
        /// <param name="Tank">tank to compare against</param>
        private void UpdateSpecialContactsWithTanks(KeyValuePair<Socket,TankInformation> Tank)
        {
            using (Graphics gr = CreateGraphics())
            {
                Random rnd = new Random();
                Region RegA = Tank.Value.tPlayerTank.GetRegion();
                foreach (cSpecialItem Item in m_lcSpecial)
                {
                    if (GetDistance(Item.SpecialPosition, Tank.Value.tPlayerTank.CurrentTankPosition) < 50)
                    {
                        Region RegB = null;

                        RegB = Item.GetRegion();      // create obstacle region for comparision
                        RegB.Intersect(RegA);
                        // if comparision returns an intersection return false
                        if (!RegB.IsEmpty(gr) && Item.ArmStatus)
                        {
                            Region RegD = RegA.Clone();     // create region clone for A

                            if (Item is cLandMine)
                            {
                                Item.Hit = true;
                                ExplosionRadiusContact(Tank.Key, (cLandMine)Item);
                            }
                            else if (Item is cOilSlick)
                            {
                                Tank.Value.tPlayerTank.TankTurn(rnd.Next(-4, 5));
                            }

                            DamageTank(Tank, Item.DamageAmount, Item.FiredByIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Does check against tank on landmine blastradius for explosive properties
        /// Used to give all non-detenator tanks in blast radius damage
        /// </summary>
        /// <param name="sSok">Tank who detonated bomb</param>
        /// <param name="cLM">Landmine Detonated</param>
        private void ExplosionRadiusContact(Socket sSok, cLandMine cLM)
        {
            using (Graphics gr = CreateGraphics())
            {
                Region RegA = cLM.GetExplosionRegion();      // create obstacle region for comparision

                lock (m_AllPlayerStats)
                {
                    foreach (KeyValuePair<Socket, TankInformation> kvpTemp in m_AllPlayerStats)
                    {
                        if (!(sSok.Equals(kvpTemp.Key)))
                        {
                            if (GetDistance(cLM.SpecialPosition, kvpTemp.Value.tPlayerTank.CurrentTankPosition) < 80)
                            {
                                Region RegB = kvpTemp.Value.tPlayerTank.GetRegion();
                                RegB.Intersect(RegA);
                                if (!RegB.IsEmpty(gr))
                                {
                                    DamageTank(kvpTemp, cLM.DamageAmount / 2, cLM.FiredByIndex);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines which player has shot the bullet that killed another tank. Updates their total kills, kill streak
        /// and the name of who killed them
        /// </summary>
        /// <param name="Tank">tank to compare against</param>
        /// <param name="DamageAmount">value of the amount of damage done by bullet</param>
        /// <param name="FiredByIndex">index of which player shot the bullet</param>
        private void DamageTank(KeyValuePair<Socket,TankInformation> Tank, int DamageAmount, int FiredByIndex)
        {
            // if tank has not yet been destroyed
            if (!Tank.Value.tPlayerTank.Hit)
            {
                Tank.Value.tPlayerTank.TankHit(DamageAmount);    // add to damage

                // if tank is dead after damage taken
                if (Tank.Value.tPlayerTank.Hit)
                {
                    Tank.Value.tPlayerTank.DeathCount += 1;        // increase death amount  

                    Socket soTemp = FindFiredByIndex(FiredByIndex);
                    string sKilledBy;
                    lock (m_AllPlayerStats)
                    {
                        if (!Tank.Key.Equals(soTemp))
                        {
                            m_AllPlayerStats[soTemp].tPlayerTank.KillCount += 1;
                            m_AllPlayerStats[soTemp].tPlayerTank.KillStreak += 1;
                            sKilledBy = m_AllPlayerStats[soTemp].tPlayerTank.TankName;
                        }
                        sKilledBy = "self. Just sad!";
                    }
                    
                    // send death frame to particular client
                    SendDeathMessage(Tank.Key, sKilledBy);
                }
            }
        }

        /// <summary>
        /// Used to get get the index of the player who shot the bullet that killed a player
        /// Adds one kill to their score upon completion
        /// </summary>
        /// <param name="iFiredBy">index of fired bullet</param>
        /// <returns>string name of offender</returns>
        private Socket FindFiredByIndex(int iFiredBy)
        {
            lock(m_AllPlayerStats)
                foreach (KeyValuePair<Socket, TankInformation> ti in m_AllPlayerStats)
                {
                    if (ti.Value.iAssignedIndex.Equals(iFiredBy))
                        return ti.Key;
                }
            return null;
        }

        /// <summary>
        /// Builds a stat array to be sent to client
        /// aquires stats from all tanks currently involved in game
        /// </summary>
        /// <param name="sSok">Socket of main player</param>
        /// <returns></returns>
        private Stats[] AssembleStats(Socket sSok)
        {
            Stats[] sSending = new Stats[m_AllPlayerStats.Count];

            int iHolder = 0;
            foreach (TankInformation kvp in m_AllPlayerStats.Values)
            {
                // bundle up the stats
                sSending[iHolder] = new Stats(kvp.tPlayerTank.DeathCount,
                                              kvp.tPlayerTank.KillCount,
                                              kvp.tPlayerTank.KillStreak,
                                              kvp.tPlayerTank.TankName);
                iHolder++;
            }

            return sSending;
        }

        /// <summary>
        /// Used to send a frame back to the client indicating that they were killed, 
        /// included inside are all the information around current game standings
        /// </summary>
        /// <param name="sSend">Socket for who the packet is meant for</param>
        /// <param name="sKilledMe">string name of who killed said person</param>
        private void SendDeathMessage(Socket sSend, string sKilledMe)   
        {
            MemoryStream mssend = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            cDeathFrame cdfTemp = new cDeathFrame();

            cdfTemp.sCurrentStandings = AssembleStats(sSend);   // aquire current standings
            cdfTemp.sKilledBy = sKilledMe;
            bf.Serialize(mssend, cdfTemp);

            sSend.Send(mssend.GetBuffer(), (int)mssend.Length, SocketFlags.None);
        }

        /// <summary>
        /// determines the reload time to be sent to the client
        /// </summary>
        /// <param name="tTank"> curent tank information</param>
        /// <returns>frame to be sent to client on shot count</returns>
        private cShotCounts SendShotReloadTime(TankInformation tTank)
        {
            cShotCounts cSc = 
                new cShotCounts(
                    tTank.tPlayerTank.SlowReloadSpeed > 0 ? 
                    tTank.tPlayerTank.SlowReloadSpeed : 0,
                    tTank.tPlayerTank.SpecialFireRate > 0 ?
                    tTank.tPlayerTank.SpecialFireRate : 0,
                    tTank.tPlayerTank.SlowTimeout,
                    tTank.tPlayerTank.SpecialTimeout);

            return cSc;
        }

        /// <summary>
        /// Packages up all current tank information of all tanks for a particular socket
        /// distributed to clients for rendering and other
        /// Always places the current player (packet reciever) at the begining of the array
        /// </summary>
        /// <param name="sMySocket">Current player socket</param>
        /// <returns>List of all tank information</returns>
        private List<KeyValuePair<int, sTank>> PackageUpTankInfoForSend(List<KeyValuePair<Socket,TankInformation>> lkvpInput)
        {
            List<KeyValuePair<int, sTank>> sTankInfoOut = new List<KeyValuePair<int, sTank>>();

            // package up tank information with primary packet collecter as [0]
            foreach (KeyValuePair<Socket, TankInformation> kvpPlayer in lkvpInput)
            {
                TankInformation tTemp = kvpPlayer.Value;
                // convert all neccessary tank info for transfer
                sTank sTankInfo = new sTank();
                sTankInfo.iTankId = WhatTank.TankConvertTypeToByte(tTemp.tPlayerTank);
                sTankInfo.iLife = tTemp.tPlayerTank.Life;
                sTankInfo.bHit = tTemp.tPlayerTank.Hit;   
                sTankInfo.fTurretRotation = tTemp.tPlayerTank.CurrentTurretAngle;
                sTankInfo.fTankRotation = tTemp.tPlayerTank.CurrentTankAngle;
                sTankInfo.pLocation = tTemp.tPlayerTank.CurrentTankPosition;

                // add to ship list
                sTankInfoOut.Add(new KeyValuePair<int, sTank>(tTemp.iAssignedIndex,sTankInfo));
            }
            return sTankInfoOut;
        }

        #endregion

        #region Send_Packet_Functions
        /// <summary>
        /// All the tank information will get ready to be packaged up and sent to the client
        /// </summary>
        /// <param name="AllTankInfo"></param>
        private void SendToClient(List<KeyValuePair<Socket,TankInformation>> AllTankInfo)
        {
            BinaryFormatter bf = new BinaryFormatter();
            cServerFrame csfTemp = new cServerFrame();

            List<KeyValuePair<int, sTank>> lkvpOutput = PackageUpTankInfoForSend(AllTankInfo);
            csfTemp.TanksInPlay = lkvpOutput;

            // convert list to smaller format size (compress) and add to out packet
            List<KeyValuePair<byte, sAmmo>> lkvpTemp = new List<KeyValuePair<byte, sAmmo>>();
            foreach (cAmmo cTemp in m_lcAmmo)
                    lkvpTemp.Add(new KeyValuePair<byte, sAmmo>(WhatAmmo.AmmoConvertToByte(cTemp),
                                    new sAmmo(cTemp.AmmoPosition, cTemp.AmmoRotation)));
            csfTemp.AmmoPoints = lkvpTemp;

            // convert list to smaller format size (compress) and add to out packet
            List<KeyValuePair<byte, sSpecial>> lkvpTemp2 = new List<KeyValuePair<byte, sSpecial>>();
            foreach (cSpecialItem cTemp in m_lcSpecial)
                lkvpTemp2.Add(new KeyValuePair<byte, sSpecial>(WhatAmmo.SpecialConvertToByte(cTemp),
                                new sSpecial(cTemp.SpecialPosition, cTemp.SpecialRotation, cTemp.ArmStatus)));
            csfTemp.SpecialItems = lkvpTemp2;
   
            List<Socket> lSockTemp = new List<Socket>();

            // collect socket list for send offs
            lock (m_AllPlayerStats)
            {
                foreach (KeyValuePair<Socket, TankInformation> kvpTemp in m_AllPlayerStats)
                {
                    MemoryStream mssend = new MemoryStream();
                    try
                    {
                        csfTemp.cShotInfo = SendShotReloadTime(kvpTemp.Value);

                        bf.Serialize(mssend, csfTemp);
                        if (kvpTemp.Key.Connected)
                            kvpTemp.Key.Send(mssend.GetBuffer(), (int)mssend.Length, SocketFlags.None);
                    }
                    catch (SocketException)
                    {
                        lSockTemp.Add(kvpTemp.Key);
                    }
                }
                foreach (Socket s in lSockTemp)
                    m_AllPlayerStats.Remove(s);
                lSockTemp.Clear();
            }
        }
        #endregion
    }
}
 