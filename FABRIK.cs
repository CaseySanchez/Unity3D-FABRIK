using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class FABRIK : MonoBehaviour 
{
    private GameObject rootObject;
    private FABRIKChain rootChain;

    private List<FABRIKChain> chains = new List<FABRIKChain>();
    private List<FABRIKChain> endChains = new List<FABRIKChain>();

    public virtual void OnFABRIK()
    {
    }

    public void Awake()
    {
        rootObject = new GameObject("FABRIK-Root-" + transform.name);

        rootObject.transform.position = transform.position;
        rootObject.transform.rotation = transform.rotation;

        transform.parent = rootObject.transform;

        rootChain = CreateSystem(rootObject.transform);

        // Inversely sort by layer, greater-first
        chains.Sort(delegate(FABRIKChain x, FABRIKChain y) { return y.layer.CompareTo(x.layer); } );
    }

    public void OnDestroy()
    {
        Destroy(rootObject);
    }

    public void Update()
    {
        OnFABRIK();

        Solve();
    }   

    public void Solve()
    {
        // We must iterate by layer in the first stage, working from target(s) to root
        foreach(FABRIKChain chain in chains)
        {
            chain.Forward();
        }

        // Provided our hierarchy, the second stage doesn't directly require an iterator
        rootChain.BackwardMulti();
    }

    private FABRIKChain CreateSystem(Transform transform, FABRIKChain parent = null, int layer = 0)
    {
        List<FABRIKEffector> effectors = new List<FABRIKEffector>();

        FABRIKEffector effector = null;

        // Use parent chain's end effector as our sub-base effector
        if(parent != null)
        {
            effector = parent.EndEffector;

            effectors.Add(effector);
        }

        // childCount > 1 is a new sub-base, childCount = 0 is an end chain (added to our list below), childCount = 1 is continuation of chain
        while(transform)
        {
            effector = FABRIKEffector.FetchComponent(transform.gameObject).Constructor(effector);

            effectors.Add(effector);

            if(transform.childCount != 1)
            {
                break;
            }

            transform = transform.GetChild(0);
        }

        FABRIKChain chain = new FABRIKChain(layer, parent, effectors.ToArray());

        chains.Add(chain);

        if(transform.childCount == 0)
        {
            endChains.Add(chain);
        }
        else foreach(Transform child in transform)
        {
            CreateSystem(child, chain, layer + 1);
        }

        return chain;
    }

    public FABRIKChain this[int index]
    {
        get
        {
            return endChains[index];
        }
    }
}
