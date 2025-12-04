using System.Collections;
using UnityEngine;

public class LightningController : MonoBehaviour
{
    public Light lightningLight;
    public AudioSource thunderAudio;
    public AudioClip[] thunderClips;

    [Header("Timing")]
    public float minDelay = 5f;
    public float maxDelay = 15f;

    [Header("Flash Settings")]
    public float flashIntensity = 6f;

    void Start()
    {
        lightningLight.intensity = 0;
        StartCoroutine(LightningRoutine());
    }

    IEnumerator LightningRoutine()
    {
        while (true)
        {
            // Wait for a random time between strikes
            float wait = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(wait);

            // Flash sequence
            yield return StartCoroutine(DoFlash());
        }
    }

    IEnumerator DoFlash()
    {
        // Multiple flickers (lightning usually double/triple flashes)
        for (int i = 0; i < Random.Range(2, 4); i++)
        {
            lightningLight.intensity = flashIntensity;
            yield return new WaitForSeconds(0.05f);

            lightningLight.intensity = 0;
            yield return new WaitForSeconds(0.07f);
        }

        // Thunder after delay
        if (thunderClips.Length > 0)
        {
            float thunderDelay = Random.Range(0.3f, 1.5f);
            yield return new WaitForSeconds(thunderDelay);

            thunderAudio.PlayOneShot(thunderClips[Random.Range(0, thunderClips.Length)]);
        }
    }
}
