using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestScript
{

    public class FullAPITest : IngameScript
    {

        HashSet<Type> blockTypes = new HashSet<Type>()
        {

        };

        string blockName = "Test";
        long playerId;

        bool FuncTest(IMyTerminalBlock block)
        {
            return true;
        }

        bool FuncTest(ITerminalAction action)
        {
            return true;
        }

        bool FuncTest(IMyFunctionalBlock block)
        {
            return true;
        }

        bool FuncTest(IMyRadioAntenna block)
        {
            IMyRadioAntenna radioAntenna = block;
            //Parent: IMyFunctionalBlock
            float Radius = radioAntenna.Radius;
            return true;
        }

        bool FuncTest(IMyRefinery block)
        {
            //Arc furnace
            //Refinery
            //Parent: IMyProductionBlock
            //Parent: IMyFunctionalBlock
            //Fields:
            bool UseConveyorSystem = block.UseConveyorSystem;
            return true;
        }

        bool FuncTest(IMyVirtualMass block)
        {
            //            Artificial Mass
            //Interface name: IMyVirtualMass
            //Parent: IMyFunctionalBlock
            return true;
        }

        bool FuncTest(IMyAssembler block)
        {

            //Assembler
            //Interface name: IMyAssembler
            //Parent: IMyProductionBlock
            //Parent: IMyFunctionalBlock
            bool UseConveyorSystem = block.UseConveyorSystem;

            return true;
        }

        bool FuncTest(IMyBatteryBlock block)
        {
            //Battery
            //Interface name: IMyBatteryBlock
            //Parent: IMyFunctionalBlock
            //Fields: 
            bool HasCapacityRemaining = block.HasCapacityRemaining;
            return true;
        }

        bool FuncTest(IMyBeacon block)
        {
            //Beacon
            //Interface name: IMyBeacon
            //Parent: IMyFunctionalBlock
            //Fields:
            float Radius = block.Radius;
            return true;
        }

        bool FuncTest(IMyButtonPanel block)
        {
            //Button Panel
            //Interface name: IMyButtonPanel
            //Fields: 
            bool AnyoneCanUse = block.AnyoneCanUse;
            return true;
        }

        bool FuncTest(IMyCameraBlock block)
        {
            //Camera
            //Interface name: IMyCameraBlock
            //Parent: IMyFunctionalBlock
            //Fields: None
            return true;
        }

        bool FuncTest(IMyCockpit block)
        {
            //Cockpit
            //Control Station
            //Flight Seat
            //Passenger Seat
            //Interface name: IMyCockpit
            //Parent: IMyShipController
            //Fields:
            bool ControlWheels = block.ControlWheels;
            bool ControlThrusters = block.ControlThrusters;
            bool HandBrake = block.HandBrake;
            bool DampenersOverride = block.DampenersOverride;
            return true;
        }

        bool FuncTest(IMyCollector block)
        {
            //Collector
            //Interface name: IMyCollector
            //Parent: IMyFunctionalBlock
            //Fields: 
            bool UseConveyorSystem = block.UseConveyorSystem;
            return true;
        }

        bool FuncTest(IMyShipConnector block)
        {
            //Connector
            //Interface name: IMyShipConnector
            //Parent: IMyFunctionalBlock
            //Fields:
            bool ThrowOut = block.ThrowOut;
            bool CollectAll = block.CollectAll;
            bool IsLocked = block.IsLocked;
            return true;
        }

        bool FuncTest(IMyControlPanel block)
        {
            //Control Panel
            //Interface name: IMyControlPanel
            return true;
        }

        bool FuncTest(IMyDoor block)
        {
            //Door
            //Interface name: IMyDoor
            //Parent: IMyFunctionalBlock
            //Fields: 
            bool Open = block.Open;
            return true;
        }

        bool FuncTest(IMyShipDrill block)
        {
            //Drill
            //Interface name: IMyShipDrill
            //Parent: IMyFunctionalBlock
            //Fields: 
            bool UseConveyorSystem = block.UseConveyorSystem;
            return true;
        }

        bool FuncTest(IMyLargeGatlingTurret block)
        {
            //Gatling Turret
            //Interface name: IMyLargeGatlingTurret
            //Parent: IMyLargeConveyorTurretBase
            //Parent: IMyLargeTurretBase
            //Parent: IMyFunctionalBlock
            //Fields:
            bool UseConveyorSystem = block.UseConveyorSystem;
            bool CanControl = block.CanControl;
            float Range = block.Range
                ;
            return true;
        }

        bool FuncTest(IMyGravityGenerator block)
        {
            //Gravity Generator
            //Interface name: IMyGravityGenerator
            //Parent: IMyGravityGeneratorBase
            //Parent: IMyFunctionalBlock
            //Fields:
            float FieldWidth = block.FieldWidth;
            float FieldHeight = block.FieldHeight;
            float FieldDepth = block.FieldDepth;
            float Gravity = block.Gravity;
            return true;
        }

        bool FuncTest(IMyShipGrinder block)
        {
            //Grinder
            //Interface name: IMyShipGrinder
            //Parent: IMyShipToolBase
            //Parent: IMyFunctionalBlock
            //Fields: None
            return true;
        }

        bool FuncTest(IMyGyro block)
        {
            //Gyroscope
            //Interface name: IMyGyro
            //Parent: IMyFunctionalBlock
            //Fields:
            float GyroPower = block.GyroPower;
            bool GyroOverride = block.GyroOverride;
            float Yaw = block.Yaw;
            float Pitch = block.Pitch;
            float Roll = block.Roll;
            return true;
        }

        bool FuncTest(IMyInteriorLight block)
        {
            //Interior Light
            //Interface name: IMyInteriorLight
            //Parent: IMyLightingBlock
            //Parent: IMyFunctionalBlock
            //Fields:
            float Radius = block.Radius;
            float Intensity = block.Intensity;
            float BlinkIntervalSeconds = block.BlinkIntervalSeconds;
            float BlinkLenght = block.BlinkLenght;
            float BlinkOffset = block.BlinkOffset;
            return true;
        }

        bool FuncTest(IMyLargeInteriorTurret block)
        {
            //Interior Turret
            //Interface name: IMyLargeInteriorTurret
            //Parent: IMyLargeTurretBase
            //Parent: IMyFunctionalBlock
            //Fields:
            bool CanControl = block.CanControl;
            float Range = block.Range;
            return true;
        }

        bool FuncTest(IMyLandingGear block)
        {
            //Landing Gear
            //Interface name: IMyLandingGear
            //Parent: IMyFunctionalBlock
            //Fields:
            float BreakForce = block.BreakForce;
            return true;
        }

        bool FuncTest(IMyCargoContainer block)
        {
            //Small Cargo Container
            //Medium Cargo Container
            //Large Cargo Container
            //Interface name: IMyCargoContainer
            return true;
        }

        bool FuncTest(IMyReactor block)
        {
            //Small Reactor
            //Large Reactor
            //Interface name: IMyReactor
            //Parent: IMyFunctionalBlock
            //Fields:
            bool UseConveyorSystem = block.UseConveyorSystem;
            return true;
        }

        bool FuncTest(IMyThrust block)
        {
            //Small Thruster
            //Large Thruster
            //Interface name: IMyThrust
            //Parent: IMyFunctionalBlock
            //Fields:
            float ThrustOverride = block.ThrustOverride;
            return true;
        }

        bool FuncTest(IMyMedicalRoom block)
        {
            //Medical Room
            //Interface name: IMyMedicalRoom
            //Parent: IMyFunctionalBlock
            return true;
        }

        bool FuncTest(IMyShipMergeBlock block)
        {
            //Merge Block
            //Interface name: IMyShipMergeBlock
            //Parent: IMyFunctionalBlock
            return true;
        }

        bool FuncTest(IMyLargeMissileTurret block)
        {
            //Missile Turret
            //Interface name: IMyLargeMissileTurret
            //Parent: IMyLargeConveyorTurretBase
            //Parent: IMyLargeTurretBase
            //Parent: IMyFunctionalBlock
            //Fields:
            bool UseConveyorSystem = block.UseConveyorSystem;
            bool CanControl = block.CanControl;
            float Range = block.Range;
            return true;
        }

        bool FuncTest(IMyOreDetector block)
        {
            //Ore Detector
            //Interace name: IMyOreDetector
            //Parent: IMyFunctionalBlock
            //Fields:
            float Range = block.Range;
            bool BroadcastUsingAntennas = block.BroadcastUsingAntennas;
            return true;
        }

        bool FuncTest(IMyPistonBase block)
        {
            //Piston
            //Interface name: IMyPistonBase
            //Parent: IMyFunctionalBlock
            //Fields:
            float Velocity = block.Velocity;
            float MinLimit = block.MinLimit;
            float MaxLimit = block.MaxLimit;
            return true;
        }

        bool FuncTest(IMyProgrammableBlock block)
        {
            //Programmable block
            //Interface name: IMyProgrammableBlock
            //Parent: IMyFunctionalBlock
            //Fields:
            bool IsRunning = block.IsRunning;
            return true;
        }

        bool FuncTest(IMySmallMissileLauncherReload block)
        {
            //Reloadable Rocket Launcher
            //Interface name: IMySmallMissileLauncherReload
            //Parent: IMyFunctionalBlock
            //Fields: 
            bool UseConveyorSystem = block.UseConveyorSystem;
            return true;
        }

        bool FuncTest(IMyRemoteControl block)
        {
            //Remote Control
            //Interface name: IMyRemoteControl
            //Parent: IMyShipController
            //Fields:
            bool ControlWheels = block.ControlWheels;
            bool ControlThrusters = block.ControlThrusters;
            bool HandBrake = block.HandBrake;
            bool DampenersOverride = block.DampenersOverride;
            return true;
        }

        bool FuncTest(IMySmallMissileLauncher block)
        {
            //Rocket Launcher
            //Interface name: IMySmallMissileLauncher
            //Parent: IMyFunctionalBlock
            //Fields:
            bool UseConveyorSystem = block.UseConveyorSystem;
            return true;
        }

        bool FuncTest(IMyMotorStator block)
        {
            //Rotor
            //Interface name: IMyMotorStator
            //Parent: IMyMotorBase
            //Parent: IMyFunctionalBlock
            //Fields:
            bool IsAttached = block.IsAttached;
            float Torque = block.Torque;
            float BrakingTorque = block.BrakingTorque;
            float Velocity = block.Velocity;
            float LowerLimit = block.LowerLimit;
            float UpperLimit = block.UpperLimit;
            float Displacement = block.Displacement;
            return true;
        }

        bool FuncTest(IMySensorBlock block)
        {
            //Sensor
            //Interface name: IMySensorBlock
            //Parent: IMyFunctionalBlock
            //Fields:
            float LeftExtend = block.LeftExtend;
            float RightExtend = block.RightExtend;
            float TopExtend = block.TopExtend;
            float BottomExtend = block.BottomExtend;
            float FrontExtend = block.FrontExtend;
            float BackExtend = block.BackExtend;

            bool DetectPlayers = block.DetectPlayers;
            bool DetectFloatingObjects = block.DetectFloatingObjects;
            bool DetectSmallShips = block.DetectSmallShips;
            bool DetectLargeShips = block.DetectLargeShips;
            bool DetectStations = block.DetectStations;
            bool DetectAsteroids = block.DetectAsteroids;
            bool DetectOwner = block.DetectOwner;
            bool DetectFriendly = block.DetectFriendly;
            bool DetectNeutral = block.DetectNeutral;
            bool DetectEnemy = block.DetectEnemy;

            //IMyEntity LastDetectedEntity = block.LastDetectedEntity;
            return true;
        }

        bool FuncTest(IMySolarPanel block)
        {
            //Solar Panel
            //Interface name: IMySolarPanel
            return true;
        }

        bool FuncTest(IMySoundBlock block)
        {
            //Sound Block
            //Interface name: IMySoundBlock
            //Parent: IMyFunctionalBlock
            //Fields:
            float Volume = block.Volume;
            float Range = block.Range;
            bool IsSoundSelected = block.IsSoundSelected;
            float LoopPeriod = block.LoopPeriod;
            return true;
        }

        bool FuncTest(IMyGravityGeneratorSphere block)
        {
            //Spherical Gravity Generator
            //Interface name: IMyGravityGeneratorSphere
            //Parent: IMyGravityGeneratorBase
            //Parent: IMyFunctionalBlock
            //Fields:
            float Radius = block.Radius;
            float Gravity = block.Gravity;
            return true;
        }

        bool FuncTest(IMyReflectorLight block)
        {
            //Spotlight
            //Interface name: IMyReflectorLight
            //Parent: IMyLightingBlock
            //Parent: IMyFunctionalBlock
            //Fields:
            float Radius = block.Radius;
            float Intensity = block.Intensity;
            float BlinkIntervalSeconds = block.BlinkIntervalSeconds;
            float BlinkLenght = block.BlinkLenght;
            float BlinkOffset = block.BlinkOffset;
            return true;
        }

        bool FuncTest(IMyTimerBlock block)
        {
            //Timer Block
            //Interface name: IMyTimerBlock
            //Parent: IMyFunctionalBlock
            //Fields:
            bool IsCountingDown = block
                .IsCountingDown;
            float TriggerDelay = block.TriggerDelay;
            return true;
        }

        bool FuncTest(IMyWarhead block)
        {
            //Warhead
            //Interface name: IMyWarhead
            //Fields:
            bool IsCountingDown = block.IsCountingDown;
            float DetonationTime = block.DetonationTime;
            return true;
        }

        bool FuncTest(IMyShipWelder block)
        {
            //Welder
            //Interface name: IMyShipWelder
            //Parent: IMyShipToolBase
            //Parent: IMyFunctionalBlock
            return true;
        }

        bool FuncTest(IMyMotorSuspension block)
        {
            //Wheel Suspension 1x1

            //Wheel Suspension 3x3
            //Wheel Suspension 5x5
            //Interface name: IMyMotorSuspension
            //Parent: IMyMotorBase
            //Parent: IMyFunctionalBlock
            //Fields:
            bool Steering = block.Steering;
            bool Propulsion = block.Propulsion;
            float Damping = block.Damping;
            float Strength = block.Strength;
            float Friction = block.Friction;
            float Power = block.Power;
            return true;
        }

        void Main()
        {
            List<IMyTerminalBlock> blocks = GridTerminalSystem.Blocks;
            List<IMyBlockGroup> blockGroups = GridTerminalSystem.BlockGroups;
            GridTerminalSystem.GetBlocksOfType<IMyCubeBlock>(blocks, FuncTest);
            GridTerminalSystem.SearchBlocksOfName(blockName, blocks, FuncTest);
            var block = GridTerminalSystem.GetBlockWithName(blockName);

            IMyCubeBlock cubeBlock = block;
            bool IsBeingHacked = cubeBlock.IsBeingHacked;
            bool IsFunctional = cubeBlock.IsFunctional;
            bool IsWorking = cubeBlock.IsWorking;
            VRageMath.Vector3I Position = cubeBlock.Position;

            IMyTerminalBlock terminalBlock = block;
            string CustomName = terminalBlock.CustomName;
            string CustomNameWithFaction = terminalBlock.CustomNameWithFaction;
            string DetailedInfo = terminalBlock.DetailedInfo;
            bool HasLocalPlayerAccess = terminalBlock.HasLocalPlayerAccess();
            bool HasPlayerAccess = terminalBlock.HasPlayerAccess(playerId);
            //terminalBlock.RequestShowOnHUD(enable);
            terminalBlock.SetCustomName(CustomName);
            //terminalBlock.SetCustomName(stringBuilder);
            bool ShowOnHUD = terminalBlock.ShowOnHUD;

            List<ITerminalAction> resultList = new List<ITerminalAction>();
            terminalBlock.GetActions(resultList, FuncTest);
            //terminalBlock.SearchActionsOfName(actionName, resultList, FuncTest);
            //ITerminalAction terminalAction = terminalBlock.GetActionWithName(actionName);

            //string Id = terminalAction.Id;
            //StringBuilder Name = terminalAction.Name;
            //terminalAction.Apply(cubeBlock);

            IMyFunctionalBlock functionalBlock = block as IMyFunctionalBlock;
            bool Enabled = functionalBlock.Enabled;

        }
    }
}
