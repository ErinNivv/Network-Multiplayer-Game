using UnityEngine;
using Unity.Netcode;

public class Torch : NetworkBehaviour
{
    [SerializeField] private Light torchLight;
    [SerializeField] private GameObject torchLightObject;
    [SerializeField] private GameObject battery;
    [SerializeField] private float detectionRadius = 1.5f;
   
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
            {
                InsertBatteryServerRpc(netObj.NetworkObjectId);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void InsertBatteryServerRpc(ulong batteryNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(batteryNetworkId, out NetworkObject netObj))
        {
            HideBatteryClientRpc(batteryNetworkId);
        }
    }

    [ClientRpc]
    private void HideBatteryClientRpc(ulong batteryNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(batteryNetworkId, out NetworkObject netObj))
        {
            netObj.gameObject.SetActive(false);
            hasBattery = true;
            Debug.Log("torchLightObject is: " + torchLightObject);
            torchLightObject.SetActive(true);
            Debug.Log("Battery inserted! Torch is on.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius); 
    }
}