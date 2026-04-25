using UnityEngine;
using Unity.Netcode;

public class PhysicalRotateButton : NetworkBehaviour
{
    [SerializeField] private StatueSnap linkedStatueSnap;

    public void Interact()
    {
        if(linkedStatueSnap == null || !linkedStatueSnap.isPlaced.Value) return;
        linkedStatueSnap.StatueRotateServerRpc();
    }
}
