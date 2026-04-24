using UnityEngine;
using Unity.Netcode;

public class KeyDiamond : NetworkBehaviour
{
    [SerializeField] private NetworkObject cabinetDoor_Diamond;
    private NetworkVariable<bool> hasUnlocked = new NetworkVariable<bool>(false);

    private void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return;
        if (hasUnlocked.Value) return;
        if (cabinetDoor_Diamond == null)
        {
            return;
        }

        if (other.gameObject.CompareTag("DiamondKey"))
        {
            Debug.Log("has collided");

            hasUnlocked.Value = true;
            cabinetDoor_Diamond.Despawn(true);
        }
    }
}
