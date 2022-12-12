using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghoul : enemyParent
{
    [SerializeField] private GameObject attack;
    [SerializeField] private float range;
    [SerializeField] private float damage;
    [SerializeField] private List<AudioSource> attackSounds;
    [SerializeField] private List<AudioSource> attackHits;

    private void Start()
    {
        setUp();
        move.setRangeX(range);

    }

    private void Awake()
    {
        setUp();
        move.setRangeX(range);
    }

    private void FixedUpdate()
    {
        if (move.getBehavior() == "chase" && attackCD < 0 && !health.getDead() && !move.waiting())
        {
            if (distanceToTargetX() < range && distanceToTargetY() < range) StartCoroutine(Attack());
        }
        attackCD -= Time.fixedDeltaTime;
    }
    IEnumerator Attack()
    {
        attackCD = 1.5f;
        anim.SetBool("Attacking", true);
        move.pauseOn();
        yield return new WaitForSeconds(0.3f);

        if (!health.getDead())
        {
            GameObject instance = Instantiate(attack, transform.position, transform.rotation);
            if (!move.getFacingRight())
            {
                Vector3 temp = instance.transform.localScale;
                temp.x *= -1;
                instance.transform.localScale = temp;
            }
            Attack a = instance.GetComponent<Attack>();
            a.damage = damage;
            a.left = !move.getFacingRight();
            a.setPlayerHit(attackHits[Random.Range(0, attackHits.Count)]);

            attackSounds[Random.Range(0, attackSounds.Count)].Play();
            yield return new WaitForSeconds(0.25f);
            move.pauseOff();
            anim.SetBool("Attacking", false);
        }

    }

    public void summoned()
    {
        StartCoroutine(summonAnim());
    }
    public IEnumerator summonAnim()
    {
        attackCD = 0.8f;
        move.pauseOn();
        anim.SetBool("Summoned", true);
        yield return new WaitForSeconds(0.7f);
        anim.SetBool("Summoned", false);
        move.pauseOff();
    }
}
