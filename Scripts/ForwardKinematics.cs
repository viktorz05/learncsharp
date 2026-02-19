using UnityEngine;
using System.Collections.Generic;

public static class ForwardKinematics
{
    public static void UpdateJointPositions(Actor actor, float[] frameData)
    {
        /*** Please write your Forward Kinematics code here ***/
        // Default Unity rotation order Euler angles is ZXY 
        /*** code to be completed by students begins ***/

        Stack<Joint> s = new Stack<Joint>();
        Joint root = actor.GetRootJoint();
        s.Push(root);
        while (s.Count > 0) {
          Joint j = s.Pop();
          Joint parent = j.GetParent();
          Quaternion frameRotation = EulerToQuaternion(frameData[3], frameData[4], frameData[5], j.RotateOrder);
          j.GlobalPosition = parent.GlobalPosition + parent.GlobalQuaternion * j.LocalPosition; 
          j.GlobalQuaternion = parent.GlobalQuaternion * frameRotation; 
          j.LocalQuaternion *=  parent.LocalQuaternion;
          
          foreach (Joint child in j.GetChildren()) {
            s.Push(child);
          }
        }

        /*** code to be completed by students ends ***/
    }

    private static Quaternion EulerToQuaternion(float xr, float yr, float zr, Joint.RotationOrder order) {

      Quaternion qx = Quaternion.AngleAxis(xr, Vector3.right);
      Quaternion qy = Quaternion.AngleAxis(yr, Vector3.up);
      Quaternion qz = Quaternion.AngleAxis(zr, Vector3.forward);

      switch (order) {
        case Joint.RotationOrder.XYZ: 
          return qx * qy * qz;
        case Joint.RotationOrder.XZY: 
          return qx * qz * qy;
        case Joint.RotationOrder.YXZ: 
          return qy * qx * qz;
        case Joint.RotationOrder.YZX: 
          return qy * qz * qx;
        case Joint.RotationOrder.ZXY: 
          return qz * qx * qy;
        case Joint.RotationOrder.ZYX: 
          return qz * qy * qx;
        default:
          return Quaternion.identity;
        } 
    }
}
