using UnityEngine;
using System.Collections.Generic;

public static class ForwardKinematics
{


    public static void UpdateJointPositions(Actor actor, float[] frameData)
    {
        /*** Please write your Forward Kinematics code here ***/
        // Default Unity rotation order Euler angles is ZXY 
        /*** code to be completed by students begins ***/

        /***  For example, the underlined values in the snippet below are the translation and rotational values for the Hips root joint in the 
        first frame. The next 3 values are then the rotational values for the LeftUpLeg joint, and so on.  ***/
        int frameDataIndex = 0;
        Joint root = actor.GetRootJoint();
        //Debug.Log($"Root rotation order: {root.RotateOrder}");
        float tx = frameData[frameDataIndex++];
        float ty = frameData[frameDataIndex++];
        float tz = frameData[frameDataIndex++];

        // Read root rotation from channels
        float ch0 = frameData[frameDataIndex++];
        float ch1 = frameData[frameDataIndex++];
        float ch2 = frameData[frameDataIndex++];

        // Set root transforms
        Vector3 rootTranslation = new Vector3(tx, ty, tz);
        root.LocalQuaternion = EulerToQuaternion(ch0, ch1, ch2, root.RotateOrder);
        root.GlobalQuaternion = root.LocalQuaternion;
        root.GlobalPosition = rootTranslation;
        //Debug.Log($"frameData.Length = {frameData.Length}, total joints = {actor.Joints.Count}");
        for (int i = 1; i < actor.Joints.Count; i++)
        {
            Joint child = actor.Joints[i];
            Joint parent = child.GetParent();

            if (child.RotateOrder == Joint.RotationOrder.NONE)
            {
                //Debug.Log("I am an end effector!: " + parent.Name);
                child.GlobalPosition = parent.GlobalPosition + parent.GlobalQuaternion * child.LocalPosition;
                continue;
            }

            //Debug.Log("Joint name: " + child.Name);
            if (frameDataIndex + 2 >= frameData.Length)
            {
                Debug.LogError("Not enough frame data for joint " + child.Name);
                break;
            }
            float jointCh0 = frameData[frameDataIndex++];
            float jointCh1 = frameData[frameDataIndex++];
            float jointCh2 = frameData[frameDataIndex++];

            Quaternion localRotate = EulerToQuaternion(jointCh0, jointCh1, jointCh2, child.RotateOrder);
            child.LocalQuaternion = localRotate;
            if (parent == null)
            {
                Debug.LogError("Parent joint not found for joint " + child.Name);
                //child.LocalQuaternion = localRotate;
                child.GlobalPosition = child.LocalPosition;
            }
            else
            {
                child.GlobalPosition = parent.GlobalPosition + parent.GlobalQuaternion * child.LocalPosition;
                child.GlobalQuaternion = parent.GlobalQuaternion * localRotate;

            }

        }

        /*** code to be completed by students ends ***/
    }

    private static Quaternion EulerToQuaternion(float ch0, float ch1, float ch2, Joint.RotationOrder order)
    {

        switch (order)
        {
            case Joint.RotationOrder.XYZ:
                return Quaternion.Euler(ch0, 0, 0) * Quaternion.Euler(0, ch1, 0) * Quaternion.Euler(0, 0, ch2);
            case Joint.RotationOrder.XZY:
                return Quaternion.Euler(ch0, 0, 0) * Quaternion.Euler(0, 0, ch1) * Quaternion.Euler(0, ch2, 0);
            case Joint.RotationOrder.YXZ:
                return Quaternion.Euler(0, ch0, 0) * Quaternion.Euler(ch1, 0, 0) * Quaternion.Euler(0, 0, ch2);
            case Joint.RotationOrder.YZX:
                return Quaternion.Euler(0, ch0, 0) * Quaternion.Euler(0, 0, ch1) * Quaternion.Euler(ch2, 0, 0);
            case Joint.RotationOrder.ZXY:
                return Quaternion.Euler(0, 0, ch0) * Quaternion.Euler(ch1, 0, 0) * Quaternion.Euler(0, ch2, 0);
            case Joint.RotationOrder.ZYX:
                return Quaternion.Euler(0, 0, ch0) * Quaternion.Euler(0, ch1, 0) * Quaternion.Euler(ch2, 0, 0);
            default: // Joint.RotationOrder.NONE
                return Quaternion.identity;
        }
    }

}