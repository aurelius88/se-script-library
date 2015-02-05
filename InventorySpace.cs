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

    public class InventorySpace : IngameScript
    {

        public static StringBuilder debug = new StringBuilder();

        const string debugName = "Debug";
        const string antennaName = "Working";
        const string stop = "xIn#0";

        const int K = 1000;

        void Main()
        {
            // initialize
            VRage.MyFixedPoint totalVolume = 0;
            VRage.MyFixedPoint totalMaxVolume = 0;

            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(blocks, FilterInventoryOwner);

            if (blocks.Count == 0)
                throw new Exception("Did not find any cargo container.");

            for (int i = 0; i < blocks.Count; ++i)
            {
                var invOwner = blocks[i] as IMyInventoryOwner;
                for (int j = 0; j < invOwner.InventoryCount; ++j)
                {
                    var inv = invOwner.GetInventory(j);
                    totalVolume += inv.CurrentVolume;
                    totalMaxVolume += inv.MaxVolume;
                }
            }

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBeacon>(blocks, FilterAntenna);
            GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(blocks, FilterAntenna);
            if (blocks.Count == 0)
                throw new Exception("Did not find the specified antenna");

            var antenna = blocks[0];
            StringBuilder sb = new StringBuilder();
            sb.Append(antennaName).Append(" - ");
            sb.Append((long)(totalVolume * K)).Append(" / ").Append((long)(totalMaxVolume * K));
            sb.Append(" (").Append(VRageMath.MathHelper.RoundOn2(100 * (float)(totalVolume * K) / (float)(totalMaxVolume * K))).Append("%)");
            antenna.SetCustomName(sb.ToString());

            if (totalVolume == totalMaxVolume)
            {
                IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(stop);
                if (block == null)
                    throw new Exception("Could not find block with name: '" + stop + "'");

                block.SetCustomName(block.CustomName + "full,");
                block.ApplyAction("Run");
            }


            Debug(debug.ToString());
            debug.Clear();
        }

        private bool FilterInventoryOwner(IMyTerminalBlock arg)
        {
            return arg != null && !(arg is IMyReactor);
        }

        private bool FilterAntenna(IMyTerminalBlock arg)
        {
            return arg != null && arg.CustomName.Contains(antennaName);
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
