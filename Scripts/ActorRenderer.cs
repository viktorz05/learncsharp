using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ActorRenderer : MonoBehaviour
{
    private Actor Actor;

    private List<Transform> JointSpheres;
    private List<Transform> Bones;
    private GameObject footTargetContainer;
    private List<GameObject> footContactMarkers = new List<GameObject>();

    public Color BoneColor = Color.gray;
    public Color JointColor = Color.red;

    public bool VisualizeIKTargets = false;
    private bool targetsVisible = false;

    private Transform CreateJointObject(Joint joint)
    {
        Transform joint_ball = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        Destroy(joint_ball.GetComponent<SphereCollider>());
        joint_ball.parent = transform;
        joint_ball.GetComponent<Renderer>().material.color = JointColor;

        return joint_ball;
    }

    private Transform CreateBoneObject(Joint joint)
    {
        Transform bone = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
        Destroy(bone.GetComponent<CapsuleCollider>());
        bone.parent = transform;
        bone.GetComponent<Renderer>().material.color = BoneColor;

        return bone;
    }

    private void InitializeSkeleton()
    {
        JointSpheres = new List<Transform>();
        Bones = new List<Transform>();

        JointSpheres.Add(CreateJointObject(Actor.GetRootJoint()));
        Bones.Add(null);

        foreach (Joint joint in Actor.Joints.Skip(1))
        {
            JointSpheres.Add(CreateJointObject(joint));
            Bones.Add(CreateBoneObject(joint));
        }
    }

    private GameObject CreateTargetObject(Vector3 target, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = target * Actor.ActorScale;
        sphere.transform.localScale = Vector3.one * 0.2f;
        var renderer = sphere.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = color;
        Destroy(sphere.GetComponent<Collider>());
        return sphere;
    }

    private void InitializeFootContactTargets()
    {
        footTargetContainer = new GameObject("FootContactTargets");
        foreach (var target in Actor.LeftFootTargets)
        {
            if (target != null)
            {
                var targetSphere = CreateTargetObject(target.Value, Color.green);
                targetSphere.transform.parent = footTargetContainer.transform;
                footContactMarkers.Add(targetSphere);
            }
        }
        foreach (var target in Actor.RightFootTargets)
        {
            if (target != null)
            {
                var targetSphere = CreateTargetObject(target.Value, Color.blue);
                targetSphere.transform.parent = footTargetContainer.transform;
                footContactMarkers.Add(targetSphere);
            }
        }
    }

    private void UpdateSkeletonPosition()
    {
        /*** Please write your code here ***/
        /*** code to be completed by students begins ***/

        // Update each JointSphere's position

        // Update each bone's position and rotation

        /*** code to be completed by students ends ***/

        // DO NOT REMOVE: Necessary to apply the required scaling
        ApplyActorScale();
    }

    private void ApplyActorScale()
    {
        if (JointSpheres.Count > 0)
        {
            JointSpheres[0].localScale = Vector3.one * 2.0f * Actor.BoneWidth * Actor.ActorScale;
            JointSpheres[0].position *= Actor.ActorScale;
        }

        for (int i = 1; i < Actor.Joints.Count; i++)
        {
            var joint = Actor.Joints[i];

            JointSpheres[i].localScale = Vector3.one * 2.0f * Actor.BoneWidth * Actor.ActorScale;
            JointSpheres[i].position *= Actor.ActorScale;

            var boneLength = Vector3.Magnitude(joint.LocalPosition);
            Bones[i].localScale = new Vector3(Actor.BoneWidth, boneLength / 2, Actor.BoneWidth) * Actor.ActorScale;
            Bones[i].position *= Actor.ActorScale;
        }
    }

    private void VisualizeFootContactTargets(bool isVisible)
    {
        foreach (var targetMarker in footContactMarkers)
        {
            targetMarker.SetActive(isVisible);
        }
    }

    private void Start()
    {
        Actor = GetComponent<Actor>();
        InitializeSkeleton();
        InitializeFootContactTargets();
        VisualizeFootContactTargets(VisualizeIKTargets);
    }

    private void Update()
    {
        UpdateSkeletonPosition();
        if (Input.GetKeyDown(KeyCode.T))
        {
            VisualizeIKTargets = !VisualizeIKTargets;
        }
        if (VisualizeIKTargets != targetsVisible)
        {
            VisualizeFootContactTargets(VisualizeIKTargets);
            targetsVisible = VisualizeIKTargets;
        }
    }
}
