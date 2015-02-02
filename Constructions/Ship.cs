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
using SE_Script_Library.Reference;

namespace SE_Script_Library.Constructions
{
    public class Ship
    {
        protected IMyTerminalBlock referenceBlock;

        protected Thrusters thrusts;
        protected Gyroscopes gyros;
        protected Sensors sensors;

        public Ship(ref IMyShipController controller)
        {
            referenceBlock = controller;
        }

        public void AddThrusters(List<IMyTerminalBlock> blocks)
        {
            if (thrusts == null)
            {
                thrusts = new Thrusters(ref referenceBlock, blocks);
                return;
            }

            thrusts.Update(blocks);
        }

        public void AddGyroskopes(List<IMyTerminalBlock> blocks)
        {
            if (gyros == null)
            {
                gyros = new Gyroscopes(ref referenceBlock, blocks);
            }

            gyros.Update(blocks);
        }

        public void AddSensors(List<IMyTerminalBlock> blocks)
        {
            if (sensors == null)
            {
                sensors = new Sensors(ref referenceBlock, blocks);
            }

            sensors.Update(blocks);
        }

        public VRageMath.BoundingBox GetBounds()
        {
            var grid = referenceBlock.CubeGrid;
            return new VRageMath.BoundingBox(grid.Min, grid.Max);
        }
    }
}
