using UnityEngine;



[RequireComponent(typeof(Light))]
public class Flame : MonoBehaviour
{
    [SerializeField] float intensityMin = 0.5f;
    [SerializeField] float intensityMax = 1f;
    [SerializeField] float speed = 1;
    Light light;

    void Awake()
    {
        light = GetComponent<Light>();
    }

    void Update()
    {
        light.intensity = Mathf.Lerp(intensityMin, intensityMax, Mathf.PerlinNoise(0, Time.time * speed));
    }
}
