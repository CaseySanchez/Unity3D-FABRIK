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

    public float sqrThreshold = 0.01F;

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
            //MeshFilter meshFilter = endEffector.gameObject.GetComponent<MeshFilter>();

            //Bounds bounds = meshFilter.mesh.bounds;

            GameObject gameObject = new GameObject();

            gameObject.hideFlags = HideFlags.DontSave;

            gameObject.transform.parent = endEffector.transform;
            gameObject.transform.localPosition = endEffector.offset;//bounds.center + Vector3.Scale(bounds.extents, endEffector.offset);

            effectors.Add(gameObject.AddComponent<FABRIKEffector>());
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

    public void Backward()
    {
        // Store the original position to be reset below
        Vector3 origin = parent == null ? BaseEffector.transform.position : BaseEffector.Position;

        // Sub-base, average for centroid
        if (children.Count > 1)
        {
            Target /= (float)children.Count;
        }

        if ((EndEffector.Position - Target).sqrMagnitude > sqrThreshold)
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
            parent.Target += BaseEffector.Position * EndEffector.Weight;// * 0.5f;
        }

        // Reset initial effector to origin
        BaseEffector.Position = origin;
    }

    public void Forward()
    {
        effectors[1].Position = BaseEffector.Position + BaseEffector.Rotation * Vector3.forward * BaseEffector.Length;

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
            Vector3 direction = Vector3.zero;

            foreach(FABRIKChain child in children)
            {
                direction += Vector3.Normalize(child.effectors[1].Position - EndEffector.Position);
            }

            direction /= (float)children.Count;

            EndEffector.ApplyConstraints(direction);
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
