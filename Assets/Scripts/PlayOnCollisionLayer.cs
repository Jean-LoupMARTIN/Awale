using System;
using UnityEngine;



public class PlayOnCollisionLayer : MonoBehaviour
{
    [SerializeField] LayerClip[] layersClips;
    [SerializeField] bool playOnce = false;
    int playCount = 0;

    void OnCollisionEnter(Collision collision)
    {
        if (playOnce && playCount > 0)
            return;

        foreach (LayerClip lc in layersClips)
        {
            if (lc.layer.Contains(collision.gameObject.layer))
            {
                AudioSourceExtension.Play(lc.clip, 1, lc.pitch);
                playCount++;
                break;
            }
        }
    }
}

[Serializable]
public class LayerClip
{
    public LayerMask layer;
    public AudioClip clip;
    public float pitch;
}