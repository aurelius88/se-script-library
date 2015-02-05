using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Constructions;
using SE_Script_Library.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;

namespace SE_Script_Library.Scripts
{

    public class AutoDrill : IngameScript
    {
        private const string InputString = "xIn#0:";
        private const string OutputString = "xOut#0:";
        private const string Seperator = ",";

        // Commands
        private const string Reset = "reset";
        private const string Stop = "stop";
        private const string Start = "start";
        private const string Found = "found";
        private const string Lost = "lost";
        private const string Debug = "debug";
        private const string Full = "full";

        private Drill drill;

        private Dictionary<string, Action<Drill.DrillEvent>> EventHandler = new Dictionary<string, Action<Drill.DrillEvent>>();
        private static Dictionary<string, Drill.DrillEvent> EventMap = new Dictionary<string, Drill.DrillEvent>() {
            {Reset, Drill.DrillEvent.Nothing},
            {Stop, Drill.DrillEvent.DrillStopInvoked},
            {Start, Drill.DrillEvent.DrillStartInvoked},
            {Found, Drill.DrillEvent.AsteroidFound},
            {Lost, Drill.DrillEvent.AsteroidLost},
            {Debug,Drill.DrillEvent.Nothing},
            {Full, Drill.DrillEvent.ContainerFull}
        };

        private bool DebugActive = false;

        void Main()
        {
            var list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(InputString, list);
            if (list.Count == 0)
                throw new Exception("Please mark a block with '" + InputString + "' for input commands.");

            var block = list[0];
            string[] tokens = block.CustomName.Split(':');
            tokens = tokens[1].Split(Seperator.ToCharArray());
            block.SetCustomName(InputString);

            if (drill == null)
                ResetDrill(Drill.DrillEvent.Nothing);

            Output(OutputString + '\n');
            Output(drill.CurrentState + '\n', true);
            if (DebugActive)
            {
                Output(String.Format("Velocity = {0:F2} m/s\n", VRageMath.MathHelper.RoundOn2((float)drill.Velocity)), true);
                Output(String.Format("ElapsedTime = {0:F2} s\n", VRageMath.MathHelper.RoundOn2((float)drill.ElapsedTime)), true);
                Output(String.Format("PitchRadians = {0:F2} rad\n", VRageMath.MathHelper.RoundOn2(drill.PitchRadians)), true);
            }
            int nothingDone = 0;
            for (int i = 0; i < tokens.Length; ++i)
            {
                string token = tokens[i].Trim().ToLower();
                if (token.Length == 0)
                {
                    nothingDone++;
                    continue;
                }

                Action<Drill.DrillEvent> action;
                if (!EventHandler.TryGetValue(token, out action))
                {
                    nothingDone++;
                    Output("-> Warning! Command not accepted: '" + token + "'\n", true);
                    continue;
                }

                Drill.DrillEvent drillEvent;
                if (!EventMap.TryGetValue(token, out drillEvent))
                {
                    nothingDone++;
                    Output("-> Warning! There exists no mapping for '" + token + "'\n", true);
                    continue;
                }

                if (DebugActive)
                    Output("Map '" + token + "' -> '" + drillEvent + "' and use '" + action + "'\n", true);
                action(drillEvent);
            }

            if (tokens.Length - nothingDone <= 0)
                drill.Handle(Drill.DrillEvent.Nothing);

            Output(drill.CurrentState, true);
        }

        private bool NoPassenger(IMyTerminalBlock arg)
        {
            return !arg.DefinitionDisplayNameText.Equals("Passenger Seat");
        }

        void Output(String message, bool append = false)
        {
            var list = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(OutputString, list);
            if (list.Count > 0)
            {
                list[0].SetCustomName(append ? list[0].CustomName + message : message);
            }
        }

        void HandleDebug(Drill.DrillEvent e)
        {
            DebugActive = !DebugActive;
            Output("Toggle debug mode to '" + DebugActive + "'", true);
        }

        void ResetDrill(Drill.DrillEvent e)
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, NoPassenger);
            if (blocks.Count == 0)
                throw new Exception("No ship controller (cockpit, etc.) available.");

