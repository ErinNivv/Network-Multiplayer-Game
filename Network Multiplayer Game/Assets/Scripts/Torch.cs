using UnityEngine;
using Unity.Netcode;

public class Torch : NetworkBehaviour
{
    [SerializeField] private Light torchLight;
    [SerializeField] private GameObject torchLightObject;
    [SerializeField] private GameObject battery;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private NetworkObject no48;
    [SerializeField] private NetworkObject no63;

    private bool hasBattery = false;
    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBattery) return;
        if (other.CompareTag("Battery"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null)
                InsertBatteryServerRpc(netObj.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InsertBatteryServerRpc(ulong batteryNetworkId)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(batteryNetworkId, out NetworkObject netObj))
        {
            // Despawn must happen on server only
            netObj.gameObject.SetActive(false);
            netObj.Despawn(true);

            if (no48 != null ) no48.Despawn(true);
            if (no63 != null ) no63.Despawn(true);

            // Tell all clients to turn the torch on
            TurnOnTorchClientRpc();
        }
    }

    [ClientRpc]
    private void TurnOnTorchClientRpc()
    {
        hasBattery = true;
        torchLightObject.SetActive(true);
        Debug.Log("Battery inserted! Torch is on.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}