using UnityEngine;
using Unity.Netcode;

public class Torch : NetworkBehaviour
{
    [SerializeField] private Light torchLight;
    [SerializeField] private GameObject torchLightObject;
    [SerializeField] private GameObject battery;
    [SerializeField] private float detectionRadius = 1.5f;
   
    private bool hasBattery = false;

    private void Update()
    {
        if (hasBattery) return;

        
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Battery"))
            {
                NetworkObject netObj = hit.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    InsertBatteryServerRpc(netObj.NetworkObjectId);
                }
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InsertBatteryServerRpc(ulong batteryNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(batteryNetworkId, out NetworkObject netObj))
        {
            netObj.Despawn(false); 
            InsertBatteryClientRpc();
            battery.SetActive(false);
        }
    }

    [ClientRpc]
    private void InsertBatteryClientRpc()
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