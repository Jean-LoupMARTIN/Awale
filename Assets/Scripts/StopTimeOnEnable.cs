using UnityEngine;

public class StopTimeOnEnable : MonoBehaviour
{
    float timeScaleMem;

    void OnEnable()
    {
        timeScaleMem = Time.timeScale;
        Time.timeScale = 0;
    }

    void OnDisable()
    {
        Time.timeScale = timeScaleMem;
    }
}
