using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            FindObjectOfType<DialogueManager>().StartDialogue(dialogue, collision.gameObject);
            Destroy(gameObject);
        }
    }
}
