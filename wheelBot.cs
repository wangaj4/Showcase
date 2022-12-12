using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wheelBot : enemyParent
{
   


    

    [Header("Shoot")]
    [Space]
    [SerializeField] private GameObject laser;
    [SerializeField] private Transform laserLocation;
    [SerializeField] private float shootRange;//Range to be within target to consider shooting
    [SerializeField] private float shootDamage;
    [SerializeField] private List<AudioSource> shootCharges;
    [SerializeField] private List<AudioSource> shootSounds;
    [SerializeField] private List<AudioSource> shootHits;
    
    private float shootTime = 0.8f;
    private float shootDelay = 0.4f;

    [SerializeField] private float shootCD;//Minimum time between end of one shoot and start of another
    private float shootCooldown;//current cd value

    [Header("Dash")]
    [Space]
    [SerializeField] private GameObject dashAttack;
    [SerializeField] private Transform dashLocation;
    [SerializeField] private float dashRange;//Range to be within target to consider dashing
    [SerializeField] private float dashDamage;
    [SerializeField] private Transform dashTarget;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private List<AudioSource> dashSounds;
    [SerializeField] private List<AudioSource> dashHits;
    private float dashLength;

    private float dashTime = 0.7f;
    private float dashDelay = 0.1f;

    [SerializeField] private float dashCD;//Minimum time between end of one dash and start of another
    private float dashCooldown;//current cd value

    private void Start()
    {
        setUp();
        move.setRangeX(shootRange);
        dashLength = Mathf.Abs(transform.position.x - dashTarget.position.x);
    }

    

    private void FixedUpdate()
    {
        if (move.getBehavior() == "chase" && attackCD < 0 && !move.waiting() && !health.getDead())
        {
            if (distanceToTargetX() < dashRange && distanceToTargetY() < 0.6f && dashCooldown < 0)
            {
                StartCoroutine(Dash());
            }
            else if (distanceToTargetX() < shootRange && distanceToTargetY() < 0.7f && shootCooldown < 0)
            {
                StartCoroutine(Shoot());
            }
        }

        dashCooldown -= Time.fixedDeltaTime;
        shootCooldown -= Time.fixedDeltaTime;
        attackCD -= Time.fixedDeltaTime;

        if (anim.GetBool("Damaged")) anim.SetBool("Dashing", false);
    }

    IEnumerator Shoot()
    {

        facePlayer();

        attackCD = shootTime + 0.2f;
        anim.SetBool("Shooting", true);
        move.pauseOn();
        shootCharges[Random.Range(0, shootCharges.Count)].Play();
        yield return new WaitForSeconds(shootDelay);

        if (!health.getDead())
        {
            GameObject attack = Instantiate(laser, laserLocation.position, laserLocation.rotation);
            if (!move.getFacingRight())
            {
                Vector3 t = attack.transform.localScale;
                t.x *= -1;
                attack.transform.localScale = t;
            }
            Attack instance = attack.GetComponent<Attack>();
            instance.damage = shootDamage;
            instance.left = !move.getFacingRight();
            shootSounds[Random.Range(0, shootSounds.Count)].Play();
        }
        yield return new WaitForSeconds(shootTime - shootDelay);
        shootCooldown = shootCD;
        attackCD = 0.2f;
        anim.SetBool("Shooting", false);
        move.pauseOff();
    }

    IEnumerator Dash()
    {
        attackCD = dashTime+0.1f;
        facePlayer();
        
        move.pauseOn();
        yield return new WaitForSeconds(dashDelay);
        if (move.getWait()<0)
        {
            
            if (!health.getDead())
            {
                anim.SetBool("Dashing", true);
                Vector2 forward = new Vector2(-1, 0);
                if (move.getFacingRight())
                {
                    forward = new Vector2(1, 0);
                }
                RaycastHit2D hit = Physics2D.Raycast(transform.position, forward, dashLength, whatIsWall);


                if (hit.collider != null) transform.position = hit.point;
                else transform.position = dashTarget.position;

                GameObject attack = Instantiate(dashAttack, dashLocation.position, dashLocation.rotation);
                Attack instance = attack.GetComponent<Attack>();
                instance.damage = dashDamage;

                dashSounds[Random.Range(0, dashSounds.Count)].Play();

            }
            yield return new WaitForSeconds(dashTime - dashDelay);
            dashCooldown = dashCD;
            anim.SetBool("Dashing", false);
            move.pauseOff();
            facePlayer();
        }
        else
        {
            attackCD = 0;
            move.pauseOff();
        }
        
    }

    
}
