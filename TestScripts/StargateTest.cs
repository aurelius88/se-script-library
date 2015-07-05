using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Reference;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;

namespace SE_Script_Library.TestScripts
{
    class StargateTest : IngameScript
    {
        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        void Main()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(blocks);

            for (int i = 0; i < blocks.Count; i++)
            {
                debug.Append(blocks[i].DefinitionDisplayNameText).Append(";")
                    .Append(blocks[i].DisplayNameText);
                
                if (blocks[i].DefinitionDisplayNameText.Equals("DHD SGU"))
                {
                    blocks[i].SetCustomName("Star Gate DHD");
                    debug.Append("-->").Append(blocks[i].DisplayNameText);
                }
                debug.AppendLine();
            }

            Debug(debug.ToString());
            debug.Clear();
        }

        void Debug(String message)
        {
            var list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(debugName, list);
            if (list.Count > 0)
                list[0].SetCustomName(debugName + ":\n\r" + message);
        }

    }

}
