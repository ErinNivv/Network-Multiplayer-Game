using UnityEngine;
using Unity.Netcode;

public class Spawn : NetworkBehaviour
{
    void SpawnPickUpObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(prefab, position, rotation);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        netObj.Spawn();
    }
}
