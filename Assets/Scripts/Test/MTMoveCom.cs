using System;
using UnityEngine;

public class MTMoveCom : MonoBehaviour
{
    [SerializeField]internal CharacterController  characterController;
    [LabelText("奔跑按键")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [LabelText("行走速度")]
    [SerializeField] private float walkSpeed = 3.5f;

    [LabelText("奔跑速度")]
    [SerializeField] private float runSpeed = 6.5f;

    [LabelText("重力")]
    [SerializeField] private float gravity = -24f;
    
    private const float GroundedStickForce = -2f;
    
    private Vector3 verticalVelocity;

    private bool isRunning;
    private bool isIdle;
    public bool IsIdle => isIdle;
    public bool IsRunning  => isRunning;
    

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public Vector3 GetMoveInput()
    {
        Vector2 axes = RuntimeInput.GetMoveAxesRaw();
        Debug.Log(axes);
        return Vector3.ClampMagnitude(new Vector3(axes.x, 0, axes.y), 1f);
    }

    public void Tick()
    {
        var dir = GetMoveInput();
        dir = MTPlayerManager.Instance.GetTransformedDir(dir);
        Tick(dir);
    }

    public void Tick(Vector3 moveDir)
    {
        isRunning = runKey != KeyCode.None && RuntimeInput.GetKey(runKey);
        isIdle = moveDir.sqrMagnitude < 0.01f;
        float speed = isRunning ? runSpeed : walkSpeed;
        
        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = GroundedStickForce;
        }
        
        
        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move((moveDir * speed + verticalVelocity) * Time.deltaTime);
        
    }
    // Update is called once per frame
    void Update()
    {
        // Tick();
    }
}
