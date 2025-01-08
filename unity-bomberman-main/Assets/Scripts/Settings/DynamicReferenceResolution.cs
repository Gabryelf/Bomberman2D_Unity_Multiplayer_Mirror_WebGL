using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class DynamicReferenceResolution : MonoBehaviour
{
    public float portraitYResolution = 28f;  // Reference Resolution �� ��� Y ��� ����������� ������
    public float landscapeYResolution = 14f; // Reference Resolution �� ��� Y ��� ���������� ������

    private CanvasScaler canvasScaler;

    public static float ScaleFactor { get; private set; } = 1f;

    private void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        UpdateResolution();
    }

    private void Update()
    {
        UpdateResolution();
    }

    private void UpdateResolution()
    {
        if (Screen.height > Screen.width)
        {
            // ���������� �����
            canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, portraitYResolution);
            ScaleFactor = 0.5f; // ��������� ������� �����
        }
        else
        {
            // ��������� �����
            canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, landscapeYResolution);
            ScaleFactor = 1f; // ������� �������
        }
    }
}

