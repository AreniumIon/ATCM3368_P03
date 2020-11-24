using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] float jumpSpeed = 20f;
    [SerializeField] float fallSpeed = 10f;
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float runAcceleration = 10f;
    [SerializeField] float runSkid = 5f;
    [SerializeField] float airSpeed = 10f;
    [SerializeField] float airAcceleration = 4f;
    [SerializeField] float airSkid = 2f;
    [SerializeField] float gravityMultiplier = 1f;
    [SerializeField] private LayerMask ground;                          // A mask determining what is ground to the character
    [SerializeField] private Transform groundCheck;                           // A position marking where to check if the player is grounded.

    private float groundCheckRadius = .02f;
    private bool isGrounded = true;
    private bool isFacingRight = true;
    private Vector3 velocity = Vector3.zero;
    private Animator animator;

    // Player Input
    private float horizontalInput;
    private bool jumpInput;
    private bool crouchInput;
    private bool parryInput;

    [Header("Parry Variables")]
    private bool isParrying = false; // true if busy with the parry animation
    public bool canReflect = false; // true if can reflect projectiles
    [SerializeField] float parryTime = 0.5f;
    [SerializeField] float parryCooldown = 0.5f;

    private float parryTimer = 0f;

    [Header("Damage Variables")]
    private bool isDamaged = false; // true if busy with the damage
    [SerializeField] float damageTime = 2f;

    private float damageTimer = 0f;

    // Sounds
    [SerializeField] AudioSource parrySound;
    [SerializeField] AudioSource takeDamageSound;
    [SerializeField] AudioSource jumpSound;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Get player input
        horizontalInput = Input.GetAxis("Horizontal");
        jumpInput = Input.GetKeyDown(KeyCode.UpArrow);
        crouchInput = Input.GetKeyDown(KeyCode.DownArrow);
        parryInput = Input.GetKeyDown(KeyCode.LeftShift);

        // Update parry timer
        if (isParrying)
        {
            parryTimer += Time.deltaTime;

            // Disallow reflecting
            if (parryTimer >= parryTime)
                canReflect = false;

            // Remove parry
            if (parryTimer >= parryTime + parryCooldown)
                isParrying = false;
        }

        // Update damage timer
        if (isDamaged)
        {
            damageTimer += Time.deltaTime;

            // Detect end of damage animation
            if (damageTimer >= damageTime)
            {
                isDamaged = false;

                // Animation
                animator.SetInteger("State", 0);
            }
        }
    }

    private void FixedUpdate()
    {
        // Check if player is grounded
        if (isGrounded || velocity.y < 0)
        {
            isGrounded = false;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, ground);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    isGrounded = true;
                }
            }
        }
        
        // Parry if can
        if (isGrounded && parryInput && !isParrying && !isDamaged)
            Parry();
        // Calculate movement if not parrying/damaged
        else if (!isParrying && !isDamaged)
            Move();

        // Apply movement
        transform.position += velocity;
    }


    public void Move()
    {
        // Jumping
        if (jumpInput && !isParrying)
        {
            Jump();
            isGrounded = false;
        }

        // Ground or Air movement
        if (isGrounded)
        {
            GroundMove();
        }
        else
        {
            AirMove();
        }

    }

    public void GroundMove()
    {
        // Horizontal velocity
        if (horizontalInput != 0)
        {
            velocity.x += horizontalInput * runAcceleration;

            // Animation
            animator.SetInteger("State", 1);
        }
        else
        {
            // Skid
            if (velocity.x > 0)
            {
                velocity.x -= runSkid;
                velocity.x = Mathf.Clamp(velocity.x, 0f, runSpeed);
            }
            else
            {
                velocity.x += runSkid;
                velocity.x = Mathf.Clamp(velocity.x, -runSpeed, 0f);
            }

            // Animation
            if (velocity.x == 0)
                animator.SetInteger("State", 0);
        }
        velocity.x = Mathf.Clamp(velocity.x, -runSpeed, runSpeed);

        // Vertical velocity
        velocity.y = 0f;

        // Flip the player if needed
        if (horizontalInput > 0 && !isFacingRight)
            Flip();
        else if (horizontalInput < 0 && isFacingRight)
            Flip();
    }

    public void AirMove()
    {
        // Horizontal velocity
        if (horizontalInput != 0)
        {
            velocity.x += horizontalInput * airAcceleration;
        }
        else
        {
            // Skid
            if (velocity.x > 0)
            {
                velocity.x -= airSkid;
                velocity.x = Mathf.Clamp(velocity.x, 0f, airSpeed);
            }
            else
            {
                velocity.x += airSkid;
                velocity.x = Mathf.Clamp(velocity.x, -airSpeed, 0f);
            }
        }
        velocity.x = Mathf.Clamp(velocity.x, -airSpeed, airSpeed);

        //Gravity
        velocity.y += Physics.gravity.y * gravityMultiplier;

        // Vertical velocity
        velocity.y = Mathf.Clamp(velocity.y, -fallSpeed, jumpSpeed);

    }

    public void Jump()
    {
        // Set vertical velocity
        velocity.y = jumpSpeed;

        // Animation
        animator.SetInteger("State", 2);
        jumpSound.Play();
    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    // PARRY MECHANICS

    // Called by pressing shift
    private void Parry()
    {
        // Set parry variables
        isParrying = true;
        canReflect = true;
        parryTimer = 0f;

        // Stop velocity
        velocity = Vector3.zero;

        // Animation
        animator.SetInteger("State", 4);
        
        //Sound Effects
        parrySound.Play();
    }

    // Called by a projectile when reflected by the player. Speeds up the parry animation so the player can move again
    public void ActivateParry()
    {
        parryTimer = parryTime + parryCooldown;

        // Animation
        animator.SetInteger("State", 0);
    }

    public void TakeDamage()
    {
        isDamaged = true;
        damageTimer = 0f;
        
        // Stop velocity
        velocity = Vector3.zero;

        // Animation
        animator.SetInteger("State", 3);
        takeDamageSound.Play();
    }
}
