using UnityEngine;
using System.Collections;

public class FABRIKEffector : MonoBehaviour
{   
    public Vector3 position, direction;
    public float distance;

    static public FABRIKEffector FetchComponent(GameObject gameObject)
    {
        FABRIKEffector effector = gameObject.GetComponent<FABRIKEffector>();

        if(effector == null)
        {
            effector = gameObject.AddComponent<FABRIKEffector>();
        }

        return effector;
    }

    public FABRIKEffector Constructor(FABRIKEffector parent = null)
    {
        position = transform.position;

        if(parent != null)
        {
            distance = Vector3.Distance(parent.position, position);
        }

        return this;
    }

    public void ApplyConstraint(Vector3 axis, float angle)
    {
        direction = ConstrainRotation(direction, axis, angle);
    }

    private Vector3 ConstrainRotation(Vector3 from, Vector3 to, float angle)
    {
        float theta = Mathf.Abs(angle / Vector3.Angle(from, to));

        if(theta < 1.0F)
        {
            return Vector3.Slerp(from, to, theta);
        }

        return to;
    }
}
