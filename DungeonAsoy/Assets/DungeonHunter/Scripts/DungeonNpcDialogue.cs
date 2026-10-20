using UnityEngine;

public sealed class DungeonNpcDialogue : DungeonInteractable
{
    [SerializeField] private string npcName = "Penjaga Gerbang";
    [TextArea, SerializeField] private string beforeArtifact = "Aktifkan menara checkpoint sebelum memasuki mansion. Artifact tersembunyi di sayap barat. Para pemanah masih menjaga tempat itu.";
    [TextArea, SerializeField] private string afterArtifact = "Artifact sudah ditemukan. Buka gerbang belakang mansion, lalu teruskan ke utara.";

    public override string GetPrompt(DungeonSession session) => $"E  Bicara dengan {npcName}";
    public override void Interact(DungeonSession session)
    {
        if (session != null) session.OpenDialogue(npcName, session.HasArtifact ? afterArtifact : beforeArtifact);
    }
}
