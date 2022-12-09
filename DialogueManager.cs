using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public bool goNext;
    public exitBox exit;

    public Animator anim;
    private Queue<string> sentences;

    public TextMeshProUGUI text;
    public AudioSource typeSound;

    private bool talking = false;
    private bool typing = false;

    private playerMove movement;
    private playerHealth health;
    private playerParent abilities;

    private bool playSound;

    private pauseMenu pause;

    private void Start()
    {
        sentences = new Queue<string>();
        pause = GetComponent<pauseMenu>();
    }

    private void Update()
    {
        if (talking && !pause.getPaused())
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!typing) DisplayNextSentence();
                else typing = false;
            }
        }
    }

    public void StartDialogue(Dialogue dialogue, GameObject player)
    {
        anim.SetBool("Opened", true);

        movement = player.GetComponent<playerMove>();
        movement.active = false;

        health = player.GetComponent<playerHealth>();
        health.tempInvincible(true);

        abilities = player.GetComponent<playerParent>();
        abilities.Deactivate();

        sentences.Clear();
        talking = true;

        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
            
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count == 0)
        {
            EndDialogue();
            
            return;
        }

        string sentence = sentences.Dequeue();
        StartCoroutine(typeSentence(sentence));
    }

    IEnumerator typeSentence(string sentence)
    {
        typing = true;
        text.text = "";
        foreach(char c in sentence.ToCharArray())
        {
            if (typing == false) break;
            text.text += c;
            if (playSound) typeSound.Play();
            playSound = !playSound;
            yield return new WaitForSeconds(0.02f);
        }
        text.text = sentence;
        typing = false;
    }

    private void EndDialogue()
    {
        if (goNext)
        {
            exit.manualContinue();
        }
        else
        {
            anim.SetBool("Opened", false);

            movement.active = true;
            StartCoroutine(delay());

            talking = false;
            health.tempInvincible(false);
        }
        
    }

    IEnumerator delay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        abilities.Activate();
    }
}
