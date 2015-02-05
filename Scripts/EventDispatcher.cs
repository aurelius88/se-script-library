using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;

namespace SE_Script_Library.Scripts
{
    class EventDispatcher : IngameScript
    {
        /// SET COMMAND HERE
        private const string Command = Start;

        // RESTRICTED AREA //
        private const string Reset = "reset";
        private const string Stop = "stop";
        private const string Start = "start";
        private const string Found = "found";
        private const string Lost = "lost";
        private const string Debug = "debug";

        private const string InputString = "xIn#0:";

        void Main()
        {
            var list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(InputString, list);
            if (list.Count == 0)
            {
                throw new Exception("waah"); // Exception
            }
            else
            {
                var block = list[0];
                block.SetCustomName(InputString + Command);
                block.ApplyAction("Run");
            }
        }
    }
}
