using UnityEngine;

public sealed class DungeonExitTrigger : MonoBehaviour
{
    [SerializeField] private DungeonSession session;
    [SerializeField] private DungeonExitDoor gate;

    public void Configure(DungeonSession gameSession, DungeonExitDoor exitDoor)
    {
        session = gameSession;
        gate = exitDoor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<DungeonPlayerController>() != null && session != null && gate != null && gate.IsOpen)
            session.CompleteRun();
    }
}
