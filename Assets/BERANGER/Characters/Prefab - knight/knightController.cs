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

and some (maybe a lot of) ChatGPT
and some magic by Beranger
*/
public class knightController : MonoBehaviour, Character
{
    [Header("Health")]
    public int maxHealth;
    public int currentHealth;
    public int defaultDamage = 10;


    [Header ("Movement")]
    public float walkSpeed;
    [Tooltip("Maximum player running speed")]
    public float sprintSpeed;
    [Tooltip("Maximum player rolling speed")]
    private float moveSpeed;
    [Tooltip("Maximum player walking speed")]
    public float rollSpeed;

    public float groundDrag;
    public bool canMove;

    [Header ("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool canJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode rollKey = KeyCode.LeftAlt;

    [Header("Ground check")]
    public LayerMask whatIsGround;
    public float groundDistance;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitSlope;
    private Vector3 slopeMoveDirection;

    [Header("Rolling")]
    public float dodgeForce;
    public float dodgeDuration;
    public float dodgeCoolDown;
    private float dodgeTimer;
    private bool dodging;


    [Header("Other")]
    public Transform orientation;
    private float horizontalInput;
    private float verticalInput;
    public knightCamController kcc;
    public HealthBarController healthBar;
    
    // Other
    private Vector3 moveDirection;
    private Rigidbody rb;


    //Animations
    private Animator animator;
    private string currentAnimation;

    readonly string Idle = "Idle";
    readonly string MoveForward = "Running_A";
    readonly string MoveBackWard = "Walking_Backwards";
    readonly string MoveLeft = "Running_Strafe_Left";
    readonly string MoveRight = "Running_Strafe_Right";
    readonly string JumpStart = "Jump_Start";
    readonly string JumpAir = "Jump_Idle";
    readonly string JumpEnd = "Jump_Land";
    public readonly string Attack1 = "1H_Melee_Attack_Slice_Diagonal";
    public readonly string DodgeLeft = "Dodge_Left";
    public readonly string DodgeRight = "Dodge_Right";
    public readonly string DodgeForward = "Dodge_Forward";
    public readonly string DodgeBackward = "Dodge_Backward";
    public readonly string Death = "Death_A";
    
    
    public MovementState state;
    public enum MovementState
    {
        Walking,
        Sprinting,
        Rolling,
        Air
    }

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.setMaxHealth(maxHealth);
        healthBar.setHealth(currentHealth);
        Debug.Log("knight health: "+currentHealth);

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        animator = GetComponent<Animator>();

        canMove = true;
        canJump = true;
        Debug.Log("Script is running");
        ChangeAnimation(Idle);
    }

    private void FixedUpdate() //Better for physics stuff
    {
        grounded = Physics.CheckSphere(transform.position, groundDistance, whatIsGround);
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
        
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
        if(state == MovementState.Walking || state == MovementState.Sprinting) rb.linearDamping = groundDrag;
        else rb.linearDamping = 0;
        
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
                Invoke(nameof(ResetJump), jumpCooldown);
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
        if(grounded && dodging)
        {
            state = MovementState.Rolling;
            moveSpeed = rollSpeed;
            
        }
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
        }
        else if(grounded)
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
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
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if(onSlope() && grounded) 
            rb.AddForce(GetSlopeMoveDirection().normalized * moveSpeed * 10f, ForceMode.Force);
        else if(grounded) 
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded) 
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !onSlope();
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
    public void Jump()
    {
        exitSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        exitSlope = false;
        canJump = true;
    }

    ////////////////////////
    // Slope
    private bool onSlope()
    {
        return Physics.Raycast(transform.position + Vector3.up, Vector3.down, out slopeHit, 0.6f) &&
            slopeHit.normal != Vector3.up &&
            Vector3.Angle(Vector3.up, slopeHit.normal) <= maxSlopeAngle;
    }


    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }
    
    
    ////////////////////////
    // Roll
    private void Roll()
    {
        if (dodgeTimer > 0) return; // Ensures no dodging if on cooldown
        dodgeTimer = dodgeCoolDown;

        dodging = true;
        RollAnimation();

        rb.AddForce(moveDirection * dodgeForce, ForceMode.Impulse);
        Invoke(nameof(resetRoll), dodgeDuration);
    }


    private void resetRoll()
    {
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
        if(currentAnimation == JumpStart || currentAnimation == JumpEnd || currentAnimation == Attack1 ||
        currentAnimation == DodgeForward || currentAnimation == DodgeBackward || currentAnimation == DodgeRight || currentAnimation == DodgeLeft ||
        currentAnimation == Death)
            return;

        if(currentAnimation == JumpAir && grounded)
        {
            ChangeAnimation(JumpEnd);
            return;
        }

        switch (verticalInput)
        {
            case 1: ChangeAnimation(MoveForward); break;
            case -1: ChangeAnimation(MoveBackWard); break;
            default:
                switch (horizontalInput)
                {
                    case 1: ChangeAnimation(MoveRight); break;
                    case -1: ChangeAnimation(MoveLeft); break;
                    default: ChangeAnimation(Idle); break;
                }
                break;
        }
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
        currentHealth += diff;
        healthBar.setMaxHealth(maxHealth);
        healthBar.setHealth(currentHealth);
    }

    public void TakeDamage()
    {
        TakeDamage(defaultDamage);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("knight taking "+damage+" damage");
        currentHealth -= damage;
        healthBar.setHealth(currentHealth);
        Debug.Log("knight health: "+currentHealth);
        if(currentHealth <= 0)
        {
            isDead();
        }
    }

    public void isDead()
    {
        canMove = false;
        ChangeAnimation(Death);
        kcc.isDead = true;
    }
}

