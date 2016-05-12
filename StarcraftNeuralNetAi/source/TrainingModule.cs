using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;
using NeuralNetTraining.Utility;

namespace NeuralNetTraining
{
    /// <summary>
    /// The TrainingModule is the entry point of the injected AI. AiBase implements several event listeners of the BWAPI.
    /// The TrainingModule supervises the training environment on a match to match basis.
    /// </summary>
    public class TrainingModule : AiBase
    {
        #region Member
        private static int matchNumber = 0;
        private SquadSupervisor squadSupervisor;
        private NeuralNetController neuralNetController;
        private bool isEnemySquadInitialized;
        private bool receivedHandshake = false;
        private bool sentHandshake = false;
        private string handshakeMessage = "Commencing Match Procedure! Training Mode: ";
        public static bool trainingMode = true;
        #endregion

        #region BWAPI Events
        /// <summary>
        /// Everything that needs to be done before the game starts, should be done inside OnStart(). Especially all members should be initialized in here due to automated matches.
        /// </summary>
        public override void OnStart()
        {
            matchNumber++;
            // Config match
            Game.EnableFlag(Flag.CompleteMapInformation); // this flag makes the information about the enemy units avaible
            Game.EnableFlag(Flag.UserInput); // this flag allows the user to take action

            if (trainingMode)
            {
                Game.SetLocalSpeed(0); // fastest game speed, maybe adding frame skipping increases game speed
            }

            // Initialize Member
            neuralNetController = NeuralNetController.GetInstance();
            isEnemySquadInitialized = false;
            squadSupervisor = new SquadSupervisor();
            // A handshake is used to check if both instances of StarCraft are ready. Due to the network delay (even on the local machine), the information about the enemy units isn't available right away
            receivedHandshake = false;
            sentHandshake = false;

            InitializeSquad();
        }

        /// <summary>
        /// OnFrame() is called every single frame. Avoid expensive looping code.
        /// </summary>
        public override void OnFrame()
        {
            if (!isEnemySquadInitialized && Game.AllUnits.Count > Game.Self.Units.Count && Game.FrameCount > 5)
            {
                InitializeEnemySquad(); // the enemy squad has to be initialized on one of the first frames, due to the asynchronus connection to the match, otherwise it would occur that there are no enemy units at all.
                isEnemySquadInitialized = true;
            }

            if(Game.FrameCount == 10)
            {
                Game.SendText(handshakeMessage + trainingMode.ToString()); // trigger handshake
                sentHandshake = true;
            }

            // if the handshake test is passed, execute the essential logics for each frame
            if (receivedHandshake && sentHandshake)
            {
                DrawOnScreen(); // draw several information on the screen
                squadSupervisor.OnFrame(); // the supervisor will trigger OnFrame on the AiCombatUnits as well.
            }
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
        /// <param name="isWinner">States if this player has won.</param>
        public override void OnEnd(bool isWinner)
        {
            if (trainingMode)
            {
                neuralNetController.ExecuteTraining();
            }
        }

        /// <summary>
        /// If a chat message has been received, this event is being triggered. I make use of a handshake to start the training procedure.
        /// </summary>
        /// <param name="player">Player who sent the message</param>
        /// <param name="text">Chat message content.</param>
        public override void OnReceiveText(Player player, string text)
        {
            if (text == handshakeMessage + trainingMode.ToString() && player.Id != Game.Self.Id)
            {
                receivedHandshake = true;
            }
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
                squadSupervisor.AddFriendlyCombatUnit(new CombatUnitTrainingBehavior(unit, squadSupervisor));
            }
        }

        /// <summary>
        /// Initializes the list of enemy combat units. This should be called as soon as the Game object is aware of the complete map information.
        /// </summary>
        private void InitializeEnemySquad()
        {
            foreach (Unit unit in Game.Enemy.Units)
            {
                squadSupervisor.AddEnemyCombatUnit(new EnemyFeedbackBehavior(unit, squadSupervisor));
            }
        }

        /// <summary>
        /// Draws some match related statistics on the screen.
        /// </summary>
        private void DrawOnScreen()
        {
            Game.DrawTextScreen(5, 0, "Player : {0}", Game.Self.Id);
            Game.DrawTextScreen(60, 0, "FPS : {0}", Game.Fps);
            Game.DrawTextScreen(5, 10, "Me/Enemy {0}/{1}", squadSupervisor.GetFriendlyCount(), squadSupervisor.GetEnemyCount());
            Game.DrawTextScreen(5, 20, "Match #{0}", matchNumber);
            if(trainingMode)
            {
                Game.DrawTextScreen(5, 30, "Training Mode");
            }
            else
            {
                Game.DrawTextScreen(5, 30, "Execution Mode");
            }
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// The TrainingModule keeps track of the match count.
        /// </summary>
        /// <returns>Returns the current number of the match.</returns>
        public static int GetMatchNumber()
        {
            return matchNumber;
        }
        #endregion
    }
}