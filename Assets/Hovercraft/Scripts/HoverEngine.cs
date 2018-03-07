using UnityEngine;

/// <summary>
/// Hovering engine. Tries to keep hover above ground at all times.
/// This engine generates an upward force depending on height above ground: force is max when height is 0, and
/// falls to some maximum height.
/// </summary>
public class HoverEngine : MonoBehaviour
{
    /// <summary>
    /// Layer mask for raycasts. Should include anything that can be used to hover over, and no things that should not be hovered over 
    /// (like FX or UI etc)
    /// </summary>
    public LayerMask RaycastMask;
    /// <summary>
    /// Max height at which engine works.
    /// </summary>
    public float MaxHeight=2;
    /// <summary>
    /// Max engine force (when height above ground is 0)
    /// </summary>
    public float GroundForce=4000;
    /// <summary>
    /// Additional damping force based on current vertical speed. Can be used to reduce "springiness" of hover
    /// </summary>
    public float Damping=2;
    /// <summary>
    /// Exponent in height-to-force relationship. When 1, force falls linearly with height, when 2 - quadratically etc
    /// </summary>
    public float Exponent=2;
    /// <summary>
    /// Maximum difference between force angle and engine angle. When set to 0, engine applies force along its local Y
    /// axis (meaning that when hover is angled, hover force would move it sideways). If MaxAngleDrift is nonzero, force
    /// direction can be rotated to match world Y axis even when engine itself is angled.
    /// </summary>
    public float MaxAngleDrift=10;
    /// <summary>
    /// Rigidbody that this engine affects. A single hovercar can have multiple hover engines to keep better balance.
    /// </summary>
    public Rigidbody Rigidbody;

    private float m_LastPower;
    private RaycastHit m_GroundHit;

    /// <summary>
    /// Does the engine "see" ground?
    /// </summary>
    public bool HasGround
    {
        get { return m_LastPower > 0; }
    }
    /// <summary>
    /// Current engine power, normalized from 0 (no ground) to 1 (directly on ground)
    /// </summary>
    public float Power
    {
        get { return m_LastPower; }
    }
    /// <summary>
    /// Raycast hit that found ground position for this engine
    /// </summary>
    public RaycastHit Ground{get { return m_GroundHit; }}

    void FixedUpdate()
    {
        // find force direction by rotating local up vector towards world up
        var up = transform.up;
        var grav = Physics.gravity.normalized;
        up = Vector3.RotateTowards(up, -grav, MaxAngleDrift*Mathf.Deg2Rad, 1);

        // check if we see ground below
        m_LastPower = 0;
        if (!Physics.Raycast(transform.position, -up, out m_GroundHit, MaxHeight, RaycastMask))
        {
            return; // no ground - no hover
        }

        // calculate power falloff
        m_LastPower = Mathf.Pow((MaxHeight - m_GroundHit.distance)/MaxHeight, Exponent);
        var force = m_LastPower*GroundForce;

        // calculate damping, which is proportional to square of engine upward velocity
        var v = Vector3.Dot(Rigidbody.GetPointVelocity(transform.position), up);
        var drag = -v*Mathf.Abs(v)*Damping;

        // add force and damping. Note that force is added at engine position
        Rigidbody.AddForceAtPosition(up*(force + drag), transform.position);
    }
}
