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

namespace SE_Script_Library.Utils
{
    abstract class ReferenceOrientedBlocks
    {

        /// <summary>
        /// The block to which the actions are oriented to.
        /// </summary>
        protected IMyTerminalBlock referenceBlock;

        protected ReferenceOrientedBlocks(ref IMyTerminalBlock referenceBlock)
        {
            this.referenceBlock = referenceBlock;
        }
    }
}
