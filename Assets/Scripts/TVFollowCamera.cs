using UnityEngine;

public class TVFollowCamera : MonoBehaviour
{
    [Header("Camera Target")]
    [Tooltip("The camera to follow. If null, automatically targets the Main Camera.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Follow Settings")]
    [Tooltip("Time taken to smooth the position tracking. Lower values mean faster follow.")]
    [SerializeField] private float positionSmoothTime = 0.3f;
    [Tooltip("Speed for smooth rotation tracking.")]
    [SerializeField] private float rotationSmoothSpeed = 3.0f;

    [Header("Behavior Options")]
    [Tooltip("If true, offsets are calculated automatically from current distance/rotation at Start. If false, offsets can be customized below.")]
    [SerializeField] private bool autoCalculateOffsets = true;

    [SerializeField] private Vector3 localPositionOffset;
    [SerializeField] private Quaternion localRotationOffset;

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Auto-assign camera if not set
        if (cameraTransform == null)
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Camera cam = FindAnyObjectByType<Camera>();
                if (cam != null)
                {
                    cameraTransform = cam.transform;
                }
                else
                {
                    Debug.LogError("[TVFollowCamera] No camera found in the scene to follow!");
                    return;
                }
            }
        }

        // Auto-calculate relative offsets at Start if desired
        if (autoCalculateOffsets)
        {
            localPositionOffset = cameraTransform.InverseTransformPoint(transform.position);
            localRotationOffset = Quaternion.Inverse(cameraTransform.rotation) * transform.rotation;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Calculate the target world position and rotation based on camera's current transform and saved relative offsets
        Vector3 targetPosition = cameraTransform.TransformPoint(localPositionOffset);
        Quaternion targetRotation = cameraTransform.rotation * localRotationOffset;

        // Smoothly move the TV to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);

        // Smoothly rotate the TV to the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
