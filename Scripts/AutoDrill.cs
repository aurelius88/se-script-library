using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Constructions;
using SE_Script_Library.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;

namespace SE_Script_Library.Scripts
{

    public class AutoDrill : IngameScript
    {

        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        private Drill drill;

        void Main()
        {
            if (drill == null)
            {
                var blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, NoPassenger);
                if (blocks.Count == 0)
                    throw new Exception("No ship controller (cockpit, etc.) available.");

                drill = new Drill(blocks[0] as IMyShipController);
                blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyGyro>(blocks);
                drill.AddGyroskopes(blocks);
                blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks);
                drill.AddThrusters(blocks);
                blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(blocks);
                drill.AddSensors(blocks);
                drill.handle(Drill.DrillEvent.DrillInitialized);
            }

            debug.Append(drill.State).AppendLine();


            Debug(debug.ToString());
            debug.Clear();
        }

        private bool NoPassenger(IMyTerminalBlock arg)
        {
            return arg.DefinitionDisplayNameText.Equals("Passenger Seat");
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
