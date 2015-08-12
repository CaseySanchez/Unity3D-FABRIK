using UnityEngine;
using System.Collections;

public class FABRIKTest : FABRIK
{
    public Transform targetTransformA;
    public Transform targetTransformB;
    public Transform targetTransformC;
    public Transform targetTransformD;
    public Transform targetTransformE;
    public Transform targetTransformF;
    public Transform targetTransformG;
    public Transform targetTransformH;

    public override void OnFABRIK ()
    {
        this[0].Target = targetTransformA.position;
        this[1].Target = targetTransformB.position;
        this[2].Target = targetTransformC.position;
        this[3].Target = targetTransformD.position;
        this[4].Target = targetTransformE.position;
        this[5].Target = targetTransformF.position;
        this[6].Target = targetTransformG.position;
        this[7].Target = targetTransformH.position;
    }
}
