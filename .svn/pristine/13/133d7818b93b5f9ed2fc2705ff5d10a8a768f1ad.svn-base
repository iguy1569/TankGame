// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    Obstacles
// Class File:  cObstacles.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach and Dan Allen
//
// Discription: Responsible for handling obstacle types, contains 2d information 
//              for render
// ///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Obstacles
{
    /// <summary>
    ///  Static class used to convert the obstacle types down to byte value and back again
    ///  Keeps bandwidth use down to a minimum, reconsructs on recieve end
    /// </summary>
    public static class WhatObstacle
    {
        /// <summary>
        /// Converts obstacle type to byte
        /// </summary>
        /// <param name="aItemType">tank class type</param>
        /// <returns>returns byte equivelent</returns>
        public static byte ObstacleConvertTypeToByte(cObstacle aItemType)
        {
            if (aItemType is TankStoppers)
                return 1;
            if (aItemType is Trees)
                return 2;
            if (aItemType is Walls)
                return 3;
            return 0;
        }

        /// <summary>
        /// Reconstructs obstacle byte back to type, including base information for constructor
        /// </summary>
        /// <param name="i">byte obstacle type</param>
        /// <param name="pLocation">location of obstacle</param>
        /// <param name="fRotation">rotation of obstacle</param>
        /// <returns></returns>
        public static cObstacle ObstacleConvertByteToType(int i, PointF pLocation, float fRot)
        {
            if (i == 1)  
                return new TankStoppers(pLocation, fRot);
            if (i == 2)  
                return new Trees(pLocation, fRot);
            if (i == 3)
                return new Walls(pLocation, fRot);     
            return null;
        }
    }

    /// <summary>
    /// Render and region interface used across all ammo types
    /// </summary>
    public interface Animatable
    {
        void Render(Graphics gr);
        Region GetRegion();
    }

    public class cObstacle : Animatable
    {
        PointF pLocation;       
        float fRotation;

        public cObstacle(PointF pPos, float fRot)
        {
            pLocation = pPos;
            fRotation = fRot;
        }

        public PointF Location { get { return pLocation; }}
        public float Rotation { get { return fRotation; }}


        /// <summary>
        /// Default get path for all obstacle types, called from derived classes
        /// passes in the graphicspath
        /// </summary>
        /// <param name="gp">graphics path to manipulate</param>
        /// <returns>rgraphics path edited</returns>
        public GraphicsPath GetPath(GraphicsPath gp)
        {
            GraphicsPath G = (GraphicsPath)gp.Clone(); //clones the static graphics path so it is not modified
            Matrix mat = new Matrix();  //new matrix for transforms
            mat.Rotate(fRotation, MatrixOrder.Append);  //
            mat.Scale(3.0f, 3.0f, MatrixOrder.Append);
            mat.Translate(pLocation.X, pLocation.Y, MatrixOrder.Append);
            G.Transform(mat);   //applies transforms to the graphics path            
            return G;   //returns the new transformed graphics path
        }

        /// <summary>
        /// default Render, overridden
        /// </summary>
        /// <returns></returns>
        public virtual void Render(Graphics gr)
        { }

        /// <summary>
        /// default GetRegion, overridden
        /// </summary>
        /// <returns></returns>
        public virtual Region GetRegion()
        {
            return null;
        }
    }


    /// <summary>
    /// Derived class for cObstacle
    /// Takes in all cObstacle class variables
    /// Sets them to TankStopper type
    /// </summary>
    public class TankStoppers : cObstacle, Animatable
    {
        /// <summary>
        ///  default constructor
        /// </summary>
        /// <param name="fRotate">rotation of Tankstopper</param>
        /// <param name="pPosition">position of Tankstopper</param>
        public TankStoppers(PointF pPos, float fRot)
            : base(pPos, fRot)
        {   }

        protected static List<GraphicsPath> lgpObstacle = new List<GraphicsPath>();      // graphics path for tank base

        /// <summary>
        /// Build static form for type of obstacle (Tankstopper)
        /// </summary>
        static TankStoppers()
        {
            // Color[0] - base
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(new PointF[] {new PointF(-5,  4),
                                        new PointF(-4,  5),
                                        new PointF( 5, -4),
                                        new PointF( 4, -5)});
            lgpObstacle.Add(gp);

            // Color[1] - base
            gp = new GraphicsPath();
            gp.AddPolygon(new PointF[] {new PointF(-5, -4),
                                        new PointF(-4, -5),
                                        new PointF( 5,  4),
                                        new PointF( 4,  5)});
            lgpObstacle.Add(gp);
        }

        /// <summary>
        /// Renders all TankStoppers and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cColor = new Color[2] { Color.Black, Color.DarkGray };
            int iColorIndex = 0;
            foreach (GraphicsPath gp in lgpObstacle)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp));
                ++iColorIndex;
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
            foreach (GraphicsPath gp in lgpObstacle)
            {
                GraphicsPath G = base.GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }
    }

    /// <summary>
    /// Derived class for cObstacle
    /// Takes in all cObstacle class variables
    /// Sets them to Trees type
    /// </summary>
    public class Trees : cObstacle, Animatable
    {
        /// <summary>
        ///  default constructor
        /// </summary>
        /// <param name="fRotate">rotation of Tree</param>
        /// <param name="pPosition">position of Tree</param>
        public Trees(PointF pPos, float fRot)
            : base(pPos, fRot)
        { }

        protected static List<GraphicsPath> lgpObstacle = new List<GraphicsPath>();      // graphics path for tank base

        /// <summary>
        /// Build static form for type of obstacle (Trees)
        /// </summary>
        static Trees()
        {
            // Color[0] - base
            GraphicsPath gp = new GraphicsPath(FillMode.Winding);
            gp.AddEllipse(new Rectangle(-10, -10, 10, 10));
            gp.AddEllipse(new Rectangle( -5, -10, 10, 10));
            gp.AddEllipse(new Rectangle(  0, -10, 10, 10));
            gp.AddEllipse(new Rectangle(  5, -10, 10, 10));

            gp.AddEllipse(new Rectangle(-15, -5, 10, 10));
            gp.AddEllipse(new Rectangle(-10, -5, 10, 10));
            gp.AddEllipse(new Rectangle( -5, -5, 10, 10));
            gp.AddEllipse(new Rectangle(  0, -5, 10, 10));
            gp.AddEllipse(new Rectangle(  5, -5, 10, 10));
            gp.AddEllipse(new Rectangle( 10, -5, 10, 10));

            gp.AddEllipse(new Rectangle(-15, 0, 10, 10));
            gp.AddEllipse(new Rectangle(-10, 0, 10, 10));
            gp.AddEllipse(new Rectangle( -5, 0, 10, 10));
            gp.AddEllipse(new Rectangle(  0, 0, 10, 10));
            gp.AddEllipse(new Rectangle(  5, 0, 10, 10));
            gp.AddEllipse(new Rectangle( 10, 0, 10, 10));

            gp.AddEllipse(new Rectangle(-10, 5, 10, 10));
            gp.AddEllipse(new Rectangle( -5, 5, 10, 10));
            gp.AddEllipse(new Rectangle(  0, 5, 10, 10));
            gp.AddEllipse(new Rectangle(  5, 5, 10, 10));
            lgpObstacle.Add(gp);

            // Color[1] - base
            gp = new GraphicsPath();
            gp.AddLines(new PointF[] { new PointF(-13, -8),
                                       new PointF( -8, -6),
                                       new PointF(  0,  2),
                                       new PointF(  4,  6),
                                       new PointF( 12,  8)});
            lgpObstacle.Add(gp);
        }

        /// <summary>
        /// Renders all Trees and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cColor = new Color[2] { Color.DarkGreen, Color.SaddleBrown };
            int iColorIndex = 0;
            foreach (GraphicsPath gp in lgpObstacle)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp));
                ++iColorIndex;
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
            foreach (GraphicsPath gp in lgpObstacle)
            {
                GraphicsPath G = base.GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }
    }

    /// <summary>
    /// Derived class for cObstacle
    /// Takes in all cObstacle class variables
    /// Sets them to Walls type
    /// </summary>
    public class Walls : cObstacle, Animatable
    {
        public Walls(PointF pPos, float fRot)
            : base(pPos, fRot)
        { }

        protected static List<GraphicsPath> lgpObstacle = new List<GraphicsPath>();      // graphics path for tank base

        /// <summary>
        /// Build static form for type of obstacle (Walls)
        /// </summary>
        static Walls()
        {
            // Color[0] - base
            GraphicsPath gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(-20, -3, 20, 6));
            lgpObstacle.Add(gp);

            // Color[1] - base
            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(  0, -3, 20, 6));
            lgpObstacle.Add(gp);
        }

        /// <summary>
        /// Renders all Trees and returns the graphic
        /// </summary>
        /// <param name="gr">graphics canvas</param>
        public override void Render(Graphics gr)
        {
            Color[] cColor = new Color[2] { Color.DarkGray, Color.Gray };
            int iColorIndex = 0;
            foreach (GraphicsPath gp in lgpObstacle)
            {
                gr.FillPath(new SolidBrush(cColor[iColorIndex]), base.GetPath(gp));
                gr.DrawPath(new Pen(Color.Black), base.GetPath(gp));
                ++iColorIndex;
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
            foreach (GraphicsPath gp in lgpObstacle)
            {
                GraphicsPath G = base.GetPath(gp);
                reg.Union(G);
            }
            return reg;
        }
    }
}
