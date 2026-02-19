using System.Collections.Generic;
using UnityEngine;

// Viktor Zepeda Sanchez    ID: 301609229
public static class Spline
{
    private static int substeps = 120;

    public static List<Vector2> GetSpline(List<Vector2> controlPoints, Vector2 startTangent, Vector2 endTangent)
    {
        List<Vector2> interpolated = new List<Vector2>();

        /*** Please explain your choice of spline and approach for the first and last segments here ***/
        /*** Since we're given the start and end tangents, I decided to use Hermite Interpolation for the first and last segments of the spline.
         * 
        /*** Please write your code here ***/

        /*** First segment using Hermite Interpolation
         * p0 -> p1 : controlPoints[0] -> controlPoints[1]
         * m0 : startTangent
         * m1 : endTangent
         * ***/
        Vector2 start = new Vector2();
        Vector2 end = new Vector2();
        
        for (int i = 0; i < substeps; i++)
        {
            float t = i / (float) substeps;
            float t2 = t * t;
            float t3 = t2 * t;
            start = (2*t3 - 3*t2 + 1) * controlPoints[0] + (t3 - 2*t2 + t) * startTangent +
                    (-2*t3 + 3*t2) * controlPoints[1] + (t3 - t2) * (0.5f * (controlPoints[2] - controlPoints[0]));
            interpolated.Add(start);
        }

        // Middle segments using Catmull-Rom Spline

        for (int i = 0; i < controlPoints.Count - 4; i++)
        {
            for (int j = 0; j < substeps; j++)
            {
                float t = j / (float)substeps;
                Vector2 point = Evaluate(t, controlPoints[i], controlPoints[i + 1], controlPoints[i + 2], controlPoints[i + 3]);
                interpolated.Add(point);
            }
        }
        // End segment using Hermite Interpolation
        int n = controlPoints.Count;
        
        for (int i = 0; i < substeps; i++)
        {
            float t, t2, t3;
            t = i / (float) substeps;
            t2 = t * t;
            t3 = t2 * t;
            end = (2 * t3 - 3 * t2 + 1) * controlPoints[n-2] + (t3 - 2 * t2 + t) * (0.5f * (controlPoints[n - 1] - controlPoints[n - 3])) +
                            (-2 * t3 + 3 * t2) * controlPoints[n-1] + (t3 - t2) * endTangent;
            interpolated.Add(end);
        }

        return interpolated;
    }

    private static Vector2 Evaluate(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        /*** Please write your code for Catmull-Rom spline here ***/
        /* 
         * This was my first approach but it allocates too many new vectors and a matrix so I figured its not very efficient
         * 
        Vector4 U = new Vector4(t * t * t, t * t, t, 1.0f);
        Matrix4x4 M = new Matrix4x4();
        M.SetColumn(0, new Vector4(-1.0f, 2.0f, -1.0f, 0.0f));
        M.SetColumn(1, new Vector4(3.0f, -5.0f, 0.0f, 2.0f));
        M.SetColumn(2, new Vector4(-3.0f, 4.0f, 1.0f, 0.0f));
        M.SetColumn(3, new Vector4(1.0f, -1.0f, 0.0f, 0.0f));
        Vector4 w = M.transpose * U * 0.5f;
        Vector2 point = w.x * p0 + w.y * p1 + w.z * p2 + w.w * p3;
        return point;
        */
        /*** Closed formula was a better approach  ***/
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ( 
            (2f * p1) + 
            (p2 - p0) * t + 
            (2f * p0 + 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 - 3f * p1 + 3f * p2 + p3) * t3 );
    }
}
