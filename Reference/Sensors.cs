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
    public class Sensors : ReferenceOrientedBlocks
    {
        public class Action
        {
            private static HashSet<Action> elements = new HashSet<Action>();

            public static Action DetectPlayers = new Action("Detect Players");
            public static Action DetectFloatingObjects = new Action("Detect Floating Objects");
            public static Action DetectSmallShips = new Action("Detect Small Ships");
            public static Action DetectLargeShips = new Action("Detect Large Ships");
            public static Action DetectStations = new Action("Detect Stations");
            public static Action DetectAsteroids = new Action("Detect Asteroids");
            public static Action DetectOwner = new Action("Detect Owner");
            public static Action DetectFriendly = new Action("Detect Friendly");
            public static Action DetectNeutral = new Action("Detect Neutral");
            public static Action DetectEnemy = new Action("Detect Enemy");

            public readonly string Name;
            public readonly int Value;
            public static int Count { get { return elements.Count; } }

            Action(string name)
            {
                Value = 1 << elements.Count;
                Name = name;
                elements.Add(this);
            }

            public static HashSet<Action>.Enumerator GetEnumerator()
            {
                return elements.GetEnumerator();
            }

        }


        private class SensorComparer : Comparer<IMySensorBlock>
        {
            private readonly VRageMath.Vector3 pos;

            public SensorComparer(VRageMath.Vector3 pos)
            {
                this.pos = pos;
            }

            public override int Compare(IMySensorBlock x, IMySensorBlock y)
            {
                return Math.Sign((x.Position - pos).LengthSquared() - (y.Position - pos).LengthSquared());
            }
        }

        private static Dictionary<VRageMath.Vector3, float> offset = new Dictionary<VRageMath.Vector3, float>() {
            {XUtils.Identity.Backward,0f},
            {XUtils.Identity.Forward,1f},
            {XUtils.Identity.Up,0.5f},
            {XUtils.Identity.Down,0.5f},
            {XUtils.Identity.Right,0.5f},
            {XUtils.Identity.Left,0.5f}
        };

        public static float getOffset(VRageMath.Vector3 dir)
        {
            return offset[dir];
        }

        private static Dictionary<VRageMath.Vector3, string> extendDirections = new Dictionary<VRageMath.Vector3, string>(){
            {XUtils.Identity.Backward,"Back"},
            {XUtils.Identity.Forward,"Front"},
            {XUtils.Identity.Up,"Top"},
            {XUtils.Identity.Down,"Bottom"},
            {XUtils.Identity.Right,"Right"},
            {XUtils.Identity.Left,"Left"}
        };

        // XXX an acceleration structur like kd-tree could be used, but may not be necessary
        List<IMySensorBlock> sensorBlocks = new List<IMySensorBlock>();

        public int CountSensors { get { return sensorBlocks.Count; } }

        public readonly float Min;
        public readonly float Max;
        public readonly float Default;

        private static List<int> EmptySet = new HashSet<int>();

        public Sensors(IMyTerminalBlock reference, List<IMyTerminalBlock> blocks)
            : base(reference)
        {
            UpdateSensors(blocks);

            if (sensorBlocks.Count > 0)
            {
                IMySensorBlock sensor = sensorBlocks[0];
                Min = sensor.GetMininum<float>("Back");
                Max = sensor.GetMaximum<float>("Back");
                Default = sensor.GetDefaultValue<float>("Back");
            }
        }

        public int GetClosestSensor(VRageMath.Vector3 point)
        {
            return GetClosestSensor(point, Sensors.EmptySet);
        }

        public int GetClosestSensor(VRageMath.Vector3 point, List<int> exclude)
        {
            if (sensorBlocks.Count == 0)
                throw new Exception("Cannot get the closest sensor, because there exists no sensor.");

            int i = 0;
            while (exclude.Contains(i) && i < CountSensors) ++i;
            if (i == CountSensors)
                return i;

            int id = i;
            float dist2NearestBlock = (sensorBlocks[i].Position - point).LengthSquared();
            for (; i < sensorBlocks.Count; ++i)
            {
                if (exclude.Contains(i))
                    continue;

                float dist2 = (sensorBlocks[i].Position - point).LengthSquared();
                if (dist2 < dist2NearestBlock)
                {
                    id = i;
                    dist2NearestBlock = dist2;
                }
            }

            return id;
        }

        public void Extend(VRageMath.Vector3 dir, int id, float value)
        {
            if (!XUtils.Directions.Contains(dir))
                throw new Exception("Invalid direction vector used: " + dir);

            if (id < 0 || id >= CountSensors)
                throw new Exception("Parameter id (= " + id + ") out of range [" + 0 + ", " + CountSensors + ").");

            if (value < Min || value > Max)
                throw new Exception("Parameter value (= " + value + ") out of range [" + Min + ", " + Max + "].");

            IMySensorBlock sensor = sensorBlocks[id];
            VRageMath.Matrix toSensor;
            sensor.Orientation.GetMatrix(out toSensor);
            VRageMath.Matrix.Transpose(ref toSensor, out toSensor);
            VRageMath.Matrix toGrid;
            referenceBlock.Orientation.GetMatrix(out toGrid);
            VRageMath.Vector3.Transform(ref dir, ref toGrid, out dir);
            VRageMath.Vector3.Transform(ref dir, ref toSensor, out dir);

            string propteryId = extendDirections[dir];
            sensor.SetValue(propteryId, value);
        }

        public void ExtendBack(int id, float value)
        {
            Extend(XUtils.Identity.Backward, id, value);
        }

        public void ExtendFront(int id, float value)
        {
            Extend(XUtils.Identity.Forward, id, value);
        }

        public void ExtendTop(int id, float value)
        {
            Extend(XUtils.Identity.Up, id, value);
        }

        public void ExtendBottom(int id, float value)
        {
            Extend(XUtils.Identity.Down, id, value);
        }

        public void ExtendRight(int id, float value)
        {
            Extend(XUtils.Identity.Right, id, value);
        }

        public void ExtendLeft(int id, float value)
        {
            Extend(XUtils.Identity.Left, id, value);
        }

        public IMySensorBlock this[int i]
        {
            get
            {
                return sensorBlocks[i];
            }
        }

        public void UpdateSensors(List<IMyTerminalBlock> blocks)
        {
            sensorBlocks = new List<IMySensorBlock>();
            for (int i = 0; i < blocks.Count; ++i)
            {
                IMyTerminalBlock block = blocks[i];
                if (block is IMySensorBlock)
                    sensorBlocks.Add(block as IMySensorBlock);
            }

            if (sensorBlocks.Count == 0)
                throw new Exception("There is no sensor within the given block list.");
        }


        public static void SetFlags(IMySensorBlock sensor, int actionBitMask)
        {
            //int actionBitMask = 0;
            //for (int i = 0; i < actions.Length; ++i)
            //    actionBitMask |= actions[i].Value;

            int xorMask = CreateActionBitMask(sensor) ^ actionBitMask;
            HashSet<Action>.Enumerator enumerator = Sensors.Action.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Sensors.Action action = enumerator.Current;
                if ((xorMask & action.Value) != 0)
                    sensor.GetActionWithName(action.Name).Apply(sensor);
            }
        }

        public static int CreateActionBitMask(IMySensorBlock sensor)
        {
            int actionBitMask = 0;
            actionBitMask |= sensor.DetectAsteroids ? Action.DetectAsteroids.Value : 0;
            actionBitMask |= sensor.DetectEnemy ? Action.DetectEnemy.Value : 0;
            actionBitMask |= sensor.DetectFloatingObjects ? Action.DetectFloatingObjects.Value : 0;
            actionBitMask |= sensor.DetectFriendly ? Action.DetectFriendly.Value : 0;
            actionBitMask |= sensor.DetectLargeShips ? Action.DetectLargeShips.Value : 0;
            actionBitMask |= sensor.DetectNeutral ? Action.DetectNeutral.Value : 0;
            actionBitMask |= sensor.DetectOwner ? Action.DetectOwner.Value : 0;
            actionBitMask |= sensor.DetectPlayers ? Action.DetectPlayers.Value : 0;
            actionBitMask |= sensor.DetectSmallShips ? Action.DetectSmallShips.Value : 0;
            actionBitMask |= sensor.DetectStations ? Action.DetectStations.Value : 0;
            return actionBitMask;
        }
    }
}
