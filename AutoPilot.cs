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

    public class AutoPilot : IngameScript
    {
        string name = "X-AutoPilot";
        public static StringBuilder debug = new StringBuilder();
        string debugName = "Debug";

        static string[] cockpitSubtypes = new string[]
        {
            "Passenger Seat",
            "Flight Seat",
            "Cockpit",
            "Fighter Cockpit",
            "Control Station"
        };

        public static VRageMath.Matrix Identity = new VRageMath.Matrix(1, 0, 0, 0, 1, 0, 0, 0, 1);
        public static List<VRageMath.Vector3> directions = new List<VRageMath.Vector3>() {
                Identity.Right,
                Identity.Up,
                Identity.Backward,
                Identity.Left,
                Identity.Down,
                Identity.Forward
            };

        IMyShipController controller;
        ShipController ship;
        VRageMath.Vector3D worldCoord;
        List<IMyCubeBlock> perpBlocks;

        int counter = 0;

        void Main()
        {
            // initialize
            var blocks = new List<IMyTerminalBlock>();
            if (counter == 0)
            {
                GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, FilterShipController);

                if (blocks.Count == 0)
                    throw new Exception("Did not find any cockpit.");

                controller = blocks[0] as IMyShipController;
                debug.Append("use ").Append(controller.CustomName).Append(':').AppendLine();

                perpBlocks = Utils.FindPerpendicularTo(controller);

                ship = new ShipController(controller);
                worldCoord = controller.GetPosition();
                debug.Append("POSITION = ").Append(worldCoord).AppendLine();

                Debug(debug.ToString());
                debug.Clear();
                counter++;
                return;
            }

            worldCoord = new VRageMath.Vector3D(0, 0, 0);

            bool orthogonal = perpBlocks.Count == 3;
            VRageMath.Matrix toWorld = orthogonal ? Utils.toWorld(GridTerminalSystem.Blocks) : Utils.toWorld(perpBlocks);

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks);
            GridTerminalSystem.GetBlocksOfType<IMyGyro>(blocks);

            debug.Append("worldCoord = ").Append(VRageMath.Vector3I.Round(worldCoord)).AppendLine();
            debug.Append("controller.GetPosition() = ").Append(VRageMath.Vector3I.Round(controller.GetPosition())).AppendLine();
            debug.Append("controller.Position = ").Append(controller.Position).AppendLine();

            debug.Append("transfrom controller.Position = ").Append(VRageMath.Vector3I.Round(VRageMath.Vector3.Transform(controller.Position, toWorld))).AppendLine();
            debug.Append("transfrom controller.Position = ").Append(VRageMath.Vector3I.Round(VRageMath.Vector3.Transform(controller.GetPosition(), VRageMath.Matrix.Invert(toWorld)))).AppendLine();

            VRageMath.Vector3 worldDir = worldCoord - controller.GetPosition();
            float distance = worldDir.LengthSquared() > 0 ? worldDir.Normalize() : 0;
            debug.Append("distance = ").Append(distance).AppendLine();
            debug.Append("direction = ").Append(worldDir).AppendLine();

            VRageMath.Matrix worldController = new VRageMath.Matrix();
            controller.Orientation.GetMatrix(out worldController);
            worldController = worldController * toWorld;
            debug.Append("worldController = ").AppendLine();
            debug.Append(worldController.Right).AppendLine();
            debug.Append(worldController.Up).AppendLine();
            debug.Append(worldController.Backward).AppendLine();
            debug.Append(worldController.Translation).AppendLine();
            debug.Append("origin worldController = ").Append(VRageMath.Vector3I.Round(VRageMath.Vector3.Transform(new VRageMath.Vector3(), worldController))).AppendLine();
            //VRageMath.Vector3 n = orthogonal ? worldController.Right : worldController.Up.Cross(worldController.Backward);
            //VRageMath.Vector3 projDir = worldDir - worldDir.Dot(n) / n.Dot(n) * n;
            //if (projDir.LengthSquared() > 0)
            //    projDir.Normalize();

            //VRageMath.Vector3 eY = worldController.Up;
            //eY.Normalize();
            //VRageMath.Vector3 eZ = worldController.Backward;
            //eZ.Normalize();

            //float cosinePhiY = eY.Dot(projDir);
            //float cosinePhiZ = eZ.Dot(projDir);

            //float pitch = (float)(cosinePhiY > 0 ? -Math.Acos(cosinePhiZ) : Math.Acos(cosinePhiZ));
            ////VRageMath.Matrix.AlignRotationToAxes();
            //debug.Append("pitch = ").Append(pitch).AppendLine();

            debug.Append("worldController.IsRotation() = ").Append(worldController.IsRotation());
            VRageMath.Matrix toAlign = VRageMath.Matrix.CreateFromDir(worldDir, worldController.Up);
            VRageMath.Matrix align = VRageMath.Matrix.AlignRotationToAxes(ref toAlign, ref worldController);
            VRageMath.Vector3 xyz = new VRageMath.Vector3();
            VRageMath.Matrix.GetEulerAnglesXYZ(ref align, out xyz);
            xyz = 0.1f * xyz;

            debug.Append(xyz).AppendLine();
            ship.UpdateBlocks(blocks);
            ship.Stop();
            ship.Rotate(Identity.Left, xyz.GetDim(0));
            ship.Rotate(Identity.Down, xyz.GetDim(1));
            ship.Rotate(Identity.Forward, xyz.GetDim(2));

            Debug(debug.ToString());
            debug.Clear();
        }

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
            return !block.DefinitionDisplayNameText.Equals(cockpitSubtypes[0]);
        }

        // ------------------------------------------------------------- CLASS

        public class Utils
        {
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
                // TODO use an epsilon value instead of 0, because of the precision error of floating point multiplication.
                while (u.Dot(v) == 0 && vIndex < blocks.Count)
                    v = blocks[++vIndex].Position - localCoord;

                if (u.Dot(v) == 0)
                    throw new Exception("All blocks are linear dependent => It's not possible to calculate a transformation matrix.");

                // third basis vector
                //VRageMath.Vector3 w = VRageMath.Vector3.Cross(u, v);

                //VRageMath.Matrix localBasis = new VRageMath.Matrix(
                //    u.GetDim(0), u.GetDim(1), u.GetDim(2), 0,
                //    v.GetDim(0), v.GetDim(1), v.GetDim(2), 0,
                //    w.GetDim(0), w.GetDim(1), w.GetDim(2), 0,
                //   localCoord.GetDim(0), localCoord.GetDim(1), localCoord.GetDim(2), 1);
                VRageMath.Matrix localBasis = VRageMath.Matrix.CreateWorld(localCoord, u, v);

                VRageMath.Vector3 worldCoord = origin.GetPosition();
                // world basis depending on the local bases (same coordinates)
                VRageMath.Vector3 ug = blocks[1].GetPosition() - worldCoord;
                VRageMath.Vector3 vg = blocks[vIndex].GetPosition() - worldCoord;
                //VRageMath.Vector3 wg = VRageMath.Vector3.Cross(ug, vg);

                //VRageMath.Matrix worldBasis = new VRageMath.MatrixD(
                //    ug.GetDim(0), ug.GetDim(1), ug.GetDim(2), 0,
                //    vg.GetDim(0), vg.GetDim(1), vg.GetDim(2), 0,
                //    wg.GetDim(0), wg.GetDim(1), wg.GetDim(2), 0,
                //    worldCoord.GetDim(0), worldCoord.GetDim(1), worldCoord.GetDim(2), 1);
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

        public class ShipController
        {
            IMyTerminalBlock referenceBlock;
            List<IMyGyro> gyros = new List<IMyGyro>();
            Dictionary<VRageMath.Vector3, List<IMyThrust>> thrusters = new Dictionary<VRageMath.Vector3, List<IMyThrust>>();

            float thrusterOverrideMin, thrusterOverrideMax;
            float gyroRotateMin, gyroRotateMax;

            private Dictionary<VRageMath.Vector3, GyroAction> gyroActions = new Dictionary<VRageMath.Vector3, GyroAction>()
            {
                {Identity.Right, GyroAction.Pitch},
                {Identity.Left, GyroAction.PitchRev},
                {Identity.Up, GyroAction.Yaw},
                {Identity.Down, GyroAction.YawRev},
                {Identity.Backward, GyroAction.Roll},
                {Identity.Forward, GyroAction.RollRev}
            };

            public ShipController(IMyTerminalBlock reference)
            {
                this.referenceBlock = reference;
            }

            public void UpdateBlocks(List<IMyTerminalBlock> list)
            {
                gyros = new List<IMyGyro>();
                thrusters = new Dictionary<VRageMath.Vector3, List<IMyThrust>>();
                VRageMath.Matrix tmp = new VRageMath.Matrix();
                for (int i = 0; i < list.Count; ++i)
                {
                    var block = list[i];
                    if (block is IMyGyro)
                    {
                        IMyGyro gyro = (IMyGyro)block;
                        if (gyros.Count == 0)
                        {
                            gyroRotateMin = gyro.GetMininum<float>(GyroAction.Pitch.GetName());
                            gyroRotateMax = gyro.GetMaximum<float>(GyroAction.Pitch.GetName());
                        }
                        gyros.Add(gyro);
                    }
                    else if (block is IMyThrust)
                    {
                        IMyThrust thruster = block as IMyThrust;

                        if (thrusters.Count == 0)
                        {
                            thrusterOverrideMin = thruster.GetMininum<float>("Override");
                            thrusterOverrideMax = thruster.GetMaximum<float>("Override");
                        }

                        block.Orientation.GetMatrix(out tmp);
                        // The exhaust is directed to the Forward vector of the thruster, so it accelerates to Backward.
                        VRageMath.Vector3 dir = tmp.Backward;
                        if (!thrusters.ContainsKey(dir))
                            thrusters[dir] = new List<IMyThrust>();
                        thrusters[dir].Add(thruster);
                    }
                }
            }

            /// <summary>
            /// Rotates the ship relative to the reference block.
            /// A positive Yaw value rotates around the Up vector, such that the Right vector is moved to the Backward vector on the shortest way.
            /// A positive Pitch value rotates around the Right vector, such that the Up vector is moved to the Backward vector on the shortest way.
            /// A positive Roll value rotates around the Backward vector, such that the Up vector is moved to the Right vector on the shortest way.
            /// </summary>
            /// <param name="axis"></param>
            /// <param name="value"></param>
            public void Rotate(VRageMath.Vector3 axis, float value)
            {
                if (value < gyroRotateMin || value > gyroRotateMax)
                    throw new Exception("Value out of range [" + gyroRotateMin + ", " + gyroRotateMax + "].");

                VRageMath.Matrix local = new VRageMath.Matrix();
                referenceBlock.Orientation.GetMatrix(out local);
                axis = VRageMath.Vector3.Transform(axis, local);

                for (int i = 0; i < gyros.Count; ++i)
                {
                    IMyGyro gyro = gyros[i] as IMyGyro;
                    gyro.Orientation.GetMatrix(out local);

                    VRageMath.Matrix toGyro = VRageMath.Matrix.Transpose(local);
                    VRageMath.Vector3 transformedAxis = VRageMath.Vector3.Transform(axis, toGyro);

                    GyroAction action = gyroActions[transformedAxis];
                    gyro.SetValue(action.GetName(), action.Reversed ? -value : value);
                }
            }

            public void StopRotation()
            {
                for (int i = 0; i < gyros.Count; ++i)
                {
                    IMyGyro gyro = gyros[i] as IMyGyro;
                    gyro.SetValue(GyroAction.Pitch.GetName(), gyro.GetDefaultValue<float>(GyroAction.Pitch.GetName()));
                    gyro.SetValue(GyroAction.Yaw.GetName(), gyro.GetDefaultValue<float>(GyroAction.Yaw.GetName()));
                    gyro.SetValue(GyroAction.Roll.GetName(), gyro.GetDefaultValue<float>(GyroAction.Roll.GetName()));
                }
            }

            /// <summary>
            /// Accelerates the ship relative to the reference block.
            /// </summary>
            /// <param name="dir">The direction relative to the reference block in which the ship should be accelerated.</param>
            /// <param name="value">The amount of force to accelerate in Newton.</param>
            public void Accelerate(VRageMath.Vector3 dir, float value)
            {
                if (value < thrusterOverrideMin || value > thrusterOverrideMax)
                    throw new Exception("Value out of range [" + thrusterOverrideMin + ", " + thrusterOverrideMax + "].");

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

            public void StopAcceleration()
            {
                for (int i = 0; i < directions.Count; ++i)
                {
                    var key = directions[i];
                    if (!thrusters.ContainsKey(key))
                        continue;

                    var list = thrusters[key];
                    for (int j = 0; j < list.Count; ++j)
                    {
                        IMyThrust thruster = list[j];
                        thruster.SetValue("Override", thruster.GetDefaultValue<float>("Override"));
                    }
                }
            }

            public void Stop()
            {
                StopAcceleration();
                StopRotation();
            }
        }

        public class GyroAction
        {
            private static Dictionary<GyroAction, string> elements = new Dictionary<GyroAction, string>();

            public static GyroAction Pitch = new GyroAction("Pitch", true);
            public static GyroAction Yaw = new GyroAction("Yaw");
            public static GyroAction Roll = new GyroAction("Roll");
            public static GyroAction PitchRev = new GyroAction("Pitch");
            public static GyroAction YawRev = new GyroAction("Yaw", true);
            public static GyroAction RollRev = new GyroAction("Roll", true);

            private int value = elements.Count;
            public int Value { get { return value; } }
            private bool reversed;
            public bool Reversed { get { return reversed; } }

            private GyroAction(string name, bool reversed = false)
            {
                this.reversed = reversed;
                elements.Add(this, name);
            }

            public override string ToString()
            {
                return elements[this];
            }

            public string GetName()
            {
                return elements[this];
            }
        }
    }

}
