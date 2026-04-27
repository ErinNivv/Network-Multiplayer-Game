using UnityEngine;
using Unity.Netcode;

public class KeyUnlock : NetworkBehaviour
{
    [SerializeField] private NetworkObject cabinetDoor_Triangle;
    private NetworkVariable<bool> hasUnlocked = new NetworkVariable<bool>(false);

    private void OnCollisionEnter(Collision other)
    {
        if(!IsServer) return;
        if (hasUnlocked.Value) return;
        if (cabinetDoor_Triangle == null)
        {
            return;
        }

        if (other.gameObject.CompareTag("TriangleKey"))
        { 
            Debug.Log("has collided");

            hasUnlocked.Value = true;
            cabinetDoor_Triangle.Despawn(true);
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
