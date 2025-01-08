using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public GameObject[] players;
    public GameObject losePanel;

    public int playerSkin;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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


    public void SetPlayerSkin(int skinIndex)
    {
        if(skinIndex == 1)
        {
            playerSkin = 2;
        }
        if (skinIndex == 2)
        {
            playerSkin = 1;

        }
        if (skinIndex == 3)
        {
            playerSkin = 4;

        }
        if (skinIndex == 4)
        {
            playerSkin = 3;

        }
    }

    public int GetPlayerSkin()
    {
        return playerSkin;
    }



}
