using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetPivot;

    [Header("Distance")]
    [SerializeField] private float distance = 4.5f;
    [SerializeField] private float sideOffset = 0.0f;
    [SerializeField] private float heightOffset = 0.0f;

    [Header("Rotation")]
    [SerializeField] private float sensitivity = 180f; // deg/sec
    [SerializeField] private float pitchMin = -35f;
    [SerializeField] private float pitchMax = 70f;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference lookAction;

    [Header("Collision (optional)")]
    [SerializeField] private bool useCollision = true;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.25f;

    private float yaw;
    private float pitch;

    private void Awake()
    {
        if (targetPivot == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) targetPivot = p.transform;
        }

        Vector3 e = transform.rotation.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }

    private void OnEnable()
    {
        lookAction?.action?.Enable();
    }

    private void OnDisable()
    {
        lookAction?.action?.Disable();
    }

    private void LateUpdate()
    {
        if (targetPivot == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        Vector2 look = Vector2.zero;
        if (lookAction != null && lookAction.action != null)
        {
            look = lookAction.action.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            look = Mouse.current.delta.ReadValue();
        }
        float mx = look.x;
        float my = look.y;

        yaw += mx * sensitivity * Time.deltaTime;
        pitch -= my * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 pivotPos = targetPivot.position + new Vector3(0f, heightOffset, 0f);

        Vector3 localOffset = new Vector3(sideOffset, 0f, -distance);
        Vector3 desiredPos = pivotPos + rot * localOffset;

        if (useCollision && collisionMask.value != 0)
        {
            Vector3 dir = desiredPos - pivotPos;
            float len = dir.magnitude;

            if (len > 0.001f)
            {
                dir /= len;
                if (Physics.SphereCast(pivotPos, collisionRadius, dir, out RaycastHit hit, len, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    desiredPos = pivotPos + dir * Mathf.Max(0.2f, hit.distance);
                }
            }
        }

        transform.position = desiredPos;
        transform.rotation = rot;
    }
}
