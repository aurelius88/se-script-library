
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
    public class AntennaGPS : IngameScript
    {

        void Main()
        {
            StringBuilder sb = new StringBuilder();
            var list = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(list);
            if (list.Count > 0)
            {
                var block = list[0];
                sb.Append(VRageMath.Vector3I.Round(block.GetPosition()));
                block.SetCustomName(sb.ToString());
            }
        }
    }
}
