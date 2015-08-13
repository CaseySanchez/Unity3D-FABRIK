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
			
			/*if(i > 0)
			{
				effectors[i].ApplyConstraint(effectors[i - 1].direction, 70.0F);
			}
			else if(parent != null)
			{
				effectors[i].ApplyConstraint(parent.EndEffector.direction, 70.0F);
			}*/
			
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
