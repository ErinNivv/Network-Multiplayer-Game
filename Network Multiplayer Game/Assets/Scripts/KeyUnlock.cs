using UnityEngine;
using Unity.Netcode;

public class KeyUnlock : NetworkBehaviour
{
    [SerializeField] private NetworkObject cabinetDoor;
    private bool hasUnlocked = false;

    private void OnCollisionEnter(Collision other)
    {
        if(!IsServer) return;
        if (hasUnlocked) return;

        if (other.gameObject.CompareTag("Key"))
        {
            hasUnlocked = true;

            //NetworkObject keyObject = other.gameObject.GetComponent<NetworkObject>();
            //if (keyObject != null)
            //    keyObject.Despawn(true);

            if(cabinetDoor != null)
                cabinetDoor.Despawn(false);
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void KeyUnlockServerRpc(ulong keyNetworkId)
    //{
    //    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(keyNetworkId, out NetworkObject netObj))
    //    {
    //        keyUnlockClientRpc(keyNetworkId);
    //    }
    //}

    //[ClientRpc]
    //private void keyUnlockClientRpc(ulong keyNetworkId)
    //{
    //    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(keyNetworkId, out NetworkObject netObj))
    //    {
    //        cabinetDoor.Despawn(false);
    //    }
    //}
}
