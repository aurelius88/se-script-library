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
        const string stop = "xIn#0";

        const int K = 1000;

        List<IMyShipConnector> ejectors;
        List<IMyShipDrill> drills;

        Sandbox.Common.ObjectBuilders.MyObjectBuilderType ORE = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Ore);

        void Main()
        {
            // initialize
            VRage.MyFixedPoint totalVolume = 0;
            VRage.MyFixedPoint totalMaxVolume = 0;

            var blocks = new List<IMyTerminalBlock>();

            if (ejectors == null)
            {
                ejectors = new List<IMyShipConnector>();
                GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(blocks, FilterEjector);
                if (ejectors.Count == 0)
                    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(blocks);

                for (int i = 0; i < blocks.Count; ++i)
                {
                    ejectors.Add(blocks[i] as IMyShipConnector);
                }
            }

            if (drills == null)
            {
                drills = new List<IMyShipDrill>();
                GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(blocks);
                for (int i = 0; i < blocks.Count; ++i)
                {
                    drills.Add(blocks[i] as IMyShipDrill);
                }
            }

            GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(blocks, FilterInventoryOwner);

            if (blocks.Count == 0)
                throw new Exception("Did not find any cargo container.");

            for (int i = 0; i < ejectors.Count; ++i)
            {
                var invOwner = ejectors[i] as IMyInventoryOwner;
                IMyInventory inv = invOwner.GetInventory(0);
                var items = inv.GetItems();
                for (int j = 0; j < items.Count; ++j)
                {
                    IMyInventoryItem item = items[j];
                    if (item.Content.TypeId == ORE && item.Content.SubtypeName.Equals("Stone"))
                        continue;

                    VRage.MyFixedPoint amount = item.Amount;
                    for (int k = 0; k < drills.Count; ++k)
                    {
                        var connInv = ((IMyInventoryOwner)drills[k]).GetInventory(0);
                        VRage.MyFixedPoint available = connInv.MaxVolume - connInv.CurrentVolume;
                        if (available < 0)
                            continue;

                        debug.Append("available = ").Append(available).AppendLine();
                        debug.Append("amount = ").Append(item.Amount).AppendLine();
                        VRage.MyFixedPoint transAmount = VRage.MyFixedPoint.Min(available * 3 * K, item.Amount);
                        if(inv.TransferItemTo(connInv, j, connInv.GetItems().Count, true, transAmount))
                            amount -= transAmount;

                        if (amount <= 0)
                            break;
                    }
                }
            }

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
                        if (item.Content.TypeId == ORE && item.Content.SubtypeName.Equals("Stone"))
                        {
                            for (int l = 0; l < ejectors.Count; ++l)
                            {
                                if (!ejectors[l].Enabled)
                                    continue;

                                var connInv = ((IMyInventoryOwner)ejectors[l]).GetInventory(0);
                                VRage.MyFixedPoint available = connInv.MaxVolume - connInv.CurrentVolume;
                                inv.TransferItemTo(connInv, k, 0, true, amount: VRage.MyFixedPoint.Min(available, item.Amount) * 3 * K);
                            }
                        }
                    }
                }
            }

            for (int l = 0; l < ejectors.Count; ++l)
            {
                var ejector = ejectors[l];
                if (!ejector.Enabled)
                    continue;

                var connInv = ((IMyInventoryOwner)ejector).GetInventory(0);
                var items = connInv.GetItems();
                if (items.Count == 0)
                {
                    if (ejector.ThrowOut)
                        ejector.GetActionWithName("ThrowOut").Apply(ejector);

                    continue;
                }

                var item = items[0];
                if (item.Content.TypeId == ORE && item.Content.SubtypeName.Equals("Stone") && !ejector.ThrowOut
                    || (item.Content.TypeId != ORE || !item.Content.SubtypeName.Equals("Stone")) && ejector.ThrowOut)
                    ejector.GetActionWithName("ThrowOut").Apply(ejector);

            }

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBeacon>(blocks, FilterAntenna);
            if (blocks.Count == 0)
            {
                GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(blocks, FilterAntenna);
            }
            if (blocks.Count == 0)
                throw new Exception("Did not find the specified antenna");

            var antenna = blocks[0];
            StringBuilder sb = new StringBuilder();
            sb.Append(antennaName).Append(" - ");
            sb.Append((long)(totalVolume * K)).Append(" / ").Append((long)(totalMaxVolume * K));
            sb.Append(" (").Append(VRageMath.MathHelper.RoundOn2(100 * (float)(totalVolume * K) / (float)(totalMaxVolume * K))).Append("%)");
            antenna.SetCustomName(sb.ToString());


            // stop condition

            for (int i = 0; i < drills.Count; ++i)
            {
                var drillInv = (drills[i] as IMyInventoryOwner).GetInventory(0);
                if (drillInv.IsFull)
                {
                    stopDrill();
                    break;
                }
            }

            Debug(debug.ToString());
            debug.Clear();
        }

        private bool FilterEjector(IMyTerminalBlock arg)
        {
            return arg.DefinitionDisplayNameText == "Ejector";
        }

        private void stopDrill()
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(stop, blocks);
            if (blocks.Count == 0)
                throw new Exception("Could not find block with name: '" + stop + "'");

            var ejectorEnum = ejectors.GetEnumerator();
            while (ejectorEnum.MoveNext())
            {
                var ejector = ejectorEnum.Current;
                if (ejector.ThrowOut)
                    ejector.GetActionWithName("ThrowOut").Apply(ejector);

            }

            var block = blocks[0];
            block.SetCustomName(block.CustomName + "," + "full");
            block.ApplyAction("Run");
        }

        private bool FilterInventoryOwner(IMyTerminalBlock arg)
        {
            return !(arg is IMyReactor) && !(arg is IMyShipConnector && ((IMyShipConnector)arg).Enabled);
        }

        private bool FilterAntenna(IMyTerminalBlock arg)
        {
            return arg.CustomName.Contains(antennaName);
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
