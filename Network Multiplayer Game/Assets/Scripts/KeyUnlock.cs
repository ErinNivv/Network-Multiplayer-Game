using UnityEngine;
using Unity.Netcode;

public class KeyUnlock : NetworkBehaviour
{
    [SerializeField] private NetworkObject cabinetDoor;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Key"))
        {

        }
    }
}
