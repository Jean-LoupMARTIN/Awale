


using System.Collections.Generic;
using System.Linq;

public static class LinqExtension
{
    static public bool Contains<T>(this T[] array, T e) => array.ToList().Contains(e);

    static public int IndexOf<T>(this T[] array, T e) => array.ToList().IndexOf(e);

    static public T Random<T>(this List<T> list) => list[UnityEngine.Random.Range(0, list.Count)];

    static public T[] Concat<T>(this T[] a1, T[] a2)
    {
        T[] a12 = new T[a1.Length + a2.Length];

        for (int i = 0; i < a1.Length; i++)
            a12[i] = a1[i];

        for (int i = 0; i < a2.Length; i++)
            a12[a1.Length + i] = a2[i];

        return a12;
    }
}
