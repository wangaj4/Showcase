using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyMovement : MonoBehaviour
{

    private Animator anim;
    private Rigidbody2D rb;
    [SerializeField] private bool meleeOnly;

    [Header("Movement")]
    [Space]
    [SerializeField] private bool facingRight;
    private float horizontalMove;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float chaseSpeed;

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


    [Header("Behavior")]
    [Space]
    [SerializeField] private float timeToWait; //time to wait when getting to patrol point
    private float waitTime;
    private bool paused;
    [SerializeField] private string behavior;

    [SerializeField] private Transform patrol1;
    [SerializeField] private Transform patrol2;
    [SerializeField] private Transform currentTarget;
    private Transform player;
    private playerMaster master;
    private bladePlayer assassin;

    private float rangeX;//Range before stopping

    [Header("Detection")]
    [Space]
    [SerializeField] private float detectionRadius;
    [SerializeField] private AudioSource alertSound;
    [SerializeField] private visionCone visionCone;

    [SerializeField] private float timeToLoseSight;//time to go without vision to swap back from chase to patrol
    private float attention;
    [SerializeField] private LayerMask whatIsVisible;

    private enemyHealth health;

    //patrol, chase
    public void setRangeX(float x)
    {
        rangeX = x;
    }

    public string getBehavior()
    {
        return behavior;
    }

    public void setWait(float amount)
    {
        waitTime = amount;
        rb.velocity = Vector2.zero;
    }

    public float getWait()
    {
        return waitTime;
    }

    public void pauseOn()
    {
        paused = true;
    }

    public void pauseOff()
    {
        paused = false;
    }

   
    private void Start()
    {
        master = GameObject.Find("Players").GetComponent<playerMaster>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<enemyHealth>();
        if(behavior=="patrol") currentTarget = patrol1;
        if (behavior == "afk")
        {
            anim.SetBool("Idle", true);
            GetComponent<enemyHealth>().hpbar.toggleParent2(false);
        }
        else anim.SetBool("Idle", false);

        player = master.playerTransform;

        assassin = master.sinPlayer.GetComponent<bladePlayer>();

        currentTarget = patrol1;
        attention = timeToLoseSight;
        
        
    }

    private void Awake()
    {
        master = GameObject.Find("Players").GetComponent<playerMaster>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<enemyHealth>();
        if (behavior == "patrol") currentTarget = patrol1;
        if (behavior == "afk")
        {
            anim.SetBool("Idle", true);
            GetComponent<enemyHealth>().hpbar.toggleParent2(false);
        }
        else anim.SetBool("Idle", false);

        player = master.playerTransform;

        assassin = master.sinPlayer.GetComponent<bladePlayer>();

        currentTarget = patrol1;
        attention = timeToLoseSight;


    }

    private void FixedUpdate()
    {
        if (!health.getDead())
        {

            if (canJump) checkGround();
            checkBehavior();
            Move();
            if (canJump && curjumpCD < 0) checkJump();
            if (usesSteps) Footstep();
            waitTime -= Time.fixedDeltaTime;
            curjumpCD -= Time.fixedDeltaTime;
        }
    }

    private void checkBehavior()
    {
        //converts behavior to active target
        if (behavior == "patrol")
        {
            if (Mathf.Abs(transform.position.x - currentTarget.position.x) < 0.3)
            {

                if (currentTarget == patrol1)
                {
                    currentTarget = patrol2;
                }
                else
                {
                    currentTarget = patrol1;
                }

                waitTime = timeToWait;

            }
            //Check if player is detected
            //If yes, setChase()
            detect();
            
        


        }
        else if(behavior == "chase")
        {
            currentTarget = player;
            //If out of vision for a while, go back to patrolling
            if (!detect())
            {
                attention -= Time.fixedDeltaTime;
                if (attention < 0)
                {
                    waitTime = 1;
                    setPatrol();
                }
                
                
            }
            else attention = timeToLoseSight;

        }
        else if (behavior == "afk")
        {
            detect();
        }
        
    }

    private void checkJump()
    {
        if (Mathf.Abs(currentTarget.position.x - transform.position.x) < 0.3f) return;
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

    private bool detect()
    {
        if (Vector2.Distance(transform.position, player.position) < detectionRadius)
        {
            setChase();
            return true;
        }
        if (master.getSin())
        {
            if (assassin.getStealth()) return false;
        }

        if(visionCone != null)
        {
            if (visionCone.seePlayer)
            {
                //Raycast towards player, if nothing in between, setchase
                Vector3 direction = player.transform.position - transform.position;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 30, whatIsVisible);
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Attack"))
                    {

                        setChase();
                        return true;
                    }
                }

            }
            if (visionCone.seeAttack)
            {
                setChase();
                return true;
            }
        }
        
        return false;
    }

    public void setPatrol()
    {
        anim.SetBool("Idle", false);
        behavior = "patrol";
        currentTarget = patrol1;
    }
    
    public void setChase()
    {
        
        if (behavior != "chase")
        {
            GetComponent<enemyHealth>().hpbar.toggleParent2(true);
            anim.SetBool("Idle", false);
            behavior = "chase";
            alertSound.Play();
            waitTime = 0.5f;
            if (player.position.x < transform.position.x && facingRight) Flip();
            if (player.position.x > transform.position.x && !facingRight) Flip();
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
                    if(land!=null) land.Play();
                    stepTimer = timeBetweenSteps;
                }
                grounded = true;
                anim.SetBool("Grounded", true);
                return;

            }

        }
        grounded = false;
    }


    private void Move()
    {
        
        horizontalMove = 0;
        
        anim.SetBool("Moving", false);
        if (behavior == "afk")
        {
            return;
        }
        if (waitTime > 0)
        {
            
            return;
        }

        if (paused)
        {
            rb.velocity = new Vector2(0,rb.velocity.y);
            return;
        }
        

        //Flip in direction of target

        if (currentTarget.position.x < transform.position.x)
        {
            horizontalMove = -1;
            if (facingRight) Flip();
        }
        else
        {
            horizontalMove = 1;
            if (!facingRight) Flip();
        }

        //if grounded and not above player

        //If x distance is slightly less than rangex stop moving
        float offset = 0.2f;
        if (meleeOnly) offset = 0;
        if (Mathf.Abs(currentTarget.position.x - transform.position.x) < rangeX - offset && behavior == "chase")
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
            if (rb.velocity.x == 0)
            {
            //If entity is at halt then don't move until out of rangex
            if (Mathf.Abs(currentTarget.position.x - transform.position.x) < rangeX && behavior == "chase")
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                return;
            }
                stepTimer = timeBetweenSteps;
            }
        
        //If enemy is above target keep moving unless directly above or under
        //if (Mathf.Abs(currentTarget.position.x - transform.position.x) < 0.05f && behavior == "chase") return;



        anim.SetBool("Moving", true);
        float speed = moveSpeed;
        if (behavior == "chase") speed = chaseSpeed;

        //Move towards target
        Vector2 rawMovement = new Vector2(horizontalMove * speed * Time.fixedDeltaTime, rb.velocity.y);
        rb.velocity = rawMovement;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(forwardCheck.position, 0.1f, whatIsWall);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                //If wall is in the way of path, jump
                anim.SetBool("Moving", false);
                stepTimer = 0;
                return;
            }

        }
    }

    private void Footstep()
    {
        if (paused || !grounded) return;
        //Play proper footstep
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


    public void Flip()
    {
        facingRight = !facingRight;
        Vector2 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void flipFaceRight()
    {
        facingRight = !facingRight;
    }

    public bool waiting()
    {
        return waitTime > 0;
    }

    public bool getFacingRight()
    {
        return facingRight;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }
}
