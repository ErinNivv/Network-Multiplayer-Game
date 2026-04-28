using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
        System.Collections.Generic.List<ulong> clientsCompleted,
        System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;
        if (sceneName != "EscapeRoomLilitha 1") return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // Skip if player already spawned
            if (NetworkManager.Singleton.ConnectedClients[client.ClientId].PlayerObject != null)
                continue;

            int index = (int)client.ClientId % spawnPoints.Length;
            Vector3 spawnPos = spawnPoints[index].position;

            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);

            Debug.Log($"Spawned player {client.ClientId} at {spawnPos}");
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }
}