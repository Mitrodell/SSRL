using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Animator animator;

    [Header("Move")]
    [SerializeField] private float moveSpeedFallback = 6f;
    [SerializeField] private float rotationSpeed = 9999f;
    [SerializeField] private float gravity = -18f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Ground")]
    [SerializeField] private float groundStickForce = -2f;

    [Header("Animator params")]
    [SerializeField] private string animMoveX = "MoveX";
    [SerializeField] private string animMoveY = "MoveY";

    private CharacterController cc;
    private Rigidbody rb;
    private Vector3 verticalVel;
    private Vector3 lastMoveDirWorld;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        if (cameraRoot == null && Camera.main != null)
            cameraRoot = Camera.main.transform;

        if (stats == null)
            stats = GetComponent<PlayerStats>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;
    }

    private void OnEnable()
    {
        moveAction?.action?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action?.Disable();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        if (stats != null && stats.IsDead) return;
        if (cameraRoot == null) return;

        HandleMove();
        HandleLook();
        UpdateAnimator();
    }

    private void HandleMove()
    {
        Vector2 move = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        float x = move.x;
        float z = move.y;

        if (Mathf.Abs(x) < 0.01f) x = 0f;
        if (Mathf.Abs(z) < 0.01f) z = 0f;

        Vector3 input = new Vector3(x, 0f, z);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 camF = cameraRoot.forward; camF.y = 0f; camF.Normalize();
        Vector3 camR = cameraRoot.right;   camR.y = 0f; camR.Normalize();

        Vector3 moveDir = camF * input.z + camR * input.x;
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);

        if (moveDir.sqrMagnitude > 0.0001f) moveDir.Normalize();
        lastMoveDirWorld = moveDir;

        float speed = (stats != null) ? stats.MoveSpeed : moveSpeedFallback;
        Vector3 horizontal = moveDir * speed;

        if (cc.isGrounded)
        {
            if (verticalVel.y < 0f) verticalVel.y = groundStickForce;
        }
        else
        {
            verticalVel.y += gravity * Time.deltaTime;
        }

        Vector3 velocity = horizontal + verticalVel;
        cc.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        Vector3 camF = cameraRoot.forward;
        camF.y = 0f;
        if (camF.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(camF.normalized, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        Vector3 moveLocal = transform.InverseTransformDirection(lastMoveDirWorld);

        animator.SetFloat(animMoveX, moveLocal.x, 0.08f, Time.deltaTime);
        animator.SetFloat(animMoveY, moveLocal.z, 0.08f, Time.deltaTime);
    }
}
