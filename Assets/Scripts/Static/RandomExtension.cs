using UnityEngine;


public static class RandomExtension
{
    static public Vector3 RandomVector3(float min = 0, float max = 1) => new Vector3(Random.Range(min, max),
                                                                                     Random.Range(min, max),
                                                                                     Random.Range(min, max));

    static public Quaternion RandomQuaternion() => Quaternion.Euler(Random.Range(0f, 360f),
                                                                    Random.Range(0f, 360f),
                                                                    Random.Range(0f, 360f));

    static public Vector3 RandomPointInSphere()
    {
        Vector3 pos;
        do pos = RandomVector3(-1f, 1f);
        while (pos.sqrMagnitude > 1);
        return pos;
    }

    static public Vector3 RandomPointInSphere(float radius) => RandomPointInSphere() * radius;
}
