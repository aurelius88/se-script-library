using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Utils;

namespace SE_Script_Library.Utils
{
    /// <summary>
    /// Pair of TypeId and SubtypeName
    /// </summary>
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
}