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
    class Test : IngameScript
    {

        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        IMyShipController controller;
        VRageMath.Vector3D worldCoord;
        List<IMyCubeBlock> perpBlocks;

        int counter = 0;

        void Main()
        {
            // initialize
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, FilterShipController);

            if (blocks.Count == 0)
                throw new Exception("Did not find any cockpit.");

            controller = blocks[0] as IMyShipController;
            debug.Append("use ").Append(controller.CustomName).Append(':').AppendLine();

            perpBlocks = Utils.FindPerpendicularTo(controller);

            for (int i = 0; i < perpBlocks.Count; ++i)
            {
                var block = perpBlocks[i];
                debug.Append(block.Position).AppendLine();
            }

            VRageMath.Vector3 cur = perpBlocks[1].Position - perpBlocks[0].Position;
            VRageMath.Vector3 next = perpBlocks[2].Position - perpBlocks[0].Position;
            debug.Append(VRageMath.Vector3.ArePerpendicular(ref cur, ref next)).AppendLine();

            worldCoord = new VRageMath.Vector3D(0, 0, 0);

            bool orthogonal = perpBlocks.Count == 3;
            VRageMath.Matrix toWorld = orthogonal ? Utils.toWorld(perpBlocks) : Utils.toWorld(GridTerminalSystem.Blocks);
            VRageMath.Vector3 r = toWorld.Right;
            VRageMath.Vector3 u = toWorld.Up;
            VRageMath.Vector3 b = toWorld.Backward;

            debug.Append(r.Dot(u)).AppendLine();
            debug.Append(u.Dot(b)).AppendLine();
            debug.Append(b.Dot(r)).AppendLine();

            debug.Append(VRageMath.Vector3.ArePerpendicular(ref r, ref u)).AppendLine();
            debug.Append(VRageMath.Vector3.ArePerpendicular(ref u, ref b)).AppendLine();
            debug.Append(VRageMath.Vector3.ArePerpendicular(ref b, ref r)).AppendLine();

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks);
            GridTerminalSystem.GetBlocksOfType<IMyGyro>(blocks);

            debug.Append("worldCoord = ").Append(VRageMath.Vector3I.Round(worldCoord)).AppendLine();
            debug.Append("controller.GetPosition() = ").Append(VRageMath.Vector3I.Round(controller.GetPosition())).AppendLine();
            debug.Append("controller.Position = ").Append(controller.Position).AppendLine();

            debug.Append("transfrom controller.Position = ").Append(VRageMath.Vector3I.Round(VRageMath.Vector3.Transform(controller.Position, toWorld))).AppendLine();
            debug.Append("transfrom controller.GetPosition() = ").Append(VRageMath.Vector3I.Round(VRageMath.Vector3.Transform(controller.GetPosition(), VRageMath.Matrix.Invert(toWorld)))).AppendLine();
            debug.Append("transfrom zero = ").Append(VRageMath.Vector3I.Round(VRageMath.Vector3.Transform(worldCoord, VRageMath.Matrix.Invert(toWorld)))).AppendLine();

            VRageMath.Vector3 worldDir = worldCoord - controller.GetPosition();
            float distance = worldDir.LengthSquared() > 0 ? worldDir.Normalize() : 0;
            debug.Append("distance = ").Append(distance).AppendLine();
            debug.Append("direction = ").Append(worldDir).AppendLine();

            VRageMath.Matrix worldController = new VRageMath.Matrix();
            controller.Orientation.GetMatrix(out worldController);
            worldController = worldController * VRageMath.Matrix.CreateTranslation(controller.Position) * toWorld;

            debug.Append("worldController = ").AppendLine();
            debug.Append(worldController.Right).AppendLine();
            debug.Append(worldController.Up).AppendLine();
            debug.Append(worldController.Backward).AppendLine();
            debug.Append(worldController.Translation).AppendLine();

            VRageMath.Vector3 a = worldController.Forward;
            VRageMath.Matrix rotation = Utils.CalculateRotation(ref worldDir, ref a);

            debug.Append((double)Math.Abs(rotation.Right.Dot(rotation.Up))).AppendLine();
            debug.Append((double)Math.Abs(rotation.Right.Dot(rotation.Backward))).AppendLine();
            debug.Append((double)Math.Abs(rotation.Up.Dot(rotation.Backward))).AppendLine();

            debug.Append("rotation transl+persp = ").Append(rotation.HasNoTranslationOrPerspective()).AppendLine();
            debug.Append("rotation rotation = ").Append(rotation.IsRotation()).AppendLine();
            debug.Append(rotation.Right).AppendLine();
            debug.Append(rotation.Up).AppendLine();
            debug.Append(rotation.Backward).AppendLine();
            debug.Append(rotation.Translation).AppendLine();

            VRageMath.Vector3 xyz = new VRageMath.Vector3();
            VRageMath.Matrix.GetEulerAnglesXYZ(ref rotation, out xyz);

            debug.Append("X = ").Append(xyz.GetDim(0)).AppendLine();
            debug.Append("Y = ").Append(xyz.GetDim(1)).AppendLine();
            debug.Append("Z = ").Append(xyz.GetDim(2)).AppendLine();

            Debug(debug.ToString());
            debug.Clear();
        }

        // IMyCubeGrid.GridIntegerToWorld
        void Debug(String message)
        {
            var list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(debugName, list);
            if (list.Count > 0)
                list[0].SetCustomName(debugName + ":\n\r" + message);
        }

        private static int numBlocks;

        static bool FilterShipController(IMyTerminalBlock block)
        {
            return !block.DefinitionDisplayNameText.Equals(Utils.cockpitSubtypes[0]);
        }

        // ------------------------------------------------------------- CLASS

        public class Utils
        {
            internal static VRageMath.Matrix Identity = new VRageMath.Matrix(1, 0, 0, 0, 1, 0, 0, 0, 1);
            internal static List<VRageMath.Vector3> directions = new List<VRageMath.Vector3>() {
                Identity.Right,
                Identity.Up,
                Identity.Backward,
                Identity.Left,
                Identity.Down,
                Identity.Forward
            };

            public static string[] cockpitSubtypes = new string[]
        {
            "Passenger Seat",
            "Flight Seat",
            "Cockpit",
            "Fighter Cockpit",
            "Control Station"
        };

            internal static VRageMath.Matrix CreateSkewSymmetricMatrix(VRageMath.Vector3 v)
            {
                return new VRageMath.Matrix(0, -v.GetDim(2), v.GetDim(1), v.GetDim(2), 0, -v.GetDim(0), -v.GetDim(1), v.GetDim(0), 0);
            }

            internal static VRageMath.Matrix CalculateRotation(ref VRageMath.Vector3 a, ref VRageMath.Vector3 b)
            {
                VRageMath.Matrix rotation = new VRageMath.Matrix();
                CalculateRotation(ref a, ref b, out rotation);
                return rotation;
            }

            internal static void CalculateRotation(ref VRageMath.Vector3 a, ref VRageMath.Vector3 b, out VRageMath.Matrix rotation)
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
            internal static List<IMyCubeBlock> FindPerpendicularTo(IMyTerminalBlock block)
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

            internal static VRageMath.Matrix toWorld(List<IMyTerminalBlock> blocks)
            {
                return toWorld(new List<IMyCubeBlock>(blocks));
            }

            /// <summary>
            /// Calculates a transformation matrix to transform grid coordinates to world coordinates.
            /// </summary>
            /// <param name="blocks"></param>
            /// <returns></returns>
            internal static VRageMath.Matrix toWorld(List<IMyCubeBlock> blocks)
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

namespace x
{
    class Test
    {
        static void test()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 100;
            timer.Enabled = true;
        }


        static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            //Debug(String.Format("The Elapsed event was raised at {0}", e.SignalTime));
        }
    }
}
