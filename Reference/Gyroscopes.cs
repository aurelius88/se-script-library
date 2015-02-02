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
    public class Gyroscopes : ReferenceOrientedBlocks
    {
        private List<IMyGyro> gyroscopeBlocks = new List<IMyGyro>();

        public int CountGyros { get { return gyroscopeBlocks.Count; } }

        public readonly float Min, Max, Default;

        public Gyroscopes(IMyTerminalBlock referenceBlock, List<IMyTerminalBlock> blocks)
            : base(referenceBlock)
        {
            UpdateGyroscopes(blocks);

            IMyGyro gyro = gyroscopeBlocks[0];
            Min = gyro.GetMininum<float>(GyroAction.Pitch.Name);
            Max = gyro.GetMaximum<float>(GyroAction.Pitch.Name);
            Default = gyro.GetDefaultValue<float>(GyroAction.Pitch.Name);
        }

        /// <summary>
        /// Rotates the ship relative to the reference block.
        /// A positive Yaw value rotates around the Up vector, such that the Right vector is moved to the Backward vector on the shortest way.
        /// A positive Pitch value rotates around the Right vector, such that the Up vector is moved to the Backward vector on the shortest way.
        /// A positive Roll value rotates around the Backward vector, such that the Up vector is moved to the Right vector on the shortest way.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="value"></param>
        private void Rotate(VRageMath.Vector3 axis, float value)
        {
            if (value < Min || value > Max)
                throw new Exception("Value '" + value + "' out of range [" + Min + ", " + Max + "].");

            VRageMath.Matrix local = new VRageMath.Matrix();
            referenceBlock.Orientation.GetMatrix(out local);
            axis = VRageMath.Vector3.Transform(axis, local);

            for (int i = 0; i < gyroscopeBlocks.Count; ++i)
            {
                IMyGyro gyro = gyroscopeBlocks[i] as IMyGyro;
                gyro.Orientation.GetMatrix(out local);

                VRageMath.Matrix toGyro = VRageMath.Matrix.Transpose(local);
                VRageMath.Vector3 transformedAxis = VRageMath.Vector3.Transform(axis, toGyro);

                GyroAction action = GyroAction.getActionAroundAxis(transformedAxis);
                gyro.SetValue(action.Name, action.Reversed ? -value : value);
            }
        }

        private float GetRotate(VRageMath.Vector3 axis)
        {
            if (!XUtils.Directions.Contains(axis))
                throw new Exception("Invalid axis vector used: " + axis);

            VRageMath.Matrix local = new VRageMath.Matrix();
            referenceBlock.Orientation.GetMatrix(out local);
            axis = VRageMath.Vector3.Transform(axis, local);

            float totalValue = 0;

            for (int i = 0; i < gyroscopeBlocks.Count; ++i)
            {
                IMyGyro gyro = gyroscopeBlocks[i] as IMyGyro;
                gyro.Orientation.GetMatrix(out local);

                VRageMath.Matrix toGyro = VRageMath.Matrix.Transpose(local);
                VRageMath.Vector3 transformedAxis = VRageMath.Vector3.Transform(axis, toGyro);

                GyroAction action = GyroAction.getActionAroundAxis(transformedAxis);
                float value = gyro.GetValue<float>(action.Name);
                totalValue += action.Reversed ? -value : value;
            }

            return totalValue;
        }

        public bool Enable
        {
            get
            {
                bool enabled = false;
                int i = 0;
                while (!enabled && i < CountGyros)
                {
                    enabled |= gyroscopeBlocks[i].Enabled;
                    i++;
                }
                return enabled;
            }
            set
            {
                for (int i = 0; i < CountGyros; ++i)
                {
                    IMyGyro block = gyroscopeBlocks[i];
                    if (block.Enabled ^ value)
                        block.GetActionWithName("OnOff").Apply(block);
                }
            }
        }

        public bool GyroOverride
        {
            get
            {
                bool gyroOverride = false;
                int i = 0;
                while (!gyroOverride && i < CountGyros)
                {
                    gyroOverride |= gyroscopeBlocks[i].GyroOverride;
                    i++;
                }
                return gyroOverride;
            }
            set
            {
                for (int i = 0; i < CountGyros; ++i)
                {
                    IMyGyro block = gyroscopeBlocks[i];
                    if (block.GyroOverride ^ value)
                        block.GetActionWithName("Override").Apply(block);
                }
            }
        }

        public float Pitch
        {
            get { return GetRotate(XUtils.Identity.Right); }
            set { Rotate(XUtils.Identity.Right, value); }
        }

        public float Yaw
        {
            get { return GetRotate(XUtils.Identity.Up); }
            set { Rotate(XUtils.Identity.Up, value); }
        }

        public float Roll
        {
            get { return GetRotate(XUtils.Identity.Backward); }
            set { Rotate(XUtils.Identity.Backward, value); }
        }


        public void UpdateGyroscopes(List<IMyTerminalBlock> blocks)
        {
            gyroscopeBlocks = new List<IMyGyro>();
            for (int i = 0; i < blocks.Count; ++i)
            {
                IMyTerminalBlock block = blocks[i];
                if (block is IMyGyro)
                    gyroscopeBlocks.Add(block as IMyGyro);
            }

            if (gyroscopeBlocks.Count == 0)
                throw new Exception("There is no gyroscope within the given block list.");
        }
    }
}
