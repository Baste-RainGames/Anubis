using UnityEngine;
using System.Collections;

public static class Vector3Extensions
{
    public static Vector3 RemoveY(this Vector3 v3)
    {
        v3.Scale(new Vector3(1, 0, 1));
        return v3;
    }

    public static Vector3 MultipliedWith (this Vector3 a, Vector3 b)
    {
        return  new Vector3(
            a.x * b.x,
            a.y * b.y,
            a.z * b.z);
    }
}