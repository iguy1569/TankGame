// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    ClientFrame
// Class File:  cClient.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach and Dan Allen
//
// Discription: Holds all cleint frame containers to send
// ///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientFrame
{
    [Serializable]
    public class cClientFrame
    {
        // Tank Movement
        protected bool _Up,
                       _Down,
                       _Left,
                       _Right;

        // Turret Movement
        protected bool _TurretLeft,
                       _TurretRight;

        // Fire States
        protected bool _FireHeavy,
                       _FireLight,
                       _FireSpecial;

        protected bool _TogglePrimary,
                       _ToggleSecondary,
                       _ToggleSpecial;

        /// <summary>
        /// Packet sent to the server with player control updates
        /// </summary>
        /// <param name="bUp">Tank forward condition</param>
        /// <param name="bDown">Tank backward condition</param>
        /// <param name="bLeft">Tank turn left condition</param>
        /// <param name="bRight">Tank turn right condition</param>
        /// <param name="bTurretLeft">Tank turret left rotation condition</param>
        /// <param name="bTurretRight">Tank turret right rotation condition</param>
        /// <param name="bFireHeavy">Tank fire heavy ammo condition</param>
        /// <param name="bFireLight">Tank fire light ammo condition</param>
        /// <param name="bFireSpecial">Tank fire special ammo condition</param>
        public cClientFrame(bool bUp, bool bDown, bool bLeft, bool bRight, bool bTurretLeft, bool bTurretRight,
                            bool bFireHeavy, bool bFireLight, bool bFireSpecial, 
                            bool bTogglePrime, bool bToggleSecond, bool bToggleThird)
        {
            _Up = bUp;
            _Down = bDown;
            _Left = bLeft;
            _Right = bRight;

            _TurretLeft = bTurretLeft;
            _TurretRight = bTurretRight;

            _FireHeavy = bFireHeavy;
            _FireLight = bFireLight;
            _FireSpecial = bFireSpecial;

            _TogglePrimary = bTogglePrime;
            _ToggleSecondary = bToggleSecond;
            _ToggleSpecial = bToggleThird;
        }

        /// <summary>
        /// A Secondary constructor for initializing a tank
        /// Feeds into default constructor all false bool values
        /// </summary>
        public cClientFrame()
            :this(false, false, false, false, false, false, false, false, false, false, false, false)
        {   }

        public bool[] GetAllInfo
        {
            get
            {
                return new bool[12] { _Up, _Down, _Left, _Right, 
                                     _TurretLeft, _TurretRight,
                                     _FireHeavy, _FireLight, _FireSpecial,
                                     _TogglePrimary, _ToggleSecondary, _ToggleSpecial};
            }
        }
    }

    [Serializable]
    public class cClientToggle
    {
        public bool bPrimary,
                    bSecondary,
                    bSpecial;
    }

    [Serializable]
    public class cRespawn
    {
        public int iTankType;
    }

    [Serializable]
    public class cClientConnect
    {
        // Client Name
        protected string _Name;

        //Client Tank Selection
        protected int _TankType;

        /// <summary>
        /// Initial packet sent to the server containing player name and index of selected tank.
        /// </summary>
        /// <param name="sName">Player Name</param>
        /// <param name="iTankType">Index of selected tank</param>
        public cClientConnect(string sName, int iTankType)
        {
            _Name = sName;
            _TankType = iTankType;
        }

        /// <summary>
        /// Returns the player's name
        /// </summary>
        public string Name
        {
            get { return _Name; }
        }

        /// <summary>
        /// Returns the player's tank index
        /// </summary>
        public int TankIndex
        {
            get
            {
                return _TankType;
            }
        }
    }
}
