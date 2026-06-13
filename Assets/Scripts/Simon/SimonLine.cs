using System;
using UnityEngine;
using UnityEngine.Events;

public enum SimonLineType
{
    Action, // Player must execute an action within a time limit, or die
    Rule,   // An effect is applied; must be reverted via ResetAllRules()
    Event   // Fire-and-forget: just execute a function, no restoration needed
}

public enum RuleType
{
    JumpBoost,
    SpeedBoost,
    GravityChange
}

[Serializable]
public class RuleDefinition
{
    public RuleType type;
    public float floatValue = 1f;
}

[Serializable]
public class SimonLine
{
    [TextArea(2, 5)]
    public string text;
    public SimonLineType type = SimonLineType.Event;
    public float delayBeforeAction = 0.5f;

    [Tooltip("How long the player has to perform the action before dying.")]
    public float actionTimeLimit = 3f;
    [Tooltip("Name of the InputSystem action to listen for (e.g. 'Player/Jump').")]
    public string requiredInputAction = "Player/Jump";
    [TextArea(1, 3)]
    public string failureMessage = "Simon says... you failed!";

    public RuleDefinition rule = new RuleDefinition();

    public UnityEvent onTriggered = new UnityEvent();
}