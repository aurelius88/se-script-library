using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SE_Script_Library.Reference;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;

namespace SE_Script_Library.TestScripts
{
    class CrashTest : IngameScript
    {

        VRageMath.Vector3 _pos;
        void Main()
        {
            _pos = GridTerminalSystem.Blocks[0].GetPosition();
            float f = Crash; // crash
        }

        public float Crash
        {
            get
            {
                float f = (_pos - _pos).LengthSquared();
                return Crash;
            }
        }
    }

}
