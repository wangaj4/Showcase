using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

    public bool ready;
    public bool cd = true;
    private GameObject player;
    [SerializeField] private Transform tpPoint;
    [SerializeField] private AudioSource tpSound;

    [SerializeField] private GameObject text;

    public Animator anim;

    [SerializeField] private Portal partner;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        text.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            if (ready && cd) teleport(player);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject entity = collision.gameObject;
        if (entity.layer == LayerMask.NameToLayer("Player"))
        {
            text.SetActive(true);
            ready = true;
            player = entity;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject entity = collision.gameObject;
        if (entity.layer == LayerMask.NameToLayer("Player"))
        {
            text.SetActive(false);
            ready = false;
        }
    }

    private void teleport(GameObject entity)
    {
        partner.tpSound.Play();
        entity.transform.position = tpPoint.position;
        StartCoroutine(animate());
        text.SetActive(false);
    }

    public IEnumerator animate()
    {
        anim.SetBool("Activated", true);
        cd = false;
        partner.cd = false;
        partner.anim.SetBool("Activated", true);
        yield return new WaitForSeconds(0.65f);
        anim.SetBool("Activated", false);
        cd = true;
        partner.cd = true;
        partner.anim.SetBool("Activated", false);
    }
}
