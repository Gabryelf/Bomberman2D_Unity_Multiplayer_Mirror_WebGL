using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CustomNetworkHUD : MonoBehaviour
{
    [Header("UI Elements")]
    public Button hostButton;
    public Button joinButton;
    public Button singlePlayerButton;
    public InputField addressInput;
    public GameObject characterSelectionPanel;
    public GameObject networkPanel;

    public NetworkManager networkManager;

    // Порт по умолчанию
    private const string defaultAddress = "localhost";
    private const ushort defaultPort = 27777;

    void Start()
    {
        hostButton.onClick.AddListener(() => {
            StopExistingConnections();
            StartHost();
        });

        joinButton.onClick.AddListener(() => {
            StopExistingConnections();
            StartClient();
        });

        singlePlayerButton.onClick.AddListener(() => {
            StopExistingConnections();
            StartSinglePlayer();
        });

        // Активируем панель выбора режима при старте
        networkPanel.SetActive(true);
    }

    void StopExistingConnections()
    {
        if (NetworkServer.active || NetworkClient.active)
        {
            networkManager.StopHost();
            Debug.Log("Stopped existing connections.");
        }
    }

    void StartHost()
    {
        networkManager.networkAddress = defaultAddress;
        SetPort(defaultPort);

        Debug.Log($"Starting Host on {networkManager.networkAddress}:{defaultPort}");
        networkManager.StartHost();
        ShowCharacterSelectionPanel();
    }

    void StartClient()
    {
        networkManager.networkAddress = string.IsNullOrEmpty(addressInput.text) ? defaultAddress : addressInput.text;
        SetPort(defaultPort);

        Debug.Log($"Connecting to {networkManager.networkAddress}:{defaultPort}");
        networkManager.StartClient();
        ShowCharacterSelectionPanel();
    }

    void StartSinglePlayer()
    {
        networkManager.networkAddress = defaultAddress;
        SetPort(defaultPort);

        Debug.Log("Starting Single Player Mode (Host)...");
        networkManager.StartHost();
        ShowCharacterSelectionPanel();
    }

    void SetPort(ushort port)
    {
        var transport = networkManager.GetComponent<Mirror.SimpleWeb.SimpleWebTransport>();
        if (transport != null)
        {
            transport.port = port;
            Debug.Log($"Set transport port to {port}");
        }
        else
        {
            Debug.LogWarning("Transport component not found.");
        }
    }

    void ShowCharacterSelectionPanel()
    {
        networkPanel.SetActive(false);
        characterSelectionPanel.SetActive(true);
    }
}



