using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Utils;

namespace SE_Script_Library.Reference
{
    public class Thrusters : ReferenceOrientedBlocks
    {
        /// <summary>
        /// The minimum value of the thruster acceleration.
        /// </summary>
        public readonly float MinAcceleration;
        /// <summary>
        /// The maximum value of the thruster acceleration.
        /// </summary>
        public readonly float MaxAcceleration;
        /// <summary>
        /// The default value of the thruster acceleration.
        /// </summary>
        public readonly float DefaultAcceleration;
        // Lists of thrusters accelerating to the given direction in grid space.
        private Dictionary<VRageMath.Vector3, List<IMyThrust>> thrusters;

        public Thrusters(IMyTerminalBlock referenceBlock, List<IMyTerminalBlock> blocks)
            : base(referenceBlock)
        {
            Update(blocks);

            Dictionary<VRageMath.Vector3, List<IMyThrust>>.ValueCollection.Enumerator enumerator = thrusters.Values.GetEnumerator();
            enumerator.MoveNext();
            IMyThrust thrust = enumerator.Current[0];
            MinAcceleration = thrust.GetMininum<float>("Override");
            MaxAcceleration = thrust.GetMaximum<float>("Override");
            DefaultAcceleration = thrust.GetDefaultValue<float>("Override");
        }

        /// <summary>
        /// Accelerates the ship relative to the reference block.
        /// </summary>
        /// <param name="dir">The direction relative to the reference block in which the ship should be accelerated.</param>
        /// <param name="value">The amount of force to accelerate in Newton.</param>
        private void Accelerate(VRageMath.Vector3 dir, float value)
        {
            if (value < MinAcceleration || value > MaxAcceleration)
                throw new Exception("Value '" + value + "' out of range [" + MinAcceleration + ", " + MaxAcceleration + "].");

            VRageMath.Matrix local = new VRageMath.Matrix();
            referenceBlock.Orientation.GetMatrix(out local);
            dir = VRageMath.Vector3.Transform(dir, local);

            if (!thrusters.ContainsKey(dir))
                throw new Exception("Warning! No thruster in direction " + dir + ".");

            var list = thrusters[dir];
            for (int i = 0; i < list.Count; ++i)
            {
                IMyThrust thruster = list[i];
                thruster.SetValueFloat("Override", value);
            }
        }

        public void AccelerateRight(float value)
        {
            Accelerate(XUtils.Identity.Right, value);
        }

        public void AccelerateLeft(float value)
        {
            Accelerate(XUtils.Identity.Left, value);
        }

        public void AccelerateUp(float value)
        {
            Accelerate(XUtils.Identity.Up, value);
        }

        public void AccelerateDown(float value)
        {
            Accelerate(XUtils.Identity.Down, value);
        }

        public void AccelerateBackward(float value)
        {
            Accelerate(XUtils.Identity.Backward, value);
        }

        public void AccelerateForward(float value)
        {
            Accelerate(XUtils.Identity.Forward, value);
        }

        internal void Update(List<IMyTerminalBlock> blocks)
        {
            thrusters = new Dictionary<VRageMath.Vector3, List<IMyThrust>>();
            VRageMath.Matrix toReference = new VRageMath.Matrix();
            referenceBlock.Orientation.GetMatrix(out toReference);
            VRageMath.Matrix.Transpose(ref toReference, out toReference);
            VRageMath.Matrix tmp = new VRageMath.Matrix();
            for (int i = 0; i < blocks.Count; ++i)
            {
                IMyTerminalBlock block = blocks[i];
                if (block is IMyThrust)
                {
                    block.Orientation.GetMatrix(out tmp);
                    // The exhaust is directed to the Forward vector of the thruster, so it accelerates to Backward.
                    VRageMath.Vector3 dir = VRageMath.Vector3.Transform(tmp.Backward, toReference);
                    if (!thrusters.ContainsKey(dir))
                        thrusters[dir] = new List<IMyThrust>();
                    thrusters[dir].Add(block as IMyThrust);
                }
            }

            if (thrusters.Count == 0)
                throw new Exception("There is no thruster within the given block list.");
        }
    }
}
