using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameBrewStudios
{
    public enum PathNodeAction
    {
        Nothing,
        Scenario,
        RollAgain,
        LoseTurn,
        Lose2Turns,
        BackToSingle,
        RollToProceed,
        AdvanceAndGetAvoidCard,
        BackToSingleOrDisregardBecauseList,
        RollAgainTwice,
        RollWith2Dice,
        Lose3Turns,
        GoToKNOWLOVE,
        AdvanceToNextPath
    }

    public enum ProceedAction
    {
        Nothing,
        BackToSingle,
        GoToRelationship,
        GoToMarriage,
        LoseATurn,
        LoseTwoTurns,
        LoseThreeTurns,
        GoToKNOWLOVE
    }

    public class PathNode : MonoBehaviour
    {
        public string nodeText;
        public PathNodeAction action = PathNodeAction.Nothing;
        
        /// <summary>
        /// Only gets used when action is "RollToProceed"
        /// </summary>
        public int rollCheck = 0;

        public ProceedAction rollPassed;
        public ProceedAction rollFailed;
    }
}