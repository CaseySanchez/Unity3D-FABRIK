using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FABRIKChain
{
    public FABRIKChain parent;
    public List<FABRIKChain> children = new List<FABRIKChain>();

    public List<FABRIKEffector> effectors = new List<FABRIKEffector>();

    public int layer, endEffector;

    public Vector3 targetPosition;

    public FABRIKChain(int layer, FABRIKChain parent, params FABRIKEffector [] effectors)
    {           
        this.layer = layer;

        this.endEffector = effectors.Length - 1;

        this.effectors.AddRange(effectors);

        this.parent = parent;

        if(parent != null)
        {
            parent.children.Add(this);
        }
    }

    public void Forward()
    {
        Vector3 rootPosition = effectors[0].position;

        // Sub-base, average for centroid
        if(children.Count != 0)
        {
            targetPosition /= (float)children.Count;
        }

        effectors[endEffector].position = targetPosition;

        for(int i = endEffector; i > 0; i--)
        {
            effectors[i].direction = Vector3.Normalize(effectors[i - 1].position - effectors[i].position);

            effectors[i - 1].position = effectors[i].position + effectors[i].direction * effectors[i].distance;
        }

        // Increment parent sub-base's target, to be averaged as above
        if(parent != null)
        {
            parent.Target += effectors[0].position;
        }

        effectors[0].position = rootPosition;
    }

    public void Backward()
    {
        for(int i = 0; i < endEffector; i++)
        {
            effectors[i].direction = Vector3.Normalize(effectors[i + 1].position - effectors[i].position);

            effectors[i + 1].position = effectors[i].position + effectors[i].direction * effectors[i + 1].distance;
        }

        if(children.Count != 0)
        {
            targetPosition = Vector3.zero;
        }
    }

    public void BackwardMulti()
    {
        Backward();

        // Sub-bases share transforms with parent chains' end effectors, update only as the end effector
        for(int i = 1; i <= endEffector; i++)
        {
            effectors[i].transform.localPosition = effectors[i - 1].transform.InverseTransformPoint(effectors[i].position);
            effectors[i].transform.localRotation = Quaternion.LookRotation(effectors[i - 1].transform.InverseTransformDirection(effectors[i].direction));
        }

        foreach(FABRIKChain child in children)
        {
            child.BackwardMulti();
        }
    }

    public Vector3 Target
    {
        get
        {
            return targetPosition;
        }

        set
        {
            targetPosition = value;
        }
    }

    public FABRIKEffector BaseEffector
    {
        get
        {
            return effectors[0];
        }
    }

    public FABRIKEffector EndEffector
    {
        get
        {
            return effectors[endEffector];
        }
    }
}

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
