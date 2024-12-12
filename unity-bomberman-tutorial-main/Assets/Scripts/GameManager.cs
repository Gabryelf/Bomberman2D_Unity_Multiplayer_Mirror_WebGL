using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public GameObject[] players;
    public GameObject losePanel;

    public void CheckWinState()
    {
        int aliveCount = 0;
        
        foreach (GameObject player in players)
        {
            if (player.activeSelf) {
                aliveCount++;
            }
        }
        
        if (aliveCount <= 1) {
            if (!isLocalPlayer) return;
            Invoke(nameof(NewRound), 3f);
        }
    }

    private void NewRound()
    {
        losePanel.SetActive(true);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StopGame()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
    }

    

}
