using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float startTime = 60f;

    private TextMeshProUGUI textComponent;
    private float timeRemaining;
    private bool isRunning;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        timeRemaining = startTime;
    }

    private void Start()
    {
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;

            // Trigger death on the player
            GameObject player = GameObject.Find("PlayerBean");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Die();
                }
            }

            UpdateDisplay();
            enabled = false;
            return;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (textComponent == null) return;

        int seconds = Mathf.FloorToInt(timeRemaining);
        int milliseconds = Mathf.FloorToInt((timeRemaining - seconds) * 1000);

        textComponent.text = $"{seconds:D2}:{milliseconds:D3}";
    }
}