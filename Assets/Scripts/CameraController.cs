using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Target player to follow")]
    public Transform target;

    [Tooltip("Offset distance from the target")]
    public Vector3 offset = new Vector3(-6f, 10f, -6f);
    public Vector3 cameraLookYOffset = new(0, 1f, 0);

    [Tooltip("Smoothing factor")]
    public float smoothTime = 1f;

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Align camera to look at the target based on the offset
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = Quaternion.LookRotation(target.position - transform.position + cameraLookYOffset);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate destination
        Vector3 targetPosition = target.position + offset;

        // Smoothly interpolate position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
