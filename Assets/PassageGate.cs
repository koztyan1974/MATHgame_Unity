using UnityEngine;

/// <summary>
/// Simple gate logic. Disables blocker visuals/collider when opened.
/// </summary>
public class PassageGate : MonoBehaviour
{
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private GameObject blockingVisual;

    public void OpenGate()
    {
        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }

        if (blockingVisual != null)
        {
            blockingVisual.SetActive(false);
        }
    }
}
