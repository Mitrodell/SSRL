using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform cameraRoot; // Camera.main.transform
    [SerializeField] private PlayerStats stats;

    [Header("Move")]
    [SerializeField] private float moveSpeedFallback = 6f;
    [SerializeField] private float rotationSpeed = 9999f; // очень быстро, чтобы "прилипало" к камере
    [SerializeField] private float gravity = -18f;

    [Header("Ground")]
    [SerializeField] private float groundStickForce = -2f;

    private CharacterController cc;
    private Vector3 verticalVel;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (cameraRoot == null && Camera.main != null)
            cameraRoot = Camera.main.transform;

        if (stats == null)
            stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        if (stats != null && stats.IsDead) return;
        if (cameraRoot == null) return;

        // 1) Всегда поворачиваем игрока по направлению камеры (по Y)
        FaceCameraYaw();

        // 2) Движение WASD относительно камеры (strafe)
        HandleMove();
    }

    private void FaceCameraYaw()
    {
        Vector3 camForward = cameraRoot.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(camForward.normalized, Vector3.up);

        // Можно мгновенно:
        // transform.rotation = targetRot;

        // Или очень быстро и плавно:
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // небольшая dead-zone, чтобы не "ползло"
        if (Mathf.Abs(x) < 0.01f) x = 0f;
        if (Mathf.Abs(z) < 0.01f) z = 0f;

        Vector3 input = new Vector3(x, 0f, z);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 camF = cameraRoot.forward; camF.y = 0f; camF.Normalize();
        Vector3 camR = cameraRoot.right;   camR.y = 0f; camR.Normalize();

        Vector3 moveDir = camF * input.z + camR * input.x;
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);

        float speed = (stats != null) ? stats.MoveSpeed : moveSpeedFallback;
        Vector3 horizontal = moveDir * speed;

        // гравитация
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
}
