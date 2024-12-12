using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    public GameObject[] playerPrefabs; // ������� ������� � ������� ���������
    public Transform[] spawnPoints;    // ����� ������ ��� �������
    public GameObject characterSelectionPrefab; // ������ ���� ������ ���������

    public GameObject characterSelectionMenu; // ������ �� ���� ������ ���������

    //private void Awake()
    //{
    //    // ��������, ��� menu ��������� ������ ��� ���������� ������
    //    if (characterSelectionMenu == null)
    //    {
    //        characterSelectionMenu = Instantiate(characterSelectionPrefab);
    //    }
    //    characterSelectionMenu.SetActive(true);
    //}

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server started successfully.");
    }


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.isLocalPlayer)
        {
            characterSelectionMenu.SetActive(true); // ���������� ���� ������ ������ ��� ���������� ������
        }
    } 

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // ���������, ��� �������� ��������� �����
        if (conn.identity.isLocalPlayer)
        {
            // ����� ����, ��� ��� ������ ������� ���������, �������� ����
            characterSelectionMenu.SetActive(false);
        }
    }

    [Server]
    public void SpawnPlayerWithCharacter(NetworkConnectionToClient conn, int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= playerPrefabs.Length)
        {
            Debug.LogError("Invalid character index for spawning.");
            return;
        }

        // �������� ����� ������ � ����������� �� ���������� �������
        Transform spawnPoint = spawnPoints[numPlayers % spawnPoints.Length];
        GameObject player = Instantiate(playerPrefabs[characterIndex], spawnPoint.position, spawnPoint.rotation);

        // ��������� ������ � ������
        NetworkServer.AddPlayerForConnection(conn, player);

        Debug.Log($"Player spawned with character index {characterIndex} at spawn point {spawnPoint.position}.");
    }

}