            drill = new Drill(blocks[0] as IMyShipController);

            EventHandler.Clear();
            EventHandler.Add(Reset, ResetDrill);
            EventHandler.Add(Start, drill.Handle);
            EventHandler.Add(Stop, drill.Handle);
            EventHandler.Add(Found, drill.Handle);
            EventHandler.Add(Lost, drill.Handle);
            EventHandler.Add(Debug, HandleDebug);
            EventHandler.Add(Full, drill.Handle);

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyGyro>(blocks);
            drill.AddGyroskopes(blocks);
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks);
            drill.AddThrusters(blocks);
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(blocks);
            drill.AddSensors(blocks);
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(blocks);
            drill.AddDrills(blocks);
            drill.Handle(Drill.DrillEvent.DrillInitialized);
        }
    }

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

    public class Ship
    {
        public readonly IMyTerminalBlock ReferenceBlock;

        public Thrusters thrusts;
        public Gyroscopes gyros;
        public Sensors sensors;

        public Ship(IMyShipController controller)
        {
            ReferenceBlock = controller;
        }

        public void AddThrusters(List<IMyTerminalBlock> blocks)
        {
            if (thrusts == null)
            {
                thrusts = new Thrusters(ReferenceBlock, blocks);
                return;
            }

            thrusts.UpdateThrusters(blocks);
        }

        public void AddGyroskopes(List<IMyTerminalBlock> blocks)
        {
            if (gyros == null)
            {
                gyros = new Gyroscopes(ReferenceBlock, blocks);
            }

            gyros.UpdateGyroscopes(blocks);
        }

        public void AddSensors(List<IMyTerminalBlock> blocks)
        {
            if (sensors == null)
            {
                sensors = new Sensors(ReferenceBlock, blocks);
            }

            sensors.UpdateSensors(blocks);
        }

        public VRageMath.BoundingBox GetBounds()
        {
            var grid = ReferenceBlock.CubeGrid;
            return new VRageMath.BoundingBox(grid.Min, grid.Max);
        }
    }

    public abstract class ReferenceOrientedBlocks
    {

        /// <summary>
        /// The block to which the actions are oriented to.
        /// </summary>
        public readonly IMyTerminalBlock referenceBlock;

        public ReferenceOrientedBlocks(IMyTerminalBlock referenceBlock)
        {
            this.referenceBlock = referenceBlock;
        }
    }

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
        private Dictionary<VRageMath.Vector3, List<IMyThrust>> thrusterBlocks;

        public int CountThrusters
        {
            get
            {
                int count = 0;
                Dictionary<VRageMath.Vector3, List<IMyThrust>>.ValueCollection.Enumerator enumerator = thrusterBlocks.Values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    count += enumerator.Current.Count;
                }
                return count;
            }
        }

        public Thrusters(IMyTerminalBlock referenceBlock, List<IMyTerminalBlock> blocks)
            : base(referenceBlock)
        {
            UpdateThrusters(blocks);

            Dictionary<VRageMath.Vector3, List<IMyThrust>>.ValueCollection.Enumerator enumerator = thrusterBlocks.Values.GetEnumerator();
            enumerator.MoveNext();
            IMyThrust thrust = enumerator.Current[0];
            MinAcceleration = thrust.GetMininum<float>("Override");
            MaxAcceleration = thrust.GetMaximum<float>("Override");
            DefaultAcceleration = thrust.GetDefaultValue<float>("Override");
        }

        public void ApplyAction(VRageMath.Vector3 dir, string name)
        {
            if (!XUtils.Directions.Contains(dir))
                throw new Exception("Invalid direction vector used: " + dir);

            VRageMath.Matrix local = new VRageMath.Matrix();
            referenceBlock.Orientation.GetMatrix(out local);
            dir = VRageMath.Vector3.Transform(dir, local);

            var list = thrusterBlocks[dir];
            for (int i = 0; i < list.Count; ++i)
            {
                IMyThrust thruster = list[i];
                thruster.GetActionWithName(name).Apply(thruster);
            }
        }

        /// <summary>
        /// Accelerates the ship relative to the reference block.
        /// </summary>
        /// <param name="dir">The direction relative to the reference block in which the ship should be accelerated.</param>
        /// <param name="value">The amount of force to accelerate in Newton.</param>
        public void Accelerate(VRageMath.Vector3 dir, float value)
        {
            if ((value < MinAcceleration || value > MaxAcceleration) && value != DefaultAcceleration)
                throw new Exception("Value '" + value + "' out of range [" + MinAcceleration + ", " + MaxAcceleration + "] + " + DefaultAcceleration + ".");

            if (!thrusterBlocks.ContainsKey(dir))
                throw new Exception("Warning! No thruster in direction " + dir + ".");

            var list = thrusterBlocks[dir];
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

        public bool AreThrustersEnabled(VRageMath.Vector3 dir)
        {
            if (!thrusterBlocks.ContainsKey(dir))
                throw new Exception("Warning! No thruster in direction " + dir + ".");

            bool enabled = false;
            var list = thrusterBlocks[dir];
            int i = 0;
            while (!enabled && i < list.Count)
            {
                enabled |= list[i].Enabled;
                i++;
            }
            return enabled;
        }

        public void SetThrustersEnabled(VRageMath.Vector3 dir, bool value)
        {
            if (!thrusterBlocks.ContainsKey(dir))
                throw new Exception("Warning! No thruster in direction " + dir + ".");

            var list = thrusterBlocks[dir];
            for (int i = 0; i < list.Count; ++i)
            {
                IMyThrust block = list[i];
                if (block.Enabled ^ value)
                    block.GetActionWithName("OnOff").Apply(block);
            }
        }

        public void UpdateThrusters(List<IMyTerminalBlock> blocks)
        {
            thrusterBlocks = new Dictionary<VRageMath.Vector3, List<IMyThrust>>();
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
                    if (!thrusterBlocks.ContainsKey(dir))
                        thrusterBlocks[dir] = new List<IMyThrust>();
                    thrusterBlocks[dir].Add(block as IMyThrust);
                }
            }

            if (thrusterBlocks.Count == 0)
                throw new Exception("There is no thruster within the given block list.");
        }
    }

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

        private static List<int> EmptySet = new List<int>();

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

    public class GyroAction
    {
        private static HashSet<GyroAction> elements = new HashSet<GyroAction>();

        public static GyroAction Pitch = new GyroAction("Pitch", true);
        public static GyroAction Yaw = new GyroAction("Yaw");
        public static GyroAction Roll = new GyroAction("Roll");
        public static GyroAction PitchRev = new GyroAction("Pitch");
        public static GyroAction YawRev = new GyroAction("Yaw", true);
        public static GyroAction RollRev = new GyroAction("Roll", true);

        private static Dictionary<VRageMath.Vector3, GyroAction> gyroActions = new Dictionary<VRageMath.Vector3, GyroAction>()
            {
                {XUtils.Identity.Right, GyroAction.Pitch},
                {XUtils.Identity.Left, GyroAction.PitchRev},
                {XUtils.Identity.Up, GyroAction.Yaw},
                {XUtils.Identity.Down, GyroAction.YawRev},
                {XUtils.Identity.Backward, GyroAction.Roll},
                {XUtils.Identity.Forward, GyroAction.RollRev}
            };

        private string name;
        public string Name { get { return name; } }
        private bool reversed;
        public bool Reversed { get { return reversed; } }

        private GyroAction(string name, bool reversed = false)
        {
            this.reversed = reversed;
            this.name = name;
            elements.Add(this);
        }

        public override string ToString()
        {
            return name;
        }

        public static HashSet<GyroAction> GetElements()
        {
            return elements;
        }

        public static GyroAction getActionAroundAxis(VRageMath.Vector3 axis)
        {
            return gyroActions[axis];
        }

    }

    public class XUtils
    {
        public static VRageMath.Matrix Identity = new VRageMath.Matrix(1, 0, 0, 0, 1, 0, 0, 0, 1);
        public static VRageMath.Vector3 One = new VRageMath.Vector3(1);

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
        public static List<IMyCubeBlock> FindPerpendicularTo(IMyCubeBlock block)
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
