

using UnityEngine;

public static class UnityExtension
{
    static public bool TryGetComponentInParent<T>(this Component from, out T component) where T : Component
    {
        component = from.GetComponentInParent<T>();
        return component != null;
    }

    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
