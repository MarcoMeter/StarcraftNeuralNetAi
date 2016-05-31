using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AttackAnalysisModule
{
    /// <summary>
    /// Deriving from te base class AiBase, the AttackAnalysisModule is the starting point for this AI bot application.
    /// This module is explicitly meant to be for one friendly unit attacking the foe's buildings.
    /// </summary>
    public class AttackAnalysisModule : AiBase
    {
        #region Member Fields
        private Unit m_myUnit;
        // Draw properties
        private int m_circleRadius = 4;
        private string m_csvHeader = "FrameCount,isAttacking,isAttackFrame,Cooldown";
        #endregion

        #region BWAPI Events
        /// <summary>
        /// Initialization should be done in here before the match starts.
        /// </summary>
        public override void OnStart()
        {
            // Config match
            Game.EnableFlag(Flag.CompleteMapInformation); // this flag makes the information about the enemy units avaible
            Game.EnableFlag(Flag.UserInput); // this flag allows the user to take action

            // assign my unit (based on the map, there should be only one friendly unit and a couple of enemy buildings)
            m_myUnit = Game.Self.Units.First<Unit>();

            // a csv file will be used as format for logging
            // add the first line : column headers
            LogUtil.AppendString(m_csvHeader);
        }

        /// <summary>
        /// OnFrame() is called every single frame.
        /// </summary>
        public override void OnFrame()
        {
            DrawDebugInfo();
            // Based on each frame, log the attack state of the unit to the csv file
            LogUtil.AppendString(Game.FrameCount + "," + m_myUnit.IsAttacking.ToString() + "," + m_myUnit.IsAttackFrame.ToString() + "," + m_myUnit.GroundWeaponCooldown.ToString());
        }

        /// <summary>
        /// OnSendText is used to enable some "commands"
        /// "log" = all collected logs get saved to file
        /// </summary>
        /// <param name="text"></param>
        public override void OnSendText(string text)
        {
            if(text.Equals("log"))
            {
                Game.Write("Start writing logs to file!");
                LogUtil.FinalizeLog();
                Game.Write("Done writing logs to file");
            }
        }
        #endregion

        #region Local Functions
        /// <summary>
        /// Writes several unit related information to the screen.
        /// </summary>
        private void DrawDebugInfo()
        {
            DrawCooldown();
            DrawIsAttacking();
            DrawIsAttackFrame();
            Game.DrawTextScreen(10, 10, "Type 'log' in the chat in order to write all gathered logs to disk!", "");
        }

        /// <summary>
        /// Draws a circle for displaying isAttacking.
        /// </summary>
        private void DrawIsAttacking()
        {
            Color color;
            // Assign color to portray the unit's property isAttacking
            // red = false, yellow = true
            if (m_myUnit.IsAttacking)
            {
                color = Color.Yellow;
            }
            else
            {
                color = Color.Red;
            }

            Game.DrawCircleMap(m_myUnit.Position.X, m_myUnit.Position.Y - 20, m_circleRadius, color, true);
            Game.DrawTextMap(m_myUnit.Position.X + 10, m_myUnit.Position.Y - 26, "isAttacking", "");
        }

        /// <summary>
        /// Draws a circle for displaying isAttackFrame
        /// </summary>
        private void DrawIsAttackFrame()
        {
            Color color;
            // Assign color to portray the unit's property isAttackFrame
            if(m_myUnit.IsAttackFrame)
            {
                color = Color.Yellow;
            }
            else
            {
                color = Color.Red;
            }

            Game.DrawCircleMap(m_myUnit.Position.X, m_myUnit.Position.Y - 30, m_circleRadius, color, true);
            Game.DrawTextMap(m_myUnit.Position.X + 10, m_myUnit.Position.Y - 37, "isAttackFrame", "");
        }

        /// <summary>
        /// Draws text on screen to display the attack cooldown of the unit.
        /// </summary>
        private void DrawCooldown()
        {
            Game.DrawTextMap(m_myUnit.Position.X - 3, m_myUnit.Position.Y - 48, "{0} Attack Cooldown", m_myUnit.GroundWeaponCooldown);
        }
        #endregion
    }
}
