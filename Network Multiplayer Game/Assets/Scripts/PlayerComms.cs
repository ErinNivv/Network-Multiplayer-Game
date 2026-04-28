using UnityEngine;
using Unity.Netcode;

public class PlayerComms : NetworkBehaviour
{
    [SerializeField] private GameObject[] icons; 
    [SerializeField] private Transform iconSpawnPoint; 
    [SerializeField] private float iconDuration = 3f;

    private GameObject currentIcon;

    public void SendComms(int iconIndex)
    {
        if (!IsOwner) return; // only owner can send
        SendCommsServerRpc(iconIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendCommsServerRpc(int iconIndex)
    {
        ShowIconClientRpc(iconIndex);
    }

    [ClientRpc]
    private void ShowIconClientRpc(int iconIndex)
    {
        if (currentIcon != null) Destroy(currentIcon);
        currentIcon = Instantiate(icons[iconIndex], iconSpawnPoint.position, Quaternion.identity);
        currentIcon.transform.SetParent(iconSpawnPoint);
        Destroy(currentIcon, iconDuration);
    }
}