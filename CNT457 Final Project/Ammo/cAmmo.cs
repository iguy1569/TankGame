// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    Ammo
// Class File:  cAmmo.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach and Dan Allen
//
// Discription: Responsible for handling ammo types, contains 2d information 
//              for render
// ///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Ammo
{
    /// <summary>
    ///  Static class used to convert the ammo types down to byte value and back again
    ///  Keeps bandwidth use down to a minimum, reconsructs on recieve end
    /// </summary>
    public static class WhatAmmo
    {
        /// <summary>
        /// Converts ammo type to byte
        /// </summary>
        /// <param name="aItemType">ammo class type</param>
        /// <returns>returns byte equivelent</returns>
        public static byte AmmoConvertToByte(cAmmo aItemType)
        {
            // light ammo ranges 1-10, heavy ammo regions 11-20, special 21-30
            if (aItemType is cMachineGun)  
                return 1;

            if (aItemType is cFlameGun)   
                return 2;

            if (aItemType is cShell)      
                return 11;
            if (aItemType is cHeavyShell)  
                return 12;
            return 0;
        }

        /// <summary>
        /// Reconstructs ammo byte back to type, including base information for constructor
        /// </summary>
        /// <param name="i">byte bullet type</param>
        /// <param name="pLocation">location of bullet</param>
        /// <param name="fRotation">rotation of bullet</param>
        /// <returns></returns>
        public static cAmmo AmmoConvertByteToType(int i, PointF pLocation, float fRotation)
        {
            // light ammo ranges 1-10, heavy ammo regions 11-20, special 21-30
            if (i == 1) 
                return new cMachineGun(fRotation, pLocation);
            if (i == 2)
                return new cFlameGun(fRotation , pLocation);

            if (i == 11)  
                return new cShell(fRotation, pLocation);
            if (i == 12)  
                return new cHeavyShell(fRotation, pLocation);
            return new cAmmo(fRotation, pLocation);     // default return type, no render
        }

        /// <summary>
        /// Converts special ammo types down to byte format for sending
        /// </summary>
        /// <param name="aItemType">special ammo class type</param>
        /// <returns>special byte equivalent</returns>
        public static byte SpecialConvertToByte(cSpecialItem aItemType)
        {
            // light ammo ranges 0-10, heavy ammo regions 11-20, special 21-30
            if (aItemType is cOilSlick)  
                return 21;

            if (aItemType is cLandMine)  
                return 22;

            return 0;                   // default return type, no render
        }

        /// <summary>
        /// Converts special ammo byte back to class type, uses base information for construction
        /// </summary>
        /// <param name="bType">type to convert</param>
        /// <param name="fRot">rotation of special</param>
        /// <param name="pPos">position of special</param>
        /// <param name="bArm">armed condition</param>
        /// <returns></returns>
        public static cSpecialItem ConvertIntToType(byte bType, float fRot, PointF pPos, bool bArm)
        {
            // light ammo ranges 0-10, heavy ammo regions 11-20, 
            if (bType == 21)  // obviously not permentent ****************
                return new cOilSlick(fRot, pPos);
            else if (bType == 22)
                return new cLandMine(fRot, pPos, bArm);
            return null;
        }
    }

    /// <summary>
    /// Used to position bullet offset based off tank turrent length and roation.
    /// Keeps bullets from spawning in tank 
    /// </summary>
    public static class AmmoOffset
    {
        public static PointF ApplyTankOffset(PointF pOrigion, float fRotation, float fTankRadius)
        {
            // calculate offset
            float fRadians = fRotation * (float)Math.PI / 180.0f,
                  fXOffset = fTankRadius * (float)Math.Cos(fRadians),
                  fYOffset = fTankRadius * (float)Math.Sin(fRadians);
            return new PointF(pOrigion.X + fXOffset, pOrigion.Y + fYOffset);    // return correct start point
        }
    }

    /// <summary>
    /// Render and region interface used across all ammo types
    /// </summary>
    public interface iAnimatable
    {
        void Render();
        Region GetRegion();
    }

    public class cAmmo
    {
        protected int    iDamage;         // damage ammo will cause to object it hits

        protected float  fScale;          // Ammo grid scale

        protected PointF pAmmoPos;        // position of the Ammo
        protected float  fRotation;       // rotation angle Ammo will get sent to

        protected float  fAmmoSpeed;         // Ammo movement speed
        protected float  fMaxFireDistance;   // max range ammo can reach

        protected int    iAmmoFireDelay;  // Ticks between ammo fires
        protected int    iFiredBy;        // tracker of who fired shot

        protected bool   bHit;            // used to indicate if bullet should be deleted

        public int FireDelayRate
        {
            get { return iAmmoFireDelay; }
        }

        /// <summary>
        /// Public constructor that defaults all values
        /// </summary>
        /// <param name="rotation">Angle Ammo gets fired at based on the tank turret rotation</param>
        /// <param name="Position">Start position of Ammo based on the tank turret position</param>
        public cAmmo(float fRotate, PointF pPosition)
        {
            iDamage = 45;

            fMaxFireDistance = 200f;
            fAmmoSpeed = 5f;
            fRotation = fRotate;
            fScale = 1f;

            pAmmoPos = pPosition;
        }

        #region Ammo_Properties
        public float AmmoRotation
        {
            get { return fRotation; }
        }

        public PointF AmmoPosition
        {
            get { return pAmmoPos; }
        }

        public int DamageAmount
        {
            get { return iDamage; }
        }

        public float SpeedOffset
        {
            set 
            { 
                fAmmoSpeed += value;
                fMaxFireDistance += value * 2.0f;
            }
        }

        public int FiredByIndex
        {
            get { return iFiredBy; }
            set { iFiredBy = value; }
        }

        public bool Hit
        {
            get { return bHit; }
            set { bHit = value; }
        }
        #endregion

        /// <summary>
        /// Updates bullet position based off fired at speed
        /// </summary>
        public void MoveBullet()
        {
            // calculate point movement
            float fRadians = fRotation * (float)Math.PI / 180.0f;
            pAmmoPos.X += fAmmoSpeed * (float)Math.Cos(fRadians);
            pAmmoPos.Y += fAmmoSpeed * (float)Math.Sin(fRadians);
            fMaxFireDistance -= fAmmoSpeed;
            if (fMaxFireDistance <= 0)
                bHit = true;
        }

        /// <summary>
        /// Default get path for all ammo types, called from derived classes
        /// passes in the graphicspath
        /// </summary>
        /// <param name="gp">graphics path to manipulate</param>
        /// <returns>rgraphics path edited</returns>
        public GraphicsPath GetPath(GraphicsPath gp)
        {
            GraphicsPath G = (GraphicsPath)gp.Clone(); //clones the static graphics path so it is not modified
            Matrix mat = new Matrix();  //new matrix for transforms
            mat.Scale(fScale/2, fScale, MatrixOrder.Append);
            mat.Rotate(fRotation + 90, MatrixOrder.Append);
            mat.Translate(pAmmoPos.X, pAmmoPos.Y, MatrixOrder.Append);
            G.Transform(mat);   //applies transforms to the graphics path
            return G;   //returns the new transformed graphics path
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
        /// default Render, overridden
        /// </summary>
        /// <returns></returns>
        public virtual void Render(Graphics gr)
        {   }
    }

    /// <summary>
    /// Derived class for fast ammo 
    /// Takes in all cAmmo class variables
    /// Sets them to fast ammo speeds
    /// </summary>
    public class cFastAmmo : cAmmo
    {
        /// <summary>
        ///  default constructor, sets fast ammo speeds and delays
        /// </summary>
        /// <param name="fRotate">rotation of fastammo</param>
        /// <param name="pPosition">position of fastammo</param>
        public cFastAmmo(float fRotate, PointF pPosition) :
            base(fRotate, pPosition)
        {
            fMaxFireDistance = 200f;
            fAmmoSpeed = 15f;
            iAmmoFireDelay = 2;
        }

        /// <summary>
        /// default GetRegion, overrridden
        /// </summary>
        /// <returns>null</returns>
        public virtual Region GetRegion()
        {
            return null;
        }

        /// <summary>
        /// default Render, overrridden
        /// </summary>        
        public virtual void Render(Graphics gr)
        { }
    }

    /// <summary>
    /// Derived class for cFastAmmo type
    /// Inherits all cFastAmmo settings
    /// </summary>
    public class cMachineGun : cFastAmmo
    {
        protected static List<GraphicsPath> lgp = new List<GraphicsPath>();      // graphics path for Ammo
    
        /// <summary>
        /// Build static form for type of ammo (machine gun bullet)
        /// </summary>
        static cMachineGun()
        {
            // base layer, gray
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon( new PointF[] { new PointF(-2, -1),
                                          new PointF(-1, -3),
                                          new PointF( 0, -4),
                                          new PointF( 1, -3),
                                          new PointF( 2, -1),
                                          new PointF( 3,  4),
                                          new PointF(-3,  4) });
            gp.CloseFigure();
            lgp.Add(gp);

            // second layer, color line 
            gp = new GraphicsPath();
            gp.AddLines(new PointF[] { new PointF(-2, -1),
                                       new PointF( 0,  0),
                                       new PointF( 2, -1) });
            lgp.Add(gp);

            // third layer, tip
            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-1, -3, 2, 3));
            lgp.Add(gp);
        }

        /// <summary>
        /// Default constructor for machinegun
        /// inherits fast ammo attributes
        /// </summary>
        /// <param name="fRotate">rotation of machine bullet</param>
        /// <param name="pPosition">position of machine bullet</param>
        public cMachineGun(float fRotate, PointF pPosition):
            base(fRotate, pPosition)
        {
            fScale = 1.2f;
            iDamage = 5;
            fMaxFireDistance = 100f;
            iAmmoFireDelay = 3;
        }

        /// <summary>
        /// Renders all machine gun ammo and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cAmmoColors = new Color[3] { Color.Gray, Color.Black, Color.White };
            int iPath = 0;
            // foreach graphic path, assign color and render
            foreach (GraphicsPath gp in lgp)
            {
                gr.FillPath(new SolidBrush(cAmmoColors[iPath]), GetPath(gp));
                iPath++;
            }
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            // foreach graphic path, combine into one unit
            foreach (GraphicsPath gp in lgp)
            {
                GraphicsPath G = GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }
    }

    /// <summary>
    /// Derived class for cFastAmmo type
    /// Inherits all cFastAmmo settings
    /// </summary>
    public class cFlameGun : cFastAmmo
    {
        protected static List<GraphicsPath> lgp = new List<GraphicsPath>();      // graphics path for Ammo

        /// <summary>
        /// Build static form for type of ammo (machine gun bullet)
        /// </summary>
        static cFlameGun()
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-2, -4, 4, 5));
            gp.AddEllipse(new Rectangle(-2, -0, 4, 5));
            lgp.Add(gp);

            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-2, -4, 4, 5));
            gp.AddEllipse(new Rectangle(-2, -0, 4, 5));
            lgp.Add(gp);

            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-2, -3, 4, 2));
            gp.AddEllipse(new Rectangle(-2,  3, 4, 2));
            lgp.Add(gp);

            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-2, -1, 4, 2));
            gp.AddEllipse(new Rectangle(-2,  1, 4, 2));
            lgp.Add(gp);
        }

        /// <summary>
        /// Default constructor for flamegun
        /// inherits fast ammo attributes
        /// </summary>
        /// <param name="fRotate">rotation of flame</param>
        /// <param name="pPosition">position of flame</param>
        public cFlameGun(float fRotate, PointF pPosition) :
            base(fRotate, pPosition)
        {
            iDamage = 3;
            fMaxFireDistance = 50f;
            iAmmoFireDelay = 1;
            fScale = 2.5f;
        }

        /// <summary>
        /// Renders all flame gun ammo and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cAmmoColors = new Color[4] { Color.Red, Color.Yellow, Color.Orange, Color.White };
            int iPath = 0;
            // foreach graphic path, assign color and render
            foreach (GraphicsPath gp in lgp)
            {
                gr.FillPath(new SolidBrush(cAmmoColors[iPath]), GetPath(gp));
                iPath++;
            }
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            // foreach graphic path, combine into one unit
            foreach (GraphicsPath gp in lgp)
            {
                GraphicsPath G = GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }
    }


    /// <summary>
    /// Derived class for slow ammo 
    /// Takes in all cAmmo class variables
    /// Sets them to slowt ammo speeds
    /// </summary
    public class cSlowAmmo : cAmmo
    {
        /// <summary>
        ///  default constructor, sets fast ammo speeds and delays
        /// </summary>
        /// <param name="fRotate">rotation of slowammo</param>
        /// <param name="pPosition">position of slowammo</param>
        public cSlowAmmo(float fRotate, PointF pPosition) :
            base(fRotate, pPosition)
        {
            fMaxFireDistance = 2500f;   // is length of map
            fAmmoSpeed = 13f;
            iAmmoFireDelay = 40;
            fScale = 2.5f;
        }

        /// <summary>
        /// default Render, overrridden
        /// </summary> 
        public virtual void Render(Graphics gr)
        {   }

        public virtual Region GetRegion()
        {
            return null;
        }
    }

    /// <summary>
    /// Derived class for cSlowAmmo type
    /// Inherits all cslowAmmo settings
    /// </summary>
    public class cShell : cSlowAmmo
    {
        protected static List<GraphicsPath> lgp = new List<GraphicsPath>();      // graphics path for Ammo

        /// <summary>
        /// Build static form for type of ammo (cShell bullet)
        /// </summary>
        static cShell()
        {
            // base layer, shell base
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(new PointF[] {  new PointF(-3, -1),
                                          new PointF(-1, -3),
                                          new PointF( 0, -4),
                                          new PointF( 1, -3),
                                          new PointF( 3, -1)});
            gp.CloseFigure();
            lgp.Add(gp);

            // second layer, shell back color
            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(-3, -1, 6, 5));
            lgp.Add(gp);

            // third layer, shiny tip
            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-1, -3, 2, 3));
            lgp.Add(gp);
        }

        /// <summary>
        /// Default constructor for Shell
        /// inherits slow ammo attributes
        /// </summary>
        /// <param name="fRotate">rotation of shell</param>
        /// <param name="pPosition">position of shell</param>
        public cShell(float fRotate, PointF pPosition) :
            base(fRotate, pPosition)
        {
            iDamage = 40;
        }

        /// <summary>
        /// Renders all shell ammo and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cAmmoColors = new Color[3] { Color.DarkGray, Color.Gold, Color.White };
            int iPath = 0;
            // foreach graphic path, assign color and render
            foreach (GraphicsPath gp in lgp)
            {
                gr.FillPath(new SolidBrush(cAmmoColors[iPath]), GetPath(gp));
                iPath++;
            }
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            // foreach graphic path, combine into one unit
            foreach (GraphicsPath gp in lgp)
            {
                GraphicsPath G = GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }
    }

    /// <summary>
    /// Derived class for cSlowAmmo type
    /// Inherits all cslowAmmo settings
    /// </summary>
    public class cHeavyShell : cSlowAmmo
    {
        protected static List<GraphicsPath> lgp = new List<GraphicsPath>();      // graphics path for Ammo

        /// <summary>
        /// Build static form for type of ammo (machine gun bullet)
        /// </summary>
        static cHeavyShell()
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(new PointF[] {  new PointF(-3, -1),
                                          new PointF(-1, -3),
                                          new PointF( 0, -4),
                                          new PointF( 1, -3),
                                          new PointF( 3, -1)});
            gp.CloseFigure();
            lgp.Add(gp);

            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(-3, -1, 6, 5));
            lgp.Add(gp);

            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-1, -3, 2, 3));
            lgp.Add(gp);
        }

        /// <summary>
        /// Default constructor for HeavyShell
        /// inherits slow ammo attributes
        /// </summary>
        /// <param name="fRotate">rotation of shell</param>
        /// <param name="pPosition">position of shell</param>
        public cHeavyShell(float fRotate, PointF pPosition) :
            base(fRotate, pPosition)
        {
            iDamage = 80;
            iAmmoFireDelay = 80;
        }

        /// <summary>
        /// Renders all shell ammo and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cAmmoColors = new Color[3] { Color.Black, Color.Red, Color.White };
            int iPath = 0;
            // foreach graphic path, assign color and render
            foreach (GraphicsPath gp in lgp)
            {
                gr.FillPath(new SolidBrush(cAmmoColors[iPath]), GetPath(gp));
                iPath++;
            }
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            // foreach graphic path, combine into one unit
            foreach (GraphicsPath gp in lgp)
            {
                GraphicsPath G = GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }
    }

    /// <summary>
    /// Base class for special ammo types 
    /// Set initial special ammo values
    /// </summary>
    public class cSpecialItem
    {
        protected int iDamage, // damage the special item will cause to object it hits
                      iFiredBy;

        protected float fScale = 2.0f,              // Ammo grid scale
                        fDuration = 0,              // actual time ticks
                        fArmTime = 200,             // time to arm weapon
                        fTimeOut = -1,              // length of time special item will stay active in environment
                        fRotation;                  // rotation angle the special item will land at based on the tank's angle

        protected PointF pSpecialPosition;    // position the special item will be dropped at

        protected bool bArmed = false,
                       bHit = false;

        /// <summary>
        /// Public constructor that defaults all values
        /// </summary>
        /// <param name="fRotate">Angle Special Item gets fired at based on the tank's rotation</param>
        /// <param name="pPos">Start position of Special item based on the tank turret position</param>
        public cSpecialItem(float fRotate, PointF pPos)
        {
            fRotation = fRotate;
            pSpecialPosition = pPos;
            iDamage = 150;
        }

        #region Special_Properties
        public bool Hit
        {
            get { return bHit; }
            set 
            {
                if (bArmed)
                    bHit = value;
                else
                    bHit = false;
            }
        }

        public PointF SpecialPosition
        {
            get { return pSpecialPosition; }
        }

        public float SpecialRotation
        {
            get { return fRotation; }
        }

        public bool ArmStatus
        {
            get { return bArmed; }
        }

        public int DamageAmount
        {
            get { return iDamage; }
        }


        public int FiredByIndex
        {
            get { return iFiredBy; }
            set { iFiredBy = value; }
        }
        #endregion

        /// <summary>
        /// Default get path for all ammo types, called from derived classes
        /// passes in the graphicspath
        /// </summary>
        /// <param name="gp">graphics path to manipulate</param>
        /// <returns>rgraphics path edited</returns>
        public GraphicsPath GetPath(GraphicsPath gp)
        {
            GraphicsPath G = (GraphicsPath)gp.Clone(); //clones the static graphics path so it is not modified
            Matrix mat = new Matrix();  //new matrix for transforms
            mat.Rotate(fRotation, MatrixOrder.Append);  //
            mat.Scale(fScale, fScale, MatrixOrder.Append);
            mat.Translate(pSpecialPosition.X, pSpecialPosition.Y, MatrixOrder.Append);    //
            G.Transform(mat);   //applies transforms to the graphics path
            return G;   //returns the new transformed graphics path
        }

        /// <summary>
        /// default Render, overrridden
        /// </summary> 
        public virtual void Render(Graphics gr)
        { }

        /// <summary>
        /// default GetRegion, overrridden
        /// </summary>
        /// <returns>null</returns>
        public virtual Region GetRegion()
        {
            return null;
        }

        /// <summary>
        /// Ticks off the arm time, until set to zero
        /// marks active once 0
        /// </summary>
        public void TickDuration()
        {
            ++fDuration;
            if (fDuration > fArmTime)
                bArmed = true;

            if (fTimeOut > 0 && fDuration > fTimeOut)
                bHit = true;
        }
    }

    /// <summary>
    /// Derived class for cSpecialAmmo
    /// Inherits all cSpecialAmmo settings
    /// </summary>
    public class cOilSlick : cSpecialItem
    {
        protected static List<GraphicsPath> lgp = new List<GraphicsPath>(); // GraphicsPath to draw out the oil slick
        const int ciArmTime = 80;
        const int ciTimeOut = 2400;

        static cOilSlick()
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-4, -4, 8, 8));
            lgp.Add(gp);
            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-6, -7, 3, 3));
            lgp.Add(gp);
            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle( 6, -7, 4, 4));
            lgp.Add(gp);

        }

        /// <summary>
        /// Sets the position of the oil dropped based on the position coming from the base class. Every oil drop will be
        /// moved at a ratio of +/- 4.
        /// </summary>
        /// <param name="fRotate">Angle oil slick gets fired at based on the tank's rotation</param>
        /// <param name="pPos">Start position of oil slick based on the tank turret position</param>
        public cOilSlick(float fRotate, PointF pPos) :
            base(fRotate, pPos)
        {
            iDamage = 2;

            Random rnd = new Random();

            fArmTime = ciArmTime;
            fTimeOut = ciTimeOut;
            pPos.X = pPos.X + rnd.Next(-4, 5);
            pPos.Y = pPos.Y + rnd.Next(-4, 5);
        }

        /// <summary>
        /// Renders all oilslick paths and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            int iPath = 0;
            foreach (GraphicsPath gp in lgp)
            {
                gr.FillPath(new SolidBrush(Color.Black), GetPath(gp));
                iPath++;
            }
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            // for all graphic paths, combine them
            foreach (GraphicsPath gp in lgp)
            {
                GraphicsPath G = GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }
    }

    public class cLandMine : cSpecialItem
    {
        protected static List<GraphicsPath> lgp = new List<GraphicsPath>(); // GraphicsPath to draw out the oil slick
        protected static GraphicsPath gpa = new GraphicsPath();             // GraphicsPath to draw out the oil slick

        const int ciArmTime = 120;
        const int ciTimeOut = -1;

        static cLandMine()
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-6, -5, 12, 10));
            lgp.Add(gp);

            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle(-5, -1, 2, 2));
            lgp.Add(gp);

            gp = new GraphicsPath();
            gp.AddEllipse(new Rectangle( 3, -1, 2, 2));
            lgp.Add(gp);

            gpa.AddEllipse(new Rectangle(-10, -10, 20, 20));
        }

        /// <summary>
        /// Sets the position of the oil dropped based on the position coming from the base class. Every oil drop will be
        /// moved at a ratio of +/- 4.
        /// </summary>
        /// <param name="fRotate">Angle oil slick gets fired at based on the tank's rotation</param>
        /// <param name="pPos">Start position of oil slick based on the tank turret position</param>
        public cLandMine(float fRotate, PointF pPos) :
            base(fRotate, pPos)
        {
            iDamage = 150;

            Random rnd = new Random();

            fArmTime = ciArmTime;
            fTimeOut = ciTimeOut;
            pPos.X = pPos.X + rnd.Next(-4, 5);
            pPos.Y = pPos.Y + rnd.Next(-4, 5);
        }

        public cLandMine(float fRotate, PointF pPos, bool bArm) :
            this(fRotate, pPos)
        {
            bArmed = bArm;
        }

        /// <summary>
        /// Renders all landmine paths and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            // set default render animations
            Color[] cMineColors = new Color[3] { Color.Chocolate, Color.Red, Color.Green };
            gr.FillPath(new SolidBrush(cMineColors[0]), GetPath(lgp[0]));
            gr.DrawPath(new Pen(Color.Black), GetPath(lgp[0]));
            // if not currently armed, draw red state, else draw green state
            if (!bArmed)
            {
                gr.FillPath(new SolidBrush(cMineColors[1]), GetPath(lgp[1]));
                gr.FillPath(new SolidBrush(Color.Black), GetPath(lgp[2]));
            }
            else
            {
                gr.FillPath(new SolidBrush(Color.Black), GetPath(lgp[1]));
                gr.FillPath(new SolidBrush(cMineColors[2]), GetPath(lgp[2]));
            }
            gr.DrawPath(new Pen(Color.Black), GetPath(lgp[1]));
            gr.DrawPath(new Pen(Color.Black), GetPath(lgp[2]));
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public override Region GetRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            // get all paths and combine them
            foreach (GraphicsPath gp in lgp)
            {
                GraphicsPath G = GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }

        /// <summary>
        /// Get region subclass, calls base class for reference
        /// </summary>
        /// <returns>union of graphics path objects</returns>
        public Region GetExplosionRegion()
        {
            Region reg = new Region();
            reg.MakeEmpty();
            GraphicsPath G = GetPath(gpa);
            reg.Union(G);

            return reg; 
        }
    }
}
