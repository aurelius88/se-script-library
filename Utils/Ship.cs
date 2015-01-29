using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Utils;

namespace SE_Script_Library
{
    public class Ship
    {
        private IMyTerminalBlock referenceBlock;

        private Thrusters thrusts;
        private Gyroscopes gyros;
        private Sensors sensors;

        Ship(IMyShipController controller)
        {
            referenceBlock = controller;
        }

        public void AddThrusters(List<IMyTerminalBlock> thrusters)
        {
            if (thrusts == null)
            {
                thrusts = new Thrusters(ref referenceBlock, thrusters);
                return;
            }

            thrusts.Update(thrusters);
        }

        public void AddGyroskopes(List<IMyTerminalBlock> gyroscopes)
        {
            if (gyros == null)
            {
                gyros = new Gyroscopes(ref referenceBlock, gyroscopes);
            }

            gyros.Update(gyroscopes);
        }
    }
}
