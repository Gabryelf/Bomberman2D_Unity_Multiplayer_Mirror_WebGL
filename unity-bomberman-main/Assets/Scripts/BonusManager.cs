using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class BonusManager : MonoBehaviour
{
    // �������� ��� ������� ���� �������
    public int extraBombCount = 0;
    public int blastRadiusCount = 0;
    public int speedIncreaseCount = 0;

    // UI �������� ��� ����������� ���������
    public Text extraBombText;
    public Text blastRadiusText;
    public Text speedIncreaseText;

    // ���� �� ������ �������� (��������������� � ����������)
    public int wallDestructionPoints = 100;
    public int bonusPickupPoints = 50;
    public int playerHitPoints = 200;

    public Text scoreText;

    // Singleton ��� �������� ������� � ����� ��������� �� ������ ��������
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

    // ������ ��� ���������� ��������� � ���������� ������
    public void AddExtraBomb()
    {
        extraBombCount++;
        UpdateText(extraBombText, " ", extraBombCount);
    }

    public void AddBlastRadius()
    {
        blastRadiusCount++;
        UpdateText(blastRadiusText, " ", blastRadiusCount);
    }

    public void AddSpeedIncrease()
    {
        speedIncreaseCount++;
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


