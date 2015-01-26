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

    public class Drill : IngameScript
    {

        public static StringBuilder debug = new StringBuilder();

        const string debugName = "Debug";
        const string antennaName = "Working";
        const string stop = "Cmd Disable Auto";

        const int K = 1000;

        List<IMyShipConnector> ejectors = new List<IMyShipConnector>();

        Sandbox.Common.ObjectBuilders.MyObjectBuilderType ORE = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Ore);

        void Main()
        {
            // initialize
            VRage.MyFixedPoint totalVolume = 0;
            VRage.MyFixedPoint totalMaxVolume = 0;

            var blocks = new List<IMyTerminalBlock>();

            if (ejectors.Count == 0)
            {
                GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(blocks);
                for (int i = 0; i < blocks.Count; ++i)
                {
                    var block = blocks[i] as IMyShipConnector;

                    if (block.DefinitionDisplayNameText != "Ejector")
                        continue;

                    ejectors.Add(block);
                }

                if (ejectors.Count == 0)
                    ejectors.Add(blocks[0] as IMyShipConnector);

                for (int i = 0; i < blocks.Count; ++i)
                {
                    var block = blocks[i] as IMyShipConnector;
                    if (!block.ThrowOut)
                        block.GetActionWithName("ThrowOut").Apply(block);

                    block.GetActionWithName("OnOff_On").Apply(block);
                }
            }

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

                    var items = inv.GetItems();
                    for (int k = 0; k < items.Count; ++k)
                    {
                        var item = items[k];
                        if (item.Content.TypeId == ORE && item.Content.SubtypeName.Equals("Stone") && ejectors != null)
                        {
                            for (int l = 0; l < ejectors.Count; ++l)
                            {
                                var connInv = ((IMyInventoryOwner)ejectors[l]).GetInventory(0);
                                VRage.MyFixedPoint available = connInv.MaxVolume - connInv.CurrentVolume;
                                debug.Append(inv.TransferItemTo(connInv, k, 0, amount: VRage.MyFixedPoint.Min(available, item.Amount) * 3 * K)).AppendLine();
                            }
                        }
                    }
                }
            }

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(blocks, FilterAntenna);
            GridTerminalSystem.GetBlocksOfType<IMyBeacon>(blocks, FilterAntenna);
            if (blocks.Count == 0)
                throw new Exception("Did not find the specified antenna");

            var antenna = blocks[0];
            StringBuilder sb = new StringBuilder();
            sb.Append(antennaName).Append(" - ");
            sb.Append((long)(totalVolume * K)).Append(" / ").Append((long)(totalMaxVolume * K));
            sb.Append(" (").Append(VRageMath.MathHelper.RoundOn2(100 * (float)(totalVolume * K) / (float)(totalMaxVolume * K))).Append("%)");
            antenna.SetCustomName(sb.ToString());

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(blocks);
            for (int i = 0; i < blocks.Count; ++i)
            {
                var drillInv = (blocks[i] as IMyInventoryOwner).GetInventory(0);
                if (drillInv.IsFull || (float)(totalVolume * K) / (float)(totalMaxVolume * K) >= .95f)
                {
                    IMyTerminalBlock stopBlock = GridTerminalSystem.GetBlockWithName(stop);
                    if (stopBlock == null)
                        throw new Exception("Could not find block with name: '" + stop + "'");

                    var ejectorEnum = ejectors.GetEnumerator();
                    while (ejectorEnum.MoveNext())
                    {
                        var ejector = ejectorEnum.Current;
                        if (ejector.ThrowOut)
                            ejector.GetActionWithName("ThrowOut").Apply(ejector);

                    }
                    stopBlock.GetActionWithName("TriggerNow").Apply(stopBlock);
                }
            }

            Debug(debug.ToString());
            debug.Clear();
        }

        private bool FilterInventoryOwner(IMyTerminalBlock arg)
        {
            return arg != null && !(arg is IMyReactor) && !(arg is IMyShipConnector);
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
