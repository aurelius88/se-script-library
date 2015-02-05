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
        public readonly IMyTerminalBlock ReferenceBlock;

        public Thrusters thrusts;
        public Gyroscopes gyros;
        public Sensors sensors;

        public Ship(IMyShipController controller)
        {
            ReferenceBlock = controller;
        }

        public void AddThrusters(List<IMyTerminalBlock> blocks)
        {
            if (thrusts == null)
            {
                thrusts = new Thrusters(ReferenceBlock, blocks);
                return;
            }

            thrusts.UpdateThrusters(blocks);
        }

        public void AddGyroskopes(List<IMyTerminalBlock> blocks)
        {
            if (gyros == null)
            {
                gyros = new Gyroscopes(ReferenceBlock, blocks);
            }

            gyros.UpdateGyroscopes(blocks);
        }

        public void AddSensors(List<IMyTerminalBlock> blocks)
        {
            if (sensors == null)
            {
                sensors = new Sensors(ReferenceBlock, blocks);
            }

            sensors.UpdateSensors(blocks);
        }

        public VRageMath.BoundingBox GetBounds()
        {
            var grid = ReferenceBlock.CubeGrid;
            return new VRageMath.BoundingBox(grid.Min, grid.Max);
        }
    }
}
