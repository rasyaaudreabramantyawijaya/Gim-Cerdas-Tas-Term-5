using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public sealed class DungeonHUD : MonoBehaviour
{
    [SerializeField] private Text healthText;
    [SerializeField] private Image healthFill;
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text promptText;
    [SerializeField] private Text feedbackText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text dialogueSpeakerText;
    [SerializeField] private Text dialogueBodyText;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Text endTitleText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private DungeonSession session;
    [SerializeField] private DungeonHealth playerHealth;
    [SerializeField] private DungeonPlayerInteraction interaction;

    private void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(CloseDialogue);
        if (restartButton != null) restartButton.onClick.AddListener(RestartRun);
        if (playerHealth != null) playerHealth.Changed += OnHealthChanged;
        if (session != null) session.Changed += Refresh;
        Refresh();
    }

    public void Configure(DungeonSession gameSession, DungeonHealth health, DungeonPlayerInteraction playerInteraction)
    {
        session = gameSession;
        playerHealth = health;
        interaction = playerInteraction;
        if (Application.isPlaying)
        {
            if (playerHealth != null) { playerHealth.Changed -= OnHealthChanged; playerHealth.Changed += OnHealthChanged; }
            if (session != null) { session.Changed -= Refresh; session.Changed += Refresh; }
        }
        Refresh();
    }

    private void Update()
    {
        if (session == null) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && session.IsDialogueOpen)
            CloseDialogue();
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame &&
            ((playerHealth != null && playerHealth.IsDead) || session.IsComplete)) session.RestartRun();
        if (objectiveText != null)
            objectiveText.text = session.IsComplete ? "ESTATE CLEARED" : session.HasArtifact ? "Buka gerbang belakang mansion" : "Cari artifact di sayap barat mansion";
        if (promptText != null)
            promptText.text = session.IsDialogueOpen ? "" : interaction != null ? interaction.CurrentPrompt : "";
        if (feedbackText != null) feedbackText.text = session.IsDialogueOpen ? "" : session.CurrentFeedback;
        bool finished = playerHealth != null && playerHealth.IsDead || session.IsComplete;
        if (endPanel != null) endPanel.SetActive(finished);
        if (finished && endTitleText != null) endTitleText.text = session.IsComplete ? "ESTATE CLEARED" : "KEMBALI KE CHECKPOINT";
        if (finished && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnHealthChanged(float current, float maximum)
    {
        if (healthText != null) healthText.text = $"VITALITY  {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(maximum)}";
        if (healthFill != null) healthFill.fillAmount = maximum <= 0f ? 0f : current / maximum;
    }

    public void Refresh()
    {
        if (session == null) return;
        if (dialoguePanel != null) dialoguePanel.SetActive(session.IsDialogueOpen);
        if (session.IsDialogueOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (dialogueSpeakerText != null) dialogueSpeakerText.text = session.DialogueSpeaker;
        if (dialogueBodyText != null) dialogueBodyText.text = session.DialogueText;
        if (objectiveText != null) objectiveText.text = session.HasArtifact ? "Objective: travel north to the exit" : "Objective: find the artifact west";
        OnHealthChanged(playerHealth != null ? playerHealth.Current : 0f, playerHealth != null ? playerHealth.Maximum : 100f);
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.Changed -= OnHealthChanged;
        if (session != null) session.Changed -= Refresh;
    }

    public void CloseDialogue()
    {
        if (session != null) session.SetDialogueOpen(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartRun()
    {
        if (session != null) session.RestartRun();
    }
}
