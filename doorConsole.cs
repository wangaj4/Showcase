using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorConsole : MonoBehaviour
{
    [SerializeField] private Animator doorAnim;
    [SerializeField] private BoxCollider2D doorCollider;
    [SerializeField] private GameObject openText;
    [SerializeField] private GameObject mainLight;
    private bool closeEnough;

    private bool opened = false;

    [SerializeField] private AudioSource beep;
    [SerializeField] private AudioSource openSound;


    public bool motionTrigger;

    private void Awake()
    {
        openText.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetButtonDown("Interact") && closeEnough)
        {
            openDoor();
        }
    }

    private void openDoor()
    {
        if (!opened)
        {
            doorCollider.enabled = false;
            openText.SetActive(false);
            doorAnim.SetBool("Open", true);
            mainLight.SetActive(false);
            opened = true;

            StartCoroutine(playNoises());
        }
    }

    IEnumerator playNoises()
    {
        beep.Play();
        yield return new WaitForSeconds(0.1f);
        openSound.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && !opened)
        {
            if (motionTrigger)
            {
                openDoor();
            }
            else
            {
                closeEnough = true;
                openText.SetActive(true);
            }
            
        }
    }

    

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && !opened)
        {
            closeEnough = false;
            openText.SetActive(false);
        }
    }
}
