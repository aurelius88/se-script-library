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
using SE_Script_Library.Utils;
using SE_Script_Library.Reference;

namespace SE_Script_Library.Constructions
{
    public class Drill : Ship
    {
        public sealed class DrillEvent
        {
            public static DrillEvent Nothing = new DrillEvent();
            public static DrillEvent DrillInitialized = new DrillEvent();
            public static DrillEvent DrillStopInvoked = new DrillEvent();
            public static DrillEvent DrillStartInvoked = new DrillEvent();
            public static DrillEvent AsteroidFound = new DrillEvent();
            public static DrillEvent AsteroidLost = new DrillEvent();
            public static DrillEvent ContainerFull = new DrillEvent();

            private static byte count = 0;

            private byte value;

            private DrillEvent()
            {
                this.value = count++;
            }

            public static implicit operator byte(DrillEvent e)
            {
                return e.value;
            }
        }

        private const string UninitializedState = "UninitializedState";
        private const string StandbyState = "StandbyState";
        private const string SearchState = "SearchState";
        private const string DrillState = "DrillState";
        private readonly Dictionary<string, Action<DrillEvent>> stateBehavior = new Dictionary<string, Action<DrillEvent>>();

        private DateTime lastTime = System.DateTime.Now;
        private VRageMath.Vector3 lastWorldPosition;
        private const float DrillVelocity = 1; // in meter per second

        private float PitchRadians = 0;
        private const float PitchRadiansLimit = 2 * VRageMath.MathHelper.TwoPi;
        private float RollRadians = 0;
        private const float RollRadiansLimit = VRageMath.MathHelper.TwoPi / 16;
        private string state;
        private List<IMyShipDrill> drills = null;

        private const float EpsilonOver2 = 0.1f * 0.1f;

        public string CurrentState { get { return state; } }
        private int AstroidDetectSize = 0; // in meter

        private List<int> sensorIds = new List<int>();

        public Drill(IMyShipController reference)
            : base(reference)
        {
            state = UninitializedState;
            lastWorldPosition = reference.GetPosition();
            stateBehavior[UninitializedState] = UpdateUninitialized;
            stateBehavior[StandbyState] = UpdateStandby;
            stateBehavior[SearchState] = UpdateSearch;
            stateBehavior[DrillState] = UpdateDrill;
        }

        public void AddDrills(List<IMyTerminalBlock> blocks)
        {
            drills = new List<IMyShipDrill>();
            for (int i = 0; i < blocks.Count; ++i)
            {
                IMyTerminalBlock block = blocks[i];
                if (block is IMyShipDrill)
                    drills.Add(block as IMyShipDrill);
            }

            if (drills.Count == 0)
                throw new Exception("There is no drill within the given block list.");
        }

        public void handle(DrillEvent e)
        {
            Action<DrillEvent> action;
            if (stateBehavior.TryGetValue(state, out action))
                action(e);

            lastTime = DateTime.Now;
            lastWorldPosition = referenceBlock.GetPosition();
        }

        private void UpdateDrill(DrillEvent e)
        {
            if (e == DrillEvent.AsteroidLost)
            {
                stopDrilling();
                startSearching();
                state = SearchState;
            }
            else if (e == DrillEvent.ContainerFull || e == DrillEvent.DrillStopInvoked)
            {
                stopDrilling();
                state = StandbyState;
            }
            else
            {
                continueDrilling();
            }
        }

        private void UpdateSearch(DrillEvent e)
        {
            if (e == DrillEvent.AsteroidFound)
            {
                stopSearching();
                startDrilling();
                state = DrillState;
            }
            else if (e == DrillEvent.DrillStopInvoked)
            {
                stopSearching();
                state = StandbyState;
            }
            else
            {
                continueSearching();
            }
        }

        private void UpdateStandby(DrillEvent e)
        {
            if (e == DrillEvent.DrillStartInvoked)
            {
                startSearching();
                state = SearchState;
            }
            else if (e == DrillEvent.DrillStopInvoked)
            {
                stopSearching();
                stopDrilling();
            }
        }

        private void UpdateUninitialized(DrillEvent e)
        {
            if (e == DrillEvent.DrillInitialized)
            {
                initializeDrill();
                state = StandbyState;
            }
        }

        private void continueDrilling()
        {
            double elapsedTime = (lastTime - DateTime.Now).TotalSeconds;
            float velocityOver2 = (float)((referenceBlock.GetPosition() - lastWorldPosition).LengthSquared() / elapsedTime * elapsedTime);
            float diffVelocity = velocityOver2 - DrillVelocity * DrillVelocity;
            // break if it's faster
            if (diffVelocity > EpsilonOver2)
            {
                thrusts.SetThrustersEnabled(XUtils.Identity.Forward, false);
                thrusts.SetThrustersEnabled(XUtils.Identity.Backward, true);
            }
            // accelerate
            else if (diffVelocity < -EpsilonOver2)
            {
                thrusts.SetThrustersEnabled(XUtils.Identity.Forward, true);
                thrusts.SetThrustersEnabled(XUtils.Identity.Backward, false);
            }
        }

        private void stopDrilling()
        {
            for (int i = 0; i < drills.Count; ++i)
            {
                IMyShipDrill block = drills[i];
                block.GetActionWithName("OnOff_Off").Apply(block);
            }

            for (int i = 0; i < XUtils.Directions.Count; ++i)
            {
                VRageMath.Vector3 dir = XUtils.Directions[i];
                thrusts.SetThrustersEnabled(dir, true);
                thrusts.Accelerate(dir, thrusts.DefaultAcceleration);
            }
        }

