using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTraining
{
    public enum CombatUnitState
    {
        Idle,
        AttackClosest,
        AttackFastest,
        AttackMostValueable,
        AttackWeakest,
        MoveTowards,
        MoveBack,
        Seek,
        Retreat
    };
}
