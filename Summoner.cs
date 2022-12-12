using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summoner : enemyParent
{


    [SerializeField] private GameObject ghoul;
    public float summonTime;

    private Transform whereToSummon;

    [SerializeField] private float range;


    [SerializeField] private int maximumGhouls;
    public int currentGhouls;
    [SerializeField] private float summonCooldown;

    [SerializeField] private List<AudioSource> summonSounds;

    void Start()
    {
        setUp();
        move.setRangeX(range);


        
        whereToSummon = gameObject.transform;
    }

    private void FixedUpdate()
    {
        if (move.getBehavior() == "chase" && attackCD < 0 && !health.getDead() && !move.waiting() && currentGhouls < maximumGhouls)
        {
            if (distanceToTargetX() < range && distanceToTargetY() < 1) StartCoroutine(Summon());
        }
        attackCD -= Time.fixedDeltaTime;
    }


    IEnumerator Summon()
    {
        currentGhouls += 1;
        attackCD = summonCooldown;
        move.pauseOn();
        anim.SetBool("Summoning", true);

        summonSounds[Random.Range(0, summonSounds.Count)].Play();

        
        GameObject newPackage = Instantiate(ghoul, whereToSummon.position, whereToSummon.rotation);

        Ghoul[] newGhoul = newPackage.GetComponentsInChildren<Ghoul>();
        enemyHealth[] ghoulHealth = newPackage.GetComponentsInChildren<enemyHealth>();

        
        newGhoul[0].summoned();
        ghoulHealth[0].summoner = this;
        

        yield return new WaitForSeconds(summonTime);
        anim.SetBool("Summoning", false);
        move.pauseOff();

    }

}
