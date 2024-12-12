using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public class CharacterSelectionManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public Button[] characterButtons; // Кнопки для выбора персонажей

    private bool hasSelected = false; // Флаг, что игрок сделал выбор

    void Start()
    {
        foreach (Button button in characterButtons)
        {
            button.onClick.AddListener(() => SelectCharacter(System.Array.IndexOf(characterButtons, button)));
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
            }
        }
    }

    [ClientRpc]
    public void RpcDisableButton(int characterIndex)
    {
        characterButtons[characterIndex].interactable = false;
    }

    private void DisableAllButtons()
    {
        foreach (Button button in characterButtons)
        {
            button.interactable = false;
        }
    }
}




