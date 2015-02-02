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
    class GridTest : IngameScript
    {
        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        void Main()
        {
            var blocks = GridTerminalSystem.Blocks;

            if (blocks.Count == 0)
                throw new Exception();

            HashSet<string> grids = new HashSet<string>();

            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                var grid = block.CubeGrid;
                grids.Add(grid.ToString());
            }

            HashSet<string>.Enumerator e = grids.GetEnumerator();
            while (e.MoveNext())
            {
                debug.Append(e.Current).AppendLine();
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
