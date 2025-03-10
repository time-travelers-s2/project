using System.Collections;
using UnityEngine;

/*
This code was made using these tutorials:
* Movement:
    Dave / GameDevelopment - (THIRD PERSON MOVEMENT in 11 MINUTES - Unity Tutorial) https://www.youtube.com/watch?v=UCwwn2q4Vys
    Dave / GameDevelopment - (FIRST PERSON MOVEMENT in 10 MINUTES - Unity Tutorial) https://www.youtube.com/watch?v=f473C43s8nE
    Dave / GameDevelopment - (SLOPE MOVEMENT, SPRINTING & CROUCHING - Unity Tutorial) https://www.youtube.com/watch?v=xCxSjgYTw9c
    Dave / GameDevelopment - (ADVANCED 3D DASH ABILITY in 11 MINUTES - Unity Tutorial) https://www.youtube.com/watch?v=QRYGrCWumFw
    Plai - (Rigidbody FPS Controller Tutorial #3 | Handling Slopes + FIXES!) https://www.youtube.com/watch?v=cTIAhwlvW9M
* Animations:
    Small Hedge Games - (Don't use the Unity Animator, Use Code Instead! - Tutorial) https://www.youtube.com/watch?v=I3_i-x9nCjs
* Health:
    Brackeys - (MELEE COMBAT in Unity) https://www.youtube.com/watch?v=sPiVz1k-fEs

and some magic by Beranger
*/
public class knightController : MonoBehaviour, Character
{
    [Header("Health")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    private int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            healthBar.setHealth(currentHealth);
            if (currentHealth <= 0) isDead();
        }
    }

    [SerializeField] private int defaultDamage = 10;


    [Header ("Movement")]
    [SerializeField] private float walkSpeed;
    [Tooltip("Maximum player running speed")]
    [SerializeField] private float sprintSpeed;
    [Tooltip("Maximum player rolling speed")]
    private float moveSpeed;
    [Tooltip("Maximum player walking speed")]
    [SerializeField] private float rollSpeed;

    [SerializeField] private float groundDrag;
    public bool canMove;

    [Header ("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private bool canJump;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode rollKey = KeyCode.LeftAlt;

    [Header("Ground check")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float groundDistance;
    public bool grounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitSlope;

    [Header("Rolling")]
    [SerializeField] private float dodgeForce;
    [SerializeField] private float dodgeDuration;
    [SerializeField] private float dodgeCoolDown;
    private float dodgeTimer;
    private bool dodging;


    [Header("Other")]
    [SerializeField] private Transform orientation;
    private float horizontalInput;
    private float verticalInput;
    [SerializeField] private knightCamController kcc;
    [SerializeField] private HealthBarController healthBar;
    
    // Other
    private Vector3 moveDirection;
    private Rigidbody rb;


    //Animations
    private Animator animator;
    private string currentAnimation;

    private readonly string Idle = "Idle";
    private readonly string MoveForward = "Running_A";
    private readonly string MoveBackWard = "Walking_Backwards";
    private readonly string MoveLeft = "Running_Strafe_Left";
    private readonly string MoveRight = "Running_Strafe_Right";
    private readonly string JumpStart = "Jump_Start";
    private readonly string JumpAir = "Jump_Idle";
    private readonly string JumpEnd = "Jump_Land";
    public readonly string Attack1 = "1H_Melee_Attack_Slice_Diagonal";
    private readonly string DodgeLeft = "Dodge_Left";
    private readonly string DodgeRight = "Dodge_Right";
    private readonly string DodgeForward = "Dodge_Forward";
    private readonly string DodgeBackward = "Dodge_Backward";
    private readonly string Death = "Death_A";
    
    
    private MovementState state;
    private enum MovementState
    {
        Walking,
        Sprinting,
        Rolling,
        Air
    }

    private void Start()
    {
        CurrentHealth = maxHealth;
        healthBar.setup(maxHealth, CurrentHealth);

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        animator = GetComponent<Animator>();

        canMove = true;
        canJump = true;
        dodging = false;
        Debug.Log("Script is running");
        ChangeAnimation(Idle);
    }

    private void FixedUpdate()
    {
        grounded = Physics.CheckSphere(transform.position, groundDistance, whatIsGround);
        Debug.Log(grounded);
    }


    private void DebugCode()
    {
        //Debug.Log($"On Slope: {onSlope()}, Slope Angle: {Vector3.Angle(Vector3.up, slopeHit.normal)}");
        //Debug.Log($"Force Applied: {moveDirection * moveSpeed}, Slope Force: {GetSlopeMoveDirection() * moveSpeed}");
        Debug.DrawLine(transform.position + Vector3.up, transform.position + orientation.forward.normalized , Color.blue);
    }
    private void OnDrawGizmosSelected() 
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up * 0.6f + orientation.forward.normalized * 1.5f);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(transform.position + Vector3.up * 0.6f, orientation.forward + Vector3.up*0.5f);
    }

    private void Update()
    {
        //DebugCode();
        MyInput();
        SpeedCOntrol();
        StateHandler();
        
        if(canMove && !dodging) MovePlayer();
        CheckAnimation();
        UpdateTimers();
        if(Input.GetKey(KeyCode.Return))
            TakeDamage();
    }

    private void UpdateTimers()
    {
        if(dodgeTimer > 0)
            dodgeTimer -= Time.deltaTime;
    }

    
    private void MyInput()
    {
        if(canMove)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
            if(grounded && Input.GetKey(jumpKey) && canJump)
            {
                canJump = false;
                ChangeAnimation(JumpStart);
                Jump();
            }

            if(grounded && Input.GetKey(rollKey) && !dodging)
            {
                Roll();
            }
            
        }else
        {
            horizontalInput = 0;
            verticalInput = 0;
        }
        
    }

    private void StateHandler()
{
    if (grounded)
    {
        state = dodging ? MovementState.Rolling :
                Input.GetKey(sprintKey) ? MovementState.Sprinting :
                MovementState.Walking;

        moveSpeed = state switch
        {
            MovementState.Rolling => rollSpeed,
            MovementState.Sprinting => sprintSpeed,
            _ => walkSpeed
        };
    }
    else
    {
        state = MovementState.Air;
    }
}


    ////////////////////////
    // Movement
    private void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // Adjust movement speed based on the state
        float targetSpeed = moveSpeed;

        if (onSlope() && grounded)
        {
            // Move along the slope without excessive sliding
            Vector3 slopeDirection = GetSlopeMoveDirection().normalized;
            rb.linearVelocity = new Vector3(slopeDirection.x * targetSpeed, rb.linearVelocity.y, slopeDirection.z * targetSpeed);
        }
        else if (grounded)
        {
            // Standard ground movement
            Vector3 newVelocity = moveDirection * targetSpeed;
            newVelocity.y = rb.linearVelocity.y; // Maintain Y velocity (gravity)
            rb.linearVelocity = newVelocity;
        }
        else
        {
            // Air movement with reduced control
            Vector3 newVelocity = moveDirection * targetSpeed * airMultiplier;
            newVelocity.y = rb.linearVelocity.y; // Maintain Y velocity (gravity)
            rb.linearVelocity = newVelocity;
        }

        // Apply linear damping to control sliding
        rb.linearDamping = grounded ? groundDrag : 0;
    }




    private void SpeedCOntrol()
    {
        if(onSlope() && !exitSlope)
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            
        }
        else
        {
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if(verticalInput == -1 && flatVelocity.magnitude > moveSpeed * 0.5f)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed * 0.5f;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }
            else if (flatVelocity.sqrMagnitude > moveSpeed * moveSpeed)
            {
                rb.linearVelocity = new Vector3(flatVelocity.normalized.x * moveSpeed, rb.linearVelocity.y, flatVelocity.normalized.z * moveSpeed);
            }

        }
    }

    ////////////////////////
    // Jump
    private void Jump()
    {
        exitSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        StartCoroutine(ResetJump());
    }

    private IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(jumpCooldown);
        exitSlope = false;
        canJump = true;
    }

    ////////////////////////
    // Slope
    private bool onSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, groundDistance, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle > 0 && angle <= maxSlopeAngle;
        }
        return false;
    }



    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }
    
    
    ////////////////////////
    // Roll
    private void Roll()
    {
        if (dodgeTimer > 0) return; 
        dodgeTimer = dodgeCoolDown;
        dodging = true;
        
        RollAnimation();
        rb.AddForce(moveDirection * dodgeForce, ForceMode.Impulse);
        
        StartCoroutine(ResetRoll());
    }

    private IEnumerator ResetRoll()
    {
        yield return new WaitForSeconds(dodgeDuration);
        dodging = false;
    }


    private void RollAnimation()
    {
        if(verticalInput == 1)
            ChangeAnimation(DodgeForward);
        else if(verticalInput == -1)
            ChangeAnimation(DodgeBackward);
        else if(horizontalInput == 1)
            ChangeAnimation(DodgeRight);
        else if(horizontalInput == -1)
            ChangeAnimation(DodgeLeft);
    }

    ////////////////////////
    // Animations
    private void CheckAnimation()
{
    if (currentAnimation == JumpStart || currentAnimation == JumpEnd || 
        currentAnimation == Attack1 || currentAnimation == Death || dodging)
        return;

    if (currentAnimation == JumpAir && grounded)
    {
        ChangeAnimation(JumpEnd);
        return;
    }

    string newAnimation = verticalInput switch
    {
        1 => MoveForward,
        -1 => MoveBackWard,
        _ => horizontalInput switch
        {
            1 => MoveRight,
            -1 => MoveLeft,
            _ => Idle
        }
    };

    ChangeAnimation(newAnimation);
}


    public void ChangeAnimation(string animation, float crossfade = 0.2f, float time = 0f)
    {
        if(time > 0 )
            StartCoroutine(Wait());
        else
            Validate();
        
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(time - crossfade);
            Validate();
        }


        void Validate()
        {
            if(currentAnimation != animation)
            {
                currentAnimation = animation;
                if(currentAnimation == "")
                    CheckAnimation();
                else
                    animator.CrossFade(animation, crossfade);
            }
        }
        
    }

    ////////////////////////
    //Health
    public void increaseMaxHealth(int diff)
    {
        maxHealth += diff;
        CurrentHealth += diff;
        healthBar.setMaxHealth(maxHealth);
        healthBar.setHealth(CurrentHealth);
    }

    public void TakeDamage()
    {
        TakeDamage(defaultDamage);
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        healthBar.setHealth(CurrentHealth);
    }

    public void isDead()
    {
        canMove = false;
        ChangeAnimation(Death);
        kcc.isDead = true;
    }
}

