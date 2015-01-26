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


namespace TestScript
{

    public class NotAus : IngameScript
    {

        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        void Main()
        {
            // initialize
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, FilterShipController);

            if (blocks.Count == 0)
                throw new Exception("Did not find any cockpit.");

            IMyShipController controller = blocks[0] as IMyShipController;

            if (!controller.DampenersOverride)
            {
                controller.GetActionWithName("DampenersOverride").Apply(controller);
            }

            Debug(debug.ToString());
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

        public static string[] cockpitSubtypes = new string[]
        {
            "Passenger Seat",
            "Flight Seat",
            "Cockpit",
            "Fighter Cockpit",
            "Control Station"
        };
    }

}
