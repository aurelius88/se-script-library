using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Game;


namespace TestScript
{

    public class InventoryList : IngameScript
    {

        class ItemTypes
        {
            public static readonly VRage.ObjectBuilders.MyObjectBuilderType ORE = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Ore);
            public static readonly VRage.ObjectBuilders.MyObjectBuilderType COMPONENT = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Component);
            public static readonly VRage.ObjectBuilders.MyObjectBuilderType INGOT = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Ingot);
            public static readonly VRage.ObjectBuilders.MyObjectBuilderType AMMO = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_AmmoMagazine);
            public static readonly VRage.ObjectBuilders.MyObjectBuilderType GUN = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_PhysicalGunObject);
            public static readonly VRage.ObjectBuilders.MyObjectBuilderType PHYSICAL_OBJECT = typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_PhysicalObject);
        }



        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";
        private Dictionary<VRage.ObjectBuilders.MyObjectBuilderType, string> typeNames = new Dictionary<VRage.ObjectBuilders.MyObjectBuilderType, string>() { 
            {ItemTypes.INGOT, "Ingot"}, 
            {ItemTypes.ORE, "Ore"}, 
            {ItemTypes.COMPONENT, "Component"}, 
            {ItemTypes.PHYSICAL_OBJECT, "PhysicalGunObject"}, 
            {ItemTypes.AMMO, "AmmoMagazine"} 
        };

        private Dictionary<string, VRage.ObjectBuilders.MyObjectBuilderType> nameTypes = new Dictionary<string, VRage.ObjectBuilders.MyObjectBuilderType>() { 
            {"Ingot", ItemTypes.INGOT}, 
            {"Ore", ItemTypes.ORE}, 
            {"Component", ItemTypes.COMPONENT}, 
            {"PhysicalObject", ItemTypes.PHYSICAL_OBJECT}, 
            {"AmmoMagazine", ItemTypes.AMMO } 
        };

        public struct MyDefinitionId : IEquatable<MyDefinitionId>
        {
            public static readonly MyDefinitionId.DefinitionIdComparerType Comparer =
                new MyDefinitionId.DefinitionIdComparerType();

            public readonly VRage.ObjectBuilders.MyObjectBuilderType TypeId;
            public readonly string SubtypeName;

            public MyDefinitionId(VRage.ObjectBuilders.MyObjectBuilderType typeId, string subtypeName)
            {
                TypeId = typeId;
                SubtypeName = subtypeName;
            }

            public string SubtypeId
            {
                get { return SubtypeName; }
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

        void Main(string argument)
        {
            // initialize
            Dictionary<MyDefinitionId, VRage.MyFixedPoint> inventory = new Dictionary<MyDefinitionId, VRage.MyFixedPoint>();

            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(blocks);
            VRage.ObjectBuilders.MyObjectBuilderType type = null;
            if (!nameTypes.TryGetValue(argument, out type))
            {
                for (int i = 0; i < nameTypes.Count; i++)
                {
                }
                return;
            }
            debug.Append(argument).Append('=').AppendLine(type.ToString() ?? "null");
            

            if (blocks.Count == 0)
                throw new Exception("Did not find any cargo container.");

            for (int i = 0; i < blocks.Count; ++i)
            {

                IMyInventoryOwner invOwner = blocks[i] as IMyInventoryOwner;
                for (int j = 0; j < invOwner.InventoryCount; ++j)
                {
                    IMyInventory inv = invOwner.GetInventory(j);
                    List<IMyInventoryItem> items = inv.GetItems();
                    for (int k = 0; k < items.Count; ++k)
                    {
                        IMyInventoryItem item = items[k];
                        var key = new MyDefinitionId(item.Content.TypeId, item.Content.SubtypeName);
                        //contents.Add(key);
                        if (!inventory.ContainsKey(key))
                            inventory[key] = item.Amount;
                        else
                            inventory[key] = inventory[key] + item.Amount;
                    }
                }
            }
            debug.AppendLine();
            var enumerator = inventory.Keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Current;
                if (key.TypeId != ItemTypes.INGOT)
                    continue;
                debug.Append(typeNames[key.TypeId]).Append(key.SubtypeName).Append(" = ").Append(inventory[key]).AppendLine();
            }

            Echo(debug.ToString());
            debug.Clear();
        }

    }

}
