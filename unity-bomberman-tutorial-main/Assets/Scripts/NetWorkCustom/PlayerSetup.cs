using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    public GameObject characterSelectionMenu; // ������ ��� ������ ���� �� Canvas

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            // ���������� ���� ������ ���������
            characterSelectionMenu.SetActive(true);
        }
    }
}

