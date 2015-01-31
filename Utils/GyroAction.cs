using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_Script_Library.Utils
{
    public class GyroAction
    {
        private static HashSet<GyroAction> elements = new HashSet<GyroAction>();

        public static GyroAction Pitch = new GyroAction("Pitch", true);
        public static GyroAction Yaw = new GyroAction("Yaw");
        public static GyroAction Roll = new GyroAction("Roll");
        public static GyroAction PitchRev = new GyroAction("Pitch");
        public static GyroAction YawRev = new GyroAction("Yaw", true);
        public static GyroAction RollRev = new GyroAction("Roll", true);

        private static Dictionary<VRageMath.Vector3, GyroAction> gyroActions = new Dictionary<VRageMath.Vector3, GyroAction>()
            {
                {XUtils.Identity.Right, GyroAction.Pitch},
                {XUtils.Identity.Left, GyroAction.PitchRev},
                {XUtils.Identity.Up, GyroAction.Yaw},
                {XUtils.Identity.Down, GyroAction.YawRev},
                {XUtils.Identity.Backward, GyroAction.Roll},
                {XUtils.Identity.Forward, GyroAction.RollRev}
            };

        private string name;
        public string Name { get { return name; } }
        private bool reversed;
        public bool Reversed { get { return reversed; } }

        private GyroAction(string name, bool reversed = false)
        {
            this.reversed = reversed;
            this.name = name;
            elements.Add(this);
        }

        public override string ToString()
        {
            return name;
        }

        public static HashSet<GyroAction> GetElements()
        {
            return elements;
        }

        public static GyroAction getActionAroundAxis(VRageMath.Vector3 axis)
        {
            return gyroActions[axis];
        }

    }
}
