using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class CharacterSelectionManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public Button[] characterButtons; // Кнопки для выбора персонажей
    public Transform[] spawnPoints;   // Точки спавна для игроков и ботов
    [SerializeField] private GameObject botPrefab;

    private bool hasSelected = false; // Флаг, что игрок сделал выбор

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

        // Найти локального игрока
        var player = NetworkClient.localPlayer;

        if (player != null)
        {
            MovementController movementController = player.GetComponent<MovementController>();
            if (movementController != null)
            {
                movementController.CmdSelectCharacter(characterIndex);
                hasSelected = true;
                DisableAllButtons();

                // Спавн бота после выбора персонажа
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
                // Выбираем случайную точку спавна
                Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

                // Создаём экземпляр бота на сервере
                Vector3 spawnPosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 90);
                GameObject bot = Instantiate(botPrefab, spawnPosition, Quaternion.identity);

                // Спавним бота по сети
                NetworkServer.Spawn(bot);

                // Получаем NetworkIdentity бота
                NetworkIdentity botNetworkIdentity = bot.GetComponent<NetworkIdentity>();

                // Проверяем, что у бота есть клиент, которому можно назначить права
                if (botNetworkIdentity != null)
                {
                    // Назначаем права серверу или по логике игры
                    // Здесь это зависит от того, какой клиент должен управлять ботом
                    // Если это обычный бот, вы можете передать права серверу или самому себе (серверу):

                    // Назначаем права серверу (если бот управляется сервером)
                    botNetworkIdentity.AssignClientAuthority(NetworkServer.localConnection);

                    // Или, если у вас есть определённый клиент, вы можете назначить ему права
                    // Пример:
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
            // Если метод вызывается на клиенте, отправляем команду на сервер
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







