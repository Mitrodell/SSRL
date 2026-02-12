using UnityEngine;

[DisallowMultipleComponent]
public sealed class AnimatorDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;

    [SerializeField] private Transform characterRoot;

    [Header("Input")]

    public Vector2 MoveInput;

    [Header("Smoothing")]
    [SerializeField] private float moveDamp = 0.08f;
    [SerializeField] private float speedDamp = 0.10f;

    [Header("Motion settings")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float fallingVelThreshold = -0.5f;

    [Header("Animator Params")]
    private string pMoveX = "MoveX";
    private string pMoveY = "MoveY";
    private string pSpeed = "Speed";
    private string pGrounded = "IsGrounded";
    private string pFalling = "IsFalling";
    private string tJump = "Jump";
    private string tLand = "Land";

    private int hMoveX, hMoveY, hSpeed, hGrounded, hFalling, hJump, hLand;

    private Vector3 lastPos;
    private bool wasGrounded;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!characterController) characterController = GetComponentInParent<CharacterController>();
        if (!characterRoot) characterRoot = characterController ? characterController.transform : transform;

        hMoveX = Animator.StringToHash(pMoveX);
        hMoveY = Animator.StringToHash(pMoveY);
        hSpeed = Animator.StringToHash(pSpeed);
        hGrounded = Animator.StringToHash(pGrounded);
        hFalling = Animator.StringToHash(pFalling);
        hJump = Animator.StringToHash(tJump);
        hLand = Animator.StringToHash(tLand);

        if (animator) animator.applyRootMotion = false;

        lastPos = characterRoot.position;
        wasGrounded = characterController ? characterController.isGrounded : true;
    }

    private void Update()
    {
        if (!animator || !characterRoot)
            return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        bool grounded = characterController ? characterController.isGrounded : true;

        float verticalVel = (characterRoot.position.y - lastPos.y) / dt;
        bool falling = !grounded && verticalVel < fallingVelThreshold;

        animator.SetBool(hGrounded, grounded);
        animator.SetBool(hFalling, falling);

        if (!wasGrounded && grounded)
        {
            if (hLand != 0) animator.SetTrigger(hLand);
        }
        else if (wasGrounded && !grounded && verticalVel > 0.01f)
        {
            if (hJump != 0) animator.SetTrigger(hJump);
        }

        Vector3 worldVel = (characterRoot.position - lastPos) / dt;

        Vector3 horizontalVel = worldVel;
        horizontalVel.y = 0f;
        float horizSpeed = horizontalVel.magnitude;

        float speed01 = maxSpeed > 0.01f ? Mathf.Clamp01(horizSpeed / maxSpeed) : 0f;
        animator.SetFloat(hSpeed, speed01, speedDamp, dt);

        Vector2 moveXY;
        if (MoveInput.sqrMagnitude > 0.0001f)
        {
            moveXY = Vector2.ClampMagnitude(MoveInput, 1f);
        }
        else
        {
            Vector3 localDir = horizSpeed > 0.001f
                ? characterRoot.InverseTransformDirection(horizontalVel.normalized)
                : Vector3.zero;

            moveXY = new Vector2(localDir.x, localDir.z);
        }

        animator.SetFloat(hMoveX, moveXY.x, moveDamp, dt);
        animator.SetFloat(hMoveY, moveXY.y, moveDamp, dt);

        lastPos = characterRoot.position;
        wasGrounded = grounded;
    }
}
