using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimonRuleManager : MonoBehaviour
{
    public static SimonRuleManager Instance { get; private set; }

    private readonly List<ISimonEffect> activeRules = new List<ISimonEffect>();
    private InputAction resetAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (InputSystem.actions != null)
        {
            resetAction = InputSystem.actions.FindAction("Player/Reset");
            if (resetAction != null)
            {
                resetAction.Enable();
            }
        }
    }

    private void Update()
    {
        if (resetAction != null && resetAction.WasPressedThisFrame())
        {
            ResetAllRules();
        }
    }

    public void ApplyRule(RuleDefinition ruleDef)
    {
        ISimonEffect effect = CreateEffect(ruleDef);
        if (effect == null) return;

        effect.Apply();
        activeRules.Add(effect);
        Debug.Log($"[SimonRuleManager] Applied rule: {ruleDef.type} (value: {ruleDef.floatValue})");
    }

    public void ResetAllRules()
    {
        if (activeRules.Count == 0) return;

        Debug.Log($"[SimonRuleManager] Resetting all {activeRules.Count} rules...");
        foreach (var effect in activeRules)
        {
            effect.Reset();
        }
        activeRules.Clear();
    }

    private ISimonEffect CreateEffect(RuleDefinition ruleDef)
    {
        return ruleDef.type switch
        {
            RuleType.JumpBoost => new JumpBoostEffect(ruleDef.floatValue),
            RuleType.SpeedBoost => new SpeedBoostEffect(ruleDef.floatValue),
            RuleType.GravityChange => new GravityChangeEffect(ruleDef.floatValue),
            _ => null
        };
    }
}