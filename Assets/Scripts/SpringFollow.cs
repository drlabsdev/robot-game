using UnityEngine;

/// <summary>
/// SpringFollow — smoothly springs this Transform toward a target Transform.
/// Attach to the object you want to move. Assign a Target in the Inspector.
/// </summary>
public class SpringFollow : MonoBehaviour
{
    // ── Target ────────────────────────────────────────────────────────────────
    [Header("Target")]
    [Tooltip("The Transform this object will spring toward.")]
    public Transform target;

    // ── Spring Settings ───────────────────────────────────────────────────────
    [Header("Spring Settings")]

    [Tooltip("How stiff the spring is. Higher = snappier response.")]
    [Range(0f, 500f)]
    public float stiffness = 100f;

    [Tooltip("How much the spring resists oscillation. Higher = less bouncy.")]
    [Range(0f, 50f)]
    public float damping = 10f;

    [Tooltip("Mass of the simulated object. Higher = slower, heavier feel.")]
    [Range(0.01f, 10f)]
    public float mass = 1f;

    // ── Position / Rotation / Scale Toggles ──────────────────────────────────
    [Header("What to Spring")]
    public bool springPosition = true;
    public bool springRotation = false;
    public bool springScale    = false;

    // ── Offset ────────────────────────────────────────────────────────────────
    [Header("Offset (local to target)")]
    [Tooltip("Positional offset applied relative to the target's local space.")]
    public Vector3 positionOffset = Vector3.zero;

    // ── Clamp / Limits ────────────────────────────────────────────────────────
    [Header("Velocity Limits")]
    [Tooltip("Maximum speed the spring can reach (0 = unlimited).")]
    public float maxVelocity = 0f;

    // ── Runtime State (read-only in Inspector) ────────────────────────────────
    [Header("Runtime State (read-only)")]
    [SerializeField, HideInInspector] private Vector3    _posVelocity    = Vector3.zero;
    [SerializeField, HideInInspector] private Vector3    _scaleVelocity  = Vector3.zero;
    [SerializeField, HideInInspector] private Vector3    _rotVelocity    = Vector3.zero; // Euler-based

    // Exposed for debugging
    [SerializeField] private Vector3 _debugPosVelocity;
    [SerializeField] private Vector3 _debugRotVelocity;

    // ─────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (target == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // ── Position Spring ───────────────────────────────────────────────────
        if (springPosition)
        {
            Vector3 goalPos = target.TransformPoint(positionOffset);
            transform.position = StepSpringVector3(
                transform.position, goalPos, ref _posVelocity, dt);
        }

        // ── Rotation Spring ───────────────────────────────────────────────────
        if (springRotation)
        {
            Vector3 currentEuler = transform.eulerAngles;
            Vector3 targetEuler  = target.eulerAngles;

            // Wrap each axis to avoid 0↔360 jumps
            Vector3 deltaEuler = WrapEuler(targetEuler - currentEuler);
            Vector3 goalEuler  = currentEuler + deltaEuler;

            Vector3 newEuler = StepSpringVector3(
                currentEuler, goalEuler, ref _rotVelocity, dt);
            transform.eulerAngles = newEuler;
            _debugRotVelocity = _rotVelocity;
        }

        // ── Scale Spring ──────────────────────────────────────────────────────
        if (springScale)
        {
            transform.localScale = StepSpringVector3(
                transform.localScale, target.localScale, ref _scaleVelocity, dt);
        }

        _debugPosVelocity = _posVelocity;
    }

    // ── Spring Integrator (per-axis Vector3) ──────────────────────────────────
    /// <summary>
    /// Semi-implicit Euler spring step for a Vector3 value.
    /// </summary>
    private Vector3 StepSpringVector3(
        Vector3 current, Vector3 goal,
        ref Vector3 velocity, float dt)
    {
        // F = -k·x - c·v   (Hooke's law + damping)
        Vector3 displacement = current - goal;
        Vector3 force        = (-stiffness * displacement) - (damping * velocity);
        Vector3 acceleration = force / mass;

        velocity += acceleration * dt;

        // Optional velocity clamp
        if (maxVelocity > 0f && velocity.magnitude > maxVelocity)
            velocity = velocity.normalized * maxVelocity;

        return current + velocity * dt;
    }

    // ── Helper: wrap Euler deltas to [-180, 180] ──────────────────────────────
    private static Vector3 WrapEuler(Vector3 euler)
    {
        return new Vector3(
            WrapAngle(euler.x),
            WrapAngle(euler.y),
            WrapAngle(euler.z));
    }

    private static float WrapAngle(float angle)
    {
        while (angle >  180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    // ── Editor Helpers ────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.TransformPoint(positionOffset), 0.05f);
    }
#endif
}