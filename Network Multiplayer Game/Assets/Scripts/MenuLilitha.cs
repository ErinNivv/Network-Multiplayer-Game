using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class MenuLilitha : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private TMP_InputField portInput;

    [Header("Defaults")]
    [SerializeField] private string defaultIP = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;

    [SerializeField] private UnityTransport transport;
    [SerializeField] private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        transport = networkManager.GetComponent<UnityTransport>();

        if (ipInput) ipInput.text = defaultIP;
        if (portInput) portInput.text = defaultPort.ToString();

    }

    public void StartHost()
    {
        if(networkManager == null) networkManager = NetworkManager.Singleton;
        if (transport == null) transport = networkManager.GetComponent<UnityTransport>();

        ushort port = GetPort();
        transport.SetConnectionData("0.0.0.0", port);

        networkManager.StartHost();
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        networkManager.SceneManager.LoadScene("EscapeRoomLilitha", LoadSceneMode.Single);
    }

    public void JoinGame()
    {
        if (networkManager == null) networkManager = NetworkManager.Singleton;
        if (transport == null) transport = networkManager.GetComponent<UnityTransport>();

        string ip = GetIP();
        ushort port = GetPort();
        transport.SetConnectionData(ip, port);

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.StartClient();
    }

    private void OnClientConnected(ulong clientId)
    {
        networkManager.OnClientConnectedCallback -= OnClientConnected;

        Debug.Log($"[Menu] Client {clientId} connected successfully");
    }

    public void StartServerOnly()
    {
        if (networkManager == null) networkManager = NetworkManager.Singleton;
        if (transport == null) transport = networkManager.GetComponent<UnityTransport>();

        ushort port = GetPort();
        transport.SetConnectionData("0.0.0.0", port);
        networkManager.StartServer();
    }

    private string GetIP()
    {
        if (!ipInput || string.IsNullOrWhiteSpace(ipInput.text))
            return defaultIP;

        return ipInput.text.Trim();
    }

    private ushort GetPort()
    {
        if (!portInput || !ushort.TryParse(portInput.text, out ushort port))
            return defaultPort;

        return port;
    }
}
