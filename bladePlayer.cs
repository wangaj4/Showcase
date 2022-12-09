using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bladePlayer : playerParent
{
    
    private SpriteRenderer sprite;
    

    [SerializeField] private cineMachineScript cam;
    [SerializeField] private Camera mainCam;

    [Space]
    [Header("Slash")]
    [Space]
    [SerializeField] private int slashDamage;
    private float slashDuration = 0.35f;
    [SerializeField] private GameObject slashAttack;
    [SerializeField] private Transform slashLocation;

    [SerializeField] private List<AudioSource> slashes;
    private int slashIndex;

    private bool slashingLeft;
    

    [Space]
    [Header("Backstab")]
    [Space]
    [SerializeField] private GameObject backstabAttack;
    [SerializeField] private float backstabMultiplier;
    

    [Space]
    [Header("Dash")]
    [Space]
    [SerializeField] private int dashDamage;
    private float dashDuration = .7f;
    private float dashDelay = 0.38f;//Time before dash/damage applies after starting the windup
    [SerializeField] private float dashAmount;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private Transform dashTarget;

    [SerializeField] private GameObject dashAttack;
    [SerializeField] private Transform dashLocation;

    [SerializeField] private List<AudioSource> dashes;
    private int dashIndex;

    [SerializeField] private List<AudioSource> dashSlashes;
    private int dashSlashIndex;

    

    [Space]
    [Header("Stealth")]
    [Space]
    [SerializeField] private Slider ultBar;
    [SerializeField] private GameObject tint;
    public float ultStatus;//Retain between scenes and save
    [SerializeField] private float ultRechargeRate;
    [SerializeField] private float ultDepleteRate;

    [SerializeField] private float timeToStealth;
    private bool stealthed;
    private bool stealthing;
    [SerializeField] private float rechargeRate;
    [SerializeField] private float slowMultiplier;//Something between 0 and 1

    [SerializeField] private GameObject indicator;

    public bool getStealth()
    {
        return stealthed;
    }


    private void Awake()
    {
        playerMovement = GetComponent<playerMove>();
        health = GetComponent<playerHealth>();
        anim = GetComponent<Animator>();
        dashAmount = dashTarget.position.x - transform.position.x;
        sprite = GetComponent<SpriteRenderer>();
        ultBar.value = ultStatus;
        tint.SetActive(stealthed);
        indicator.SetActive(stealthed);
        
    }

    private void FixedUpdate()
    {

        if (stealthed)
        {
            ultStatus -= ultDepleteRate * Time.fixedDeltaTime;
            if (ultStatus <= 0)
            {
                StartCoroutine(Stealth());
                ultStatus = 0;
            }
        }
        else
        {
            ultStatus += ultRechargeRate * Time.fixedDeltaTime;
            if (ultStatus >= 1) ultStatus = 1;
        }
        ultBar.value = ultStatus;
    }

    private void Update()
    {
        if (!gamePaused && !health.getDead() && active)
        {
            if (Input.GetButton("Fire1"))
            {
                if (!attacking && !health.getDamaged())
                {
                    if (stealthed)
                    {
                        StartCoroutine(Stealth());
                        StartCoroutine(Backstab());
                    }
                    else if (slashingLeft)
                    {
                        slashingLeft = false;
                        StartCoroutine(Slash1());
                    }
                    else
                    {
                        slashingLeft = true;
                        StartCoroutine(Slash2());
                    }
                }
            }

            if (Input.GetButtonDown("Fire2"))
            {
                if (!health.getDamaged())
                {
                    if (!attacking) StartCoroutine(Dash());
                    if (stealthed) StartCoroutine(Stealth());
                }

            }

            if (Input.GetButtonDown("Fire3"))
            {
                if (!attacking && !stealthing && !health.getDamaged()) StartCoroutine(Stealth());
            }
        }
        
        
    }


    IEnumerator Slash1()
    {
        checkFlip();

        attacking = true;
        playerMovement.pauseOn();

        slashes[slashIndex].Play();
        slashIndex += 1;
        if (slashIndex >= slashes.Count) slashIndex = 0;

        yield return new WaitForSeconds(.1f);
        if (!health.getDead())
        {
            anim.SetBool("Slashing1", true);
            GameObject newSlash = Instantiate(slashAttack, slashLocation.position, slashLocation.rotation);
            Attack newAttack = newSlash.GetComponent<Attack>();
            newAttack.damage = slashDamage;
            newAttack.left = (transform.localScale.x < 0);
            cam.shakeCamera(0.3f, 0.2f);

            yield return new WaitForSeconds(slashDuration);

            playerMovement.pauseOff();
            anim.SetBool("Slashing1", false);
            attacking = false;
        }

        
    }

    IEnumerator Slash2()
    {
        checkFlip();

        attacking = true;
        playerMovement.pauseOn();

        slashes[slashIndex].Play();
        slashIndex += 1;
        if (slashIndex >= slashes.Count) slashIndex = 0;

        yield return new WaitForSeconds(.1f);
        if (!health.getDead())
        {
            anim.SetBool("Slashing2", true);
            GameObject newSlash = Instantiate(slashAttack, slashLocation.position, slashLocation.rotation);
            Attack newAttack = newSlash.GetComponent<Attack>();
            newAttack.damage = slashDamage;
            newAttack.left = (transform.localScale.x < 0);
            cam.shakeCamera(0.2f, 0.2f);

            yield return new WaitForSeconds(slashDuration);

            playerMovement.pauseOff();
            anim.SetBool("Slashing2", false);
            attacking = false;
        }
        
    }

    IEnumerator Backstab()
    {
        checkFlip();

        attacking = true;
        playerMovement.pauseOn();

        slashes[slashIndex].Play();
        slashIndex += 1;
        if (slashIndex >= slashes.Count) slashIndex = 0;
        anim.SetBool("Backstabbing", true);

        yield return new WaitForSeconds(.1f);

        if (!health.getDead())
        {
            GameObject newSlash = Instantiate(backstabAttack, slashLocation.position, slashLocation.rotation);
            Attack newAttack = newSlash.GetComponent<Attack>();
            newAttack.damage = slashDamage;
            newAttack.left = (transform.localScale.x < 0);
            newAttack.critMultiplier = backstabMultiplier;
            cam.shakeCamera(0.2f, 0.2f);

            yield return new WaitForSeconds(slashDuration);

            playerMovement.pauseOff();
            anim.SetBool("Backstabbing", false);
            attacking = false;
        }

        
    }
    IEnumerator Dash()
    {
        checkFlip();

        anim.SetBool("Dashing", true);
        attacking = true;
        playerMovement.pauseOn();
        playerMovement.freezeOn();
        dashes[dashIndex].Play();
        dashIndex += 1;
        if (dashIndex >= dashes.Count) dashIndex = 0;

        yield return new WaitForSeconds(dashDelay);
        if (!health.getDead())
        {
            playerMovement.freezeOff();
            //Dash here
            cam.shakeCamera(0.5f, 0.4f);
            GameObject newDash = Instantiate(dashAttack, dashLocation.position, dashLocation.rotation);
            newDash.GetComponent<Attack>().damage = dashDamage;
            newDash.GetComponent<Attack>().left = (transform.localScale.x < 0);

            Vector2 forward = new Vector2(-1, 0);
            if (playerMovement.rightwards())
            {
                forward = new Vector2(1, 0);
            }
            RaycastHit2D hit = Physics2D.Raycast(transform.position, forward, dashAmount, whatIsWall);


            if (hit.collider != null) transform.position = hit.point;
            else transform.position = dashTarget.position;

            dashSlashes[dashSlashIndex].Play();
            dashSlashIndex += 1;
            if (dashSlashIndex >= dashSlashes.Count) dashSlashIndex = 0;





            yield return new WaitForSeconds(dashDuration - dashDelay);

            anim.SetBool("Dashing", false);
            attacking = false;
            playerMovement.pauseOff();
        }
        
    }

    IEnumerator Stealth()
    {
        if (!stealthed)
        {
            Image uiTint = tint.GetComponent<Image>();
            Color tintColor = uiTint.color;
            tint.SetActive(true);
            uiTint.color = tintColor;
            tintColor.a = 0;
            uiTint.color = tintColor;

            stealthing = true;
            Color temp = sprite.color;

            for(float i = 0; i < 1; i += 0.2f)
            {
                yield return new WaitForSeconds(0.1f);
                temp.a = 1-(i*0.8f);
                tintColor.a = i*0.06f;
                sprite.color = temp;
                uiTint.color = tintColor;
            }

            

            

            

            stealthed = true;
            stealthing = false;
            indicator.SetActive(true);
            playerMovement.changeMove(slowMultiplier);
        }
        else
        {
            tint.SetActive(false);
            stealthed = false;
            yield return new WaitForSeconds(0);
            playerMovement.changeMove(1/slowMultiplier);
            Color temp = sprite.color;
            temp.a = 1;
            sprite.color = temp;
            indicator.SetActive(false);
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
}
