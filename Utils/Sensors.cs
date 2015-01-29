using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace SE_Script_Library.Utils
{
    class Sensors : ReferenceOrientedBlocks
    {
        // TODO

        Sensors(ref IMyTerminalBlock reference, List<IMyTerminalBlock> blocks)
            : base(ref reference)
        {

        }
    }
}
