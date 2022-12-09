using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class enemyHealth : MonoBehaviour
{
    [SerializeField] private int scrap;
    [SerializeField] private TextMeshPro scrapTexts;

    public healthBar hpbar;
    private playerMaster master;
    [SerializeField] private int maxHealth;
    [SerializeField] private bool cycleSounds;//cycles manually between all hit sounds
    private int counter = 0;

    [SerializeField] private float damagedDuration;
    private Animator anim;
    [SerializeField] private float health;
    private bool damaged;
    
    [SerializeField] private float timeToDisappear;//time between dying and gameobject destruction

    [SerializeField] private List<AudioSource> hurtNormal;
    [SerializeField] private List<AudioSource> hurtCrit;

    private AudioSource idle;
    [SerializeField] private bool hasIdle;

    private enemyMovement move;



    private bool dead;


    [SerializeField] private Material white;
    private Material og;
    private SpriteRenderer sprite;

    [Header("Summoned")]
    [Space]
    public Summoner summoner;
    //If summoned, summoner will be stored here, and on death, summoner will have its currentGhouls -= 1


    private void Awake()
    {
        health = maxHealth;
        anim = GetComponent<Animator>();
        master = GameObject.Find("Players").GetComponent<playerMaster>();
        if(hasIdle) idle = GetComponent<AudioSource>();
        move = GetComponent<enemyMovement>();

        sprite = GetComponent<SpriteRenderer>();
        og = sprite.material;


        counter = Random.Range(0, hurtNormal.Count);
    }

    public bool getDead()
    {
        return dead;
    }


    public void takeDamage(float amount, bool crit, float critMultiplier = 1, float stunTime = 0.3f)
    {
        if (dead) return;
        move.setWait(stunTime);
        //Handle damage
        if(master.getSin())
        {
            if (crit && move.getBehavior()=="patrol" || move.getBehavior() == "afk")
            {
                health -= amount * critMultiplier;


            } else
            {
                health -= amount;
                crit = false;
            }
        }

        else health -= amount;
        if (health < 0) health = 0;
        hpbar.updateRatio(health / maxHealth);
        //Handle sound
        //Crit attacks will play crit sound even if crit multiplier does not apply, for attacks like
            //The spear's spear crush
        if (health <= 0)
        {
            StartCoroutine(dieAnim());
            int r = Random.Range(0, hurtCrit.Count);
            hurtCrit[r].Play();
            dead = true;
        }
        else
        {
            move.setChase();
            StartCoroutine(damagedAnim());
            if (!crit)
            {
                if (cycleSounds)
                {
                    hurtNormal[counter].Play();
                    counter += 1;
                    if (counter >= hurtNormal.Count) counter = 0;
                }
                else hurtNormal[Random.Range(0,hurtNormal.Count)].Play();
                
            }
            else
            {
                hurtCrit[Random.Range(0,hurtCrit.Count)].Play();
                
            }
        }
    }

    IEnumerator damagedAnim()
    {
        if (!damaged)
        {
            damaged = true;
            anim.SetBool("Damaged", true);

            sprite.material = white;
            yield return new WaitForSeconds(0.1f);
            sprite.material = og;

            yield return new WaitForSeconds(damagedDuration-0.1f);
            anim.SetBool("Damaged", false);
            damaged = false;
        }
        
    }

    IEnumerator dieAnim()
    {
        
        scrapTexts.gameObject.SetActive(true);
        if (scrap != 0) scrapTexts.text = "+ " + scrap.ToString() + " scrap";
        scrapTexts.gameObject.transform.position = gameObject.transform.position;

        master.changeScrap(scrap);
        anim.SetBool("Dead", true);
        move.pauseOn();
        if (hasIdle)
        {
            float idleVolume = idle.volume;
            yield return new WaitForSeconds(timeToDisappear/5);
            idle.volume = idleVolume * 4 / 5;
            yield return new WaitForSeconds(timeToDisappear / 5);
            idle.volume = idleVolume * 3 / 5;
            yield return new WaitForSeconds(timeToDisappear / 5);
            idle.volume = idleVolume * 2 / 5;
            yield return new WaitForSeconds(timeToDisappear / 5);
            idle.volume = idleVolume * 1 / 5;
            yield return new WaitForSeconds(timeToDisappear / 5);
            idle.volume = idleVolume * 0 / 5;
        }
        else
        {
            yield return new WaitForSeconds(timeToDisappear);
        }

        if(summoner != null)
        {
            summoner.currentGhouls -= 1;
        }
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && dead)
        {
            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && dead)
        {
            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }


}
