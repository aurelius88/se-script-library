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
    public abstract class ReferenceOrientedBlocks
    {

        /// <summary>
        /// The block to which the actions are oriented to.
        /// </summary>
        public readonly IMyTerminalBlock referenceBlock;

        public ReferenceOrientedBlocks(IMyTerminalBlock referenceBlock)
        {
            this.referenceBlock = referenceBlock;
        }
    }
}
