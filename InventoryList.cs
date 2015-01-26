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

    public class InventoryList : IngameScript
    {

        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        private static Dictionary<Sandbox.Common.ObjectBuilders.MyObjectBuilderType, string> typeNames = new Dictionary<Sandbox.Common.ObjectBuilders.MyObjectBuilderType, string>() {
            {(Sandbox.Common.ObjectBuilders.MyObjectBuilderType)typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Ingot), "Ingot"},
            {(Sandbox.Common.ObjectBuilders.MyObjectBuilderType)typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Ore), "Ore"},
            {(Sandbox.Common.ObjectBuilders.MyObjectBuilderType)typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Component), "Component"},
            {(Sandbox.Common.ObjectBuilders.MyObjectBuilderType)typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_PhysicalGunObject), "PhysicalGunObject"},
            {(Sandbox.Common.ObjectBuilders.MyObjectBuilderType)typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_AmmoMagazine), "AmmoMagazine"}
        };

        public struct MyDefinitionId : IEquatable<MyDefinitionId>
        {
            public static readonly MyDefinitionId.DefinitionIdComparerType Comparer =
                new MyDefinitionId.DefinitionIdComparerType();

            public readonly Sandbox.Common.ObjectBuilders.MyObjectBuilderType TypeId;
            public readonly string SubtypeName;

            public string SubtypeId
            {
                get { return SubtypeName; }
            }

            public MyDefinitionId(Sandbox.Common.ObjectBuilders.MyObjectBuilderType type)
            {
                TypeId = type;
                SubtypeName = (string)null;
            }

            public MyDefinitionId(Sandbox.Common.ObjectBuilders.MyObjectBuilderType type, string subtypeName)
            {
                TypeId = type;
                SubtypeName = subtypeName;
            }

            public static bool operator ==(MyDefinitionId l, MyDefinitionId r)
            {
                return l.Equals(r);
            }

            public static bool operator !=(MyDefinitionId l, MyDefinitionId r)
            {
                return !l.Equals(r);
            }

            public override int GetHashCode()
            {
                return this.TypeId.GetHashCode() << 16 ^ this.SubtypeId.GetHashCode();
            }

            public long GetHashCodeLong()
            {
                return (long)this.TypeId.GetHashCode() << 32 ^ (long)this.SubtypeId.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is MyDefinitionId)
                    return this.Equals((MyDefinitionId)obj);
                else
                    return false;
            }

            public override string ToString()
            {
                return string.Format("{0}/{1}", !this.TypeId.IsNull ? (object)this.TypeId.ToString() : (object)"(null)",
                    !string.IsNullOrEmpty(this.SubtypeName) ? (object)this.SubtypeName : (object)"(null)");
            }

            public bool Equals(MyDefinitionId other)
            {
                if (this.TypeId == other.TypeId)
                    return this.SubtypeName == other.SubtypeName;
                else
                    return false;
            }

            public class DefinitionIdComparerType : IEqualityComparer<MyDefinitionId>
            {
                public bool Equals(MyDefinitionId x, MyDefinitionId y)
                {
                    if (x.TypeId == y.TypeId)
                        return x.SubtypeName == y.SubtypeName;
                    else
                        return false;
                }

                public int GetHashCode(MyDefinitionId obj)
                {
                    return obj.GetHashCode();
                }
            }
        }

        void Main()
        {
            // initialize
            Dictionary<MyDefinitionId, VRage.MyFixedPoint> inventory = new Dictionary<MyDefinitionId, VRage.MyFixedPoint>();

            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(blocks);
            string typeName = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Ingot).ToString();
            debug.Append(typeName).AppendLine();

            if (blocks.Count == 0)
                throw new Exception("Did not find any cargo container.");

            for (int i = 0; i < blocks.Count; ++i)
            {

                var invOwner = blocks[i] as IMyInventoryOwner;
                for (int j = 0; j < invOwner.InventoryCount; ++j)
                {
                    var inv = invOwner.GetInventory(j);
                    var items = inv.GetItems();
                    for (int k = 0; k < items.Count; ++k)
                    {
                        var item = items[k];
                        var key = new MyDefinitionId(item.Content.TypeId, item.Content.SubtypeName);
                        //contents.Add(key);
                        if (!inventory.ContainsKey(key))
                            inventory[key] = item.Amount;
                        else
                            inventory[key] = inventory[key] + item.Amount;
                    }
                }
            }

            var enumerator = inventory.Keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Current;
                if (key.TypeId != typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Ingot))
                    continue;
                debug.Append(typeNames[key.TypeId]).Append(key.SubtypeName).Append(" = ").Append(inventory[key]).AppendLine();
            }

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

    }

}
