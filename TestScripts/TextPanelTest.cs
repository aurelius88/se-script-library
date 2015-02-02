using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;


namespace SE_Script_Library.TestScripts
{

    public class TextPanelTest : IngameScript
    {

        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        static string[] cockpitSubtypes = new string[]
        {
            "Passenger Seat",
            "Flight Seat",
            "Cockpit",
            "Fighter Cockpit",
            "Control Station"
        };

        IMyShipController controller;

        void Main()
        {
            // initialize
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, FilterShipController);

            if (blocks.Count == 0)
                throw new Exception("Did not find any cockpit.");

            controller = blocks[0] as IMyShipController;
            debug.Append("use ").Append(controller.CustomName).Append(':').AppendLine();
            debug.Append(controller.HasInventory());
            if (controller.HasInventory())
            {
                for (int i = 0; i < controller.GetInventoryCount(); ++i)
                {
                    IMyInventory inv = controller.GetInventory(i);
                    debug.Append(inv).AppendLine();
                    var items = inv.GetItems();
                    for (int j = 0; j < items.Count; ++j)
                    {
                        debug.Append(items[j].Content).AppendLine();
                    }
                }
            }

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks);
            if (blocks.Count == 0)
                return;

            IMyTextPanel text = blocks[0] as IMyTextPanel;
            var actions = new List<ITerminalAction>();
            text.GetActions(actions);
            for (int j = 0; j < actions.Count; ++j)
            {
                var action = actions[j];
                debug.Append(action.Id).AppendLine();
            }

            var properties = new List<ITerminalProperty>();
            text.GetProperties(properties);
            for (int j = 0; j < actions.Count; ++j)
            {
                var property = properties[j];
                debug.Append(property.TypeName).Append(" ").Append(property.Id).AppendLine();
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

        static bool FilterShipController(IMyTerminalBlock block)
        {
            return !block.DefinitionDisplayNameText.Equals(cockpitSubtypes[0]);
        }

    }

}
