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

using VRage;

namespace SE_Script_Library.Utils
{
    class XUtils
    {
        public static VRageMath.Matrix Identity = new VRageMath.Matrix(1, 0, 0, 0, 1, 0, 0, 0, 1);

        public static List<VRageMath.Vector3> Directions = new List<VRageMath.Vector3>() {
                Identity.Right,
                Identity.Up,
                Identity.Backward,
                Identity.Left,
                Identity.Down,
                Identity.Forward
            };

        public static string ToString(Sandbox.Common.ObjectBuilders.MyObjectBuilderType typeId)
        {
            return typeId.ToString().Split('_')[1];
        }

        /// <summary>
        /// Searches for two other blocks that are perpendicular to the given one.
        /// If 
        /// </summary>
        /// <param name="block">The given block</param>
        /// <returns></returns>
        public static List<IMyCubeBlock> FindPerpendicularTo(IMyTerminalBlock block)
        {
            if (block == null)
                throw new Exception("The block is null.");

            IMyCubeGrid grid = block.CubeGrid;

            int maxIndex = Math.Max(grid.Min.AbsMax(), grid.Max.AbsMax());
            List<IMyCubeBlock> perpBlocks = new List<IMyCubeBlock>();
            perpBlocks.Add(block);
            int firstIndex = -1;
            // do a kind of first breath search
            for (int i = 1; perpBlocks.Count < 3 && i < maxIndex; ++i)
            {
                for (int j = 0; j < Directions.Count; ++j)
                {
                    if (j == firstIndex)
                        continue;

                    VRageMath.Vector3I v = new VRageMath.Vector3I(Directions[j]);
                    IMySlimBlock slim = grid.GetCubeBlock(block.Position + i * v);
                    if (slim != null && slim.FatBlock != null)
                    {
                        perpBlocks.Add(slim.FatBlock);
                        if (perpBlocks.Count == 3)
                            break;
                        firstIndex = j;
                    }
                }
            }

            return perpBlocks;
        }

        public static VRageMath.Matrix ToWorld(List<IMyTerminalBlock> blocks)
        {
            return ToWorld(new List<IMyCubeBlock>(blocks));
        }

        /// <summary>
        /// Calculates a transformation matrix to transform grid coordinates to world coordinates.
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static VRageMath.Matrix ToWorld(List<IMyCubeBlock> blocks)
        {
            if (blocks == null)
                throw new Exception("The block list is null");

            if (blocks.Count < 3)
                throw new Exception("Need at least 3 blocks.");

            IMyCubeBlock origin = blocks[0];
            VRageMath.Vector3 localCoord = origin.Position;

            // first basis vector
            VRageMath.Vector3 u = blocks[1].Position - localCoord;

            // second basis vector
            int vIndex = 2;
            VRageMath.Vector3 v = blocks[vIndex].Position - localCoord;
            // TODO use an epsilon value instead of 0, because of the precision error of floating point multiplication.
            while (u.Dot(v) == 0 && vIndex < blocks.Count)
                v = blocks[++vIndex].Position - localCoord;

            if (u.Dot(v) == 0)
                throw new Exception("All blocks are linear dependent => It's not possible to calculate a transformation matrix.");

            VRageMath.Matrix localBasis = VRageMath.Matrix.CreateWorld(localCoord, u, v);

            VRageMath.Vector3 worldCoord = origin.GetPosition();
            // world basis depending on the local bases (same coordinates)
            VRageMath.Vector3 ug = blocks[1].GetPosition() - worldCoord;
            VRageMath.Vector3 vg = blocks[vIndex].GetPosition() - worldCoord;

            VRageMath.Matrix worldBasis = VRageMath.Matrix.CreateWorld(worldCoord, ug, vg);

            VRageMath.Matrix inverseLocalBasis;
            // if local basis is orthogonal, take the transposed matrix, because then
            // the transposed and the inverse matrix are the same and it's obviously
            // easier to get the transposed matrix.
            if (VRageMath.Vector3.ArePerpendicular(ref u, ref v))
                inverseLocalBasis = VRageMath.Matrix.Transpose(localBasis);
            else
                inverseLocalBasis = VRageMath.Matrix.Invert(localBasis);

            return inverseLocalBasis * worldBasis;
        }
    }
}
