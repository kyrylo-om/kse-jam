using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimonRuleManager : MonoBehaviour
{
    public static SimonRuleManager Instance { get; private set; }

    [Header("Ball Prefab")]
    [SerializeField] private GameObject ballPrefab;

    private readonly List<ISimonEffect> activeRules = new List<ISimonEffect>();
    private readonly List<RuleType> activeRuleTypes = new List<RuleType>();
    private InputAction resetAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

    private void OnDestroy()
    {
        // Always reset all rules when the scene/play mode ends
        ResetAllRules();
    }

    private void Update()
    {
        
    }

    public void ApplyRule(RuleDefinition ruleDef)
    {
        // Don't apply if this rule type is already active
        if (activeRuleTypes.Contains(ruleDef.type))
        {
            Debug.Log($"[SimonRuleManager] Rule '{ruleDef.type}' already active — skipping duplicate");
            return;
        }

        ISimonEffect effect = CreateEffect(ruleDef);
        if (effect == null) return;

        effect.Apply();
        activeRules.Add(effect);
        activeRuleTypes.Add(ruleDef.type);
        Debug.Log($"[SimonRuleManager] Applied rule: {ruleDef.type} (value: {ruleDef.floatValue})");
    }

    public bool IsRuleActive(RuleType type)
    {
        return activeRuleTypes.Contains(type);
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
        activeRuleTypes.Clear();
    }

    private ISimonEffect CreateEffect(RuleDefinition ruleDef)
    {
        return ruleDef.type switch
        {
            RuleType.JumpBoost => new JumpBoostEffect(ruleDef.floatValue),
            RuleType.SpeedBoost => new SpeedBoostEffect(ruleDef.floatValue),
            RuleType.GravityChange => new GravityChangeEffect(ruleDef.floatValue),
            RuleType.FPSLimit => new FPSLimitEffect(ruleDef.floatValue),
            RuleType.NoFriction => new NoFrictionEffect(ruleDef.floatValue),
            RuleType.Pixelation => new PixelationEffect(ruleDef.floatValue),
            RuleType.InvertedControls => new InvertedControlsEffect(ruleDef.floatValue),
            RuleType.Wider => new WiderEffect(ruleDef.floatValue),
            RuleType.Longer => new LongerEffect(ruleDef.floatValue),
            RuleType.Psychedelic => new PsychedelicEffect(ruleDef.floatValue),
            _ => null
        };
    }

    public void ManyBalls()
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning("[SimonRuleManager] No ballPrefab assigned!");
            return;
        }

        GameObject player = GameObject.Find("PlayerBean");
        if (player == null) return;

        float playerZ = player.transform.position.z;

        for (int i = 0; i < 100; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(0f, 8f),
                8f,
                playerZ + Random.Range(0f, 15f)
            );

            GameObject ball = Instantiate(ballPrefab, pos, Random.rotation);
            ball.name = $"Ball_{i}";

            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            }
        }

        Debug.Log("[SimonRuleManager] Spawned 100 balls!");
    }
}