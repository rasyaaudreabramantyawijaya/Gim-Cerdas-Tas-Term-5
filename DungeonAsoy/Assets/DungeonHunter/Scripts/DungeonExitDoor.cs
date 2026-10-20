using UnityEngine;

public sealed class DungeonExitDoor : DungeonInteractable
{
    [SerializeField] private Transform gateVisual;
    [SerializeField] private Collider gateCollider;
    [SerializeField] private DungeonSession session;
    private bool opened;
    public bool IsOpen => opened;

    public void Configure(DungeonSession gameSession, Transform visual, Collider solid)
    {
        session = gameSession;
        gateVisual = visual;
        gateCollider = solid;
    }

    public override string GetPrompt(DungeonSession gameSession) => opened ? "Gerbang terbuka" : "E  Periksa gerbang tersegel";

    public override void Interact(DungeonSession gameSession)
    {
        if (opened) return;
        if (!gameSession.HasArtifact)
        {
            gameSession.SetFeedback("Cari artifact di sayap barat mansion terlebih dahulu.");
            return;
        }
        opened = true;
        if (gateCollider != null) gateCollider.enabled = false;
        if (gateVisual != null) gateVisual.gameObject.SetActive(false);
        gameSession.SetFeedback("Gerbang terbuka. Teruskan ke utara!");
    }

}
