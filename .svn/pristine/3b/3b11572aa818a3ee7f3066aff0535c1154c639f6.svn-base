// ///////////////////////////////////////////////////////////////////////////
// Project:     CNT457 Final Project
// Assembly:    Terrain
// Class File:  cTerrain.cs
//
// Date:        April 17, 2013
// Authors:     Chris Harris, Ian Nalbach and Dan Allen
//
// Discription: Holds all game maps for 2d client
// ///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Terrain
{
    public class cTerrain
    {
        // list of map boards
        List<Bitmap> bmMapsAvailable = new List<Bitmap>();

        public cTerrain()
        {
            bmMapsAvailable.Add(Properties.Resources.MapTerrain1);
            bmMapsAvailable.Add(Properties.Resources.MapTerrain2);
        }

        // retrieve map list
        public List<Bitmap> GetMapsList
        {
            get { return bmMapsAvailable; }
        }
    }
}
