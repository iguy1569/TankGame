// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    Tank
// Class File:  cTank.cs
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
using Ammo;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tanks
{
    /// <summary>
    /// Render and region interface used across all tank types
    /// </summary>
    public interface Animatable
    {
        void Render(Graphics gr);
        Region GetRegion();
        Region GetTurretRegion();
    }

    /// <summary>
    ///  Static class used to convert the tank types down to byte value and back again
    ///  Keeps bandwidth use down to a minimum, reconsructs on recieve end
    /// </summary>
    public static class WhatTank
    {
        /// <summary>
        /// Converts tank type to byte
        /// </summary>
        /// <param name="aItemType">tank class type</param>
        /// <returns>returns byte equivelent</returns>
        public static byte TankConvertTypeToByte(cTank aItemType)
        {
            // light ammo ranges 1-10, heavy ammo regions 11-20
            if (aItemType is cTankType1)  
                return 1;
            if (aItemType is cTankType2)  
                return 2;
            if (aItemType is cTankType3) 
                return 3;
            if (aItemType is cTankType4) 
                return 4;
            return 0;
        }

        /// <summary>
        /// Reconstructs tank byte back to type, including base information for constructor
        /// </summary>
        /// <param name="i">type of tank</param>
        /// <param name="pLocation">location of tank</param>
        /// <param name="fTankAngle">rotation of tank</param>
        /// <param name="fTurretAngle">rotation of turret</param>
        /// <param name="iLife">current life amount</param>
        /// <param name="bHit">tank hit condition</param>
        /// <returns></returns>
        public static cTank TankConvertByteToType(int i, PointF pLocation, float fTankAngle, float fTurretAngle, int iLife, bool bHit)
        {
            if (i == 1)  
                return new cTankType1(pLocation, fTankAngle, fTurretAngle, iLife, bHit);
            if (i == 2)  
                return new cTankType2(pLocation, fTankAngle, fTurretAngle, iLife, bHit);
            if (i == 3) 
                return new cTankType3(pLocation, fTankAngle, fTurretAngle, iLife, bHit);
            if (i == 4)  
                return new cTankType4(pLocation, fTankAngle, fTurretAngle, iLife, bHit);
            return null;
        }
    }

    /// <summary>
    /// Makes identical copy of tank type in memory 
    /// </summary>
    public static class TankClone
    {
        /// <summary>
        /// Makes identical copy of tank type in memory 
        /// </summary>
        /// <param name="other">tank to clone</param>
        /// <returns>cloned tank location in memory</returns>
        public static cTank DeepCopy(cTank other)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, other);
                ms.Position = 0;
                return (cTank)formatter.Deserialize(ms);
            }
        }
    }

    /// <summary>
    /// Base tank type (no type)
    /// contains all information that all tanks will require to run
    /// </summary>
    [Serializable]
    public class cTank : Animatable
    {

        protected string sPlayerName;       // player name

        protected int iLife,                // amount of life left
                      iMaxLife,             // tank life limitation

                      iKills,               // tanks destroyed
                      iDeaths,              // times destroyed
                      iKillStreak,          // longest kill amount

                      iSlowReloadSpeed = 0,       // reload speed for the slow ammo aka the Big cannon type Gun
                      iFastFireRate = 0,          // fire rate for the fast ammo aka the machine gun type turret
                      iSpecialFireRate = 0,       // reload speed for the special ammo aka the mine type weapons

                      iSlowReloadTimeout = 0,     // reload speed for the slow ammo aka the Big cannon type Gun
                      iSpecialFireTimeout = 0;    // reload speed for the special ammo aka the mine type weapons

        protected float fScale = 1;               // tank grid scale

        protected float fRadarRadius;             // radius range, based off map distance 
        protected List<PointF> lpEnemyTargets;    // enemy target locations

        protected PointF pLocation;               // tank location

        protected float fTankAngle,       // tank angle on map (360)
                        fTurretAngle,     // turret angle, independent of tank angle, except for if tank angle changes
                        fTankSpeed,       // tank movement speed
                        fTurretSpeed,     // turret turn angle speed, in degrees
                        fTurnSpeed;       // tank turn speed, in degrees

        protected bool bHit;

        protected List<KeyValuePair<string, int>> lkvpAmmo = new List<KeyValuePair<string, int>>();          // list of all ammo that a tank has
        protected List<KeyValuePair<string, int>> lkvpSpecialItems = new List<KeyValuePair<string, int>>();  // list of all special items a tank can have

        /// <summary>
        /// Public constructor that defaults all values
        /// </summary>
        /// <param name="pLoc">start location of tank</param>
        /// <param name="fTankAng">start angle of tank</param>
        public cTank(PointF pLoc, float fTankAng)
        {
            iLife = 100;
            iKills = 0;
            iDeaths = 0;
            iKillStreak = 0;

            fScale = 1;
            fTankSpeed = 2f;
            fTurnSpeed = 5;
            fTurretAngle = fTankAngle = fTankAng;
            fTurretSpeed = 5;

            pLocation = pLoc;
        }

        #region Tank_Properties
        public int KillCount
        {
            get { return iKills; }
            set { iKills = value; }
        }

        public int DeathCount
        {
            get { return iDeaths; }
            set
            {
                iDeaths = value;
                iKillStreak = 0;
            }
        }

        public int KillStreak
        {
            get { return iKillStreak; }
            set { iKillStreak = value; }
        }

        public int SlowReloadSpeed
        {
            get { return iSlowReloadSpeed; }
            set { iSlowReloadSpeed = value; }
        }

        public int FastFireRate
        {
            get { return iFastFireRate; }
            set { iFastFireRate = value; }
        }

        public int SpecialFireRate
        {
            get { return iSpecialFireRate; }
            set { iSpecialFireRate = value; }
        }

        public int SlowTimeout
        {
            get { return iSlowReloadTimeout; }
            set { iSlowReloadTimeout = value; }
        }

        public int SpecialTimeout
        {
            get { return iSpecialFireTimeout; }
            set { iSpecialFireTimeout = value; }
        }

        public string TankName
        {
            get { return sPlayerName; }
            set
            {
                sPlayerName = value;
            }
        }

        public float Speed
        {
            get { return fTankSpeed; }
        }

        public float TurnSpeed
        {
            get { return fTurnSpeed; }
        }

        public float CurrentTankAngle
        {
            get { return fTankAngle; }
        }

        public float CurrentTurretAngle
        {
            get { return fTurretAngle; }
        }

        public List<PointF> EnemyLocations
        {
            get { return lpEnemyTargets; }
        }

        public PointF CurrentTankPosition
        {
            get { return pLocation; }
        }

        public bool Hit
        {
            get { return bHit; }
        }

        public int Life
        {
            get { return iLife; }
        }

        public int LifeLimit
        {
            get { return iMaxLife; }
        }
        #endregion

        /// <summary>
        /// Default get path for all ammo types, called from derived classes
        /// passes in the graphicspath
        /// </summary>
        /// <param name="gp">graphics path to manipulate</param>
        /// <returns>rgraphics path edited</returns>
        public GraphicsPath GetPath(GraphicsPath gp, float fAngle)
        {
            GraphicsPath G = (GraphicsPath)gp.Clone(); //clones the static graphics path so it is not modified
            Matrix mat = new Matrix();  //new matrix for transforms
            mat.Rotate(fAngle + 90, MatrixOrder.Append);  //
            mat.Scale(fScale,fScale, MatrixOrder.Append);
            mat.Translate(pLocation.X, pLocation.Y, MatrixOrder.Append);
            G.Transform(mat);   //applies transforms to the graphics path            
            return G;   //returns the new transformed graphics path
        }

        /// <summary>
        /// default Render, overridden
        /// </summary>
        public virtual void Render(Graphics gr)
        { }

        /// <summary>
        /// Animation assist for tanks that have died
        /// </summary>
        /// <param name="gr">graphics screen</param>
        public void RenderSmoke(Graphics gr)
        {
            Random rnd = new Random();
            Color[] cSmokeCol = new Color[2] { Color.Black, Color.DarkGray};
            int[] iSmokeDemension = new int[9] { 1, 2, rnd.Next(3, 5), 
                                                 rnd.Next(3, 5), rnd.Next(3, 5), rnd.Next(3, 5),
                                                 rnd.Next(3, 5), rnd.Next(2, 4), rnd.Next(0, 2) };

            for(int i = 0; i < iSmokeDemension.Length; i++)
                for (int j = -iSmokeDemension[i]; j <= iSmokeDemension[i]; j++)
                    gr.FillEllipse(new SolidBrush(Color.FromArgb(220 - i * 15, cSmokeCol[rnd.Next(0, cSmokeCol.Length)])),
                                    new Rectangle((int)pLocation.X + j * 5, (int)pLocation.Y - 5 * i, 5, 5));
        }

        /// <summary>
        /// default GetRegion, overridden
        /// </summary>
        /// <returns></returns>
        public virtual Region GetRegion()
        {
            return null;
        }

        /// <summary>
        /// default GetTurretRegion, overridden
        /// </summary>
        /// <returns></returns>
        public virtual Region GetTurretRegion()
        {
            return null;
        }

        /// <summary>
        /// Used to turn the turret by the amount prespecified for the tank type
        /// Positive number = right, negative numbr = left
        /// </summary>
        /// <param name="fTurnDirection">Left/Right</param>
        public void TurretTurn(float fTurnDirection)
        {
            if (fTurnDirection > 0)
                fTurretAngle += fTurretSpeed * Math.Abs(fTurnDirection);       // right
            else
                fTurretAngle -= fTurretSpeed * Math.Abs(fTurnDirection);       // left
        }

        /// <summary>
        /// Used to turn the tank by the amount prespecified for tank type
        /// Positive number = right, negative numbr = left
        /// </summary>
        /// <param name="iTurnDirection">left/right</param>
        public void TankTurn(float fTurnDirection)
        {
            if (fTurnDirection > 0)
            {
                fTankAngle += fTurnSpeed * Math.Abs(fTurnDirection);
                fTurretAngle += fTurnSpeed * Math.Abs(fTurnDirection);
            }
            else
            {
                fTankAngle -= fTurnSpeed * Math.Abs(fTurnDirection);
                fTurretAngle -= fTurnSpeed * Math.Abs(fTurnDirection);
            }
        }

        /// <summary>
        /// Used to move the tank in a forward or backwards direction based off off current tank angle
        /// positive float = forward, negative float = backwards
        /// 0 through infinity becomes movement scale
        /// </summary>
        public void MoveTank(float fMovementDirection)
        {
            float fRadians = fTankAngle * (float)Math.PI/180.0f;
            float fXMovement = (float)Math.Cos(fRadians) * fTankSpeed;
            float fYMovement = (float)Math.Sin(fRadians) * fTankSpeed;

            if (fMovementDirection > 0)
            {
                pLocation.X += fXMovement * Math.Abs(fMovementDirection);
                pLocation.Y += fYMovement * Math.Abs(fMovementDirection);
            }
            else
            {
                pLocation.X -= fXMovement * Math.Abs(fMovementDirection);
                pLocation.Y -= fYMovement * Math.Abs(fMovementDirection);
            }
        }

        /// <summary>
        /// Deducts tank health based off damage int
        /// triggers bHit if becomes less then 0
        /// </summary>
        /// <param name="iDamage">damage factor</param>
        public void TankHit(int iDamage)
        {
            iLife -= iDamage;
            if (iLife < 0)
                bHit = true;
        }
    }

    /// <summary>
    /// Derived class for tank 1
    /// Takes in all cTank class variables
    /// Sets them to tank 1 standards
    /// </summary>
    [Serializable]
    public class cTankType1 : cTank, Animatable
    {
        const int ciLifeMax = 600;  // default max life

        /// <summary>
        /// default constructor, sets tank type default speeds, and life
        /// </summary>
        /// <param name="pLoc">location of creation</param>
        /// <param name="fTankAng">rotation at creation</param>
        public cTankType1(PointF pLoc, float fTankAng)
            : base(pLoc, fTankAng)
        {
            bHit = false;
            iLife = iMaxLife = ciLifeMax;
            fTankSpeed = 5f;
            fTurnSpeed = 5;
            fTurretSpeed = 5;
            fScale = 2f;
        }

        /// <summary>
        /// secondary constructor, client side rebuild
        /// sets tank type default speeds, life, angles, and hit condition 
        /// </summary>
        /// <param name="pLoc">location of tank</param>
        /// <param name="fTankAng">rotation of tank</param>
        /// <param name="fTurretAng">rotation of turret</param>
        /// <param name="iLi">life of tank</param>
        /// <param name="bHi">hit condition of tank</param>
        public cTankType1(PointF pLoc, float fTankAng, float fTurretAng, int iLi, bool bHi)
            : base(pLoc, fTankAng)
        {
            pLocation = pLoc;
            fTankAngle = fTankAng;
            fTurretAngle = fTurretAng;
            iMaxLife = ciLifeMax;
            iLife = iLi;
            fScale = 2f;
            bHit = bHi;
        }

        protected static List<GraphicsPath> lgpTank = new List<GraphicsPath>();      // graphics path for tank base
        protected static List<GraphicsPath> lgpTurret = new List<GraphicsPath>();    // graphics path for turret

        /// <summary>
        /// Build static form for type of tank (Tank 1)
        /// </summary>
        static cTankType1()
        {
            // Color[0] - base
            GraphicsPath gp = new GraphicsPath();
            gp.AddRectangles(new Rectangle[] { new Rectangle(-5, -6, 10, 12),
                                               new Rectangle(-8, -10, 16, 4),
                                               new Rectangle(-8, 6, 16, 4)});

            lgpTank.Add(gp);

            // Color[1] - base
            gp = new GraphicsPath();
            gp.AddRectangles(new Rectangle[] { new Rectangle(-7, -6, 2, 12), 
                                               new Rectangle( 5, -6, 2, 12) });
            lgpTank.Add(gp);

            // Color - turret
            gp = new GraphicsPath(FillMode.Winding);
            gp.AddEllipse(new Rectangle(-5, -5, 10, 10));
            gp.AddRectangle(new Rectangle(-2, -14, 4, 11));
            lgpTurret.Add(gp);
        }

        /// <summary>
        /// Renders all cTankType1 and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cColor = new Color[3] { Color.DarkGreen, Color.Black, Color.Black};
            int iColorIndex = 0;
            //render tank related graphics
            foreach (GraphicsPath gp in lgpTank)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp, fTankAngle));
                gr.DrawPath(new Pen(Color.Navy), base.GetPath(gp, fTankAngle));
                ++iColorIndex;
            }

            // render turret related graphics
            foreach (GraphicsPath gp in lgpTurret)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp, fTurretAngle));
                gr.DrawPath(new Pen(Color.Navy), base.GetPath(gp, fTurretAngle));
                ++iColorIndex;
            }

            // if hit, render smoke indicator
            if (bHit)
                RenderSmoke(gr);
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            foreach (GraphicsPath gp in lgpTank)
            {
                GraphicsPath G = base.GetPath(gp, fTankAngle);
                reg.Union(G);
            }
            return reg;
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetTurretRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            foreach (GraphicsPath gp in lgpTurret)
            {
                GraphicsPath G = base.GetPath(gp, fTurretAngle);
                reg.Union(G);
            }
            return reg;
        }
    }

    /// <summary>
    /// Derived class for tank 2
    /// Takes in all cTank class variables
    /// Sets them to tank 2 standards
    /// </summary>
    [Serializable]
    public class cTankType2 : cTank, Animatable
    {
        const int ciLifeMax = 300;  // default life maximum

        /// <summary>
        /// default constructor, sets tank type default speeds, and life
        /// </summary>
        /// <param name="pLoc">location of creation</param>
        /// <param name="fTankAng">rotation at creation</param>
        public cTankType2(PointF pLoc, float fTankAng)
            : base(pLoc, fTankAng)
        {
            iLife = iMaxLife = ciLifeMax;
            fTankSpeed = 10f;
            fTurnSpeed = 10;
            fTurretSpeed = 10;
            fScale = 1.2f;
        }

        /// <summary>
        /// secondary constructor, client side rebuild
        /// sets tank type default speeds, life, angles, and hit condition 
        /// </summary>
        /// <param name="pLoc">location of tank</param>
        /// <param name="fTankAng">rotation of tank</param>
        /// <param name="fTurretAng">rotation of turret</param>
        /// <param name="iLi">life of tank</param>
        /// <param name="bHi">hit condition of tank</param>
        public cTankType2(PointF pLoc, float fTankAng, float fTurretAng, int iLi, bool bHi)
            : base(pLoc, fTankAng)
        {
            pLocation = pLoc;
            fTankAngle = fTankAng;
            fTurretAngle = fTurretAng;
            iMaxLife = ciLifeMax;
            iLife = iLi;
            fScale = 1.5f;
            bHit = bHi;
            fScale = 1.2f;
        }

        protected static List<GraphicsPath> lgpTank = new List<GraphicsPath>();      // graphics path for tank base
        protected static List<GraphicsPath> lgpTurret = new List<GraphicsPath>();    // graphics path for turret  // list of all special items a tank can have

        /// <summary>
        /// Build static form for type of tank (Tank 2)
        /// </summary>
        static cTankType2()
        {
            // Color[0] - base
            GraphicsPath gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(-10, -14, 20, 6));
            gp.AddRectangle(new Rectangle(-10, 8, 20, 6));

            gp.CloseFigure();
            lgpTank.Add(gp);

            // Color[1] - base
            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(-8, -16, 16, 32));

            lgpTank.Add(gp);

            // Color[2] - base
            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(-4, -4, 8, 8));

            lgpTank.Add(gp);

            // Color - turret
            gp = new GraphicsPath(FillMode.Winding);
            gp.AddRectangle(new Rectangle(-4, -4, 8, 8));
            gp.AddRectangle(new Rectangle(-2, -18, 4, 20));
            lgpTurret.Add(gp);
        }

        /// <summary>
        /// Renders all cTankType2 and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cColor = new Color[4] { Color.Black, Color.Blue, Color.Green, Color.Black };
            int iColorIndex = 0;
            //render tank related graphics
            foreach (GraphicsPath gp in lgpTank)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp, fTankAngle));
                gr.DrawPath(new Pen(Color.Navy), base.GetPath(gp, fTankAngle));
                ++iColorIndex;
            }
            //render turret related graphics
            foreach (GraphicsPath gp in lgpTurret)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp, fTurretAngle));
                gr.DrawPath(new Pen(Color.Navy), base.GetPath(gp, fTurretAngle));
                ++iColorIndex;
            }

            // if hit, render smoke indicator
            if (bHit)
                RenderSmoke(gr);
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            foreach (GraphicsPath gp in lgpTank)
            {
                GraphicsPath G = base.GetPath(gp, fTankAngle); 
                reg.Union(G);
            }
            return reg;
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetTurretRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            foreach (GraphicsPath gp in lgpTurret)
            {
                GraphicsPath G = base.GetPath(gp, fTurretAngle);
                reg.Union(G);
            }
            return reg;
        }
    }

    /// <summary>
    /// Derived class for tank 3
    /// Takes in all cTank class variables
    /// Sets them to tank 3 standards
    /// </summary>
    [Serializable]
    public class cTankType3 : cTank, Animatable
    {
        const int ciLifeMax = 900;  // default life maximum

        /// <summary>
        /// default constructor, sets tank type default speeds, and life
        /// </summary>
        /// <param name="pLoc">location of creation</param>
        /// <param name="fTankAng">rotation at creation</param>
        public cTankType3(PointF pLoc, float fTankAng)
            : base(pLoc, fTankAng)
        {
            bHit = false;
            iLife = iMaxLife = ciLifeMax;
            fTankSpeed = 3f;
            fTurnSpeed = 4;
            fTurretSpeed = 6;
            fScale = 2f;
        }

        /// <summary>
        /// secondary constructor, client side rebuild
        /// sets tank type default speeds, life, angles, and hit condition 
        /// </summary>
        /// <param name="pLoc">location of tank</param>
        /// <param name="fTankAng">rotation of tank</param>
        /// <param name="fTurretAng">rotation of turret</param>
        /// <param name="iLi">life of tank</param>
        /// <param name="bHi">hit condition of tank</param>
        public cTankType3(PointF pLoc, float fTankAng, float fTurretAng, int iLi, bool bHi)
            : base(pLoc, fTankAng)
        {
            pLocation = pLoc;
            fTankAngle = fTankAng;
            fTurretAngle = fTurretAng;
            iMaxLife = ciLifeMax;
            iLife = iLi;
            fScale = 2f;
            bHit = bHi;
        }

        protected static List<GraphicsPath> lgpTank = new List<GraphicsPath>();      // graphics path for tank base
        protected static List<GraphicsPath> lgpTurret = new List<GraphicsPath>();    // graphics path for turret

        /// <summary>
        /// Build static form for type of tank (Tank 3)
        /// </summary>
        static cTankType3()
        {
            // Color[0] - base
            GraphicsPath gp = new GraphicsPath();
            gp.AddRectangles(new Rectangle[] { new Rectangle( -8,  -8, 16, 16),
                                               new Rectangle(-11, -10, 22,  2),
                                               new Rectangle(-11,   8, 22,  2),
                                               new Rectangle(-11,  -8,  1, 16), 
                                               new Rectangle( 10,  -8,  1, 16) });
            lgpTank.Add(gp);

            // Color[1] - base
            gp = new GraphicsPath();
            gp.AddRectangles(new Rectangle[] { new Rectangle(-10, -8, 2, 16), 
                                               new Rectangle(  8, -8, 2, 16) });
            lgpTank.Add(gp);

           // Color[2] - base
           gp = new GraphicsPath();

           // bottom tank spike
           gp.AddPolygon(new PointF[] { new PointF(-11,  -10),
                                        new PointF(  0,  -13), 
                                        new PointF( 11,  -10)});
           gp.CloseFigure();

           // top tank spike
           gp.AddPolygon(new PointF[] { new PointF(-11,  10),
                                        new PointF(  0,  13), 
                                        new PointF( 11,  10)});
           gp.CloseFigure();

           // front left tank spike
           gp.AddPolygon(new PointF[] { new PointF(-11, 9),
                                        new PointF(-13, 5), 
                                        new PointF(-11, 0)});
           gp.CloseFigure();

           // back left tank spike
           gp.AddPolygon(new PointF[] { new PointF(-11, -9),
                                        new PointF(-13, -5), 
                                        new PointF(-11,  0)});
           gp.CloseFigure();

           // back right tank spikev
           gp.AddPolygon(new PointF[] { new PointF(11, 9),
                                        new PointF(13, 5), 
                                        new PointF(11, 0)});
           gp.CloseFigure();

           // front right tank spike
           gp.AddPolygon(new PointF[] { new PointF(11, -9),
                                        new PointF(13, -5), 
                                        new PointF(11,  0)});
           gp.CloseFigure();

           lgpTank.Add(gp);

            // Color - turret
            gp = new GraphicsPath(FillMode.Winding);
            gp.AddEllipse(new Rectangle(-5, -5, 10, 10));
            gp.AddRectangle(new Rectangle(-2, -14, 4, 11));
            lgpTurret.Add(gp);
        }

        /// <summary>
        /// Renders all cTankType3 and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cColor = new Color[4] { Color.Tan, Color.DarkSlateGray, Color.Gray, Color.Black };
            int iColorIndex = 0;
            // render tank related graphics
            foreach (GraphicsPath gp in lgpTank)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp, fTankAngle));
                gr.DrawPath(new Pen(Color.Navy), base.GetPath(gp, fTankAngle));
                ++iColorIndex;
            }

            // render turret related graphics
            foreach (GraphicsPath gp in lgpTurret)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp, fTurretAngle));
                gr.DrawPath(new Pen(Color.Black), base.GetPath(gp, fTurretAngle)); 
                ++iColorIndex;
            }

            // if hit, render smoke indicator
            if (bHit)
                RenderSmoke(gr);
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            foreach (GraphicsPath gp in lgpTank)
            {
                GraphicsPath G = base.GetPath(gp, fTankAngle);
                reg.Union(G);
            }
            return reg;
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetTurretRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            foreach (GraphicsPath gp in lgpTurret)
            {
                GraphicsPath G = base.GetPath(gp, fTurretAngle);
                reg.Union(G);
            }
            return reg;
        }
    }

    /// <summary>
    /// Derived class for tank 4
    /// Takes in all cTank class variables
    /// Sets them to tank 4 standards
    /// </summary>
    [Serializable]
    public class cTankType4 : cTank, Animatable
    {
        const int ciLifeMax = 1500; // default life maximum

        /// <summary>
        /// default constructor, sets tank type default speeds, and life
        /// </summary>
        /// <param name="pLoc">location of creation</param>
        /// <param name="fTankAng">rotation at creation</param>
        public cTankType4(PointF pLoc, float fTankAng)
            : base(pLoc, fTankAng)
        {
            iLife = iMaxLife = ciLifeMax;
            fTankSpeed = 2f;
            fTurnSpeed = 3;
            fTurretSpeed = 5;
            fScale = 2f;
        }

        /// <summary>
        /// secondary constructor, client side rebuild
        /// sets tank type default speeds, life, angles, and hit condition 
        /// </summary>
        /// <param name="pLoc">location of tank</param>
        /// <param name="fTankAng">rotation of tank</param>
        /// <param name="fTurretAng">rotation of turret</param>
        /// <param name="iLi">life of tank</param>
        /// <param name="bHi">hit condition of tank</param>
        public cTankType4(PointF pLoc, float fTankAng, float fTurretAng, int iLi, bool bHi)
            : base(pLoc, fTankAng)
        {
            pLocation = pLoc;
            fTankAngle = fTankAng;
            fTurretAngle = fTurretAng;
            iMaxLife = ciLifeMax;
            iLife = iLi;
            fScale = 2f;
            bHit = bHi;
        }

        protected static List<GraphicsPath> lgpTank = new List<GraphicsPath>();      // graphics path for tank base
        protected static List<GraphicsPath> lgpTurret = new List<GraphicsPath>();    // graphics path for turret

        /// <summary>
        /// Build static form for type of tank (Tank 4)
        /// </summary>
        static cTankType4()
        {
            // Color[0] - base
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-17, -17, 34, 34));
            lgpTank.Add(gp);

            // Color[1] - base windows
            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-10, -10, 4, 4));
            gp.AddEllipse(new Rectangle(-10,   6, 4, 4));
            gp.AddEllipse(new Rectangle(  6, -10, 4, 4));
            gp.AddEllipse(new Rectangle(  6,   6, 4, 4));
            gp.AddEllipse(new Rectangle( -2,  10, 4, 4));
            gp.AddEllipse(new Rectangle( -2, -14, 4, 4));
            gp.AddEllipse(new Rectangle( 10,  -2, 4, 4));
            gp.AddEllipse(new Rectangle(-14,  -2, 4, 4));
            lgpTank.Add(gp);

            // top end spike for directional help
            gp = new GraphicsPath();
            gp.AddPolygon(new PointF[] { new PointF( 0, -17),
                                         new PointF(-2, -14), 
                                         new PointF( 2, -14)});
            gp.CloseFigure();
            lgpTank.Add(gp);


            // Color - turret
            gp = new GraphicsPath(FillMode.Winding);
            gp.AddEllipse(new Rectangle(-5, -5, 10, 10));
            gp.AddRectangle(new Rectangle(-2, -14, 4, 11));
            lgpTurret.Add(gp);
        }

        /// <summary>
        /// Renders all cTankType4 and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cColor = new Color[4] { Color.Silver, Color.White, Color.DarkRed, Color.Navy };
            int iColorIndex = 0;
            // render tank related graphics
            foreach (GraphicsPath gp in lgpTank)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp, fTankAngle));
                gr.DrawPath(new Pen(Color.Black), base.GetPath(gp, fTankAngle));
                ++iColorIndex;
            }

            //render turret related graphics
            foreach (GraphicsPath gp in lgpTurret)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp, fTurretAngle));
                gr.DrawPath(new Pen(Color.Black), base.GetPath(gp, fTurretAngle));
                ++iColorIndex;
            }

            // if hit, render smoke indicator
            if (bHit)
                RenderSmoke(gr);
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            foreach (GraphicsPath gp in lgpTank)
            {
                GraphicsPath G = base.GetPath(gp, fTankAngle);
                reg.Union(G);
            }
            return reg;
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetTurretRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            foreach (GraphicsPath gp in lgpTurret)
            {
                GraphicsPath G = base.GetPath(gp, fTurretAngle);
                reg.Union(G);
            }
            return reg;
        }
    }
}
