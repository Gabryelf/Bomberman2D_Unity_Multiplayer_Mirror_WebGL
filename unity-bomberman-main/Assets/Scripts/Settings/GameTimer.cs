using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;

public class GameTimer : NetworkBehaviour
{
    public float matchDuration = 180f; // 3 минуты
    private float timer;
    public Text timerText; // Ссылка на UI текст для отображения таймера

    public GameObject losePanel;
    public GameObject winPanel;
    public GameObject drawPanel;

    public Text playerScoreText;
    public Text opponentScoreText;

    [SyncVar]
    private bool gameEnded = false;

    [SyncVar(hook = nameof(OnPlayerScoreChanged))]
    private int playerScore; // Очки игрока

    [SyncVar(hook = nameof(OnOpponentScoreChanged))]
    private int opponentScore; // Очки бота или другого игрока

    public GameObject drawObj;
    public GameObject winObj;
    public GameObject loseObj;

    public GameObject wallObj;

    private void Start()
    {
        timer = matchDuration;
        UpdateTimerDisplay();
    }

    public IEnumerator StartCountdownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isServer && !gameEnded)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (!isServer || gameEnded) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = 0;
            EndGame();
        }

        if(timer < 45)
        {
            wallObj.SetActive(true);
        }

        UpdateTimerDisplay();
    }

    private void StartTimer()
    {
        timer = matchDuration;
    }


    [Server]
    private void EndGame()
    {
        gameEnded = true;
        AddPlayerScore();
        // Передаем финальные результаты через RPC
        RpcShowGameOver(playerScore, opponentScore);
    }

    [ClientRpc]
    private void RpcShowGameOver(int finalPlayerScore, int finalOpponentScore)
    {
        Debug.Log("Game Over!");

        // Обновляем очки в UI
        playerScoreText.text = $"{finalPlayerScore}";
        opponentScoreText.text = $"{finalOpponentScore}";

        // Логика определения победителя
        if (finalPlayerScore > finalOpponentScore)
        {
            winPanel.SetActive(true);
            winObj.SetActive(true);
            loseObj.SetActive(false);
        }
        else if (finalPlayerScore < finalOpponentScore)
        {
            losePanel.SetActive(true);
            loseObj.SetActive(true);
            winObj.SetActive(false);
        }
        else
        {
            // Если ничья
            drawPanel.SetActive(true); 
            drawObj.SetActive(false);
            loseObj.SetActive(false);
            winObj.SetActive(false);
        }
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
    }

    // Вызывается для увеличения очков игрока
    [Server]
    public void AddPlayerScore()
    {
        MovementController player = FindObjectOfType<MovementController>();
        playerScore = player.score;
    }

    // Вызывается для увеличения очков оппонента
    [Server]
    public void AddOpponentScore(int points)
    {
        opponentScore += points;
    }

    // Хук для изменения очков игрока
    private void OnPlayerScoreChanged(int oldScore, int newScore)
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = $"{newScore}";
        }
    }

    // Хук для изменения очков оппонента
    private void OnOpponentScoreChanged(int oldScore, int newScore)
    {
        if (opponentScoreText != null)
        {
            opponentScoreText.text = $"{newScore}";
        }
    }
}



