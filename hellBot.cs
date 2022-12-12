using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hellBot : enemyParent
{


    [Header("Shoot")]
    [Space]
    [SerializeField] private GameObject laser;
    [SerializeField] private Transform laserLocation;
    [SerializeField] private float shootRange;//Range to be within target to consider shooting
    [SerializeField] private float shootDamage;
    [SerializeField] private List<AudioSource> shootSounds;
    [SerializeField] private List<AudioSource> shootHits;

    private float shootTime = 0.5f;
    private float shootDelay = 0.2f;

    [SerializeField] private float shootCD;//Minimum time between end of one shoot and start of another
    private float shootCooldown;//current cd value

    [Header("Melee")]
    [Space]
    [SerializeField] private GameObject meleeAttack;
    [SerializeField] private Transform meleeLocation;
    [SerializeField] private float meleeRange;
    [SerializeField] private float meleeDamage;
    [SerializeField] private List<AudioSource> meleeSounds;
    [SerializeField] private List<AudioSource> meleeHits;

    private float meleeTime = 0.54f;
    private float meleeDelay = 0.3f;

    [SerializeField] private float meleeCD;//minimum time between end of one melee and start of another
    private float meleeCooldown;//current cd value



    private void Start()
    {
        setUp();
        move.setRangeX(shootRange);
    }

    

    private void FixedUpdate()
    {
        if (move.getBehavior() == "chase" && attackCD < 0 && !health.getDead() && !move.waiting())
        {
            if (distanceToTargetX() < meleeRange && distanceToTargetY() < 0.8f && meleeCooldown < 0)
            {
                move.setRangeX(shootRange);
                StartCoroutine(Melee());
            }
            else if (distanceToTargetX() < shootRange && distanceToTargetY() < 0.5f && shootCooldown < 0)
            {
                StartCoroutine(Shoot());
            }
        }
        attackCD -= Time.fixedDeltaTime;
        meleeCooldown -= Time.fixedDeltaTime;
        shootCooldown -= Time.fixedDeltaTime;
       
    }

        IEnumerator Shoot()
        {

            facePlayer();
            move.pauseOn();
            attackCD = shootTime + shootDelay + 0.2f;
            yield return new WaitForSeconds(shootDelay);

            anim.SetBool("Shooting", true);
            


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
                instance.setPlayerHit(shootHits[Random.Range(0, shootHits.Count)]);
                instance.damage = shootDamage;
                instance.left = !move.getFacingRight();
                shootSounds[Random.Range(0, shootSounds.Count)].Play();
            }
            yield return new WaitForSeconds(shootTime);
            shootCooldown = shootCD;
            attackCD = 0.2f;
            anim.SetBool("Shooting", false);
            move.pauseOff();
        }

        IEnumerator Melee()
        {
            facePlayer();
            attackCD = meleeTime + 0.1f;
            anim.SetBool("Melee", true);
            move.pauseOn();
            yield return new WaitForSeconds(meleeDelay);
            if (!health.getDead())
            {
                GameObject attack = Instantiate(meleeAttack, meleeLocation.position, meleeLocation.rotation);
                Attack instance = attack.GetComponent<Attack>();
                instance.setPlayerHit(meleeHits[Random.Range(0, meleeHits.Count)]);
                instance.damage = meleeDamage;
                instance.left = !move.getFacingRight();
                meleeSounds[Random.Range(0, meleeSounds.Count)].Play();
            }
            yield return new WaitForSeconds(meleeTime - meleeDelay);
            meleeCooldown = meleeCD;
            anim.SetBool("Melee", false);
            move.pauseOff();
        }

    }

