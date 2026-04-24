using UnityEngine;

/// <summary>
/// SpringFollow — smoothly springs this Transform toward a target Transform.
/// Attach to the object you want to move. Assign a Target in the Inspector.
/// </summary>
public class SpringFollow : MonoBehaviour
{
    // ── Forward Axis ──────────────────────────────────────────────────────────
    /// <summary>
    /// Which axis on the target is treated as "forward".
    /// This rotates the basis used when applying positionOffset and when
    /// matching the target's orientation during rotation springing.
    /// </summary>
    public enum ForwardAxis
    {
        PositiveZ,  // Unity default (+Z forward)
        NegativeZ,
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
    }

    // ── Target ────────────────────────────────────────────────────────────────
    [Header("Target")]
    [Tooltip("The Transform this object will spring toward.")]
    public Transform target;

    [Tooltip("Which axis of the target is treated as its forward direction.\n" +
             "• Affects how positionOffset is interpreted (offset X = right relative to this forward).\n" +
             "• Affects the rotation goal so this object's own +Z faces the chosen axis.")]
    public ForwardAxis forwardAxis = ForwardAxis.PositiveZ;

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
    public bool springScale = false;

    // ── Offset ────────────────────────────────────────────────────────────────
    [Header("Offset (local to target, relative to chosen forward axis)")]
    [Tooltip("Positional offset applied relative to the target's chosen-forward-axis space.")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("Euler angle offset added on top of the target's rotation (e.g. 90,0,0 to face a different direction).")]
    public Vector3 rotationOffset = Vector3.zero;

    // ── Clamp / Limits ────────────────────────────────────────────────────────
    [Header("Velocity Limits")]
    [Tooltip("Maximum speed the spring can reach (0 = unlimited).")]
    public float maxVelocity = 0f;

    // ── Runtime State (read-only in Inspector) ────────────────────────────────
    [Header("Runtime State (read-only)")]
    [SerializeField, HideInInspector] private Vector3 _posVelocity = Vector3.zero;
    [SerializeField, HideInInspector] private Vector3 _rotVelocity = Vector3.zero; // Euler-based
    [SerializeField, HideInInspector] private Vector3 _scaleVelocity = Vector3.zero;

    // Exposed for debugging
    [SerializeField] private Vector3 _debugPosVelocity;
    [SerializeField] private Vector3 _debugRotVelocity;

    // ─────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (target == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Build a rotation that remaps the target's space so the chosen axis
        // behaves as +Z (forward).  All offset / rotation math uses this basis.
        Quaternion axisRemapLocal = GetAxisRemapRotation(forwardAxis);
        Quaternion remappedTargetRot = target.rotation * axisRemapLocal;

        // ── Position Spring ───────────────────────────────────────────────────
        if (springPosition)
        {
            // Transform offset using the remapped basis instead of raw local space
            Vector3 goalPos = target.position + remappedTargetRot * positionOffset;
            transform.position = StepSpringVector3(
                transform.position, goalPos, ref _posVelocity, dt);
        }

        // ── Rotation Spring ───────────────────────────────────────────────────
        if (springRotation)
        {
            Vector3 currentEuler = transform.eulerAngles;
            // Use the remapped rotation as the goal so this object's +Z aligns
            // with the target's chosen forward axis.
            Quaternion goalQuat = remappedTargetRot *
                                  Quaternion.Euler(rotationOffset);
            Vector3 targetEuler = goalQuat.eulerAngles;

            // Wrap each axis to avoid 0↔360 jumps
            Vector3 deltaEuler = WrapEuler(targetEuler - currentEuler);
            Vector3 goalEuler = currentEuler + deltaEuler;

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

    // ── Axis Remap ────────────────────────────────────────────────────────────
    /// <summary>
    /// Returns a local-space rotation that re-orients the target so the chosen
    /// axis points in the +Z direction.  Applying this to target.rotation gives
    /// a "remapped" world rotation whose forward (+Z) is the chosen axis.
    /// </summary>
    private static Quaternion GetAxisRemapRotation(ForwardAxis axis)
    {
        return axis switch
        {
            ForwardAxis.PositiveZ  => Quaternion.identity,
            ForwardAxis.NegativeZ  => Quaternion.Euler(0f, 180f, 0f),
            ForwardAxis.PositiveX  => Quaternion.Euler(0f, -90f, 0f),
            ForwardAxis.NegativeX  => Quaternion.Euler(0f,  90f, 0f),
            ForwardAxis.PositiveY  => Quaternion.Euler(90f,  0f, 0f),
            ForwardAxis.NegativeY  => Quaternion.Euler(-90f, 0f, 0f),
            _                      => Quaternion.identity,
        };
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
        Vector3 force = (-stiffness * displacement) - (damping * velocity);
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
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    // ── Editor Helpers ────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Visualise the remapped forward direction
        Quaternion remapped = target.rotation * GetAxisRemapRotation(forwardAxis);
        Vector3 remappedForward = remapped * Vector3.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(
            target.position + remapped * positionOffset, 0.05f);

        // Draw the chosen forward axis in blue
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(target.position, remappedForward * 0.3f);
    }
#endif
}