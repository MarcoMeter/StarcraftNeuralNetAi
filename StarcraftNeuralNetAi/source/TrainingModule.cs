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
        #region Member Fields
        // Match info
        private static int m_matchNumber = 0;
        private static bool m_trainingMode = false;

        // To be initialized
        private SquadSupervisor m_squadSupervisor;
        private NeuralNetController m_neuralNetController;
        private bool m_isEnemySquadInitialized;

        // Handshake before training start
        private bool m_receivedHandshake = false;
        private bool m_sentHandshake = false;
        private const int m_handShakeFrameTime = 10;
        private string m_handshakeMessage = "Commencing Match Procedure! Training Mode: ";
        #endregion

        #region Member Properties
        /// <summary>
        /// Read-only. The TrainingModule keeps track of the match count.
        /// </summary>
        public static int MatchNumber
        {
            get
            {
                return m_matchNumber;
            }
        }

        /// <summary>
        /// Read-only. Stated by the TrainingModule, if the match is in training mode or execution mode.
        /// </summary>
        public static bool TrainingMode
        {
            get
            {
                return m_trainingMode;
            }
        }
        #endregion

        #region BWAPI Events
        /// <summary>
        /// Everything that needs to be done before the game starts, should be done inside OnStart(). Especially all members should be initialized in here due to automated matches.
        /// </summary>
        public override void OnStart()
        {
            m_matchNumber++;
            // Config match
            Game.EnableFlag(Flag.CompleteMapInformation); // this flag makes the information about the enemy units avaible
            Game.EnableFlag(Flag.UserInput); // this flag allows the user to take action

            if (m_trainingMode)
            {
                Game.SetLocalSpeed(0); // fastest game speed, maybe adding frame skipping increases game speed
            }

            // Initialize Member
            m_neuralNetController = NeuralNetController.Instance;
            m_isEnemySquadInitialized = false;
            m_squadSupervisor = new SquadSupervisor();
            // A handshake is used to check if both instances of StarCraft are ready. Due to the network delay (even on the local machine), the information about the enemy units isn't available right away
            m_receivedHandshake = false;
            m_sentHandshake = false;

            InitializeSquad();
        }

        /// <summary>
        /// OnFrame() is called every single frame. Avoid expensive looping code.
        /// </summary>
        public override void OnFrame()
        {
            if (!m_isEnemySquadInitialized && Game.AllUnits.Count > Game.Self.Units.Count && Game.FrameCount > 5)
            {
                InitializeEnemySquad(); // the enemy squad has to be initialized on one of the first frames, due to the asynchronus connection to the match, otherwise it would occur that there are no enemy units at all.
                m_isEnemySquadInitialized = true;
            }

            // initialize a handshake test to check if both game instances are ready
            if(Game.FrameCount == m_handShakeFrameTime)
            {
                Game.SendText(m_handshakeMessage + m_trainingMode.ToString()); // trigger handshake
                m_sentHandshake = true;
            }

            // if the handshake test is passed, execute the essential logics for each frame
            if (m_receivedHandshake && m_sentHandshake)
            {
                DrawOnScreen(); // draw several information on the screen
                m_squadSupervisor.OnFrame(); // the supervisor will trigger OnFrame on the AiCombatUnits as well.
            }
        }

        /// <summary>
        /// OnUnitDestroy is triggered if any unit dies or gets destroyed. Keep in mind, destroyed units can still be referenced.
        /// </summary>
        /// <param name="unit">Is triggered on any kind of unit which is being destroyed.</param>
        public override void OnUnitDestroy(Unit unit)
        {
            m_squadSupervisor.OnUnitDestroy(unit); // pass event to the SquadSupervisor
        }

        /// <summary>
        /// OnEnd is called as soon as the match concludes. This event is supposed to be used to finalize the generated training data.
        /// </summary>
        /// <param name="isWinner">States if this player has won.</param>
        public override void OnEnd(bool isWinner)
        {
            if (m_trainingMode)
            {
                m_neuralNetController.ExecuteTraining();
            }
        }

        /// <summary>
        /// If a chat message has been received, this event is being triggered. I make use of a handshake to start the training procedure.
        /// </summary>
        /// <param name="player">Player who sent the message</param>
        /// <param name="text">Chat message content.</param>
        public override void OnReceiveText(Player player, string text)
        {
            if (text == m_handshakeMessage + m_trainingMode.ToString() && player.Id != Game.Self.Id)
            {
                m_receivedHandshake = true;
            }
        }
        #endregion

        #region Local Functions
        /// <summary>
        /// Initializes the units commanded by the SquadSupervisor.
        /// </summary>
        private void InitializeSquad()
        {
            int idToAssign = 0;
            foreach(Unit unit in Game.Self.Units)
            {
                m_squadSupervisor.AddFriendlyCombatUnit(new CombatUnitTrainingBehavior(unit, m_squadSupervisor, idToAssign));
                idToAssign++;
            }
        }

        /// <summary>
        /// Initializes the list of enemy combat units. This should be called as soon as the Game object is aware of the complete map information.
        /// </summary>
        private void InitializeEnemySquad()
        {
            foreach (Unit unit in Game.Enemy.Units)
            {
                m_squadSupervisor.AddEnemyCombatUnit(new EnemyBehavior(unit, m_squadSupervisor));
            }
        }

        /// <summary>
        /// Draws some match related statistics on the screen.
        /// </summary>
        private void DrawOnScreen()
        {
            Game.DrawTextScreen(5, 0, "Player : {0}", Game.Self.Id);
            Game.DrawTextScreen(60, 0, "FPS : {0}", Game.Fps);
            Game.DrawTextScreen(5, 10, "Me/Enemy {0}/{1}", m_squadSupervisor.FriendlyCount, m_squadSupervisor.EnemyCount);
            Game.DrawTextScreen(5, 20, "Match #{0}", m_matchNumber);
            if(m_trainingMode)
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
        #endregion
    }
}