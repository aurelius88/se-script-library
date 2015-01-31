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

namespace SE_Script_Library.Utils
{
    public class Gyroscopes : ReferenceOrientedBlocks
    {
        private List<IMyGyro> gyros = new List<IMyGyro>();

        public readonly float Min, Max;

        public Gyroscopes(ref IMyTerminalBlock referenceBlock, List<IMyTerminalBlock> blocks)
            : base(ref referenceBlock)
        {
            Update(blocks);

            IMyGyro gyro = gyros.First();
            Min = gyro.GetMininum<float>(GyroAction.Pitch.Name);
            Max = gyro.GetMaximum<float>(GyroAction.Pitch.Name);
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
                throw new Exception("Value out of range [" + Min + ", " + Max + "].");

            VRageMath.Matrix local = new VRageMath.Matrix();
            referenceBlock.Orientation.GetMatrix(out local);
            axis = VRageMath.Vector3.Transform(axis, local);

            for (int i = 0; i < gyros.Count; ++i)
            {
                IMyGyro gyro = gyros[i] as IMyGyro;
                gyro.Orientation.GetMatrix(out local);

                VRageMath.Matrix toGyro = VRageMath.Matrix.Transpose(local);
                VRageMath.Vector3 transformedAxis = VRageMath.Vector3.Transform(axis, toGyro);

                GyroAction action = GyroAction.getActionAroundAxis(transformedAxis);
                gyro.SetValue(action.Name, action.Reversed ? -value : value);
            }
        }

        public void Pitch(float value)
        {
            Rotate(XUtils.Identity.Right, value);
        }

        public void Yaw(float value)
        {
            Rotate(XUtils.Identity.Up, value);
        }

        public void Roll(float value)
        {
            Rotate(XUtils.Identity.Backward, value);
        }


        internal void Update(List<IMyTerminalBlock> blocks)
        {
            gyros = new List<IMyGyro>();
            for (int i = 0; i < blocks.Count; ++i)
            {
                IMyTerminalBlock block = blocks[i];
                if (block is IMyGyro)
                    gyros.Add(block as IMyGyro);
            }

            if (gyros.Count == 0)
                throw new Exception("There is no gyroscope within the given block list.");
        }
    }
}