        private void continueSearching()
        {
            double timeElapsed = (DateTime.Now - lastTime).TotalSeconds;
            PitchRadians += (float)(gyros.Pitch * timeElapsed);
            RollRadians += (float)(gyros.Roll * timeElapsed);
            // stop rolling
            if (RollRadians > RollRadiansLimit)
            {
                RollRadians = 0;
                gyros.Roll = gyros.Default;
            }

            // start rolling
            if (PitchRadians > PitchRadiansLimit)
            {
                PitchRadians = 0;
                gyros.Roll = 1 * VRageMath.MathHelper.RPMToRadiansPerSecond;
            }
        }

        private void startDrilling()
        {
            for (int i = 0; i < drills.Count; ++i)
            {
                IMyShipDrill block = drills[i];
                block.GetActionWithName("OnOff_On").Apply(block);
            }
            // accelerate forwards
            thrusts.SetThrustersEnabled(XUtils.Identity.Forward, true);
            thrusts.AccelerateForward(thrusts.MaxAcceleration / 4);
            // deactivate backward thrusters
            thrusts.SetThrustersEnabled(XUtils.Identity.Backward, false);
        }

        private void stopSearching()
        {
            gyros.GyroOverride = false;
            gyros.Roll = gyros.Default;
            gyros.Pitch = gyros.Default;
            RollRadians = 0;
            PitchRadians = 0;
        }

        private void startSearching()
        {
            gyros.Enable = true;
            gyros.GyroOverride = true;
            gyros.Pitch = 1 * VRageMath.MathHelper.RPMToRadiansPerSecond;
        }

        private void initializeDrill()
        {
            if (drills == null)
                throw new Exception("No drills have been added!");

            if (drills.Count == 0)
                throw new Exception("There are no drills on the ship!");

            gyros.Enable = true;
            gyros.Yaw = gyros.Default;
            stopSearching();
            stopDrilling();
            setupSensors();
        }

        private void setupSensors()
        {
            if (sensors == null)
                throw new Exception("No sensors available (sensors == null).");

            VRageMath.BoundingBox bb = GetBounds();
            VRageMath.Matrix fromReference = new VRageMath.Matrix();
            referenceBlock.Orientation.GetMatrix(out fromReference);
            //VRageMath.Vector3 v = m.Forward.Min() < 0 ? -m.Forward * bb.Min + (XUtils.One + m.Forward) * bb.Max : (XUtils.One - m.Forward) * bb.Min + m.Forward * bb.Max;
            sensorIds.Clear();
            // Asteroid search laser
            int id = sensors.GetClosestSensor(bb.Center, sensorIds);
            if (id == sensors.CountSensors)
                throw new Exception("Not enough sensors.");
            sensorIds.Add(id);
            var sensor = sensors[id];
            sensors.ExtendFront(id, sensors.Max);
            sensors.ExtendBack(id, sensors.Min);
            sensors.ExtendBottom(id, sensors.Min);
            sensors.ExtendLeft(id, sensors.Min);
            sensors.ExtendRight(id, sensors.Min);
            sensors.ExtendTop(id, sensors.Min);
            Sensors.SetFlags(sensor, Sensors.Action.DetectAsteroids.Value);
            sensor.GetActionWithName("OnOff_On").Apply(sensor);
            sensor.RequestShowOnHUD(true);
            sensor.SetCustomName(sensor.DefinitionDisplayNameText + " X Laser");

            // Asteroid collision detector
            id = sensors.GetClosestSensor(bb.Center, sensorIds);
            if (id == sensors.CountSensors)
                throw new Exception("Not enough sensors.");
            sensorIds.Add(id);
            sensor = sensors[id];
            setupDrillSensor(fromReference, id, bb);
            Sensors.SetFlags(sensor, Sensors.Action.DetectAsteroids.Value);
            sensor.GetActionWithName("OnOff_On").Apply(sensors[id]);
            sensor.RequestShowOnHUD(true);
            sensor.SetCustomName(sensor.DefinitionDisplayNameText + " X Drill");
        }

        private void setupDrillSensor(VRageMath.Matrix fromReference, int id, VRageMath.BoundingBox bb)
        {
            // grid size in meters per block
            float gridSize = sensors[id].CubeGrid.GridSize;

            // matrix from grid coordinate system to sensor coordinate system
            VRageMath.Matrix toSensor = new VRageMath.Matrix();
            sensors[id].Orientation.GetMatrix(out toSensor);
            // matrix is orthogonal => transposed matrix = inversed matrix
            VRageMath.Matrix.Transpose(ref toSensor, out toSensor);

            VRageMath.Vector3[] corners = bb.GetCorners();
            VRageMath.Vector3 diffMax = corners[1] - sensors[id].Position;
            VRageMath.Vector3 diffMin = corners[7] - sensors[id].Position;

            List<VRageMath.Vector3>.Enumerator enumerator = XUtils.Directions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                VRageMath.Vector3 dir = enumerator.Current;
                VRageMath.Vector3 gridDir = VRageMath.Vector3.Transform(dir, fromReference);
                float lengthToMax = (diffMax * gridDir).Max();
                float lengthToMin = (diffMin * gridDir).Max();
                float offset = Sensors.getOffset(VRageMath.Vector3.Transform(gridDir, toSensor));
                float value = AstroidDetectSize + (Math.Max(lengthToMax, lengthToMin) + offset) * gridSize;
                value = Math.Max(Math.Min(value, sensors.Max), sensors.Min);
                sensors.Extend(dir, id, value);
            }
        }
    }
}
