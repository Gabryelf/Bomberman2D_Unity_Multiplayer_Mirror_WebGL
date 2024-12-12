using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    public GameObject[] playerPrefabs; // Префабы игроков с разными спрайтами
    public Transform[] spawnPoints;    // Точки спавна для игроков
    public GameObject characterSelectionPrefab; // Префаб меню выбора персонажа

    public GameObject characterSelectionMenu; // Ссылка на меню выбора персонажа

    //private void Awake()
    //{
    //    // Убедимся, что menu создается только для локального игрока
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
            characterSelectionMenu.SetActive(true); // Показываем меню выбора только для локального игрока
        }
    } 

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // Проверяем, что добавлен локальный игрок
        if (conn.identity.isLocalPlayer)
        {
            // После того, как все игроки выбрали персонажа, скрываем меню
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

        // Выбираем точку спавна в зависимости от количества игроков
        Transform spawnPoint = spawnPoints[numPlayers % spawnPoints.Length];
        GameObject player = Instantiate(playerPrefabs[characterIndex], spawnPoint.position, spawnPoint.rotation);

        // Добавляем игрока в сервер
        NetworkServer.AddPlayerForConnection(conn, player);

        Debug.Log($"Player spawned with character index {characterIndex} at spawn point {spawnPoint.position}.");
    }

}














