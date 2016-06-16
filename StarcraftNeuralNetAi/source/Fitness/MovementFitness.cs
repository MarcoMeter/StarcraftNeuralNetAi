using Encog.ML.Data;
using NeuralNetTraining.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetTraining
{
    /// <summary>
    /// The MovementFitness measures the fitness of movement actions.
    /// </summary>
    public class MovementFitness : FitnessMeasure
    {
        #region Constructor
        /// <summary>
        /// Initializes the MovementFitness with initial input data, the random action and the computed output.
        /// </summary>
        /// <param name="inputVector">The initial input information, which was used to feed the neural net.</param>
        /// <param name="randomAction">The chosen action by random.</param>
        /// <param name="computedOutput">The computed outputs by the neural net.</param>
        public MovementFitness(InputVector inputVector, CombatUnitState randomAction, IMLData computedOutput) : base(inputVector, randomAction, computedOutput)
        {
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Before an action is being executed, the FitnessMeasure is already initialized with some initial data.
        /// In order to complete the measure of how good that action was, the final state (after executing an action) has to be supplied.
        /// After that, the desired output gets computed and a data pair based on the initial input with the new desired output is getting sent to the neural net controller.
        /// </summary>
        /// <param name="finalInputInfo">Add the final InputInformation in order to compare the initial and final state of the combat which has been altered after executing an action.</param>
        /// <param name="hit">In the case of movement enter false</param>
        /// <param name="killed">In the case of movement enter false</param>
        public override void ComputeDataPair(InputVector finalInputVector, bool hit, bool killed)
        {
            PersistenceUtil.WriteLine("<<< START MOVEMENT FITNESS >>>");
            PersistenceUtil.WriteLine("Close range");
            PersistenceUtil.WriteLine("Friendly hit points : " + m_initialInputVector.CloseRangeFriendlyHitPoints + " Friendly Count : " + m_initialInputVector.CloseRangeFriendlyCount + " Enemy hit points : " + m_initialInputVector.CloseRangeEnemyHitPoints + " Enemy count : " + m_initialInputVector.CloseRangeEnemyCount);
            PersistenceUtil.WriteLine("Friendly hit points : " + finalInputVector.CloseRangeFriendlyHitPoints + " Friendly Count : " + finalInputVector.CloseRangeFriendlyCount + " Enemy hit points : " + finalInputVector.CloseRangeEnemyHitPoints + " Enemy count : " + finalInputVector.CloseRangeEnemyCount);
            PersistenceUtil.WriteLine("Far range");
            PersistenceUtil.WriteLine("Friendly hit points : " + m_initialInputVector.FarRangeFriendlyHitPoints + " Friendly Count : " + m_initialInputVector.FarRangeFriendlyCount + " Enemy hit points : " + m_initialInputVector.FarRangeEnemyHitPoints + " Enemy count : " + m_initialInputVector.FarRangeEnemyCount);
            PersistenceUtil.WriteLine("Friendly hit points : " + finalInputVector.FarRangeFriendlyHitPoints + " Friendly Count : " + finalInputVector.FarRangeFriendlyCount + " Enemy hit points : " + finalInputVector.FarRangeEnemyHitPoints + " Enemy count : " + finalInputVector.FarRangeEnemyCount);
            PersistenceUtil.WriteLine("distance before : " + m_initialInputVector.ClosestEnemyDistance);
            PersistenceUtil.WriteLine("distance after : " + finalInputVector.ClosestEnemyDistance);
            PersistenceUtil.WriteLine("hit points before : " + m_initialInputVector.UnitHitPoints);
            PersistenceUtil.WriteLine("hit points after : " + finalInputVector.UnitHitPoints);
            

            // Step #1: Compare situations
            double friendlyCloseRatio = ((m_initialInputVector.CloseRangeFriendlyHitPoints / finalInputVector.CloseRangeFriendlyHitPoints) + (m_initialInputVector.CloseRangeFriendlyCount / finalInputVector.CloseRangeFriendlyCount)) / 2;
            double enemyCloseRatio = ((m_initialInputVector.CloseRangeEnemyHitPoints / finalInputVector.CloseRangeEnemyHitPoints) + (m_initialInputVector.CloseRangeEnemyCount / finalInputVector.CloseRangeEnemyCount)) / 2;
            double friendlyFarRatio = ((m_initialInputVector.FarRangeFriendlyHitPoints / finalInputVector.FarRangeFriendlyHitPoints) + (m_initialInputVector.FarRangeFriendlyCount / finalInputVector.FarRangeFriendlyCount)) / 2;
            double enemyFarRatio = ((m_initialInputVector.FarRangeEnemyHitPoints / finalInputVector.FarRangeEnemyHitPoints) + (m_initialInputVector.FarRangeEnemyCount / finalInputVector.FarRangeEnemyCount)) / 2;
            double distanceRatio = m_initialInputVector.ClosestEnemyDistance / finalInputVector.UnitHitPoints;

            double closeSituation = friendlyCloseRatio - enemyCloseRatio;

            PersistenceUtil.WriteLine("frienflyCloseRatio: " + friendlyCloseRatio);
            PersistenceUtil.WriteLine("enemyCloseRatio: " + enemyCloseRatio);
            PersistenceUtil.WriteLine("friendlyFarRatio: " + friendlyFarRatio);
            PersistenceUtil.WriteLine("enemyFarRatio: " + enemyFarRatio);
            PersistenceUtil.WriteLine("distanceRatio: " + distanceRatio);
            PersistenceUtil.WriteLine("closeSituation: " + closeSituation);

            if(m_initialInputVector.CloseRangeFriendlyHitPoints < finalInputVector.CloseRangeFriendlyHitPoints)
            {
                // situation improved
            }
            else
            {

            }

            if (m_initialInputVector.CloseRangeFriendlyCount < finalInputVector.CloseRangeFriendlyCount)
            {
                // situation improved
            }
            else
            {

            }

            if (m_initialInputVector.CloseRangeEnemyHitPoints > finalInputVector.CloseRangeEnemyHitPoints)
            {
                // situation improved
            }
            else
            {

            }

            if(m_initialInputVector.CloseRangeEnemyCount > finalInputVector.CloseRangeFriendlyCount)
            {
                // situation improved
            }
            else
            {

            }
        }
        #endregion

        #region Local Functions
        #endregion
    }
}
