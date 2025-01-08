using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class DynamicReferenceResolution : MonoBehaviour
{
    public float portraitYResolution = 28f;  // Reference Resolution по оси Y для портретного режима
    public float landscapeYResolution = 14f; // Reference Resolution по оси Y для альбомного режима

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
            // Портретный режим
            canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, portraitYResolution);
            ScaleFactor = 0.5f; // Уменьшаем масштаб вдвое
        }
        else
        {
            // Альбомный режим
            canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, landscapeYResolution);
            ScaleFactor = 1f; // Обычный масштаб
        }
    }
}

