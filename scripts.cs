using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // New block list
            List<IMyThrust> thrusters = new List<IMyThrust>();

            // Get list of thrusters
            GridTerminalSystem.GetBlocksOfType(thrusters);

            // Get cockpit/ship controller
            IMyShipController s_controller = (IMyShipController)GridTerminalSystem.GetBlockWithName("Right CP");

            // Get inter light for status change
            IMyInteriorLight golight = (IMyInteriorLight)GridTerminalSystem.GetBlockWithName("GoLight");
            golight.Color = Color.Blue; // Default blue when on atmos

            // Get gravity well strength
            var g_strength = ((s_controller.GetNaturalGravity().Length()) / 9.81);

            // Tested 3 atmos thruster can reach no higher than 850 meters at 1.15 g from 1.20g

            // Switch to thruster on condition change
            //6500 meters @ 0.35g Ovveride Thruster Dampener

            if (g_strength > 1.15d)
            {           
                foreach (var t in thrusters)
                {
                    if (t.DefinitionDisplayNameText.Contains("Hydrogen"))
                    {
                        // Turn off Hydrogen Thrusters
                        t.ApplyAction("OnOff_Off");
                    }
                    else
                        // Turn on atmospheric Thrusters
                        t.ApplyAction("OnOff_On");                          
                }

                // Blue Status for Atmos
                golight.Color = Color.Blue;

                // Turn off Dampeners when Atmos Enabled
                s_controller.DampenersOverride = true;
            }
            else if (g_strength < 1.15 && g_strength != 0.0)
            {
                foreach (var t in thrusters)
                {
                    if (t.DefinitionDisplayNameText.Contains("Atmospheric"))
                    {
                        // Turn off atmospheric Thrusters
                        t.ApplyAction("OnOff_Off");
                    }
                    else
                        // Turn on Hydrogen Thrusters
                        t.ApplyAction("OnOff_On");

                    if (t.CustomName.Contains("#"))
                    {
                        // De-activate Dampeners
                        t.ThrustOverridePercentage = AdjustThrusterOverride(g_strength, t);
                    }
                }

                // Override not working

                // Hydrogen Green Indicator
                golight.Color = Color.Green;

                // Turn off Dampeners when Hydrogen Enabled
                s_controller.DampenersOverride = false;
            }

            Clear(g_strength, s_controller, golight);
        }

        /// <summary>
        /// When leaving gravity well, reset config, turn dampeners back on to slow down.
        /// Update status light.
        /// </summary>
        /// <param name="strength"></param>
        /// <param name="controller"></param>
        /// <param name="status"></param>
        public void Clear(double strength, IMyShipController controller, IMyInteriorLight status)
        {
            if (strength == 0)
            {
                controller.DampenersOverride = true;
                status.Color = Color.Red;
            }
        }

        /// <summary>
        /// Adjust thruster override by percentage based on strength of gravity well.
        /// </summary>
        /// <param name="gravity"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public float AdjustThrusterOverride(double gravity, IMyThrust t)
        {
            // If gravity strength, decrease thruster override strength
            if (gravity <= 0.35 && gravity > 0.30)
            {
                 return 1f;
            }

            if (gravity <= 0.30 && gravity > 0.25)
            {
                return 0.8f;
            }

            if (gravity <= 0.25 && gravity > 0.20)
            {
                return 0.6f;
            }

            if (gravity <= 0.20 && gravity > 0.15)
            {
                return 0.4f;
            }

            if (gravity <= 0.15 && gravity > 0)
            {
                 return 0.2f;
            }
            else
            {
                // Disable thruster override.
                return  0f;
            }            
        }
    }
}
