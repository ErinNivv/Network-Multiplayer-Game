using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class StatueCount : NetworkBehaviour
{
    [SerializeField] private NetworkObject exitDoor;

    private NetworkVariable<int> statueCount = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private const int RequiredCount = 4;
    private bool doorOpened = false;

    public override void OnNetworkSpawn()
    {
        statueCount.OnValueChanged += OnStatueCountChanged;
    }

    public override void OnNetworkDespawn()
    {
        statueCount.OnValueChanged -= OnStatueCountChanged;
    }
    public void CorrectStatueCount()
    {
        if (!IsServer) return;
        statueCount.Value = Mathf.Min(statueCount.Value + 1, RequiredCount);
        CheckWinCondition();
    }

    public void IncorrectStatueCount()
    {
        if (!IsServer) return;
        statueCount.Value = Mathf.Max(statueCount.Value - 1, 0);
    }

    private void CheckWinCondition()
    {
        if (!IsServer) return;
        if(doorOpened) return;
        if(statueCount.Value != RequiredCount) return;

        doorOpened = true;

        if(exitDoor != null && exitDoor.IsSpawned)
            exitDoor.Despawn(true);
    }

    private void OnStatueCountChanged(int previous, int current)
    {
        Debug.Log($"Statue count changed: {previous} -> {current}");
    }
}
