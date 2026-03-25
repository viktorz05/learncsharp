using UnityEngine;

/// <summary>
/// Rope is a 1D chain of particles; first particle is pinned at anchor position.
/// Modify the parameters below to change the rope's behavior.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class RopeSimulation : MonoBehaviour
{
    public StateController controller;

    [Header("Rope Setup")]
    [Tooltip("Number of particles in the rope (segments = numParticles - 1)")]
    public int numParticles = 5;
    [Tooltip("Rest length of each segment")]
    public float segmentLength = 0.0f;
    [Tooltip("World position of the fixed anchor (first particle)")]
    public Vector3 anchorPosition = new Vector3(0f, 3f, 0f);

    [Header("Mouse Follow")]
    [Tooltip("Anchor (first particle) follows mouse position on a horizontal plane")]
    public bool followMouse = true;
    [Tooltip("Camera for mouse-to-world projection (leave empty to use Main Camera)")]
    public Camera cam;
    [Tooltip("Height of the plane used to project mouse position")]
    public float mousePlaneHeight = 3f;
    [Tooltip("Smoothing: lower = heavier/slower anchor follow")]
    [Range(0.0f, 0.0f)]
    public float anchorSmoothSpeed = 0.12f;

    [Header("Simulation")]
    public float g = 9.81f;
    [Tooltip("Iterations per step for constraint satisfaction (higher = stiffer)")]
    public int constraintIterations = 0;
    [Tooltip("Velocity damping per step (higher = heavier, less swing)")]
    [Range(0f, 0.0f)]
    public float damping = 0.0f;
    [Tooltip("Mass gradient along rope: lower particles move less, feels heavier (0 = uniform)")]
    [Range(0f, 0f)]
    public float massGradient = 0.0f;
    [Tooltip("Ground plane height (particles cannot go below this)")]
    public float groundHeight = 0f;
    [Tooltip("Key to toggle rope visibility (optional)")]
    public KeyCode visibilityToggleKey = KeyCode.H;

    // Current and previous positions (world space) for Verlet integration
    protected Vector3[] positions;
    protected Vector3[] prevPositions;

    protected float restLength;
    protected LineRenderer lineRenderer;

    protected virtual void Awake()
    {
        restLength = segmentLength;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numParticles;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = new Color(1f, 1f, 1f, 1f);
        lineRenderer.enabled = false;
    }

    public void ResetState()
    {
        if (positions == null || positions.Length != numParticles)
        {
            positions = new Vector3[numParticles];
            prevPositions = new Vector3[numParticles];
        }

        for (int i = 0; i < numParticles; i++)
        {
            positions[i] = anchorPosition + Vector3.down * (i * restLength);
            prevPositions[i] = positions[i];
        }
    }

    public void AdvanceTimeStep()
    {
        if (positions == null || controller == null) return;
        Integrate(controller.dt);
    }

    private void Integrate(float dt)
    {
        /*** Part 5: Rope Simulation ***/
        Vector3 a = new Vector3(0, -g, 0);
        for (int i = 1; i < numParticles; i++) {
            prevPositions[i] = positions[i];
            positions[i] = positions[i] + (1f - damping) * (positions[i] - prevPositions[i]) + (dt*dt) * a;

        }
        positions[0] = anchorPosition;
        prevPositions[0] = anchorPosition;
        for (int iter = 0; iter < constraintIterations; iter++) {
            for (int i = 0; i < numParticles - 1; i++) {
                Vector3 dl = positions[i+1] - positions[i];
                float length = dl.magnitude;
            }
        }
        for (int i = 1; i < numParticles; i++) {
            if (positions[i].y <= groundHeight) {
                positions[i].y = groundHeight;
                prevPositions[i].y = groundHeight;
            }
        }

        /*** Part 5 ends ***/

        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer == null || positions == null) return;
        for (int i = 0; i < numParticles; i++)
            lineRenderer.SetPosition(i, positions[i]);
    }

    void Start()
    {
        if (cam == null) cam = Camera.main;
        ResetState();
        UpdateLineRenderer();
    }

    /// <summary>Toggle rope line visibility (simulation keeps running).</summary>
    public void ToggleVisibility()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = !lineRenderer.enabled;
    }

    /// <summary>Set rope line visible (true) or hidden (false).</summary>
    public void SetVisible(bool visible)
    {
        if (lineRenderer != null)
            lineRenderer.enabled = visible;
    }

    void Update()
    {
        if (visibilityToggleKey != KeyCode.None && Input.GetKeyDown(visibilityToggleKey))
            ToggleVisibility();
        if (followMouse && cam != null)
        {
            Vector3 target = GetMouseWorldPosition();
            anchorPosition = Vector3.Lerp(anchorPosition, target, anchorSmoothSpeed);
            if (positions != null && positions.Length > 0)
            {
                positions[0] = anchorPosition;
                prevPositions[0] = anchorPosition;
            }
        }
    }

    /// <summary>
    /// Project mouse position onto a horizontal plane at mousePlaneHeight.
    /// </summary>
    protected Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0f, mousePlaneHeight, 0f));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);
        return anchorPosition;
    }

    void FixedUpdate()
    {
        if (controller != null && !controller.simPaused)
            AdvanceTimeStep();
    }
}
