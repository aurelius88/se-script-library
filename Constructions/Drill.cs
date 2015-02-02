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
    class Drill : Ship
    {
        public sealed class DrillEvent
        {
            public static const DrillEvent DrillInitialized = new DrillEvent();
            public static const DrillEvent DrillStopped = new DrillEvent();
            public static const DrillEvent DrillStarted = new DrillEvent();
            public static const DrillEvent AsteroidFound = new DrillEvent();
            public static const DrillEvent AsteroidLost = new DrillEvent();
            public static const DrillEvent ContainerFull = new DrillEvent();

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

        private State state;
        private int AstroidDetectSize = 10;

        public State State { get { return state; } }

        private HashSet<int> sensorIds;
        private List<bool> sensorStates;

        public Drill(IMyShipController reference)
            : base(ref reference)
        {
            state = new UninitializedState(this);
            state.enter();
        }

        public void checkSensors()
        {

        }

        public void handle(DrillEvent e)
        {
            State newState = state.update(e);
            if (newState == null)
                return;

            state.exit();
            state = newState;
            state.enter();
        }

        void initialize()
        {
            setupSensors();
            handle(DrillEvent.DrillInitialized);
        }

        private void setupSensors()
        {
            VRageMath.BoundingBox bb = GetBounds();
            VRageMath.Matrix toReference;
            referenceBlock.Orientation.GetMatrix(out toReference);
            //VRageMath.Vector3 v = m.Forward.Min() < 0 ? -m.Forward * bb.Min + (XUtils.One + m.Forward) * bb.Max : (XUtils.One - m.Forward) * bb.Min + m.Forward * bb.Max;
            sensorIds = new HashSet<int>();
            sensorStates = new List<bool>();
            // Asteroid search laser
            int id = sensors.GetClosestSensor(bb.Center, sensorIds);
            sensorIds.Add(id);
            var sensor = sensors[id];
            sensors.ExtendFront(id, sensors.Max);
            sensors.ExtendBack(id, sensors.Min);
            sensors.ExtendBottom(id, sensors.Min);
            sensors.ExtendLeft(id, sensors.Min);
            sensors.ExtendRight(id, sensors.Min);
            sensors.ExtendTop(id, sensors.Min);
            Sensors.SetFlags(sensor, Sensors.Action.DetectAsteroids);
            sensor.GetActionWithName("OnOff_On").Apply(sensor);
            sensor.RequestShowOnHUD(true);
            sensor.SetCustomName(sensors[id].CustomName + " X Laser");
            // Asteroid collision detector
            id = sensors.GetClosestSensor(bb.Center, sensorIds);
            sensorIds.Add(id);
            sensor = sensors[id];
            setupDrillSensor(ref toReference, id, ref bb);
            Sensors.SetFlags(sensor, Sensors.Action.DetectAsteroids);
            sensor.GetActionWithName("OnOff_On").Apply(sensors[id]);
            sensor.RequestShowOnHUD(true);
            sensor.SetCustomName(sensor.CustomName + " X Drill");
        }

        private void setupDrillSensor(ref VRageMath.Matrix toReference, int id, ref VRageMath.BoundingBox bb)
        {
            VRageMath.Vector3 diffMax = bb.Max - sensors[id].Position;
            VRageMath.Vector3 diffMin = bb.Min - sensors[id].Position;
            float max = (diffMax * toReference.Forward).Max();
            float min = (diffMin * toReference.Forward).Max();
            float value = AstroidDetectSize + (max > 0 ? max : min > 0 ? min : 0);
            sensors.ExtendFront(id, value);
            max = (diffMax * toReference.Backward).Max();
            min = (diffMin * toReference.Backward).Max();
            value = AstroidDetectSize + (max > 0 ? max : min > 0 ? min : 0);
            sensors.ExtendFront(id, value);
            max = (diffMax * toReference.Up).Max();
            min = (diffMin * toReference.Up).Max();
            value = AstroidDetectSize + (max > 0 ? max : min > 0 ? min : 0);
            sensors.ExtendFront(id, value);
            max = (diffMax * toReference.Down).Max();
            min = (diffMin * toReference.Down).Max();
            value = AstroidDetectSize + (max > 0 ? max : min > 0 ? min : 0);
            sensors.ExtendFront(id, value);
            max = (diffMax * toReference.Right).Max();
            min = (diffMin * toReference.Right).Max();
            value = AstroidDetectSize + (max > 0 ? max : min > 0 ? min : 0);
            sensors.ExtendFront(id, value);
            max = (diffMax * toReference.Left).Max();
            min = (diffMin * toReference.Left).Max();
            value = AstroidDetectSize + (max > 0 ? max : min > 0 ? min : 0);
            sensors.ExtendFront(id, value);
        }

        abstract class State
        {
            protected Drill drill;

            internal State(ref Drill drill)
            {
                this.drill = drill;
            }

            internal abstract void enter();
            internal abstract State update(DrillEvent e);
            internal abstract void exit();
        }

        class UninitializedState : State
        {
            public UninitializedState(Drill drill) : base(ref drill) { }

            internal override void enter()
            {
                drill.initialize();
            }

            internal override State update(DrillEvent e)
            {
                if (e == DrillEvent.DrillInitialized)
                    return new StandbyState(drill);

                return null;
            }

            internal override void exit()
            {
                throw new NotImplementedException();
            }
        }

        class StandbyState : State
        {
            public StandbyState(Drill drill) : base(ref drill) { }

            internal override void enter()
            {
                throw new NotImplementedException();
            }

            internal override State update(DrillEvent e)
            {
                if (e == DrillEvent.DrillStarted)
                    return new SearchState(drill);

                return null;
            }

            internal override void exit()
            {
                throw new NotImplementedException();
            }
        }

        class SearchState : State
        {
            public SearchState(Drill drill) : base(ref drill) { }

            internal override void enter()
            {
                throw new NotImplementedException();
            }

            internal override State update(DrillEvent e)
            {
                if (e == DrillEvent.DrillStarted)
                    return new SearchState(drill);

                return null;
            }

            internal override void exit()
            {
                throw new NotImplementedException();
            }
        }

        class DrillState : State
        {
            public DrillState(Drill drill) : base(ref drill) { }

            internal override void enter()
            {
                throw new NotImplementedException();
            }

            internal override State update(DrillEvent e)
            {
                if (e == DrillEvent.AsteroidLost)
                    return new SearchState(drill);

                if (e == DrillEvent.ContainerFull)
                    return new StandbyState(drill);

                return null;
            }

            internal override void exit()
            {
                throw new NotImplementedException();
            }
        }
    }
}
