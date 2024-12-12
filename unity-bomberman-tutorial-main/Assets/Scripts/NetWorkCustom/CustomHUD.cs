using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CustomHUD : MonoBehaviour
{
    public NetworkManager networkManager;
    public GameObject panel;

    [Header("UI Elements")]
    public Button hostButton;
    public Button clientButton;
    public Button stopButton;
    public Text statusText;

    private void Start()
    {
        // Подписываемся на события кнопок
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        stopButton.onClick.AddListener(StopConnection);
    }

    private void Update()
    {
        // Обновляем статус подключения
        if (NetworkClient.isConnected)
        {
            statusText.text = "Status: Connected as Client";
        }
        else if (NetworkServer.active)
        {
            statusText.text = "Status: Hosting Server";
        }
        else
        {
            statusText.text = "Status: Disconnected";
        }
    }

    private void StartHost()
    {
        if (!NetworkServer.active && !NetworkClient.isConnected)
        {
            networkManager.StartHost();
        }
        else
        {
            Debug.LogWarning("Server or Client is already running.");
        }
    }

    private void StartClient()
    {
        networkManager.StartClient();
        panel.SetActive(false);
    }

    public void StopConnection()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            networkManager.StopHost();
            panel.SetActive(true);
        }
        else if (NetworkClient.isConnected)
        {
            networkManager.StopClient();
            panel.SetActive(true);
        }
    }
}

