using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_Script_Library
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
}
