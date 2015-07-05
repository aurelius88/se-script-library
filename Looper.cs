using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Utils;
using SE_Script_Library.Reference;

namespace SE_Script_Library.TestScripts
{
    class Looper : IngameScript
    {
        void Main(string argument)
        {
            // Name;CubeGrid;NumberInGrid;Arguments
            string[] args = argument.Split(new char[]{';'},4);
            Echo("Arguments:");
            for (int i = 0; i < args.Length; i++)
            {
                Echo(args[i]);
            }

            int gridNum = -1;
            if(!int.TryParse(args[2], out gridNum)) {
                Echo("Could not parse 2nd argument to integer.");
                return;
            }
            
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(blocks, 
                b => b.NumberInGrid == gridNum && b.CubeGrid.ToString().Equals(args[1]) && b.CustomName.Equals(args[0]));

            if (blocks.Count > 1)
            {
                Echo("Too many Programmable Blocks with name '"
                    + args[0] + "' and grid number '" + gridNum + "'.");
                return;
            } else if (blocks.Count == 0)
            {
                Echo("Could not found the Programmable Block with name '"
                    + args[0] + "' and grid number '" + gridNum + "'.");
                return;
            }
            Echo("Run Prog: "+blocks[0].CustomName);
            Echo("Argument: " + args[3]);
            IMyProgrammableBlock prog = blocks[0] as IMyProgrammableBlock;
            prog.ApplyAction("Run",
                new List<TerminalActionParameter>() { 
                    TerminalActionParameter.Deserialize(args[3], TypeCode.String)
                });
        }



    }
}
