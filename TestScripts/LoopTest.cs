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
    class LoopTest : IngameScript
    {
        int _globalCount = 0;
        int _count;

        void Main(string argument)
        {
            Echo("Global Count: " + _globalCount);
            _globalCount++;
            if (argument != null)
            {
                Echo("Argument: " + argument);
                if (!argument.Equals(""))
                {
                    try
                    {
                        _count = int.Parse(argument);
                        Echo("Argument parsed as " + _count);
                    }
                    catch (Exception)
                    {
                        Echo("Argument not parsable");
                        _count = 0;
                    }
                }
            }
            else
            {
                Echo("Argument null");
                return;
            }
            if (_globalCount < 10)
            {
                string newArg = (_count + 1).ToString();
                Echo("Try: " + newArg);
                //Me.ApplyAction("Run");
                IMyProgrammableBlock prog = GridTerminalSystem.GetBlockWithName("Prog Loop 2") as IMyProgrammableBlock;
                if (prog != null)
                {
                    prog.ApplyAction("Run",
                        new List<TerminalActionParameter>() { 
                        TerminalActionParameter.Get(newArg)  
                    });
                }

                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(blocks);
                if (blocks.Count > 0)
                {
                    IMyTimerBlock timer = blocks[0] as IMyTimerBlock;
                    if (timer != null)
                    {
                        timer.ApplyAction("TriggerNow");
                    }
                }
            }
        }


    }
}
