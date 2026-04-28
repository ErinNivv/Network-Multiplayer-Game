using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MenuLilitha : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMPro.TextMeshProUGUI joinCodeDisplay;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;

    private NetworkManager networkManager;
    private UnityTransport transport;

    private async void Start()
    {
        networkManager = NetworkManager.Singleton;
        transport = networkManager.GetComponent<UnityTransport>();

        // Initialize Unity Services
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (statusText) statusText.text = "Ready";
    }

    public async void StartHost()
    {
        if (statusText) statusText.text = "Starting host...";

        try
        {
            // Create relay allocation for max 2 players
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            networkManager.StartHost();

            // Show join code on screen for client to use
            if (joinCodeDisplay)
            {
                joinCodeDisplay.gameObject.SetActive(true);
                joinCodeDisplay.text = "Code: " + joinCode;
            }

            if (statusText) statusText.text = "Hosting...\nWaiting for player...";

            // Listen for client connecting then load scene
            networkManager.OnClientConnectedCallback += OnPlayerConnected;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Host failed: " + e.Message);
            if (statusText) statusText.text = "Host failed!";
        }
    }

    private void OnPlayerConnected(ulong clientId)
    {
        if (!networkManager.IsHost) return;

        if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2)
        {
            networkManager.OnClientConnectedCallback -= OnPlayerConnected;
            networkManager.SceneManager.LoadScene("EscapeRoomLilitha", LoadSceneMode.Single);
        }
    }

    public async void JoinGame()
    {
        if (networkManager.IsClient || networkManager.IsHost)
        {
            Debug.LogWarning("Already connected!");
            return;
        }

        string code = joinCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            if (statusText) statusText.text = "Enter a join code!";
            return;
        }

        if (statusText) statusText.text = "Connecting...";

        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(code);

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            networkManager.StartClient();

            if (statusText) statusText.text = "Connected!\nWaiting for host to start...";
        }
        catch (System.Exception e)
        {
            Debug.LogError("Join failed: " + e.Message);
            if (statusText) statusText.text = "Join failed!\nCheck your code.";
        }
    }
}
