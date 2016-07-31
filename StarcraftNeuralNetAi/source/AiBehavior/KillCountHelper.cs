using BroodWar.Api;

namespace NeuralNetTraining
{
    /// <summary>
    /// The KillCountHelper is supposed to support figuring out if the combat unit killed its target or just harmed it.
    /// With some delay, the KillCountHelper triggers the process of computing the training data pair.
    /// </summary>
    public class KillCountHelper
    {
        #region Member Fields
        private Unit m_unit;
        private AttackFitness m_fitnessMeasure;
        private InputVector m_inputVector;
        private int m_frameDuration;
        private int m_tempKillCount;
        #endregion

        #region Member Properties
        /// <summary>
        /// Read-only rest of the observed frame duration.
        /// </summary>
        public int FrameDuration
        {
            get
            {
                return this.m_frameDuration;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates a KillCountHelper to observe the unit's kill count for indicating the attack's feedback.
        /// </summary>
        /// <param name="unit">Requesting unit</param>
        /// <param name="fitnessMeasure">The complete FitnessMeasure featuring initial input information</param>
        /// <param name="inputVector">The input vector featuring the final input information</param>
        /// <param name="frameDuration">The duration to observe the unit's kill count</param>
        public KillCountHelper(Unit unit, AttackFitness fitnessMeasure, InputVector inputVector, int frameDuration)
        {
            this.m_unit = unit;
            this.m_fitnessMeasure = fitnessMeasure;
            this.m_inputVector = inputVector;
            this.m_frameDuration = frameDuration;
            this.m_tempKillCount = unit.KillCount;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Decrements the frame duration specified by the constructor.
        /// </summary>
        public void DecreaseDuration()
        {
            m_frameDuration--;
        }

        /// <summary>
        /// Checks if the kill count increased. It uses then provides this information to the fitness function.
        /// </summary>
        public void ProcessFitnessMeasure(int currentKillCount)
        {
            if (currentKillCount > m_tempKillCount)
            {
                m_fitnessMeasure.ComputeDataPair(m_inputVector, true, true);
            }
            else
            {
                m_fitnessMeasure.ComputeDataPair(m_inputVector, true, false);
            }
        }
        #endregion
    }
}