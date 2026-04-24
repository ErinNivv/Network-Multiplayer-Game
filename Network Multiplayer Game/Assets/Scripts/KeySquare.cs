using Unity.Netcode;
using UnityEngine;

public class KeySquare : NetworkBehaviour
{
    [SerializeField] private NetworkObject cabinetDoor_Square;
    private NetworkVariable<bool> hasUnlocked = new NetworkVariable<bool>(false);

    private void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return;
        if (hasUnlocked.Value) return;
        if (cabinetDoor_Square == null)
        {
            return;
        }

        if (other.gameObject.CompareTag("SquareKey"))
        {
            Debug.Log("has collided");

            hasUnlocked.Value = true;
            cabinetDoor_Square.Despawn(true);
        }
    }
}
