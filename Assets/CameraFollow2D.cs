using UnityEngine;

/// <summary>
/// Smoothly follows the player inside level bounds.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private float followSpeed = 6f;

    private Transform target;
    private Rect bounds;
    private bool hasBounds;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
    }

    public void SetBounds(Rect area)
    {
        bounds = area;
        hasBounds = true;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = new Vector3(target.position.x, target.position.y, -10f);
        Vector3 next = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSpeed);

        if (hasBounds && Camera.main != null)
        {
            float halfHeight = Camera.main.orthographicSize;
            float halfWidth = halfHeight * Camera.main.aspect;

            float minX = bounds.xMin + halfWidth;
            float maxX = bounds.xMax - halfWidth;
            float minY = bounds.yMin + halfHeight;
            float maxY = bounds.yMax - halfHeight;

            // If camera viewport is bigger than map bounds, center it safely.
            if (minX > maxX)
            {
                next.x = (bounds.xMin + bounds.xMax) * 0.5f;
            }
            else
            {
                next.x = Mathf.Clamp(next.x, minX, maxX);
            }

            if (minY > maxY)
            {
                next.y = (bounds.yMin + bounds.yMax) * 0.5f;
            }
            else
            {
                next.y = Mathf.Clamp(next.y, minY, maxY);
            }
        }

        transform.position = next;
    }
}
