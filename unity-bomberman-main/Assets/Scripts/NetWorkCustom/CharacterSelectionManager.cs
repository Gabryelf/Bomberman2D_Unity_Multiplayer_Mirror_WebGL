using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

public class CharacterSelectionManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public Button[] characterButtons;
    public Transform[] spawnPoints;
    [SerializeField] private GameObject botPrefab;

    private bool hasSelected = false;
    public GameObject timerGame;
    public GameObject selectionPanel;
    private int totalPlayers;
    private int playersReady = 0;

    void Start()
    {
        if (isServer)
        {
            totalPlayers = NetworkServer.connections.Count;
        }

        foreach (Button button in characterButtons)
        {
            button.onClick.AddListener(() => SelectCharacter(System.Array.IndexOf(characterButtons, button)));
        }
    }

    public void SelectCharacter(int characterIndex)
    {
        if (hasSelected) return;

        var player = NetworkClient.localPlayer;

        if (player != null)
        {
            MovementController movementController = player.GetComponent<MovementController>();
            if (movementController != null)
            {
                movementController.CmdSelectCharacter(characterIndex);
                hasSelected = true;
                DisableAllButtons();
                CmdPlayerReady();

                SpawnBotOnServer();
                timerGame.SetActive(true);

                if (totalPlayers <= 1)
                {
                    StartCoroutine(HidePanelAfterDelay(2));
                    StartTimerAfterDelay(2);
                }
            }
        }
    }

    [Command]
    private void CmdPlayerReady()
    {
        playersReady++;
        RpcUpdatePlayersReady(playersReady);
        if (playersReady >= totalPlayers)
        {
            StartCoroutine(HidePanelAfterDelay(2));
            StartTimerAfterDelay(2);
        }
    }

    [ClientRpc]
    private void RpcUpdatePlayersReady(int readyCount)
    {
        playersReady = readyCount;
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    private void StartTimerAfterDelay(float delay)
    {
        GameTimer gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
        {
            StartCoroutine(gameTimer.StartCountdownAfterDelay(delay));
        }
    }

    void SpawnBotOnServer()
    {
        if (isServer)
        {
            if (botPrefab != null)
            {
                Transform botSpawnPoint = spawnPoints.Length > 1 ? spawnPoints[0] : spawnPoints[1];
                Vector3 spawnPosition = new Vector3(botSpawnPoint.position.x, botSpawnPoint.position.y, 90);
                GameObject bot = Instantiate(botPrefab, spawnPosition, Quaternion.identity);
                bot.transform.localScale *= DynamicReferenceResolution.ScaleFactor;
                NetworkServer.Spawn(bot);

                NetworkIdentity botNetworkIdentity = bot.GetComponent<NetworkIdentity>();
                if (botNetworkIdentity != null)
                {
                    botNetworkIdentity.AssignClientAuthority(NetworkServer.localConnection);
                }
            }
            else
            {
                Debug.LogError("BotPrefab reference is not set in the Inspector!");
            }
        }
        else
        {
            CmdRequestSpawnBot();
        }
    }

    [Command]
    void CmdRequestSpawnBot()
    {
        SpawnBotOnServer();
    }

    [ClientRpc]
    public void RpcDisableButton(int characterIndex)
    {
        if (characterIndex >= 0 && characterIndex < characterButtons.Length)
        {
            characterButtons[characterIndex].interactable = false;
        }
    }

    private void DisableAllButtons()
    {
        foreach (Button button in characterButtons)
        {
            button.interactable = false;
        }
    }
}










