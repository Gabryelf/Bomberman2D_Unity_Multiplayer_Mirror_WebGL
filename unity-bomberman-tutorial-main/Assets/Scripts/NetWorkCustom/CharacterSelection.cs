using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CharacterSelection : NetworkBehaviour
{
    public Button[] characterButtons;  // ������ ������ ��� ������ ���������
    public Text countdownText;         // ����� ��� ��������� �������
    private bool hasSelected = false;  // ���� ��� �������� ������ ���������

    [SyncVar(hook = nameof(OnCharacterSelected))]
    public int selectedCharacter = -1; // ����� ���������� ���������

    private GameObject characterSelectionMenu; // ������ �� ���� ������ ���������

    private void Start()
    {
        // ��������, ��� ��� ��������� �����, ����� ������������ ������
        if (!isLocalPlayer) return;

        // ������������� ����, ���������� ����� CustomNetworkManager
        var customNetworkManager = (CustomNetworkManager)NetworkManager.singleton;
        characterSelectionMenu = customNetworkManager.characterSelectionMenu;

        // ���������� ����������� ������� �� ������
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => SelectCharacter(index));
        }

        characterSelectionMenu.SetActive(true);  // ���������� ���� ������
    }

    public void SelectCharacter(int index)
    {
        if (hasSelected) return;  // ���������� ����������, ���� ����� ��� ��� ������

        hasSelected = true;  // ��������, ��� �������� ������
        CmdSelectCharacter(index);  // �������� ������� �� �������, ����� ������� ���������
        DisableButtons();
    }

    [Command]
    void CmdSelectCharacter(int index)
    {
        // ������ ������������ ����� ���������
        selectedCharacter = index;

        // ����� ������ ��������� ��������� ������ �� ���� ��������
        RpcStartCountdown();
    }

    [ClientRpc]
    public void RpcStartCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    void OnCharacterSelected(int oldIndex, int newIndex)
    {
        // ������������ ������ ��� ���������� ���������
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

    // ���������, ��� ������ �������, ������ ��� ������� ����� ������
    if (NetworkServer.active)
    {
        Debug.Log("Server is active, spawning player...");
        SpawnPlayer();
        RpcHideCharacterSelectionMenu(); // ������ ����
    }
    else
    {
        Debug.LogWarning("Server is not active, skipping spawn.");
    }
}




    [ClientRpc]
    void RpcHideCharacterSelectionMenu()
    {
        // ���������, ������� �� ������ � ���� ���� ��� �� ������
        if (characterSelectionMenu != null)
        {
            Debug.Log("Hiding character selection menu...");
            characterSelectionMenu.SetActive(false); // ������ ����
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




