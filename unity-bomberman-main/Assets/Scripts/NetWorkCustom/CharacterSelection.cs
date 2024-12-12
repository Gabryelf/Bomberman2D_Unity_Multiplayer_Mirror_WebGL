using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CharacterSelection : NetworkBehaviour
{
    public Button[] characterButtons;  // Массив кнопок для выбора персонажа
    public Text countdownText;         // Текст для обратного отсчета
    private bool hasSelected = false;  // Флаг для проверки выбора персонажа

    [SyncVar(hook = nameof(OnCharacterSelected))]
    public int selectedCharacter = -1; // Номер выбранного персонажа

    private GameObject characterSelectionMenu; // Ссылка на меню выбора персонажа

    private void Start()
    {
        // Убедимся, что это локальный игрок, чтобы активировать кнопки
        if (!isLocalPlayer) return;

        // Устанавливаем меню, переданное через CustomNetworkManager
        var customNetworkManager = (CustomNetworkManager)NetworkManager.singleton;
        characterSelectionMenu = customNetworkManager.characterSelectionMenu;

        // Подключаем обработчики нажатий на кнопки
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => SelectCharacter(index));
        }

        characterSelectionMenu.SetActive(true);  // Показываем меню выбора
    }

    public void SelectCharacter(int index)
    {
        if (hasSelected) return;  // Возвращаем управление, если выбор уже был сделан

        hasSelected = true;  // Помечаем, что персонаж выбран
        CmdSelectCharacter(index);  // Вызываем команду на сервере, чтобы выбрать персонажа
        DisableButtons();
    }

    [Command]
    void CmdSelectCharacter(int index)
    {
        // Сервер обрабатывает выбор персонажа
        selectedCharacter = index;

        // После выбора персонажа запускаем отсчет на всех клиентах
        RpcStartCountdown();
    }

    [ClientRpc]
    public void RpcStartCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    void OnCharacterSelected(int oldIndex, int newIndex)
    {
        // Деактивируем кнопки для выбранного персонажа
        if (newIndex >= 0 && newIndex < characterButtons.Length)
        {
            characterButtons[newIndex].interactable = false;
        }
    }

    void DisableButtons()
    {
        foreach (var button in characterButtons)
        {
            button.interactable = false;
        }
    }

    IEnumerator CountdownCoroutine()
{
    int countdown = 5;
    while (countdown > 0)
    {
        countdownText.text = $"Game starts in {countdown}...";
        yield return new WaitForSeconds(1f);
        countdown--;
    }
    countdownText.text = "Go!";

    // Убедитесь, что сервер активен, прежде чем вызвать метод спавна
    if (NetworkServer.active)
    {
        Debug.Log("Server is active, spawning player...");
        SpawnPlayer();
        RpcHideCharacterSelectionMenu(); // Скрыть меню
    }
    else
    {
        Debug.LogWarning("Server is not active, skipping spawn.");
    }
}




    [ClientRpc]
    void RpcHideCharacterSelectionMenu()
    {
        // Проверяем, активен ли сервер и если меню еще не скрыто
        if (characterSelectionMenu != null)
        {
            Debug.Log("Hiding character selection menu...");
            characterSelectionMenu.SetActive(false); // Скрыть меню
        }
        else
        {
            Debug.LogError("Character selection menu is null!");
        }
    }


    void SpawnPlayer()
    {
        if (!isLocalPlayer) return;

        var customNetworkManager = (CustomNetworkManager)NetworkManager.singleton;

        Debug.Log("Spawning player with character index: " + selectedCharacter);

        customNetworkManager.SpawnPlayerWithCharacter(connectionToClient, selectedCharacter);
        Debug.Log($"Player spawned with character {selectedCharacter}");
    }

}




