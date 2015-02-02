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
    class EventTest : IngameScript
    {
        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        const string EventA = "Event A";
        const string EventB = "Event B";
        Dictionary<string, EventHandler> map = new Dictionary<string, EventHandler>();

        public event EventHandler ActionHandler;

        void Main()
        {
            Test test = new Test();

            map.Add(EventA, test.OnEventAReached);
            map.Add(EventB, test.OnEventBReached);

            string[] es = { "test", EventA, EventB, "blub", "" };

            for (int i = 0; i < es.Length; ++i)
            {
                string e = es[i];
                if (map.ContainsKey(e))
                {
                    map[e](this, EventArgs.Empty);
                }
                else
                {
                    debug.Append("Event " + e + " not accepted.").AppendLine();
                }
            }

            ActionHandler += test.OnEventAReached;
            ActionHandler += test.OnEventBReached;
            ActionHandler(this, EventArgs.Empty);
            ActionHandler -= test.OnEventAReached;
            ActionHandler -= test.OnEventBReached;

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

        class Test
        {
            public void OnEventAReached(object o, EventArgs e)
            {
                debug.Append("Handle EventA").AppendLine();
            }

            public void OnEventBReached(object o, EventArgs e)
            {
                debug.Append("Handle EventB").AppendLine();
            }
        }

    }

}
