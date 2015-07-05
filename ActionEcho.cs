
using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.Input;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.Game.Weapons;
using Sandbox.Definitions;
using Sandbox.Engine;
using VRage.Common;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace IngameProg
{
    public class ActionEcho : IngameScript
    {
        private const char Seperator = ';';
        private int CountArguments = 3;

        void Main(string argument)
        {
            StringBuilder debug = new StringBuilder();
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<
////////////// EDIT TYPE HERE //////////////
            IMyAirVent
//////////////      END EDIT       //////////////
                >(blocks);

            if (blocks.Count == 0)
            {
                Echo("No blocks found.");
                return;
            }

            debug.Append("Porperties:").AppendLine();
            debug.Append("-->Structure:").AppendLine();
            debug.Append("Name (Default Value) [Min Value, Max Value] : Type Name").AppendLine();
            var block = blocks[0];
            List<ITerminalProperty> properties = new List<ITerminalProperty>();
            block.GetProperties(properties);
            for (int i = 0; i < properties.Count; ++i)
            {

                ITerminalProperty property = properties[i];
                debug.Append('-');
                debug.Append(property.Id);
                switch (property.TypeName)
                {
                    case "Boolean":
                        AppendProperty<Boolean>( debug,  block,  property);
                        break;
                    default:
                        AppendProperty<Single>( debug,  block,  property);
                        break;
                }
                
                debug.Append(property.TypeName);
                if (i < properties.Count - 1)
                    debug.Append(", ");
                //if (i % 10 == 9)
                debug.AppendLine();
            }
            debug.AppendLine();

            List<ITerminalAction> actions = new List<ITerminalAction>();
            block.GetActions(actions);
            debug.Append("Actions:").AppendLine();
            List<string> actionNames = new List<string>();
            for (int i = 0; i < actions.Count; ++i)
            {
                ITerminalAction action = actions[i];
                debug.Append('-');
                debug.Append(action.Id);
                actionNames.Add(action.Id);

                if (i < actions.Count - 1)
                    debug.Append(", ");
                //if (i % 10 == 9)
                debug.AppendLine();
            }



            Echo(debug.ToString());
            debug.Clear();
        }

        void AppendProperty<T>(StringBuilder debug, IMyTerminalBlock block, ITerminalProperty property)
        {
            debug.Append(" (").Append(block.GetDefaultValue<T>(property.Id)).Append(") [");
            debug.Append(block.GetMininum<T>(property.Id)).Append(',').Append(block.GetMaximum<T>(property.Id)).Append("] : ");
        }
    }
}

