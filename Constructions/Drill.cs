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

            private static byte _count = 0;

            private byte _value;

            private DrillEvent()
            {
                this._value = _count++;
            }

            public static implicit operator byte(DrillEvent e)
            {
                return e._value;
            }
        }

        private const string _uninitializedState = "UninitializedState";
        private const string _standbyState = "StandbyState";
        private const string _searchState = "SearchState";
        private const string _drillState = "DrillState";
        private readonly Dictionary<string, Action<DrillEvent>> _stateBehavior = new Dictionary<string, Action<DrillEvent>>();

        private DateTime _lastTime = System.DateTime.Now;
        private VRageMath.Vector3 _lastWorldPosition;

        private float _pitchRadians = 0;
        public float PitchRadians { get { return _pitchRadians; } }
        // mass / 100,000 * 2 pi
        private const float _pitchRadiansLimit = 3.49781f * VRageMath.MathHelper.TwoPi;
        private float _rollRadians = 0;
        private const float _rollRadiansLimit = VRageMath.MathHelper.TwoPi / 16;
        private string _state;

        private List<IMyShipDrill> _drills = null;

        private readonly float _drillVelocity; // in meter per second
        private readonly float _astroidDetectSize; // in meter
        private readonly float _epsilon;

        public string CurrentState { get { return _state; } }

        private List<int> _sensorIds = new List<int>();

        public Drill(IMyShipController reference, float drillVelocity = 1.5f, float asteroidDetectSize = 1.5f, float epsilon = 0.1f)
            : base(reference)
        {
            _state = _uninitializedState;
            _lastWorldPosition = reference.GetPosition();
            _stateBehavior[_uninitializedState] = UpdateUninitialized;
            _stateBehavior[_standbyState] = UpdateStandby;
            _stateBehavior[_searchState] = UpdateSearch;
            _stateBehavior[_drillState] = UpdateDrill;
            _drillVelocity = drillVelocity;
            _astroidDetectSize = asteroidDetectSize;
            _epsilon = epsilon;
        }

        public void AddDrills(List<IMyTerminalBlock> blocks)
        {
            _drills = new List<IMyShipDrill>();
            for (int i = 0; i < blocks.Count; ++i)
            {
                IMyTerminalBlock block = blocks[i];
                if (block is IMyShipDrill)
                    _drills.Add(block as IMyShipDrill);
            }

            if (_drills.Count == 0)
                throw new Exception("There is no drill within the given block list.");
        }

        public void Handle(DrillEvent e)
        {
            Action<DrillEvent> action;
            if (_stateBehavior.TryGetValue(_state, out action))
                action(e);

            _lastTime = DateTime.Now;
            _lastWorldPosition = ReferenceBlock.GetPosition();
        }

        public double ElapsedTime
        {
            get
            {
                return (DateTime.Now - _lastTime).TotalSeconds;
            }
        }

        public double Velocity
        {
            get
            {
                return Math.Sqrt(VelocitySquared);
            }
        }

        public double VelocitySquared
        {
            get
            {
                double elapsedTime = ElapsedTime;
                return (ReferenceBlock.GetPosition() - _lastWorldPosition).LengthSquared() / (elapsedTime * elapsedTime);
            }
        }

        private void UpdateDrill(DrillEvent e)
        {
            if (e == DrillEvent.AsteroidLost)
            {
                StopDrilling();
                StartSearching();
                _state = _searchState;
            }
            else if (e == DrillEvent.ContainerFull || e == DrillEvent.DrillStopInvoked)
            {
                StopDrilling();
                _state = _standbyState;
            }
            else
            {
                ContinueDrilling();
            }
        }

        private void UpdateSearch(DrillEvent e)
        {
            if (e == DrillEvent.AsteroidFound)
            {
                StopSearching();
                StartDrilling();
                _state = _drillState;
            }
            else if (e == DrillEvent.ContainerFull || e == DrillEvent.DrillStopInvoked)
            {
                StopSearching();
                _state = _standbyState;
            }
            else
            {
                ContinueSearching();
            }
        }

        private void UpdateStandby(DrillEvent e)
        {
            if (e == DrillEvent.DrillStartInvoked)
            {
                StartSearching();
                _state = _searchState;
            }
            else if (e == DrillEvent.DrillStopInvoked)
            {
                StopSearching();
                StopDrilling();
            }
        }

        private void UpdateUninitialized(DrillEvent e)
        {
            if (e == DrillEvent.DrillInitialized)
            {
                InitializeDrill();
                _state = _standbyState;
            }
        }

        private void ContinueDrilling()
        {
            float diffVelocity = (float)VelocitySquared - (_drillVelocity * _drillVelocity);
            // break if it's faster
            float epsilonSquared = _epsilon * _epsilon;
            if (diffVelocity > epsilonSquared)
            {
                thrusts.SetThrustersEnabled(XUtils.Identity.Forward, false);
                thrusts.SetThrustersEnabled(XUtils.Identity.Backward, true);
            }
            // accelerate
            else if (diffVelocity < -epsilonSquared)
            {
                thrusts.SetThrustersEnabled(XUtils.Identity.Forward, true);
                thrusts.SetThrustersEnabled(XUtils.Identity.Backward, false);
            }
            else
            {
                thrusts.SetThrustersEnabled(XUtils.Identity.Forward, false);
                thrusts.SetThrustersEnabled(XUtils.Identity.Backward, false);
            }
        }

        private void StopDrilling()
        {
            for (int i = 0; i < _drills.Count; ++i)
            {
                IMyShipDrill block = _drills[i];
                block.GetActionWithName("OnOff_Off").Apply(block);
            }

            for (int i = 0; i < XUtils.Directions.Count; ++i)
            {
                VRageMath.Vector3 dir = XUtils.Directions[i];
                thrusts.SetThrustersEnabled(dir, true);
                thrusts.Accelerate(dir, thrusts.DefaultAcceleration);
            }
        }

        private void ContinueSearching()
        {
            double timeElapsed = (DateTime.Now - _lastTime).TotalSeconds;
            _pitchRadians += (float)(gyros.Pitch * timeElapsed);
            _rollRadians += (float)(gyros.Roll * timeElapsed);
            // stop rolling
            if (_rollRadians > _rollRadiansLimit)
            {
                _rollRadians = 0;
                gyros.Roll = gyros.Default;
            }

            // start rolling
            if (_pitchRadians > _pitchRadiansLimit)
            {
                _pitchRadians = 0;
                gyros.Roll = 1 * VRageMath.MathHelper.RPMToRadiansPerSecond;
            }
        }

        private void StartDrilling()
        {
            for (int i = 0; i < _drills.Count; ++i)
            {
                IMyShipDrill block = _drills[i];
                block.GetActionWithName("OnOff_On").Apply(block);
            }
            // accelerate forwards
            thrusts.SetThrustersEnabled(XUtils.Identity.Forward, true);
            thrusts.AccelerateForward(thrusts.MaxAcceleration / 4);
            // deactivate backward thrusters
            thrusts.SetThrustersEnabled(XUtils.Identity.Backward, false);
            thrusts.AccelerateBackward(thrusts.MaxAcceleration / 4);
        }

        private void StopSearching()
        {
            gyros.GyroOverride = false;
            gyros.Roll = gyros.Default;
            gyros.Pitch = gyros.Default;
            _rollRadians = 0;
            _pitchRadians = 0;
        }

        private void StartSearching()
        {
            gyros.Enable = true;
            gyros.GyroOverride = true;
            gyros.Pitch = 1 * VRageMath.MathHelper.RPMToRadiansPerSecond;
        }

        private void InitializeDrill()
        {
            if (_drills == null)
                throw new Exception("No drills have been added!");

            if (_drills.Count == 0)
                throw new Exception("There are no drills on the ship!");

            gyros.Enable = true;
            gyros.Yaw = gyros.Default;
            StopSearching();
            StopDrilling();
            SetupSensors();
        }

        private void SetupSensors()
        {
            if (sensors == null)
                throw new Exception("No sensors available (sensors == null).");

            VRageMath.BoundingBox bb = GetBounds();
            VRageMath.Matrix fromReference = new VRageMath.Matrix();
            ReferenceBlock.Orientation.GetMatrix(out fromReference);
            //VRageMath.Vector3 v = m.Forward.Min() < 0 ? -m.Forward * bb.Min + (XUtils.One + m.Forward) * bb.Max : (XUtils.One - m.Forward) * bb.Min + m.Forward * bb.Max;
            _sensorIds.Clear();
            // Asteroid search laser
            int id = sensors.GetClosestSensor(bb.Center, _sensorIds);
            if (id == sensors.CountSensors)
                throw new Exception("Not enough sensors.");
            _sensorIds.Add(id);
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
            id = sensors.GetClosestSensor(bb.Center, _sensorIds);
            if (id == sensors.CountSensors)
                throw new Exception("Not enough sensors.");
            _sensorIds.Add(id);
            sensor = sensors[id];
            SetupDrillSensor(fromReference, id, bb);
            Sensors.SetFlags(sensor, Sensors.Action.DetectAsteroids.Value);
            sensor.GetActionWithName("OnOff_On").Apply(sensors[id]);
            sensor.RequestShowOnHUD(true);
            sensor.SetCustomName(sensor.DefinitionDisplayNameText + " X Drill");
        }

        private void SetupDrillSensor(VRageMath.Matrix fromReference, int id, VRageMath.BoundingBox bb)
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
                float value = _astroidDetectSize + (Math.Max(lengthToMax, lengthToMin) + offset) * gridSize;
                value = Math.Max(Math.Min(value, sensors.Max), sensors.Min);
                sensors.Extend(dir, id, value);
            }
        }
    }
}
