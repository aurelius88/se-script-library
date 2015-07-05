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
    class LoopTest2 : MyGridProgram
    {
        int count = 0;
        IMyProgrammableBlock loopProg;
        string Seperator = ";";

        void Main(string argument)
        {
            if (count == 0)
                loopProg = GridTerminalSystem.GetBlockWithName("Prog Loop") as IMyProgrammableBlock;
            Echo(count + ":" + argument);
            count++;
            if (count < 100)
                Loop("test" + count);
        }
        private void Loop(string key)
        {
            loopProg.ApplyAction("Run",
                new List<TerminalActionParameter>() {   
                        TerminalActionParameter.Get(  
                        Me.CustomName+Seperator+  
                        Me.CubeGrid+Seperator+  
                        Me.NumberInGrid+Seperator+  
                        key)    
                });
        }


    }
}
