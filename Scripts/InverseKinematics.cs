using System;
using System.Collections.Generic;
using UnityEngine;

public static class InverseKinematics
{
    public static void ApplyIK(Actor actor, Joint endEffector, Vector3 targetPosition)
    {
        /*** Please write your Inverse Kinematics code here ***/
        /*** Add a brief explanation of the algorithm you choose and any other design decisions you make ***/

        /*** code to be completed by students begins ***/
        Debug.Log("endEffector: " + endEffector.Name);
        Debug.Log("parent: " + endEffector.GetParent()?.Name);
        // Build the IK chain
        List<Joint> ikJoints = new List<Joint>();
        Joint iter = endEffector.GetParent();
        while (iter != null)
        {
            ikJoints.Add(iter);
            if (iter.GetParent() == null) break;                          // reached root
            if (iter.Name.ToLower().Contains("up")) break;   // found stop joint
            iter = iter.GetParent();
        }
        foreach (Joint j in ikJoints) {
            Debug.Log($"IK chain: {j.Name}");
        }
        int maxIter = 20;
        float threshold = 0.01f;

        // Apply the chosen algorithm: CCD
        // Based on: https://rodolphe-vaillant.fr/entry/114/cyclic-coordonate-descent-inverse-kynematic-ccd-ik
        for (int i = 0; i < maxIter; i++)
        {
            if ((endEffector.GlobalPosition - targetPosition).magnitude < threshold) break; // close enough

            for (int j = 0; j < ikJoints.Count; j++)
            {
                Joint joint = ikJoints[j];

                // e_t, t_i
                Vector3 toEndEffector = endEffector.GlobalPosition - joint.GlobalPosition;
                Vector3 toTarget = targetPosition - joint.GlobalPosition;
                Quaternion rotationToTarget = Quaternion.FromToRotation(toEndEffector.normalized, toTarget.normalized);

                Joint parent;
                Quaternion parentGlobalRotation;
                Quaternion localRotationToTarget;
                if ((parent = joint.GetParent()) != null)
                {
                    // Convert rotation to local space
                    parentGlobalRotation = parent.GlobalQuaternion;
                    localRotationToTarget = Quaternion.Inverse(parentGlobalRotation) * rotationToTarget * parentGlobalRotation;
                    joint.LocalQuaternion = localRotationToTarget * joint.LocalQuaternion;
                }
                else
                {
                    parentGlobalRotation = Quaternion.identity;
                    localRotationToTarget = Quaternion.Inverse(parentGlobalRotation) * rotationToTarget * parentGlobalRotation;
                    joint.LocalQuaternion = localRotationToTarget * joint.LocalQuaternion; // apply rotation in local space
                }
                // Update global positions of the end effector and all joints in the chain
                UpdateGlobalPositions(actor.GetRootJoint());
                if ((endEffector.GlobalPosition - targetPosition).magnitude < threshold) break; // close enough
            }
        }
        /*** code to be completed by students ends ***/
    }
    private static void UpdateGlobalPositions(Joint joint)
    {
        if (joint.GetParent() == null)
        {
            joint.GlobalQuaternion = joint.LocalQuaternion;
        }
        else
        {
            Joint parent = joint.GetParent();
            joint.GlobalPosition = parent.GlobalPosition + parent.GlobalQuaternion * joint.LocalPosition;
            joint.GlobalQuaternion = parent.GlobalQuaternion * joint.LocalQuaternion;
        }
        foreach (Joint child in joint.GetChildren())
        {
            UpdateGlobalPositions(child);
        }
    }
}