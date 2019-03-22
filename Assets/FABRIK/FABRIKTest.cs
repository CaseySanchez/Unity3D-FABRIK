using UnityEngine;
using System.Collections;

public class FABRIKTest : FABRIK
{
    public Transform sphere_right;
    public Transform sphere_left;

    public override void OnFABRIK ()
    {
        float speed = 10.0F;
        float step = Time.deltaTime * speed;

        FABRIKChain right = GetEndChain("right");
        FABRIKChain left = GetEndChain("left");

        right.Target = Vector3.MoveTowards(right.EndEffector.Position, sphere_right.position, step);
        left.Target = Vector3.MoveTowards(left.EndEffector.Position, sphere_left.position, step);
    }
}
