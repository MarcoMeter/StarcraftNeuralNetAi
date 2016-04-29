﻿using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using NeuralNetTraining.Utility;

namespace NeuralNetTraining
{
    /// <summary>
    /// Entry point of the injected AI. AiBase implements several event listeners.
    /// The TrainingModule supervises the training environment.
    /// </summary>
    public class TrainingModule : AiBase
    {
        #region Member
        private static int matchNumber = 0;
        private SquadSupervisor squadSupervisor;
        private NeuralNetController neuralNetController;
        private bool isEnemySquadInitialized;
        #endregion

        #region Events
        /// <summary>
        /// Everything that needs to be done before the game starts, should be done inside OnStart(). Especially all members should be initialized in here due to automated matches.
        /// </summary>
        public override void OnStart()
        {
            matchNumber++;
            // Config match
            Game.EnableFlag(Flag.CompleteMapInformation); // this flag makes the information about the enemy units avaible
            Game.EnableFlag(Flag.UserInput); // this flag allows the user to take action
            Game.SetLocalSpeed(0); // fastest game speed, maybe adding frame skipping increases game speed

            // Initialize Member
            neuralNetController = NeuralNetController.GetInstance();
            isEnemySquadInitialized = false;
            squadSupervisor = new SquadSupervisor();

            InitializeSquad();
        }

        /// <summary>
        /// OnFrame() is called every single frame. Avoid expensive looping code.
        /// </summary>
        public override void OnFrame()
        {
            if (!isEnemySquadInitialized && Game.AllUnits.Count > Game.Self.Units.Count)
            {
                InitializeEnemySquad(); // the enemy squad has to be initialized on the first frame, due to the asynchronus connection to the match, otherwise it would occur that there are no enemy units at all.
                isEnemySquadInitialized = true;
                
                // Test call to engage a fight
                //squadSupervisor.ForceAttack(Utility.ConvertTilePosition(new TilePosition(29, 27))); // send units to attack some spot between the two armies -> Test
            }

            DrawOnScreen();

            squadSupervisor.OnFrame(); // the supervisor will trigger OnFrame on the AiCombatUnits as well.
        }

        /// <summary>
        /// OnUnitDestroy is triggered if any unit dies or gets destroyed. Keep in mind, destroyed units can still be referenced.
        /// </summary>
        /// <param name="unit">Is triggered on any kind of unit which is being destroyed.</param>
        public override void OnUnitDestroy(Unit unit)
        {
            squadSupervisor.OnUnitDestroy(unit); // pass event to the SquadSupervisor
        }

        /// <summary>
        /// OnEnd is called as soon as the match concludes. This event is supposed to be used to finalize the generated training data.
        /// </summary>
        /// <param name="isWinner"></param>
        public override void OnEnd(bool isWinner)
        {
            neuralNetController.ExecuteTraining();
        }
        #endregion

        #region Local Functions
        /// <summary>
        /// Initializes the units commanded by the SquadSupervisor.
        /// </summary>
        private void InitializeSquad()
        {
            foreach(Unit unit in Game.Self.Units)
            {
                squadSupervisor.AddCombatUnit(new CombatUnitTrainingBehavior(unit, squadSupervisor));
            }
        }

        /// <summary>
        /// Initializes the list of enemy combat units. This should be called as soon as the Game object is aware of the complete map information.
        /// </summary>
        private void InitializeEnemySquad()
        {
            foreach (Unit unit in Game.Enemy.Units)
            {
                squadSupervisor.AddEnemyCombatUnit(unit);
            }
        }

        /// <summary>
        /// Draws unit counts and FPS on the screen.
        /// </summary>
        private void DrawOnScreen()
        {
            Game.DrawTextScreen(5, 0, "Player : {0}", Game.Self.Id);
            Game.DrawTextScreen(60, 0, "FPS : {0}", Game.Fps);
            Game.DrawTextScreen(5, 10, "Me/Enemy {0}/{1}", Game.Self.Units.Count, Game.Enemy.Units.Count);
        }
        #endregion

        #region Public Functions
        public static int GetMatchNumber()
        {
            return matchNumber;
        }
        #endregion
    }
}