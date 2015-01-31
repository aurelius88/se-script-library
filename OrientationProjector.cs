using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
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
    class OrientationProjector : IngameScript
    {
        public static StringBuilder debug = new StringBuilder();

        IMyTerminalBlock reference;
        VRageMath.Vector3D worldCoord;
        List<IMyCubeBlock> perpBlocks;

        public void Main()
        {
            var blocks = GridTerminalSystem.Blocks;

            if (blocks.Count == 0)
                throw new Exception("Did not find any block. Not even this one?! WTF?!");

            reference = blocks[0];
            perpBlocks = Utils.FindPerpendicularTo(reference);

            for (int i = 0; i < perpBlocks.Count; ++i)
            {
                var block = perpBlocks[i];
                debug.Append(block.Position).AppendLine();
            }

            return;

            bool orthogonal = perpBlocks.Count == 3;
            VRageMath.Matrix toWorld = orthogonal ? Utils.toWorld(perpBlocks) : Utils.toWorld(GridTerminalSystem.Blocks);

            var projectors = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProjector>(projectors);

            if (projectors.Count == 0)
                throw new Exception("Did not find any projector.");

            var projector = projectors[0];
            var resultList = new List<ITerminalProperty>();
            projector.GetProperties(resultList);
            for (int i = 0; i < resultList.Count; ++i)
                debug.Append(resultList[i].TypeName).Append(" ").Append(resultList[i].Id).AppendLine();

        }

        public class Utils
        {
            public static VRageMath.Matrix Identity = new VRageMath.Matrix(1, 0, 0, 0, 1, 0, 0, 0, 1);
            public static List<VRageMath.Vector3> directions = new List<VRageMath.Vector3>() {
                Identity.Right,
                Identity.Up,
                Identity.Backward,
                Identity.Left,
                Identity.Down,
                Identity.Forward
            };

            public static VRageMath.Matrix CreateSkewSymmetricMatrix(VRageMath.Vector3 v)
            {
                return new VRageMath.Matrix(0, -v.GetDim(2), v.GetDim(1), v.GetDim(2), 0, -v.GetDim(0), -v.GetDim(1), v.GetDim(0), 0);
            }

            public static VRageMath.Matrix CalculateRotation(ref VRageMath.Vector3 a, ref VRageMath.Vector3 b)
            {
                VRageMath.Matrix rotation = new VRageMath.Matrix();
                CalculateRotation(ref a, ref b, out rotation);
                return rotation;
            }

            public static void CalculateRotation(ref VRageMath.Vector3 a, ref VRageMath.Vector3 b, out VRageMath.Matrix rotation)
            {
                if (!VRageMath.Vector3.IsUnit(ref a))
                    a.Normalize();
                if (!VRageMath.Vector3.IsUnit(ref b))
                    b.Normalize();

                VRageMath.Vector3 v = a.Cross(b);
                float s = v.Length();   // sine
                float c = a.Dot(b);     // cosine
                VRageMath.Matrix cross = Utils.CreateSkewSymmetricMatrix(v);
                rotation = Identity + cross + cross * cross * (1 - c) / s;
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
                    for (int j = 0; j < directions.Count; ++j)
                    {
                        if (j == firstIndex)
                            continue;

                        VRageMath.Vector3I v = new VRageMath.Vector3I(directions[j]);
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

            public static VRageMath.Matrix toWorld(List<IMyTerminalBlock> blocks)
            {
                return toWorld(new List<IMyCubeBlock>(blocks));
            }

            /// <summary>
            /// Calculates a transformation matrix to transform grid coordinates to world coordinates.
            /// </summary>
            /// <param name="blocks"></param>
            /// <returns></returns>
            public static VRageMath.Matrix toWorld(List<IMyCubeBlock> blocks)
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
                while (u.Dot(v) * u.Dot(v) == u.LengthSquared() * v.LengthSquared() && vIndex < blocks.Count)
                {
                    v = blocks[++vIndex].Position - localCoord;
                }

                if (u.Dot(v) * u.Dot(v) == u.LengthSquared() + v.LengthSquared())
                    throw new Exception("All blocks are linear dependent => It's not possible to calculate a transformation matrix.");

                debug.Append("choose: ").Append(u).Append(v).AppendLine();

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
}
