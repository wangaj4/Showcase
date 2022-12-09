using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class magePlayer : playerParent
{

    

    [SerializeField] private cineMachineScript cam;
    [SerializeField] private Camera mainCam;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private SpriteRenderer cursor;

    [Space]
    [Header("Explosion")]
    [Space]
    [SerializeField] private float rangeX;
    [SerializeField] private float rangeY;
    [SerializeField] private int explosionDamage;
    private float explosionDuration = 0.7f;
    private float explosionDelay = 0.45f;//Time between click and instantiate
    [SerializeField] private GameObject explodeAttack;
    [SerializeField] private List<AudioSource> charges;
    private int chargeIndex;

    [SerializeField] private List<AudioSource> explosions;
    private int explosionIndex;

    



    [Space]
    [Header("Blast")]
    [Space]
    [SerializeField] private int blastDamage;
    [SerializeField] private GameObject blastAttack;
    [SerializeField] private Transform blastLocation;
    private float blastDuration = 1f;
    private float blastDelay = 0.5f;//Time before damage applies after starting the chargeup
    [SerializeField] private List<AudioSource> blastCharges;
    private int blastChargeIndex;

    [SerializeField] private List<AudioSource> blastExplosions;
    private int blastExplosionIndex;


    [Space]
    [Header("Power")]
    [Space]
    [SerializeField] private GameObject tint;
    [SerializeField] private Slider ultBar;
    public float ultStatus;//Retain between scenes and save
    [SerializeField] private float ultRechargeRate;
    [SerializeField] private float ultDepleteRate;

    [SerializeField] private float damageMultiplier;
    [SerializeField] private float healAmount;
    [SerializeField] private float empowerTime;

    private bool empowered;//Retain between scenes and save
    private float empowerDuration = 1.2f;
    [SerializeField] private List<AudioSource> powerSounds;
    private int powerIndex;
    [SerializeField] private List<AudioSource> empowerStarts;
    private int powerStartIndex;
    private float empowerDelay = 0.5f;

    [SerializeField] private GameObject indicator;


    private void Awake()
    {
        playerMovement = GetComponent<playerMove>();
        anim = GetComponent<Animator>();
        health = GetComponent<playerHealth>();
        ultBar.value = ultStatus;
        tint.SetActive(empowered);
        indicator.SetActive(empowered);
        
    }

    private void FixedUpdate()
    {
        
        if (empowered)
        {
            ultStatus -= ultDepleteRate * Time.fixedDeltaTime;
            if(ultStatus <= 0)
            {
                empowered = false;
                indicator.SetActive(false);
                StartCoroutine(tintFade(false));
                ultStatus = 0;
            }
            ultBar.value = ultStatus;
        }
        
    }



    private void Update()
    {
        if (!gamePaused && !health.getDead() && active)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!attacking && playerMovement.getGrounded()) StartCoroutine(Explosion());
            }

            if (Input.GetButtonDown("Fire2"))
            {
                if (!attacking && playerMovement.getGrounded()) StartCoroutine(Blast());
            }

            if (Input.GetButtonDown("Fire3"))
            {
                if (!attacking && playerMovement.getGrounded() && !empowered && ultStatus >= 1) StartCoroutine(Empower());
            }
        }
        

        checkRange();
        

    }

    private bool checkRange()
    {

        Color x = cursor.color;

        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        mousePos.y += 0.5f;


        Collider2D[] colliders = Physics2D.OverlapCircleAll(mousePos, 0.05f, whatIsWall);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                x.a = 0.1f;
                cursor.color = x;
                return false;

            }

        }

        //raycast m down to find ground
        
        Vector2 down = new Vector2(0, -1);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, down, 100, whatIsWall);


        Vector3 hitspot = hit.point;

        

        if (Mathf.Abs(transform.position.x - hitspot.x) > rangeX || Mathf.Abs(transform.position.y - hitspot.y) > rangeY)
        {
            x.a = 0.1f;
            cursor.color = x;
            return false;
        }

        x.a = 1;
        cursor.color = x;
        return true;
    }


    IEnumerator Explosion()
    {
        bool valid = checkRange();
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Transform m = explodeAttack.transform;
        mousePos.y += 0.5f;

        //If invalid, skip the coroutine

        if (valid)
        {
            Vector2 down = new Vector2(0, -1);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, down, 100, whatIsWall);


            m.position = hit.point;

            checkFlip();
            //Stop movement and play charge sound
            attacking = true;
            anim.SetBool("Exploding", true);
            playerMovement.pauseOn();

            charges[chargeIndex].Play();
            chargeIndex += 1;
            if (chargeIndex >= charges.Count) chargeIndex = 0;


            yield return new WaitForSeconds(explosionDelay);
            if (!health.getDead())
            {
                //Instantiate explosion at target
                GameObject explode = Instantiate(explodeAttack, m.position, m.rotation);
                Attack newAttack = explode.GetComponent<Attack>();
                newAttack.damage = explosionDamage;


                explosions[explosionIndex].gameObject.transform.position = m.position;

                if (empowered)
                {
                    explosions[explosionIndex].volume = 0.8f;
                    explosions[explosionIndex].pitch = 0.8f;
                    newAttack.crit = true;
                    newAttack.damage = explosionDamage * damageMultiplier;
                    cam.shakeCamera(1.5f, 0.7f);
                }
                else
                {
                    explosions[explosionIndex].volume = 0.6f;
                    explosions[explosionIndex].pitch = 1;
                    cam.shakeCamera(0.9f, 0.7f);
                }
                explosions[explosionIndex].Play();
                explosionIndex += 1;
                if (explosionIndex >= explosions.Count) explosionIndex = 0;


                yield return new WaitForSeconds(explosionDuration - explosionDelay);
                playerMovement.pauseOff();
                anim.SetBool("Exploding", false);

                yield return new WaitForSeconds(0.4f);
                attacking = false;
            }
                
        }
    }

    IEnumerator Blast()
    {
        checkFlip();

        attacking = true;
        anim.SetBool("Blasting", true);
        playerMovement.pauseOn();

        blastCharges[blastChargeIndex].Play();
        blastChargeIndex += 1;
        if (blastChargeIndex >= blastCharges.Count) blastChargeIndex = 0;


        yield return new WaitForSeconds(blastDelay);
        if (!health.getDead())
        {
            //Instantiate blast attack
            GameObject explode = Instantiate(blastAttack, blastLocation.position, blastLocation.rotation);
            Attack newAttack = explode.GetComponent<Attack>();
            newAttack.damage = blastDamage;
            newAttack.left = (transform.localScale.x < 0);

            blastExplosions[blastExplosionIndex].gameObject.transform.position = blastLocation.position;

            if (empowered)
            {
                blastExplosions[blastExplosionIndex].volume = 0.8f;
                blastExplosions[blastExplosionIndex].pitch = 0.9f;
                newAttack.crit = true;
                newAttack.damage = blastDamage * damageMultiplier;
                cam.shakeCamera(2.2f, 0.7f);

            }
            else
            {
                blastExplosions[blastExplosionIndex].volume = 0.6f;
                blastExplosions[blastExplosionIndex].pitch = 1;
                cam.shakeCamera(1, 0.7f);
            }
            blastExplosions[blastExplosionIndex].Play();
            blastExplosionIndex += 1;
            if (blastExplosionIndex >= blastExplosions.Count) blastExplosionIndex = 0;


            yield return new WaitForSeconds(blastDuration - blastDelay);
            playerMovement.pauseOff();
            anim.SetBool("Blasting", false);

            yield return new WaitForSeconds(0.15f);
            attacking = false;
        }
            
    }

    IEnumerator Empower()
    {
        
        attacking = true;
        anim.SetBool("Empower", true);
        playerMovement.pauseOn();

        empowerStarts[powerStartIndex].Play();
        powerStartIndex += 1;
        if (powerStartIndex >= empowerStarts.Count) powerStartIndex = 0;

        yield return new WaitForSeconds(empowerDelay);
        if (!health.getDead())
        {
            powerSounds[powerIndex].Play();
            powerIndex += 1;
            if (powerIndex >= powerSounds.Count) powerIndex = 0;

            StartCoroutine(tintFade(true));

            yield return new WaitForSeconds(empowerDuration - empowerDelay);


            empowered = true;
            indicator.SetActive(true);
            playerMovement.pauseOff();
            anim.SetBool("Empower", false);
            attacking = false;
        }

            
    }

    IEnumerator tintFade(bool on)
    {
        Image rend = tint.GetComponent<Image>();
        Color c = rend.color;
        if (on)
        {
            tint.SetActive(true);
            c.a = 0;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.01f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.02f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.03f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.04f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.05f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);
        }
        else
        {
            
            c.a = 0.1f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.08f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.06f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.04f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0.02f;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            c.a = 0;
            rend.color = c;
            yield return new WaitForSeconds(0.05f);

            tint.SetActive(false);
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
        if (!empowered) ultStatus += amount * ultRechargeRate * 0.001f;
        if (ultStatus > 1) ultStatus = 1;
        ultBar.value = ultStatus;
    }

}
