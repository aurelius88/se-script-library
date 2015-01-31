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
    class SensorTest : IngameScript
    {
        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        void Main()
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(blocks);

            if (blocks.Count == 0)
                throw new Exception();

            var block = blocks[0] as IMySensorBlock;
            debug.Append("LeftExtend = ").Append(block.LeftExtend).AppendLine();
            debug.Append("RightExtend = ").Append(block.RightExtend).AppendLine();
            debug.Append("TopExtend = ").Append(block.TopExtend).AppendLine();
            debug.Append("BottomExtend = ").Append(block.BottomExtend).AppendLine();
            debug.Append("FrontExtend = ").Append(block.FrontExtend).AppendLine();
            debug.Append("BackExtend = ").Append(block.BackExtend).AppendLine();

            debug.Append("DetectPlayers = ").Append(block.DetectPlayers).AppendLine();
            debug.Append("DetectFloatingObjects = ").Append(block.DetectFloatingObjects).AppendLine();
            debug.Append("DetectSmallShips = ").Append(block.DetectSmallShips).AppendLine();
            debug.Append("DetectLargeShips = ").Append(block.DetectLargeShips).AppendLine();
            debug.Append("DetectStations = ").Append(block.DetectStations).AppendLine();
            debug.Append("DetectAsteroids = ").Append(block.DetectAsteroids).AppendLine();
            debug.Append("DetectOwner = ").Append(block.DetectOwner).AppendLine();
            debug.Append("DetectFriendly = ").Append(block.DetectFriendly).AppendLine();
            debug.Append("DetectNeutral = ").Append(block.DetectNeutral).AppendLine();
            debug.Append("DetectEnemy = ").Append(block.DetectEnemy).AppendLine();

            var properties = new List<ITerminalProperty>();
            block.GetProperties(properties);
            for (int i = 0; i < properties.Count; ++i)
            {

                var property = properties[i];
                debug.Append(property.Id);
                debug.Append(" (").Append(block.GetDefaultValue<float>(property.Id)).Append(") [");
                debug.Append(block.GetMininum<float>(property.Id)).Append(',').Append(block.GetMaximum<float>(property.Id)).Append("] : ");
                debug.Append(property.TypeName);
                if (i < properties.Count - 1)
                    debug.Append(", ");
                if (i % 10 == 9)
                    debug.AppendLine();
            }
            debug.AppendLine();

            var actions = new List<ITerminalAction>();
            block.GetActions(actions);
            debug.Append("Actions:").AppendLine();
            List<string> actionNames = new List<string>();
            for (int i = 0; i < actions.Count; ++i)
            {
                var action = actions[i];
                debug.Append(action.Id);
                actionNames.Add(action.Id);

                if (i < actions.Count - 1)
                    debug.Append(", ");
                if (i % 10 == 9)
                    debug.AppendLine();
            }

            for (int i = 0; i < actionNames.Count; ++i)
            {
                block.GetActionWithName(actionNames[i]);
            }

            HashSet<Sensors.Action>.Enumerator enumerator = Sensors.Action.GetEnumerator();
            while (enumerator.MoveNext())
            {
                block.GetActionWithName(enumerator.Current.Name);
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
