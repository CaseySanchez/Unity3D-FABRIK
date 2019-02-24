using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FABRIKChain
{
    private FABRIKChain parent = null;

    private List<FABRIKChain> children = new List<FABRIKChain>();

    private List<FABRIKEffector> effectors;

    private int layer;

    private bool isEndChain;

    public float threshold = 0.1F;

    public FABRIKChain(FABRIKChain parent, List<FABRIKEffector> effectors, int layer)
    {
        FABRIKEffector endEffector = effectors[effectors.Count - 1];

        this.isEndChain = endEffector.transform.childCount == 0;

        // If the current endEffector has no children, then the chain is considered an "end chain"
        // Add a NEW end effector that is offset from the CURRENT end effector's position ...
        // ... where the offset reflects its positioning within the CURRENT end effector's bounding box
        // e.g. Vector3.zero is centered in the box
        if(this.isEndChain)
		{
			effectors.Add(CreateEndEffector(endEffector));
		}

        // Now that we have all effectors accounted for, calculate the length of each segment
        for (int i = 1; i < effectors.Count; i++)
        {
            effectors[i - 1].Length = Vector3.Distance(effectors[i].transform.position, effectors[i - 1].transform.position);
        }

        this.effectors = new List<FABRIKEffector>(effectors);

        this.layer = layer;

        // Add this chain to the parent chain's children
        if (parent != null)
        {
            this.parent = parent;

            parent.children.Add(this);
        }
    }

    private FABRIKEffector CreateEndEffector(FABRIKEffector parent)
    {
        MeshFilter meshFilter = parent.gameObject.GetComponent<MeshFilter>();

        Bounds bounds = meshFilter.mesh.bounds;

        GameObject gameObject = new GameObject();

        gameObject.hideFlags = HideFlags.DontSave;

        gameObject.transform.parent = parent.transform;
        gameObject.transform.localPosition = bounds.center + Vector3.Scale(bounds.extents, parent.offset);

        foreach (Transform child in parent.transform)
        {
            if (child != gameObject.transform)
            {
                child.parent = gameObject.transform;
            }
        }

        return gameObject.AddComponent<FABRIKEffector>();
    }

    public void Backward()
    {
        // Store the original position to be reset below
        // This is done to calculate the Forward iteration
        Vector3 origin = effectors[0].Position;

        // Sub-base, average for centroid
        if (children.Count != 0)
        {
            Target /= (float)children.Count;
        }

        if (Vector3.Distance(EndEffector.Position, Target) > threshold)
        {
            // Set the end effector Position to Target to calculate the Backward iteration
            EndEffector.Position = Target;

            for (int i = effectors.Count - 2; i >= 0; i--)
            {
                Vector3 direction = Vector3.Normalize(effectors[i].Position - effectors[i + 1].Position);

                effectors[i].Position = effectors[i + 1].Position + direction * effectors[i].Length;
            }
        }

        // Increment parent sub-base's target, to be averaged as above
        if (parent != null)
        {
            parent.Target += effectors[0].Position * effectors[0].weight;// * 0.5f;
        }

        // Reset initial effector to origin
        effectors[0].Position = origin;
    }

    public void Forward()
    {
        if (parent != null)
        {
            Vector3 direction = (effectors[1].Position - effectors[0].Position).normalized;

            effectors[1].Position = effectors[0].transform.position + direction * effectors[0].Length;

            effectors[0].Rotation = Quaternion.LookRotation(direction);
        }

        for (int i = 2; i < effectors.Count; i++)
        {
            Vector3 direction = Vector3.Normalize(effectors[i].Position - effectors[i - 1].Position);
                        
            effectors[i - 1].ApplyConstraints(direction);

            effectors[i].Position = effectors[i - 1].Position + effectors[i - 1].Rotation * Vector3.forward * effectors[i - 1].Length;
        }

        // This is a sub-base, reset Target to zero to be recalculated in Backward
        if (children.Count != 0)
        {
            Target = Vector3.zero;

            // In order to constrain a sub-base end effector, we must average the directions of its children
            Vector3 average = Vector3.zero;

            foreach(FABRIKChain child in children)
            {
                Vector3 direction = (child.effectors[1].Position - EndEffector.Position).normalized;

                child.effectors[1].Position = EndEffector.Position + direction * EndEffector.Length;         

                average += direction;
            }

            average /= (float)children.Count;

            EndEffector.ApplyConstraints(average);
        }
    }

    public void ForwardMulti()
    {
        Forward();

        for (int i = 1; i < effectors.Count; i++)
        {
            effectors[i].UpdateTransform();
        }

        foreach (FABRIKChain child in children)
        {
            child.ForwardMulti();
        }
    }

    public bool IsEndChain
    {
        get
        {
            return isEndChain;
        }
    }

    public int Layer
    {
        get
        {
            return layer;
        }
    }

    public Vector3 Target
    {
        get;
        set;
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
            return effectors[effectors.Count - 1];
        }
    }
}
