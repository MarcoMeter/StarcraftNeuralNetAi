﻿using BroodWar.Api;
using NeuralNetTraining.Utility;
using System.Threading;

namespace NeuralNetTraining
{
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
        public int FrameDuration
        {
            get
            {
                return this.m_frameDuration;
            }
        }
        #endregion

        #region Constructor
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