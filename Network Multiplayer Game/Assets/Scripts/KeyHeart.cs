using Unity.Netcode;
using UnityEngine;

public class KeyHeart : NetworkBehaviour
{
    [SerializeField] private NetworkObject cabinetDoor_Heart;
    private NetworkVariable<bool> hasUnlocked = new NetworkVariable<bool>(false);

    private void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return;
        if (hasUnlocked.Value) return;
        if (cabinetDoor_Heart == null)
        {
            return;
        }

        if (other.gameObject.CompareTag("HeartKey"))
        {
            Debug.Log("has collided");

            hasUnlocked.Value = true;
            cabinetDoor_Heart.Despawn(true);
        }
    }
}
