using UnityEngine;



public class PlayOnCollision : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [SerializeField] float pitch = 1;
    [SerializeField] float plusMinusPitch = 0;
    [SerializeField] bool playOnce = false;
    int playCount = 0;

    void OnCollisionEnter(Collision collision)
    {
        if (playOnce && playCount > 0)
            return;

        AudioSourceExtension.Play(clip, 1, pitch + Random.Range(-plusMinusPitch, plusMinusPitch));
        playCount++;
    }
}
