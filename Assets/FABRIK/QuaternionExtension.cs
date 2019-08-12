using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtension
{
    public static void Decompose(this Quaternion quaternion, Vector3 direction, out Quaternion swing, out Quaternion twist)
    {
        Vector3 vector = new Vector3(quaternion.x, quaternion.y, quaternion.z);
        Vector3 projection = Vector3.Project(vector, direction);

        twist = new Quaternion(projection.x, projection.y, projection.z, quaternion.w).normalized;
        swing = quaternion * Quaternion.Inverse(twist);
    }

    public static Quaternion Constrain(this Quaternion quaternion, float angle)
    {
        float magnitude = Mathf.Sin(0.5F * angle);
        float sqrMagnitude = magnitude * magnitude;

        Vector3 vector = new Vector3(quaternion.x, quaternion.y, quaternion.z);

        if (vector.sqrMagnitude > sqrMagnitude)
        {
            vector = vector.normalized * magnitude;

            quaternion.x = vector.x;
            quaternion.y = vector.y;
            quaternion.z = vector.z;
            quaternion.w = Mathf.Sqrt(1.0F - sqrMagnitude) * Mathf.Sign(quaternion.w);
        }

        return quaternion;
    }
}
