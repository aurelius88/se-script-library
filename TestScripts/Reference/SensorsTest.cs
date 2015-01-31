using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

namespace SE_Script_Library.TestScripts.Reference
{
    class SensorsTest : IngameScript
    {
        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        IMyTerminalBlock controller;

        void Main()
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, NoPassenger);
            if (blocks.Count == 0)
                throw new Exception("No ship controller (cockpit, etc.) available.");

            controller = blocks[0] as IMyShipController;
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(blocks);

            Sensors sensors = new Sensors(controller, blocks);

            debug.Append(sensors.Min).AppendLine();
            debug.Append(sensors.Max).AppendLine();
            debug.Append(sensors.Default).AppendLine();

            for (int i = 0; i < blocks.Count; ++i)
            {
                var block = blocks[i];
                int id = sensors.GetClosestSensor(block.Position);
                if (block != sensors[id])
                    throw new Exception("Didn't get the same sensor");
            }

            sensors.ExtendBack(0, sensors.Default);
            sensors.ExtendFront(0, sensors.Default);
            sensors.ExtendTop(0, sensors.Default);
            sensors.ExtendBottom(0, sensors.Default);
            sensors.ExtendRight(0, sensors.Default);
            sensors.ExtendLeft(0, sensors.Default);

            sensors.ExtendBack(0, sensors.Max);
            sensors.ExtendFront(0, sensors.Max);
            sensors.ExtendTop(0, sensors.Max);
            sensors.ExtendBottom(0, sensors.Max);
            sensors.ExtendRight(0, sensors.Max);
            sensors.ExtendLeft(0, sensors.Max);

            Sensors.SetFlags(sensors[0], Sensors.Action.DetectAsteroids.Value | Sensors.Action.DetectOwner.Value);

            Debug(debug.ToString());
            debug.Clear();
        }

        private bool NoPassenger(IMyTerminalBlock arg)
        {
            return !arg.DefinitionDisplayNameText.Equals("Passenger Seat");
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
