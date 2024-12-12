using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    public GameObject characterSelectionMenu; // Префаб или объект меню на Canvas

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            // Активируем меню выбора персонажа
            characterSelectionMenu.SetActive(true);
        }
    }
}

