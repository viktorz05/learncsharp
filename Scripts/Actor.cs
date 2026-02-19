using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Actor : MonoBehaviour
{
	private float[][] FrameData; // Jagged array 
	private float FrameTime;
	private int NumFrames;
	private bool PauseFrame;
    private List<Transform> Stairs;

	private String IK_CLIP_NAME = "walk_sloped.bvh";
	private String FOOT_SLIDING_CLIP_NAME = "dance_corrupted.bvh";

    [Range(0.0f, 1.0f)]
    public float ActorScale = 0.04f;
    public float BoneWidth = 1f;

    public string BVHFilePath;
	public int CurrentFrame;

    [Header("Foot IK")]
    public GameObject StairParent;
	[HideInInspector]
    public bool ApplyIk = false;

    [HideInInspector]
    public List<Joint> Joints;

    [HideInInspector]
    public List<Joint> FootEndEffectors;

    [HideInInspector]
    public bool isIKClip = false;

    [HideInInspector]
    public Vector3?[][] FootTargets;
    [HideInInspector]
    public Vector3?[] LeftFootTargets;
    [HideInInspector]
    public Vector3?[] RightFootTargets;

    private void LoadBVH()
	{
		var bp = new BVHParser(File.ReadAllText(BVHFilePath));

		FrameTime = bp.frameTime;
		NumFrames = bp.frames;

		FrameData = new float[NumFrames][];
		var channels = bp.GetChannels();
		for (var frameIdx = 0; frameIdx < NumFrames; frameIdx++)
		{
			FrameData[frameIdx] = new float[channels.Length];

			for (var channelIdx = 0; channelIdx < channels.Length; channelIdx++)
			{
				FrameData[frameIdx][channelIdx] = channels[channelIdx][frameIdx];
			}
		}

		Joints = new List<Joint>();

		Func<BVHParser.BVHBone, Joint, int> recursion = null;
		recursion = (bvhBone, parentBone) => {
			bool isRoot = (parentBone == null);

			// create new joint
			Joint joint = new Joint(this);
			Joints.Add(joint);
			joint.Index = Joints.Count() - 1;
			joint.Name = bvhBone.name;

			// set joint local offset
			joint.LocalPosition = new Vector3(bvhBone.offsetX, bvhBone.offsetY, bvhBone.offsetZ);

			// compute rotation order for children
			string order = (!isRoot)
				? string.Join("", bvhBone.channelOrder.Take(3))
				: string.Join("", bvhBone.channelOrder.Skip(3).Take(3));

			switch (order)
			{
				case "345":
					joint.RotateOrder = Joint.RotationOrder.XYZ;
					break;
				case "354":
					joint.RotateOrder = Joint.RotationOrder.XZY;
					break;
				case "435":
					joint.RotateOrder = Joint.RotationOrder.YXZ;
					break;
				case "453":
					joint.RotateOrder = Joint.RotationOrder.YZX;
					break;
				case "534":
					joint.RotateOrder = Joint.RotationOrder.ZXY;
					break;
				case "543":
					joint.RotateOrder = Joint.RotationOrder.ZYX;
					break;
				default:
					joint.RotateOrder = Joint.RotationOrder.NONE;
					break;
			}

			// set joint parent
			if (!isRoot)
			{
				joint.ParentIdx = parentBone.Index;
			}

			// recursively call children
			parentBone = joint;
			foreach (var child in bvhBone.children)
			{
				var childIdx = recursion(child, parentBone);
				joint.ChildrenIdx.Add(childIdx);
			}

			return joint.Index;
		};
		recursion(bp.root, null);

        FootEndEffectors = new List<Joint>()
        {
            Joints.Find(j => j.Name.ToLower().Contains("site") && j.GetParent().Name.ToLower().Equals("lefttoe")),
            Joints.Find(j => j.Name.ToLower().Contains("site") && j.GetParent().Name.ToLower().Equals("righttoe")),
        };
    }

	private (float minZ, float maxZ, float height) GetStairAttributes(int stairIdx)
	{
        float height = (Stairs[stairIdx].localScale.y / 2) / ActorScale;
        float minZ = (Stairs[stairIdx].position.z - StairParent.transform.localScale.z * (Stairs[stairIdx].localScale.z / 2f)) / ActorScale;
        float maxZ = (Stairs[stairIdx].position.z + StairParent.transform.localScale.z * (Stairs[stairIdx].localScale.z / 2f)) / ActorScale;
		return (minZ, maxZ, height);
    }

	public void DetectFootPenetration()
	{
		/*** Please write your code for foot-penetration with stairs here ***/
        /*** code to be completed by students begins ***/


        /*** code to be completed by students ends ***/
	}

	public void DetectFootSliding()
	{
		/*** Please write your code for foot-sliding here ***/
        /*** code to be completed by students begins ***/


        /*** code to be completed by students ends ***/
	}

    private void InitializeStairs()
    {
        Stairs = new List<Transform>();

        foreach (Transform child in StairParent.transform)
        {
            Stairs.Add(child);
        }
    }

    private void SetActorScaleAndBoneWidth()
    {
        ActorScale = 1.0f;
        var legJoint = Joints.Find(x => x.Name.Contains("Leg"));
        if (legJoint is not null)
        {
            ActorScale = 0.5f / Vector3.Magnitude(legJoint.LocalPosition);
            BoneWidth = Mathf.Ceil(Vector3.Magnitude(legJoint.LocalPosition) * 0.2f);
        }
    }

    private void UpdateJointsPosition()
    {
		ForwardKinematics.UpdateJointPositions(this, FrameData[CurrentFrame % NumFrames]);
    }

	public Joint GetRootJoint()
    {
		return Joints[0];
    }

	void Start()
    {
		LoadBVH();
        Time.fixedDeltaTime = FrameTime;
        SetActorScaleAndBoneWidth();
		StairParent.SetActive(false);
        LeftFootTargets = new Vector3?[NumFrames];
        RightFootTargets = new Vector3?[NumFrames];
        FootTargets = new Vector3?[][] { LeftFootTargets, RightFootTargets };
        if (BVHFilePath.ToLower().Contains(IK_CLIP_NAME))
		{
			isIKClip = true;
			ActorScale = 0.04f;
            StairParent.SetActive(true);
            InitializeStairs();
            DetectFootPenetration();
        }
        else if (BVHFilePath.ToLower().Contains(FOOT_SLIDING_CLIP_NAME))
        {
            isIKClip = true;
            DetectFootSliding();
        }
        CurrentFrame = 0;
        UpdateJointsPosition();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
			PauseFrame = !PauseFrame;
		}
		if (Input.GetKeyDown(KeyCode.I) && isIKClip)
		{
            ApplyIk = !ApplyIk;
			LoadBVH();
        }
    }

	void FixedUpdate()
	{
		if (!ApplyIk)
		{
			UpdateJointsPosition();
			if (!PauseFrame)
			{
				CurrentFrame = (CurrentFrame + 1) % NumFrames;
			}
		}
		else
		{
            UpdateJointsPosition();
            for (int i = 0; i < FootEndEffectors.Count; i++)
			{
				var endEffector = FootEndEffectors[i];
				var target = FootTargets[i][CurrentFrame];
				if (target != null)
				{
                    InverseKinematics.ApplyIK(this, endEffector, target.Value);
                }
			}
            if (!PauseFrame)
            {
                CurrentFrame = (CurrentFrame + 1) % NumFrames;
            }
        }
    }

}

[Serializable]
public class Joint
{
	public Actor Actor;
	public string Name;
	public int Index;
	public int ParentIdx;
	public List<int> ChildrenIdx;

	public Vector3 LocalPosition;
	public Vector3 GlobalPosition;
	public Quaternion LocalQuaternion;
	public Quaternion GlobalQuaternion;
	public RotationOrder RotateOrder;

	public enum RotationOrder
	{
		NONE, XYZ, XZY, YXZ, YZX, ZXY, ZYX
	}

	public Joint(Actor actor)
	{
		Actor = actor;
		ParentIdx = -1;
		ChildrenIdx = new List<int>();
		LocalQuaternion = Quaternion.identity;
		GlobalQuaternion = Quaternion.identity;
		RotateOrder = RotationOrder.NONE;
	}

	public Joint GetParent()
	{
		return ParentIdx == -1 ? null : Actor.Joints[ParentIdx];
	}

	public List<Joint> GetChildren()
	{
		return ChildrenIdx.ConvertAll(idx => Actor.Joints[idx]);
	}
}
