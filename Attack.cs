using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private playerMaster master;
    private magePlayer mage;
    private bladePlayer blade;
    private spearPlayer spear;
    private List<AudioSource> hitSounds = new List<AudioSource>();
    [SerializeField] private bool allied;//true if player attacked, false if enemy attacked
    public float damage;
    public bool left;//true if player facing left, false if facing right
    public float critMultiplier;//Here for assassin only, their backstab multiplier
    [SerializeField] private float lifetime;
    [SerializeField] private float knockbackAmount;
    [SerializeField] private string knockbackDirection; //either "away" or "up"

    public bool crit;//Will attack trigger special hit sound by enemy?
    public float stunTime;//time unable to move after hit lands

    [SerializeField] private bool isMageAttack;//If true, attack will not destruct upon hitting enemy
    [SerializeField] private bool isSinAttack;//If true do bonus damage to bosses so its playable for assassin
    private AudioSource hitPlayerSound;

    public bool spawnSomething;
    public GameObject[] spawnThis;

    public void setPlayerHit(AudioSource sound)
    {
        hitPlayerSound = sound;
    }
    public float getTime()
    {
        return lifetime;
    }

    private void Awake()
    {
        StartCoroutine(Destruct());
        master = GameObject.Find("Players").GetComponent<playerMaster>();
        if (allied)
        {
            if (master.getTank()) hitSounds = master.spearHits;
            if (master.getSin()) hitSounds = master.bladeHits;

            spear = master.tankPlayer.GetComponent<spearPlayer>();
            blade = master.sinPlayer.GetComponent<bladePlayer>();
            mage = master.magePlayer.GetComponent<magePlayer>();
        }
        

    }

    IEnumerator Destruct()
    {
        yield return new WaitForSeconds(lifetime);
        if (spawnSomething)
        {
            int r = Random.Range(0, spawnThis.Length);
            Instantiate(spawnThis[r], gameObject.transform.position, gameObject.transform.rotation);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject entity = collision.gameObject;
        
        if (entity.layer == LayerMask.NameToLayer("Animal"))
        {
            entity.GetComponent<AnimalScript>().die();
            
        }

        if (entity.layer == LayerMask.NameToLayer("Boss") && allied)
        {
            bossHealth eh = entity.GetComponent<bossHealth>();
            if (eh.getDead()) return;
            if (isSinAttack) damage *= 1.25f;
            eh.takeDamage(damage);
            
            if (hitSounds.Count > 0)
            {
                int x = Random.Range(0, hitSounds.Count);
                hitSounds[x].gameObject.transform.position = transform.position;
                hitSounds[x].Play();
            }
            if (!isMageAttack) Destroy(gameObject);
            if (master.getMage()) mage.fillUlt(1);
            if (master.getTank() && !crit) spear.fillUlt(1);

        }

        if (entity.layer == LayerMask.NameToLayer("Enemy") && allied)
        {
            //If allied attack hits enemy:
            enemyHealth eh = entity.GetComponent<enemyHealth>();
            if (eh.getDead()) return;
            eh.takeDamage(damage, crit, critMultiplier, stunTime);
            if (hitSounds.Count > 0)
            {
                int x = Random.Range(0, hitSounds.Count);
                hitSounds[x].gameObject.transform.position = transform.position;
                hitSounds[x].Play();
            }
                
            
            knockBackEnemy(entity);
            if(!isMageAttack) Destroy(gameObject);
            if(master.getMage()) mage.fillUlt(1);
            if (master.getTank() && !crit) spear.fillUlt(1);
            
            
        }
        else if(entity.layer == LayerMask.NameToLayer("Player") && !allied)
        {
            //If enemy attack hits player:
            if (spawnSomething)
            {
                int r = Random.Range(0, spawnThis.Length);
                Instantiate(spawnThis[r], gameObject.transform.position, gameObject.transform.rotation);
            }

            playerHealth ph = entity.GetComponent<playerHealth>();
            if (!ph.getDead())
            {
                if (hitPlayerSound != null)
                {
                    hitPlayerSound.gameObject.transform.position = transform.position;
                    hitPlayerSound.Play();
                }
                ph.takeDamage(damage);
                if (!master.getTank() && !ph.getInvincible()) knockbackPlayer(entity);
                if (!isMageAttack) Destroy(gameObject);
            }
            

        }
        
    }

    private void knockbackPlayer(GameObject target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (knockbackDirection == "directional")
        {
            if (left)
            {
                targetRb.AddForce(new Vector2(-knockbackAmount, 0));
                
            }
            else
            {
                targetRb.AddForce(new Vector2(knockbackAmount, 0));
                
            }
        }
        else if (knockbackDirection == "up")
        {
            targetRb.AddForce(new Vector2(0, knockbackAmount));

        }
        else if (knockbackDirection == "away")
        {
            if (target.transform.position.x < transform.position.x)
            {
                targetRb.AddForce(new Vector2(-knockbackAmount, 0));
                
            }
            else
            {
                targetRb.AddForce(new Vector2(knockbackAmount, 0));
                
            }
        }
    }

    private void knockBackEnemy(GameObject target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        enemyMovement enemyMove = target.GetComponent<enemyMovement>();
        if (knockbackDirection == "directional")
        {
            if (left)
            {
                targetRb.AddForce(new Vector2(-knockbackAmount, 0));
                if (!enemyMove.getFacingRight())
                {
                    enemyMove.Flip();
                }
            }
            else
            {
                targetRb.AddForce(new Vector2(knockbackAmount, 0));
                if (enemyMove.getFacingRight())
                {
                    enemyMove.Flip();
                }
            }
            }
        else if (knockbackDirection == "up")
        {
            targetRb.AddForce(new Vector2(0, knockbackAmount));

        }
        else if (knockbackDirection == "away")
        {
            if (target.transform.position.x < transform.position.x)
            {
                targetRb.AddForce(new Vector2(-knockbackAmount, 0));
                if (!enemyMove.getFacingRight())
                {
                    enemyMove.Flip();
                }
            }
            else
            {
                targetRb.AddForce(new Vector2(knockbackAmount, 0));
                if (enemyMove.getFacingRight())
                {
                    enemyMove.Flip();
                }
            }
        }
    }
}
