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

namespace SE_Script_Library.Reference
{
    class ReferenceOrientedFactory : ReferenceOrientedBlocks
    {
        public IMyTerminalBlock Reference { get { return referenceBlock; } set { referenceBlock = value; } }

        public ReferenceOrientedFactory(IMyTerminalBlock block) : base(block) { }

        public Gyroscopes createGyroscopes(List<IMyTerminalBlock> blocks)
        {
            return new Gyroscopes(referenceBlock, blocks);
        }

        public Thrusters createThrusters(List<IMyTerminalBlock> blocks)
        {
            return new Thrusters(referenceBlock, blocks);
        }

        public Sensors createSensors(List<IMyTerminalBlock> blocks)
        {
            return new Sensors(referenceBlock, blocks);
        }
    }
}
