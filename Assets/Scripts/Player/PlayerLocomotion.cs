using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerLocomotion : MonoBehaviour
{
    private const float GroundedStickForce = -2f;

    [LabelText("奔跑按键")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [LabelText("行走速度")]
    [SerializeField] private float walkSpeed = 3.5f;

    [LabelText("奔跑速度")]
    [SerializeField] private float runSpeed = 6.5f;

    [LabelText("重力")]
    [SerializeField] private float gravity = -24f;

    private CharacterController characterController;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnValidate()
    {
        walkSpeed = Mathf.Max(0f, walkSpeed);
        runSpeed = Mathf.Max(walkSpeed, runSpeed);
        gravity = Mathf.Approximately(gravity, 0f) ? -24f : -Mathf.Abs(gravity);
    }

    public Vector3 ReadMoveInput()
    {
        Vector2 axes = RuntimeInput.GetMoveAxesRaw();
        return Vector3.ClampMagnitude(new Vector3(axes.x, 0f, axes.y), 1f);
    }

    public void Tick(Vector3 moveDirection, PlayerThirdPersonView thirdPersonView = null)
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        bool isRunning = runKey != KeyCode.None && RuntimeInput.GetKey(runKey);
        float speed = isRunning ? runSpeed : walkSpeed;

        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = GroundedStickForce;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move((moveDirection * speed + verticalVelocity) * Time.deltaTime);

        if (thirdPersonView == null)
        {
            return;
        }

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;
        thirdPersonView.SetMovementState(
            moveDirection,
            horizontalVelocity.magnitude,
            isRunning,
            characterController.isGrounded);
    }

    public void ResetVerticalVelocity()
    {
        verticalVelocity = Vector3.zero;
    }
}
