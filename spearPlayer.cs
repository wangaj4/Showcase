using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class spearPlayer : playerParent
{
    

    [SerializeField] private cineMachineScript cam;
    [SerializeField] private Camera mainCam;

    [Header("Slam")]
    [Space]
    [SerializeField] private int slamDamage;
    private float slamDuration = 0.6f;
    private float slamDelay = 0.24f;//Time before damage applies after starting the windup
    [SerializeField] private List<AudioSource> slams;
    private int slamIndex;


    [SerializeField] private List<AudioSource> electroCharges;
    private bool useFirstCharge;

    [SerializeField] private GameObject slamAttack;
    [SerializeField] private Transform slamLocation;

    [Header("Sweep")]
    [Space]
    [SerializeField] private int sweepDamage;
    private float sweepDuration = 1;
    private float sweepDashDelay = 0.3f;//Time before damage applies after starting the windup
    [SerializeField] private List<AudioSource> sweeps;
    private int sweepIndex;

    [SerializeField] private GameObject sweepAttack;
    [SerializeField] private Transform sweepLocation;

    [Header("Crush")]
    [Space]
    [SerializeField] private Slider ultBar;
    public float ultStatus;//Retain between scenes and save
    [SerializeField] private float ultRechargeRate;

    [SerializeField] private int crushDamage;
    private float crushDuration = 1.1f;
    private float crushdelay = 0.35f;//Time before damage applies after starting the windup

    [SerializeField] private GameObject crushAttack;
    [SerializeField] private Transform crushLocation;

    [SerializeField] private List<AudioSource> explosions;
    
    private int explosionIndex;
    [SerializeField] GameObject crushFX;

    [SerializeField] private GameObject indicator;
    

    private void Awake()
    {
        playerMovement = GetComponent<playerMove>();
        anim = GetComponent<Animator>();
        health = GetComponent<playerHealth>();
        ultBar.value = ultStatus;
    }
    private void Update()
    {
        if (!gamePaused && !health.getDead() && active)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!attacking) StartCoroutine(Slam());
            }

            if (Input.GetButtonDown("Fire2"))
            {
                if (!attacking) StartCoroutine(Sweep());
            }

            if (Input.GetButtonDown("Fire3"))
            {
                if (!attacking && playerMovement.getGrounded() && ultStatus == 1) StartCoroutine(Crush());
            }
        }
        
    }

    IEnumerator Slam()
    {
        checkFlip();
        StartCoroutine(Slamming());
        attacking = true;
        playerMovement.pauseOn();
        anim.SetBool("Slamming", true);
        //Alternate electro charging noise
        if (useFirstCharge)
        {
            useFirstCharge = false;
            electroCharges[0].Play();
        }
        else
        {
            electroCharges[1].Play();
            useFirstCharge = true;
        }

        yield return new WaitForSeconds(slamDuration);
        playerMovement.pauseOff();
        anim.SetBool("Slamming", false);
        attacking = false;

    }

    IEnumerator Slamming()
    {
        yield return new WaitForSeconds(slamDelay);
        //play sound after slam delay
        slams[slamIndex].Play();
        slamIndex += 1;
        if (slamIndex >= slams.Count) slamIndex = 0;

        yield return new WaitForSeconds(0.1f);
        //add another little delay then instantiate attack
        if (!health.getDead())
        {
            GameObject attack = Instantiate(slamAttack, slamLocation.position, slamLocation.rotation);
            Attack instance = attack.GetComponent<Attack>();
            instance.damage = slamDamage;
            instance.left = (transform.localScale.x < 0);

            cam.shakeCamera(1, 0.3f);
        }
        
    }

    IEnumerator Sweep()
    {
        checkFlip();

        StartCoroutine(Sweeping());
        attacking = true;
        playerMovement.pauseOn();
        anim.SetBool("Sweeping", true);

        electroCharges[2].Play();

        yield return new WaitForSeconds(sweepDuration);
        playerMovement.pauseOff();
        anim.SetBool("Sweeping", false);
        attacking = false;

    }

    IEnumerator Sweeping()
    {
        yield return new WaitForSeconds(sweepDashDelay);
        if (!health.getDead())
        {
            sweeps[sweepIndex].Play();
            sweepIndex += 1;
            if (sweepIndex >= sweeps.Count) sweepIndex = 0;

            GameObject attack = Instantiate(sweepAttack, sweepLocation.position, sweepLocation.rotation);
            Attack instance = attack.GetComponent<Attack>();
            instance.damage = sweepDamage;
            instance.left = (transform.localScale.x < 0);

            cam.shakeCamera(1, 0.3f);
        }
            

    }


    IEnumerator Crush()
    {
        checkFlip();

        attacking = true;
        playerMovement.pauseOn();
        anim.SetBool("Crushing", true);
        indicator.SetActive(true);
        
        electroCharges[2].Play();
        StartCoroutine(Crushing());

        yield return new WaitForSeconds(crushDuration);
        playerMovement.pauseOff();
        anim.SetBool("Crushing", false);
        crushFX.SetActive(false);
        attacking = false;
        indicator.SetActive(false);

    }

    IEnumerator Crushing()
    {
        ultStatus = 0;
        ultBar.value = ultStatus;
        yield return new WaitForSeconds(crushdelay);
        if (!health.getDead())
        {
            crushFX.SetActive(true);

            //Play different explosion noises
            explosions[explosionIndex].Play();
            explosionIndex += 1;
            if (explosionIndex >= explosions.Count) explosionIndex = 0;

            GameObject attack = Instantiate(crushAttack, crushLocation.position, crushLocation.rotation);
            Attack instance = attack.GetComponent<Attack>();
            instance.damage = crushDamage;

            cam.shakeCamera(3f, 0.5f);
        }
            
    }

    private void checkFlip()
    {
        //turns the player towards the mouse
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        if (mousePos.x < transform.position.x && transform.localScale.x > 0)
        {
            playerMovement.Flip();
        }
        else if (mousePos.x > transform.position.x && transform.localScale.x < 0)
        {
            playerMovement.Flip();
        }
    }

    public void fillUlt(int amount)
    {
        ultStatus += amount * ultRechargeRate * 0.001f;
        if (ultStatus > 1) ultStatus = 1;
        ultBar.value = ultStatus;
    }
}
