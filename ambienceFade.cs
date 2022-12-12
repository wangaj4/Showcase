using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ambienceFade : MonoBehaviour
{
    [SerializeField] private float targetVolume;
    [SerializeField] private float duration;

    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(fade());
    }

    IEnumerator fade()
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += .015f;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }

    public IEnumerator fadeOut()
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < 1)
        {
            currentTime += .015f;
            audioSource.volume = Mathf.Lerp(start, 0, currentTime / 1);
            yield return null;
        }
        yield break;
    }
}
