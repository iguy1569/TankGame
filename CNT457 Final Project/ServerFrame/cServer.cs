// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    ServerFrame
// Class File:  cServer.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach and Dan Allen
//
// Discription: Holds all server frame containers to send to client
// ///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using Obstacles;

namespace ServerFrame
{
    /// <summary>
    /// Standard server to client frame, sent to update map board
    /// </summary>
    [Serializable]
    public class cServerFrame
    {
        public List<KeyValuePair<int,sTank>> TanksInPlay;               // 4 slot max
        public List<KeyValuePair<byte, sAmmo>>  AmmoPoints;             // ammo currently fired
        public List<KeyValuePair<byte, sSpecial>>  SpecialItems;        // special ammo locations
        public cShotCounts cShotInfo;                                   // current shot timers
    }

    /// <summary>
    /// Ammo information required for rebuild on client
    /// </summary>
    [Serializable]
    public class sAmmo
    {
        public PointF pLocation;
        public float fRotation;
        
        public sAmmo(PointF pLoc, float fRot)
        {
            pLocation = pLoc;
            fRotation = fRot;
            
        }
    }

    /// <summary>
    /// Special information required for rebuild on client
    /// </summary>
    [Serializable]
    public class sSpecial
    {
        public PointF pLocation;    // current location
        public float fRotation;     // current rotation
        public bool bArmed;         // arm condition

        public sSpecial(PointF pLoc, float fRot, bool bArm)
        {
            pLocation = pLoc;
            fRotation = fRot;
            bArmed = bArm;
        }
    }

    /// <summary>
    /// Essential tank information to send to client
    /// </summary>
    [Serializable]
    public class sTank
    {
        public PointF pLocation;            // current location of tank            
        public float  fTurretRotation;      // current rotation of turret
        public float  fTankRotation;        // current tank angle
        public int    iLife;                // current life amount
        public int    iTankId;              // tank type
        public bool   bHit;                 // tank hit animation bool
    }

    /// <summary>
    /// Shot timers to send to client
    /// </summary>
    [Serializable]
    public class cShotCounts
    {
        // current states
        public int iSlowAmmoCurrent,
                   iSpecialAmmoCurrent;

        // shot maximums
        public int iSlowAmmoTimeout,
                   iSpecialAmmoTimeout;

        public cShotCounts(int iSAC, int iSpAC, int iSAT, int iSpAT)
        {
            iSlowAmmoCurrent = iSAC;
            iSpecialAmmoCurrent = iSpAC;
            iSlowAmmoTimeout = iSAT;
            iSpecialAmmoTimeout = iSpAT;
        }
    }

    /// <summary>
    /// Client intitializtion, including map
    /// </summary>
    [Serializable]
    public class cInitialFrame
    {
        public Bitmap bmTerrainMap;                             // terrain map for client render (one time send on initialization)
        public List<KeyValuePair<int, sObstacle>> loObstacles;  // list of all obstacles that need to be initiated
        public int iIndex;                                      // player assigned index
    }

    /// <summary>
    /// obstacle information required for rebuild on client
    /// </summary>
    [Serializable]
    public class sObstacle
    {
        public PointF pLocation;
        public float fRotation;
    }

    /// <summary>
    /// on death sent to client to change state
    /// </summary>
    [Serializable]
    public class cDeathFrame
    {
        public Stats[] sCurrentStandings;      // players current standings (scoreboard)
        public string sKilledBy;               // who killed me   

        /// <summary>
        /// Returns an array containing the statistics for all tanks in play.
        /// </summary>
        public Stats[] Stats
        {
            get { return sCurrentStandings; }
        }
    }

    /// <summary>
    /// Scoreboard information from a particular tank
    /// </summary>
    [Serializable]
    public class Stats
    {
        public int iKills,          // current kills
                   ideaths,         // current death
                   iKillStreak;     // current streak
        public string sTankName;    // name

        public Stats(int iK, int iD, int iKS, string sTN)
        {
            iKills = iK;
            ideaths = iD;
            iKillStreak = iKS;
            sTankName = sTN;
        }

        /// <summary>
        /// Returns the kill count of the tank
        /// </summary>
        public int Kills
        {
            get { return iKills; }
        }

        /// <summary>
        /// Returns the death count of the tank
        /// </summary>
        public int Deaths
        {
            get
            {
                return ideaths;
            }
        }

        /// <summary>
        /// Returns the player's name
        /// </summary>
        public string Name
        {
            get
            {
                return sTankName;
            }
        }
    }

    /// <summary>
    /// frame sent to client if connection refused
    /// </summary>
    [Serializable]
    public class NoConnection
    {
        public int iNoConnect = 0;
    }
}
