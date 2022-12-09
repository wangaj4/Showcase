using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossMovement : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    private Transform player;
    private playerMaster master;
    private bossHealth health;

    public float range;
    public bool paused;

    [Header("Movement")]
    [Space]
    [SerializeField] private bool facingRight;
    private float horizontalMove;
    [SerializeField] private float moveSpeed;

    [SerializeField] bool usesSteps;
    [SerializeField] float timeBetweenSteps;
    private float stepTimer;
    [SerializeField] private AudioSource leftStep;
    [SerializeField] private AudioSource rightStep;
    [SerializeField] private AudioSource land;
    [SerializeField] private AudioSource jumpSound;
    private bool steppingRight;

    [Header("Jump")]
    [Space]
    [SerializeField] private bool canJump;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float grounded_radius;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float jumpPower;

    [SerializeField] private Transform forwardCheck;
    //[SerializeField] private Transform forwardGroundCheck;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float jumpCD;
    private float curjumpCD;
    private bool grounded = true;


    public bool isGrounded()
    {
        return grounded;
    }

    public void pause()
    {
        
        paused = true;
    }

    public void resume()
    {
        checkFlip();
        paused = false;
    }

    public bool getFacingRight()
    {
        return facingRight;
    }
    void prep()
    {
        master = GameObject.Find("Players").GetComponent<playerMaster>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        player = master.playerTransform;
        health = GetComponent<bossHealth>();

    }
    void Start()
    {
        prep();
    }
    private void Awake()
    {
        prep();
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        //if alive
        if(!health.getDead())
        {
            if (canJump) checkGround();
            if (canJump && curjumpCD < 0) checkJump();
            Move();
            if (usesSteps) Footstep();
            curjumpCD -= Time.fixedDeltaTime;
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
        
    }

    private void checkGround()
    {
        //check ground to see if jump is available, set grounded variable

        anim.SetBool("Grounded", false);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, grounded_radius, whatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                if (!grounded)
                {
                    if (land != null) land.Play();
                    stepTimer = timeBetweenSteps;
                }
                grounded = true;
                anim.SetBool("Grounded", true);
                anim.SetBool("isJumping", false);
                return;

            }

        }
        grounded = false;
    }
    private void checkJump()
    {
        if (Mathf.Abs(player.position.x - transform.position.x) < range) return;
        //If right next to player, don't jump

        if (!grounded || paused) return;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(forwardCheck.position, 0.1f, whatIsWall);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                //If wall is in the way of path, jump
                Jump();

                return;
            }

        }



    }
    private void Jump()
    {
        float xForce = -jumpPower / 4;
        if (facingRight) xForce *= -1;
        rb.AddForce(new Vector2(xForce, jumpPower));
        anim.SetBool("isJumping", true);
        curjumpCD = jumpCD;
        jumpSound.Play();
    }
    private void Move()
    {
        if (!grounded)
        {
            Vector2 rawMovement = new Vector2(horizontalMove * moveSpeed * Time.fixedDeltaTime, rb.velocity.y);
            rb.velocity = rawMovement;
            return;
        }
        //move towards player always unless in range of an attack
        if (paused)
        {
            anim.SetBool("Moving", false);
            Vector2 rawMovement = new Vector2(0, rb.velocity.y);
            rb.velocity = rawMovement;
            return;
        }
        if (transform.position.y - player.position.y > 0.3f)
        {
            anim.SetBool("Moving", true);
            Vector2 rawMovement = new Vector2(horizontalMove * moveSpeed * Time.fixedDeltaTime, rb.velocity.y);
            rb.velocity = rawMovement;
            return;
        }
            
        checkFlip();
        
        if (Mathf.Abs(player.position.x - transform.position.x) > range)
        {
            anim.SetBool("Moving", true);
            Vector2 rawMovement = new Vector2(horizontalMove * moveSpeed * Time.fixedDeltaTime, rb.velocity.y);
            rb.velocity = rawMovement;
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            anim.SetBool("Moving", false);
        }
    }

    void checkFlip()
    {
        if (player.position.x < transform.position.x)
        {
            horizontalMove = -1;
            if (facingRight) Flip();
        }
        else
        {
            horizontalMove = 1;
            if (!facingRight) Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector2 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Footstep()
    {
        if (!grounded || !usesSteps) return;
        if (grounded) stepTimer -= Time.fixedDeltaTime;
        if (stepTimer <= 0 && anim.GetBool("Moving"))
        {
            if (steppingRight)
            {
                rightStep.Play();
                steppingRight = false;
            }
            else
            {
                leftStep.Play();
                steppingRight = true;
            }
            stepTimer = timeBetweenSteps;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }
}
