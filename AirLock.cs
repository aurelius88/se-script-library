
using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.Input;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.Game.Weapons;
using Sandbox.Definitions;
using Sandbox.Engine;
using VRage.Common;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace IngameProg
{
    public class AirLock : MyGridProgram
    {
        private const char ArgumentSep = ';';
        private const char CommandSep = '#';
        private const char StorageSep = '\n';
        private readonly char[] SplitArgumentSep = new char[] { ArgumentSep };
        private readonly char[] SplitCommandSep = new char[] { CommandSep };

        private struct Command
        {
            // 0 args
            public const string List = "list";
            public const string Reset = "reset";
            public const string Help = "help";
            // 1 arg
            public const string Get = "get";
            public const string Remove = "remove";
            public const string OpenInner = "openinner";
            public const string OpenOuter = "openouter";
            // 4 args
            public const string Add = "add";
        }
        private string _lastKey;
        private string _openPosition;
        private float _oxyLevelBefore = -1f;
        //private IMyProgrammableBlock loopProg;

        public class Configuration
        {
            public IMyDoor outerDoor;
            public IMyDoor innerDoor;
            public IMyAirVent airVent;
            public IMyTimerBlock timer;
            public Action<string> State;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder("## Lock Room '");
                sb.Append(airVent.CustomName).AppendLine("' ##");
                sb.Append("Air Vent = '").Append(airVent.CustomName).AppendLine("'");
                sb.Append("Outer Door = '").Append(outerDoor.CustomName).AppendLine("'");
                sb.Append("Inner Door = '").Append(innerDoor.CustomName).AppendLine("'");
                sb.Append("Timer Block = '").Append(timer.CustomName).AppendLine("'");
                sb.Append("State = '").Append(State.Method.Name).AppendLine("'");
                return sb.ToString();
            }
        }


        private Dictionary<string, Configuration> _configs;
        private IMyProgrammableBlock _logger;

        void Main(string argument)
        {
            if (_logger == null)
                _logger = (IMyProgrammableBlock)GridTerminalSystem.GetBlockWithName("Prog Logger");
            if (_configs == null)
            {
                _configs = new Dictionary<string, Configuration>();
                ParseStorage();
            }
            // parse arguments
            ParseArgument(argument);

        }

        void ParseStorage()
        {
            string[] commands = Storage.Split(StorageSep);
            for (int i = 0; i < commands.Length; i++)
            {
                if (commands[i].Length > 0)
                {
                    Log("Storage Parse command: " + commands[i].Replace(ArgumentSep, ','));
                    ParseArgument(commands[i]);
                }
            }
        }

        void ParseArgument(string argument)
        {
            if (argument == null)
            {
                PrintUsage();
                return;
            }
            string[] commandPair = argument.Split(SplitCommandSep, 2);
            if (commandPair.Length == 1)
            {
                switch (commandPair[0].ToLower())
                {
                    case Command.List:
                        ListLockRooms();
                        break;
                    case Command.Reset:
                        _configs.Clear();
                        UpdateStorage();
                        _lastKey = null;
                        break;
                    case Command.Help:
                        PrintUsage();
                        break;
                    default:
                        ReenterFromLoop(argument);
                        break;
                }
            }
            else if (commandPair.Length == 2)
            {
                string[] args = commandPair[1].Split(ArgumentSep);
                Echo("Command=" + commandPair[0]);
                PrintArgs(args);
                switch (commandPair[0].ToLower())
                {
                    case Command.Add:
                        if (args.Length < 4)
                        {
                            PrintAddUsage();
                        }
                        else
                        {
                            AddLockRoom(args);
                            TryPrintIgnoredArguments(args, 4);
                        }
                        break;
                    case Command.Remove:
                        RemoveLockRoom(args[0]);
                        TryPrintIgnoredArguments(args, 1);
                        break;
                    case Command.OpenInner:
                        OpenInnerDoor(args[0]);
                        TryPrintIgnoredArguments(args, 1);
                        break;
                    case Command.OpenOuter:
                        OpenOuterDoor(args[0]);
                        TryPrintIgnoredArguments(args, 1);
                        break;
                    case Command.Get:
                        GetLockRoom(args[0]);
                        TryPrintIgnoredArguments(args, 1);
                        break;
                    default:
                        PrintUsage();
                        break;
                }
            }
        }

        void TryPrintIgnoredArguments(string[] args, int expectedArgSize)
        {
            if (args.Length > expectedArgSize)
            {
                Echo("Ignored arguments:");
                for (int i = expectedArgSize; i < args.Length; i++)
                {
                    Echo("->" + args[i]);
                }
            }
        }

        void PrintUsage()
        {
            Echo("Usage:");
            PrintListUsage();
            PrintGetUsage();
            PrintAddUsage();
            PrintOpenOuterUsage();
            PrintOpenInnerUsage();
            PrintRemoveUsage();
            PrintResetUsage();
            Echo("! Make sure the names are UNIQUE or the wrong door might be chosen.");
            Echo("! Commands are NOT case-sensitive.");
        }

        void PrintAddUsage()
        {
            Echo("-> Type '" + Command.Add + CommandSep + "Air Vent;Outer Door;Inner Door;Timer Block'" +
                " to add a new Air Lock Room. Where");
            Echo("1.  'Air Vent' is the name of the air vent inside the lock room. (used as key)");
            Echo("2.  'Outer Door' is the name of the door facing the outside.");
            Echo("3.  'Inner Door' is the name of the door facing the inside.");
            Echo("4.  'Timer Block' is the name of the timer block that runs this program without argument. (for loops)");
        }

        void PrintRemoveUsage()
        {
            Echo("-> Type '" + Command.Remove + CommandSep + "Air Vent'" +
                " to remove an existing Air Lock Room. Where");
            Echo("'Air Vent' is the name of the air vent inside the lock room. (used as key)");
        }

        void PrintGetUsage()
        {
            Echo("-> Type '" + Command.Get + CommandSep + "Air Vent'" +
                " to get information about an specific existing Air Lock Room. Where");
            Echo("'Air Vent' is the name of the air vent inside the lock room. (used as key)");
        }

        void PrintOpenInnerUsage()
        {
            Echo("-> Type '" + Command.OpenInner + CommandSep + "Air Vent'" +
                " to open the inner door of the Air Lock Room. Where");
            Echo("'Air Vent' is the name of the air vent inside the lock room. (used as key)");
        }

        void PrintOpenOuterUsage()
        {
            Echo("-> Type '" + Command.OpenOuter + CommandSep + "Air Vent'" +
                " to open the inner door of the Air Lock Room. Where");
            Echo("'Air Vent' is the name of the air vent inside the lock room. (used as key)");
        }

        void PrintResetUsage()
        {
            Echo("-> Type '" + Command.Reset + "' to remove all existing Air Lock Rooms.");
        }

        void PrintListUsage()
        {
            Echo("-> Type '" + Command.List + "' to list all existing Air Lock Rooms.");
        }

        void GetLockRoom(string key)
        {
            Configuration config;
            if (_configs.TryGetValue(key, out config))
            {
                Echo(config.ToString());
            }
            else
            {
                Echo("Program has not been initialized or wrong key has been used. Used key was '" + key + "'");
            }
        }

        void PrintArgs(string[] args)
        {
            StringBuilder sb = new StringBuilder("Arguments(");
            sb.Append(args.Length).Append(")=");
            for (int i = 0; i < args.Length; i++)
            {
                sb.Append("'").Append(args[i]).Append("'");
                if (i < args.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.AppendLine();
            Echo(sb.ToString());
        }

        void AddLockRoom(string[] args)
        {
            string key = args[0];
            // init
            if (!_configs.ContainsKey(key))
            {
                Configuration config = new Configuration();
                config.airVent = GridTerminalSystem.GetBlockWithName(args[0]) as IMyAirVent;
                config.outerDoor = GridTerminalSystem.GetBlockWithName(args[1]) as IMyDoor;
                config.innerDoor = GridTerminalSystem.GetBlockWithName(args[2]) as IMyDoor;
                config.timer = GridTerminalSystem.GetBlockWithName(args[3]) as IMyTimerBlock;

                bool error = false;
                if (config.airVent == null)
                {
                    Echo("Could not find Air Vent with name '" + args[0] + "'.");
                    error = true;
                }
                if (config.outerDoor == null)
                {
                    Echo("Could not find Door with name '" + args[1] + "'.");
                    error = true;
                }
                if (config.innerDoor == null)
                {
                    Echo("Could not find Door with name '" + args[2] + "'.");
                    error = true;
                }
                if (config.timer == null)
                {
                    Echo("Could not find Timer Block with name '" + args[3] + "'.");
                    error = true;
                }
                if (error) return;

                // every thing fine

                config.State = OuterClosingEnter;
                _configs.Add(key, config);
                _lastKey = key;
                _openPosition = Command.OpenInner;
                UpdateStorage();

                Echo("Added new Air Lock Room.");
                PrintOpenInnerUsage();
                PrintOpenOuterUsage();

                // execute for initial setup
                _configs[key].State(key);
            }
            else
            {
                Configuration config = _configs[key];
                Echo("This Air Lock Room has already been saved and is ready to use.");
                PrintOpenInnerUsage();
                PrintOpenOuterUsage();
                GetLockRoom(key);
            }
        }

        void UpdateStorage()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, Configuration>.ValueCollection.Enumerator it = _configs.Values.GetEnumerator();
            bool hasNext = it.MoveNext();

            while (hasNext)
            {
                sb.Append(Command.Add).Append(CommandSep);
                sb.Append(it.Current.airVent.CustomName).Append(ArgumentSep);
                sb.Append(it.Current.outerDoor.CustomName).Append(ArgumentSep);
                sb.Append(it.Current.innerDoor.CustomName).Append(ArgumentSep);
                sb.Append(it.Current.timer.CustomName);
                hasNext = it.MoveNext();
                if (hasNext) sb.Append(StorageSep);
            }
            Storage = sb.ToString();
            Log("Update Storage:\n" + Storage.Replace(ArgumentSep, ','));
        }

        void RemoveLockRoom(string key)
        {
            if (key != null && _configs.Remove(key))
            {
                Echo("Removed Air Lock Room '" + key + "'.");
                UpdateStorage();
            }
            else
            {
                Echo("Nothing to remove.");
            }
        }

        void OpenInnerDoor(string key)
        {
            OpenDoor(key, Command.OpenInner);
        }

        void OpenOuterDoor(string key)
        {
            OpenDoor(key, Command.OpenOuter);
        }

        void OpenDoor(string key, string doorPos)
        {
            if (!_configs.ContainsKey(key))
            {
                Echo("Program has not been initialized or wrong key has been used. Used key was '" + key + "'\n");
                ListLockRooms();
            }
            else
            {
                _lastKey = key;
                _openPosition = doorPos;
                _configs[key].State(key);
            }
        }

        void ListLockRooms()
        {
            Echo("Saved " + _configs.Count + " Air Vent Locks:");
            Dictionary<string, Configuration>.Enumerator it = _configs.GetEnumerator();
            while (it.MoveNext())
            {
                Echo("-> " + it.Current.Value);
            }
        }

        void ReenterFromLoop(string argument)
        {
            string key;
            if (argument.Length == 0)
            {
                key = _lastKey;
            }
            else
            {
                key = argument;
            }
            if (key != null && _configs.ContainsKey(key))
            {
                // loop
                Configuration config = _configs[key];
                config.State(key);
            }
            else
            {
                Echo("Failed to loop. There is no Air Lock Room with key '" + key + "'.\n");
                PrintUsage();
            }
        }

        // States

        void InnerOpened(string key)
        {
            Log("InnerOpened");
            Configuration config = _configs[key];
            config.innerDoor.ApplyAction("OnOff_Off");
            config.State = InnerClosingEnter;
            Log("InnerClosingEnter");

        }

        void InnerClosingEnter(string key)
        {
            if (_openPosition.Equals(Command.OpenOuter))
            {
                Configuration config = _configs[key];
                config.innerDoor.ApplyAction("OnOff_On");
                config.innerDoor.ApplyAction("Open_Off");
                config.airVent.ApplyAction("Depressurize_On");
                NextState(key, InnerClosing);
            }
        }

        void InnerClosing(string key)
        {
            Configuration config = _configs[key];
            float openRatio = config.innerDoor.OpenRatio;
            Log("State=" + config.State.Method.Name
                + "\nOxy-Level=" + config.airVent.GetOxygenLevel()
                + "\nInner Door OpenRatio=" + openRatio + "\n"
                + (config.innerDoor.Open ? "Open" : "Closed"));
            if (openRatio <= 0f)
            {
                Log("->Inner Door Closed");
                // Inner door closed
                config.innerDoor.ApplyAction("OnOff_Off");
                _oxyLevelBefore = -1f;
                NextState(key, Depressurizing);
            }
            else
            {
                Loop(key);
            }
        }

        bool AreOxygenTanksFull()
        {
            List<IMyTerminalBlock> oxygenTanks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyOxygenTank>(oxygenTanks);
            for (int i = 0; i < oxygenTanks.Count; i++)
            {
                IMyOxygenTank tank = (IMyOxygenTank)oxygenTanks[i];
                if (tank.GetOxygenLevel() < 1f)
                {
                    return false;
                }
            }
            return true;
        }

        bool AreOxygenTanksEmpty()
        {
            List<IMyTerminalBlock> oxygenTanks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyOxygenTank>(oxygenTanks);
            for (int i = 0; i < oxygenTanks.Count; i++)
            {
                IMyOxygenTank tank = (IMyOxygenTank)oxygenTanks[i];
                if (tank.GetOxygenLevel() > 0f)
                {
                    return false;
                }
            }
            return true;
        }

        float AverageOxygenLevel()
        {
            List<IMyTerminalBlock> oxygenTanks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyOxygenTank>(oxygenTanks);
            float sum = 0f;
            int count = oxygenTanks.Count;
            for (int i = 0; i < count; i++)
            {
                IMyOxygenTank tank = (IMyOxygenTank)oxygenTanks[i];
                sum += tank.GetOxygenLevel();
            }
            return sum / (float)count;
        }

        void Depressurizing(string key)
        {
            Configuration config = _configs[key];
            float oxyLevel = config.airVent.GetOxygenLevel();
            Log("State=" + config.State.Method.Name
                + "\nOxy-Level=" + oxyLevel);
            if (oxyLevel == _oxyLevelBefore && config.airVent.IsDepressurizing)
            {
                bool oxygenTanksFull = AreOxygenTanksFull();
                if (oxygenTanksFull)
                {
                    NextState(key, OuterOpeningEnter);
                }
            }
            if (oxyLevel == 0f)
            {
                NextState(key, OuterOpeningEnter);
            }
            else
            {
                _oxyLevelBefore = oxyLevel;
                Loop(key);
            }
        }


        void OuterOpeningEnter(string key)
        {
            Configuration config = _configs[key];
            Log("->Depressurized");

            // Oxygen gone -> Open outer door
            config.outerDoor.ApplyAction("OnOff_On");
            config.outerDoor.ApplyAction("Open_On");
            NextState(key, OuterOpening);
        }

        void OuterOpening(string key)
        {

            Configuration config = _configs[key];
            float oxyLevel = config.airVent.GetOxygenLevel();
            float openRatio = config.outerDoor.OpenRatio;
            Log("State=" + config.State.Method.Name
                + "\nOxy-Level=" + oxyLevel
                + "\nOuter Door OpenRatio=" + openRatio + "\n"
                + (config.outerDoor.Open ? "Open" : "Closed"));
            if (openRatio >= 1f)
            {
                Log("->Outer Door Opened");

                // Outer door opened
                config.outerDoor.ApplyAction("OnOff_Off");
                config.State = OuterClosingEnter;
            }
            else
            {
                Loop(key);
            }
        }

        void OuterClosingEnter(string key)
        {
            if (_openPosition.Equals(Command.OpenInner))
            {
                Log("OuterClosingInnerOpeningEnter");
                Configuration config = _configs[key];
                // close outer
                config.outerDoor.ApplyAction("OnOff_On");
                config.outerDoor.ApplyAction("Open_Off");
                NextState(key, OuterClosing);
            }
        }

        void OuterClosing(string key)
        {
            Configuration config = _configs[key];
            float openRatio = config.outerDoor.OpenRatio;
            Log("State=" + config.State.Method.Name
                + "\nOxy-Level=" + config.airVent.GetOxygenLevel()
                + "\nOuter Door OpenRatio=" + openRatio + "\n"
                + (config.outerDoor.Open ? "Open" : "Closed"));
            if (openRatio <= 0f)
            {
                Log("->Outer Door Closed");
                // Outer door closed
                config.outerDoor.ApplyAction("OnOff_Off");
                // fill room with oxygen
                config.airVent.ApplyAction("Depressurize_Off");
                _oxyLevelBefore = -1f;
                NextState(key, Pressurizing);
            }
            else
            {
                Loop(key);
            }
        }

        void Pressurizing(string key)
        {
            Configuration config = _configs[key];
            float oxyLevel = config.airVent.GetOxygenLevel();
            Log("State=" + config.State.Method.Name
                + "\nOxy-Level=" + oxyLevel);

            if (oxyLevel == _oxyLevelBefore && !config.airVent.IsDepressurizing)
            {
                bool oxygenTanksEmpty = AreOxygenTanksEmpty();
                if (oxygenTanksEmpty)
                {
                    NextState(key, InnerOpeningEnter);
                }
            }
            if (oxyLevel >= 1f)
            {
                NextState(key, InnerOpeningEnter);
            }
            else
            {
                _oxyLevelBefore = oxyLevel;
                Loop(key);
            }
        }

        void InnerOpeningEnter(string key)
        {
            Configuration config = _configs[key];
            Log("->Depressurized");

            // Oxygen present -> Open inner door
            config.innerDoor.ApplyAction("OnOff_On");
            config.innerDoor.ApplyAction("Open_On");
            NextState(key, InnerOpening);
        }

        void InnerOpening(string key)
        {
            Configuration config = _configs[key];
            float openRatio = config.innerDoor.OpenRatio;
            Log("State=" + config.State.Method.Name
                + "\nOxy-Level=" + config.airVent.GetOxygenLevel()
                + "\nInner Door OpenRatio=" + openRatio + "\n"
                + (config.innerDoor.Open ? "Open" : "Closed"));
            if (openRatio >= 1f)
            {
                Log("->Inner Door Opened");
                // Inner door opened
                config.innerDoor.ApplyAction("OnOff_Off");
                NextState(key, InnerOpened);
            }
            else
            {
                Loop(key);
            }
        }


        void NextState(string key, Action<string> state)
        {
            Configuration config = _configs[key];
            Log("Transition (" + key + ")\n" + config.State.Method.Name + " -> " + state.Method.Name);
            config.State = state;
            state(key);
        }

        void Loop(string key)
        {
            _configs[key].timer.ApplyAction("TriggerNow");
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="msg">The message to be logged.</param>
        void Log(string msg)
        {
            if (_logger != null)
                _logger.ApplyAction("Run",
                    new List<TerminalActionParameter>() { 
                        TerminalActionParameter.Get("log="+msg)  
                });
        }

    }
}

