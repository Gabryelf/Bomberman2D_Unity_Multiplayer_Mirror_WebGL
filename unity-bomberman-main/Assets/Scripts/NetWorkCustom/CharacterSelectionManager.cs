using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class CharacterSelectionManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public Button[] characterButtons; // ������ ��� ������ ����������
    public Transform[] spawnPoints;   // ����� ������ ��� ������� � �����
    [SerializeField] private GameObject botPrefab;

    private bool hasSelected = false; // ����, ��� ����� ������ �����

    void Start()
    {
        foreach (Button button in characterButtons)
        {
            button.onClick.AddListener(() => SelectCharacter(Array.IndexOf(characterButtons, button)));
        }
    }

    public void SelectCharacter(int characterIndex)
    {
        if (hasSelected) return;

        // ����� ���������� ������
        var player = NetworkClient.localPlayer;

        if (player != null)
        {
            MovementController movementController = player.GetComponent<MovementController>();
            if (movementController != null)
            {
                movementController.CmdSelectCharacter(characterIndex);
                hasSelected = true;
                DisableAllButtons();

                // ����� ���� ����� ������ ���������
                SpawnBotOnServer();
            }
        }
    }

    void SpawnBotOnServer()
    {
        if (isServer)
        {
            if (botPrefab != null)
            {
                // �������� ��������� ����� ������
                Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

                // ������ ��������� ���� �� �������
                Vector3 spawnPosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 90);
                GameObject bot = Instantiate(botPrefab, spawnPosition, Quaternion.identity);

                // ������� ���� �� ����
                NetworkServer.Spawn(bot);

                // �������� NetworkIdentity ����
                NetworkIdentity botNetworkIdentity = bot.GetComponent<NetworkIdentity>();

                // ���������, ��� � ���� ���� ������, �������� ����� ��������� �����
                if (botNetworkIdentity != null)
                {
                    // ��������� ����� ������� ��� �� ������ ����
                    // ����� ��� ������� �� ����, ����� ������ ������ ��������� �����
                    // ���� ��� ������� ���, �� ������ �������� ����� ������� ��� ������ ���� (�������):

                    // ��������� ����� ������� (���� ��� ����������� ��������)
                    botNetworkIdentity.AssignClientAuthority(NetworkServer.localConnection);

                    // ���, ���� � ��� ���� ����������� ������, �� ������ ��������� ��� �����
                    // ������:
                    // botNetworkIdentity.AssignClientAuthority(connectionToClient);

                }
            }
            else
            {
                Debug.LogError("BotPrefab reference is not set in the Inspector!");
            }
        }
        else
        {
            // ���� ����� ���������� �� �������, ���������� ������� �� ������
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







