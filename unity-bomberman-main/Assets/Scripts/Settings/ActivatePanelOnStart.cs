using UnityEngine;

public class ActivatePanelOnStart : MonoBehaviour
{
    [Header("Panel to Activate")]
    public GameObject panelToActivate;

    void Start()
    {
        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Panel to Activate is not assigned in the inspector.");
        }
    }
}

