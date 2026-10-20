using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class DungeonSession : MonoBehaviour
{
    public bool HasArtifact { get; private set; }
    public bool IsComplete { get; private set; }
    public bool IsDialogueOpen { get; private set; }
    public string DialogueSpeaker { get; private set; }
    public string DialogueText { get; private set; }
    public string Feedback { get; private set; }
    public event Action Changed;
    private float previousTimeScale = 1f;
    private float feedbackUntil;

    public bool CollectArtifact()
    {
        if (HasArtifact || IsComplete) return false;
        HasArtifact = true;
        SetFeedback("Artifact diperoleh. Buka gerbang belakang mansion dengan E.");
        Changed?.Invoke();
        return true;
    }

    public bool CompleteRun()
    {
        if (!HasArtifact || IsComplete) return false;
        IsComplete = true;
        Changed?.Invoke();
        return true;
    }

    public void OpenDialogue(string speaker, string text)
    {
        if (IsComplete) return;
        DialogueSpeaker = speaker;
        DialogueText = text;
        if (!IsDialogueOpen)
        {
            previousTimeScale = Time.timeScale;
            IsDialogueOpen = true;
            Time.timeScale = 0f;
        }
        Changed?.Invoke();
    }

    public void SetDialogueOpen(bool open)
    {
        if (open == IsDialogueOpen) return;
        IsDialogueOpen = open;
        Time.timeScale = open ? 0f : Mathf.Max(0f, previousTimeScale);
        Changed?.Invoke();
    }

    public void SetFeedback(string message)
    {
        Feedback = message;
        feedbackUntil = Time.unscaledTime + 2.5f;
        Changed?.Invoke();
    }

    public string CurrentFeedback => Time.unscaledTime <= feedbackUntil ? Feedback : string.Empty;

    public void RestartRun()
    {
        SetDialogueOpen(false);
        Time.timeScale = 1f;
        var respawn = FindFirstObjectByType<DungeonPlayerRespawn>();
        if (!IsComplete && respawn != null)
        {
            respawn.Respawn();
            Changed?.Invoke();
            return;
        }
        var scene = SceneManager.GetActiveScene();
        if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
        else SceneManager.LoadScene(scene.name);
    }

    private void OnDisable()
    {
        if (!IsDialogueOpen) return;
        IsDialogueOpen = false;
        Time.timeScale = Mathf.Max(0f, previousTimeScale);
    }
}
