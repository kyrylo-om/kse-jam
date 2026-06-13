using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;

    [Header("Positioning Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 11f, -13f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Rotation Settings")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(35f, 0f, 0f);

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Set the camera's initial rotation
        transform.rotation = Quaternion.Euler(fixedRotation);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Target position based on player and world-relative offset
        Vector3 targetPosition = target.position + offset;

        // Smoothly move the camera
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // Keep rotation locked to the specified fixed angle
        transform.rotation = Quaternion.Euler(fixedRotation);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}