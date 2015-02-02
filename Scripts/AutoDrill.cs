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
        private const string InputString = "xIn#0:";
        private const string OutputString = "xOut#0:";
        private const string Seperator = ",";

        // Commands
        private const string Reset = "reset";
        private const string Stop = "stop";
        private const string Start = "start";
        private const string Found = "found";
        private const string Lost = "lost";
        private const string Debug = "debug";

        private Drill drill;

        private Dictionary<string, Action<Drill.DrillEvent>> EventHandler = new Dictionary<string, Action<Drill.DrillEvent>>();
        private static Dictionary<string, Drill.DrillEvent> EventMap = new Dictionary<string, Drill.DrillEvent>() {
            {Reset, Drill.DrillEvent.Nothing},
            {Stop, Drill.DrillEvent.DrillStopInvoked},
            {Start, Drill.DrillEvent.DrillStartInvoked},
            {Found, Drill.DrillEvent.AsteroidFound},
            {Lost, Drill.DrillEvent.AsteroidLost},
            {Debug,Drill.DrillEvent.Nothing}
        };

        private bool DebugActive = false;

        void Main()
        {
            var list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(InputString, list);
            if (list.Count == 0)
                throw new Exception("Please mark a block with '" + InputString + "' for input commands.");

            var block = list[0];
            string[] tokens = block.CustomName.Split(':');
            tokens = tokens[1].Split(Seperator.ToCharArray());
            block.SetCustomName(InputString);

            if (drill == null)
                ResetDrill(Drill.DrillEvent.Nothing);

            Output(OutputString + '\n');
            Output(drill.CurrentState + '\n', true);
            for (int i = 0; i < tokens.Length; ++i)
            {
                string token = tokens[i].Trim().ToLower();
                if (token.Length == 0)
                    continue;

                Action<Drill.DrillEvent> action;
                if (!EventHandler.TryGetValue(token, out action))
                {
                    Output("-> Warning! Command not accepted: '" + token + "'\n", true);
                    continue;
                }

                Drill.DrillEvent drillEvent;
                if (!EventMap.TryGetValue(token, out drillEvent))
                {
                    Output("-> Warning! There exists no mapping for '" + token + "'\n", true);
                    continue;
                }

                if (DebugActive)
                    Output("Map '" + token + "' -> '" + drillEvent + "' and use '" + action + "'\n", true);
                action(drillEvent);
            }

            Output(drill.CurrentState, true);
        }

        private bool NoPassenger(IMyTerminalBlock arg)
        {
            return !arg.DefinitionDisplayNameText.Equals("Passenger Seat");
        }

        void Output(String message, bool append = false)
        {
            var list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(OutputString, list);
            if (list.Count > 0)
            {
                list[0].SetCustomName(append ? list[0].CustomName + message : message);
            }
        }

        void HandleDebug(Drill.DrillEvent e)
        {
            DebugActive = !DebugActive;
            Output("Toggle debug mode to '" + DebugActive + "'", true);
        }

        void ResetDrill(Drill.DrillEvent e)
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, NoPassenger);
            if (blocks.Count == 0)
                throw new Exception("No ship controller (cockpit, etc.) available.");

            drill = new Drill(blocks[0] as IMyShipController);

            EventHandler.Clear();
            EventHandler.Add(Reset, ResetDrill);
            EventHandler.Add(Start, drill.handle);
            EventHandler.Add(Stop, drill.handle);
            EventHandler.Add(Found, drill.handle);
            EventHandler.Add(Lost, drill.handle);
            EventHandler.Add(Debug, HandleDebug);

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyGyro>(blocks);
            drill.AddGyroskopes(blocks);
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks);
            drill.AddThrusters(blocks);
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(blocks);
            drill.AddSensors(blocks);
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(blocks);
            drill.AddDrills(blocks);
            drill.handle(Drill.DrillEvent.DrillInitialized);
        }
    }
}
