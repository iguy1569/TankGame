using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Video;
using IrrlichtLime.Scene;

namespace _3DClient
{
    class DeviceSettings : IrrlichtCreationParameters
    {
        public Color BackColor; // "null" for skybox

        public DeviceSettings(IntPtr hh, DriverType dt, byte aa, Color bc, bool vs)
        {
            WindowID = hh;
            DriverType = dt;
            AntiAliasing = aa;
            BackColor = bc;
            VSync = vs;
        }
    }
}
