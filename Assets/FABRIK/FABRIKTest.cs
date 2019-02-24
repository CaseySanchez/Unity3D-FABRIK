using UnityEngine;
using System.Collections;

public class FABRIKTest : FABRIK
{
    public Transform targetTransformA;
    public Transform targetTransformB;
    public Transform targetTransformC;
    public Transform targetTransformD;

    public override void OnFABRIK ()
    {
        float speed = 10.0F;
        float step = Time.deltaTime * speed;

        FABRIKChain up_left = GetEndChain("up_left");
        FABRIKChain down_left = GetEndChain("down_left");
        FABRIKChain up_right = GetEndChain("up_right");
        FABRIKChain down_right = GetEndChain("down_right");

        up_left.Target = Vector3.MoveTowards(up_left.EndEffector.Position, targetTransformA.position, step);
        down_left.Target = Vector3.MoveTowards(down_left.EndEffector.Position, targetTransformB.position, step);
        up_right.Target = Vector3.MoveTowards(up_right.EndEffector.Position, targetTransformC.position, step);
        down_right.Target = Vector3.MoveTowards(down_right.EndEffector.Position, targetTransformD.position, step);
    }
}
