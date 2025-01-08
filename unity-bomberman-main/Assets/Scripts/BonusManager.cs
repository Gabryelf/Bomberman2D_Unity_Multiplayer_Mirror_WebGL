using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class BonusManager : MonoBehaviour
{
    // Счетчики для каждого типа бонусов
    public int extraBombCount = 0;
    public int blastRadiusCount = 0;
    public int speedIncreaseCount = 0;

    // UI элементы для отображения счетчиков
    public Text extraBombText;
    public Text blastRadiusText;
    public Text speedIncreaseText;

    // Очки за разные действия (устанавливаются в инспекторе)
    public int wallDestructionPoints = 100;
    public int bonusPickupPoints = 50;
    public int playerHitPoints = 200;

    public Text scoreText;

    // Singleton для удобного доступа к этому менеджеру из других скриптов
    public static BonusManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    // Методы для увеличения счетчиков и обновления текста
    public void AddExtraBomb()
    {
        extraBombCount++;
        MovementController player = FindObjectOfType<MovementController>();
        player.CmdAddScore(bonusPickupPoints);
        UpdateText(extraBombText, " ", extraBombCount);
    }

    public void AddBlastRadius()
    {
        blastRadiusCount++;
        MovementController player = FindObjectOfType<MovementController>();
        player.CmdAddScore(bonusPickupPoints);
        UpdateText(blastRadiusText, " ", blastRadiusCount);
    }

    public void AddSpeedIncrease()
    {
        speedIncreaseCount++;
        MovementController player = FindObjectOfType<MovementController>();
        player.CmdAddScore(bonusPickupPoints);
        UpdateText(speedIncreaseText, " ", speedIncreaseCount);
    }

    private void UpdateText(Text textComponent, string label, int count)
    {
        if (textComponent != null)
        {
            textComponent.text = label + count.ToString();
        }
    }

    public void UpdateScoreUI()
    {
        MovementController player = FindObjectOfType<MovementController>();
        scoreText.text = $"{player.score}".ToString();
    }
}


