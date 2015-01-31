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
using SE_Script_Library.Utils;
using SE_Script_Library.Reference;

namespace SE_Script_Library.TestScripts.Utils
{

    public class GyroActionTest : IngameScript
    {
        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        IMyTerminalBlock controller;

        void Main()
        {
            HashSet<GyroAction> actions = GyroAction.GetElements();
            HashSet<GyroAction>.Enumerator enumerator = actions.GetEnumerator();
            while(enumerator.MoveNext())
                debug.Append(enumerator.Current).AppendLine();

            debug.Append(XUtils.Identity.Backward.Equals(XUtils.Identity.Backward)).AppendLine();

            debug.Append(GyroAction.getActionAroundAxis(XUtils.Identity.Backward)).AppendLine();
            debug.Append(GyroAction.getActionAroundAxis(XUtils.Identity.Forward)).AppendLine();
            debug.Append(GyroAction.getActionAroundAxis(XUtils.Identity.Up)).AppendLine();
            debug.Append(GyroAction.getActionAroundAxis(XUtils.Identity.Down)).AppendLine();
            debug.Append(GyroAction.getActionAroundAxis(XUtils.Identity.Right)).AppendLine();
            debug.Append(GyroAction.getActionAroundAxis(XUtils.Identity.Left)).AppendLine();

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
