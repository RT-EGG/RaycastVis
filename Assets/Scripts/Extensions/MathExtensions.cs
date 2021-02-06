using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtensions
{
    public static Vector3 ToVector(this (float, float, float) inValue)
        => new Vector3(inValue.Item1, inValue.Item2, inValue.Item3);

    public static (float, float, float) Deconstruction(this Vector3 inValue)
        => (inValue.x, inValue.y, inValue.z);
}
