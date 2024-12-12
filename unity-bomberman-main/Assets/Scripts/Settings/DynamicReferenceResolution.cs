using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class DynamicReferenceResolution : MonoBehaviour
{
    public float portraitYResolution = 28f;  // Reference Resolution по оси Y для портретного режима
    public float landscapeYResolution = 14f; // Reference Resolution по оси Y для альбомного режима

    private CanvasScaler canvasScaler;

    private void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        UpdateResolution();
    }

    private void Update()
    {
        // Проверяем изменение ориентации и обновляем разрешение
        UpdateResolution();
    }

    private void UpdateResolution()
    {
        if (Screen.height > Screen.width)
        {
            // Портретный режим
            canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, portraitYResolution);
        }
        else
        {
            // Альбомный режим
            canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, landscapeYResolution);
        }
    }
}
