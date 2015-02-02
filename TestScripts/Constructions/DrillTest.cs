﻿using System;
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
using SE_Script_Library.Constructions;

namespace SE_Script_Library.TestScripts.Constructions
{
    class DrillTest : IngameScript
    {
        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        IMyShipController controller;

        void Main()
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, NoPassenger);
            if (blocks.Count == 0)
                throw new Exception("No ship controller (cockpit, etc.) available.");

            controller = blocks[0] as IMyShipController;
            Drill drill = new Drill(controller);

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyGyro>(blocks);
            drill.AddGyroskopes(blocks);

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks);
            drill.AddThrusters(blocks);

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(blocks);
            drill.AddSensors(blocks);

            debug.Append(drill.GetBounds().Center).AppendLine();

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
