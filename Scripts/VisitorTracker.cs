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
    class VisitorTracker : IngameScript
    {
        struct VisitorData
        {

            public VisitorData(DateTime time, Sandbox.ModAPI.IMyEntity entity)
            {
                this.time = time;
                this.entity = entity;
            }

            public DateTime time;
            public Sandbox.ModAPI.IMyEntity entity;
        }

        List<VisitorData> visitors = new List<VisitorData>();

        void Main()
        {
            IMySensorBlock sensor = null;
            visitors.Add(new VisitorData(DateTime.Now, sensor.LastDetectedEntity));
        }

    }
}
