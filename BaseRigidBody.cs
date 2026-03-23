using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseRigidbody : MonoBehaviour
{
    public StateController controller;

    public GameObject slopedPlane;

    public Vector3 initialPosition;
    public Vector3 initialRotation;
    public Vector3 initialVelocity;

    public float mass;
    public float ks;
    public float kd;
    public float mu;
    public float g = 9.81f;

    protected Vector3 position; // p
    protected Quaternion rotation; // q
    protected Vector3 linear_velocity; // v
    protected Vector3 angular_velocity; // w

    protected abstract Vector3[] LocalVertices { get; }
    protected abstract Matrix3x3 InertiaRefMatrix { get; }

    private List<(Vector3, Vector3, float)> GetCollidedVertices()
    {
        // Each tuple contains: collided vertex, surface normal, penetration depth
        List<(Vector3, Vector3, float)> CollidedVertices = new List<(Vector3, Vector3, float)>();

        /*** Part 2: Collision Detection with the slope and ground ***/
        Vector3 n = slopedPlane.transform.up;
        Vector3 p0 = slopedPlane.transform.position;
        foreach (Vector3 p in LocalVertices)
        {
            Vector3 global = transform.TransformPoint(p);

            if (global.y <= 0.0f) // ground collision
            {
                CollidedVertices.Add((global, Vector3.up, Mathf.Abs(global.y)));
                continue;
            }
            
            float dist = Vector3.Dot(n, (global - p0)); // slope collision

            if (dist <= 0.0f)
            {
                CollidedVertices.Add((global, n, dist));
            }
           
        }

        /*** part 2 coding ends ***/

        return CollidedVertices;
    }

    private (Vector3, Vector3) ComputeForceAndTorque()
    {
        Vector3 netForce = Vector3.zero;
        Vector3 netTorque = Vector3.zero;

        /*** Part 3: Calculate Forces and Torques ***/
        // Need repulsion force and dynamic friction force
        List<(Vector3 collided, Vector3 normal, float depth)> vertices = GetCollidedVertices();

        Vector3 fn = Vector3.zero;
        Vector3 ff = Vector3.zero;
        Vector3 f_i = Vector3.zero; // force accumulator
        Vector3 w = new Vector3(0.0f, -g, 0.0f);
        Matrix3x3 R = new Matrix3x3(rotation);

        // -mg
        netForce += mass*w;
        foreach ((Vector3 collided, Vector3 normal, float depth) v in vertices)
        {
            
            fn = ks * v.depth * v.normal - kd * Vector3.Dot(v.normal, linear_velocity) * v.normal;
            Vector3 vt = linear_velocity - Vector3.Dot(linear_velocity, v.normal) * v.normal;

            if (vt.magnitude > 0.001f)
                ff = -mu * fn.magnitude * vt.normalized;
            else
                ff = Vector3.zero;
            Debug.Log($"Friction: {ff}");

            f_i = ff + fn;
            netForce += f_i;

            Vector3 r = v.collided - position;
            netTorque += Vector3.Cross(R*r, f_i);
        }
        /*** part 3 coding ends ***/
        //Debug.Log($"Net torque: {netTorque}");
        return (netForce, netTorque);
    }

    private void Integrate(float deltaTime)
    {
        var (force, torque) = ComputeForceAndTorque();

        /*** Part 4: Integrate Timestep ***/
        Matrix3x3 R = new Matrix3x3(rotation);
        Matrix3x3 I = R * InertiaRefMatrix * R.transpose;

        linear_velocity += deltaTime * force / mass; // F = ma -> a = F/m
        position += deltaTime * linear_velocity;

        Vector3 IxW = Vector3.Cross(angular_velocity, I * angular_velocity);
        angular_velocity += deltaTime * (I.inverse * (torque - IxW));

        Quaternion wq = new Quaternion(
            angular_velocity.x, 
            angular_velocity.y, 
            angular_velocity.z,
            0f
        );

        Quaternion lhs = wq * rotation;

        rotation = Quaternion.Normalize(
            new Quaternion(
                rotation.x + 0.5f*deltaTime*lhs.x,
                rotation.y + 0.5f*deltaTime*lhs.y,
                rotation.z + 0.5f*deltaTime*lhs.z,
                rotation.w + 0.5f*deltaTime*lhs.w
            )
        );
        /*** part 4 coding ends ***/
    }

    public void AdvanceTimeStep()
    {
        Integrate(controller.dt);
    }

    public void ResetState()
    {
        position = initialPosition;
        linear_velocity = initialVelocity;
        rotation = Quaternion.Euler(initialRotation);
        angular_velocity = Vector3.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetState();
    }

    // FixedUpdate is called every fixed frame-rate frame
    // Read more here: https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html
    void FixedUpdate()
    {
        if (!controller.simPaused)
        {
            AdvanceTimeStep();
        }
        transform.position = position;
        transform.rotation = rotation;
        Time.fixedDeltaTime = controller.dt;
    }
}
