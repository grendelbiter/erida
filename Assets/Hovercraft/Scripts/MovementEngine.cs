using UnityEngine;

/// <summary>
/// Hover movement engine. Actually moves the craft around. Several engines can be used on a single hovercraft with different axes
/// (i.e. forward and strafe)
/// </summary>
public class MovementEngine : MonoBehaviour
{
    /// <summary>
    /// Thrust value. Set by external code, from player input or AI
    /// </summary>
    public float Thrust;

    /// <summary>
    /// Max acceleration when going forward
    /// </summary>
    public float MaxForwardAcceleration = 20;
    /// <summary>
    /// Max acceleration when going backward
    /// </summary>
    public float MaxReverseAcceleration = 15;
    /// <summary>
    /// Max acceleration when braking
    /// </summary>
    public float MaxBrakingDeceleration = 30;
    /// <summary>
    /// Deceleration when Thrust==0 (to stop faster when no buttons are pressed)
    /// </summary>
    public float AutoBrakingDeceleration = 20;

    /// <summary>
    /// Max speed
    /// </summary>
    public float MaxSpeed = 30;
    /// <summary>
    /// Relation between acceleration and speed. Can be used to make hover accelerate faster when slow, etc
    /// </summary>
    public AnimationCurve AccelerationBySpeed;

    /// <summary>
    /// Acceleration reduction when hover is nearly-vertical. Used to prevent climbing along walls. Acceleration
    /// reduction scales linearly from MaxPitchAngle to 90 degrees - at MaxPitchAngle there is no reduction, at 90
    /// force is divided by VerticalReduction (i.e. if VerticalReduction==10, force would be reduced to 1/10). 
    /// </summary>
    public float VerticalReduction = 10;
    /// <summary>
    /// Angle at which vertical acceleration reduction starts
    /// </summary>
    public float MaxPitchAngle = 45;

    /// <summary>
    /// Rigidbody that this engine affects. A single hovercar can have multiple movement engines.
    /// </summary>
    public Rigidbody Rigidbody;

    /// <summary>
    /// Hover engines that must see ground for movement to work. If some of these do not, movement engine stops working - 
    /// this prevents "flying" on a hovercar high above ground.
    /// If this array is empty, movement always works.
    /// </summary>
    public HoverEngine[] HoverEngines;

    private SideDependentDrag m_Drag;

    void Start()
    {
        Rigidbody = Rigidbody ?? GetComponent<Rigidbody>();
        m_Drag = Rigidbody.GetComponent<SideDependentDrag>();
    }

    void FixedUpdate()
    {
        // Stop movement engine if hover is in the air (i.e. engines do not see ground)
        foreach (var hoverEngine in HoverEngines)
        {
            if (!hoverEngine.HasGround)
                return;
        }

        // current speed along forward axis
        var fwd = transform.forward;
        var speed = Vector3.Dot(Rigidbody.velocity, fwd);
        var thrust = Thrust;

        // if we don't have a button pressed, apply automatic brake 
        var isAutoBraking = Mathf.Approximately(thrust, 0)
                            && AutoBrakingDeceleration > 0;
        if (isAutoBraking)
        {
            // later, this will get multiplied by MaxBrakingDeceleration (b/c we're braking), giving the needed acceleration as a result
            thrust = -Mathf.Sign(speed) * AutoBrakingDeceleration / MaxBrakingDeceleration;
        }

        // are we braking (i.e. speed and thrust have opposing signs)
        var isBraking = Thrust * speed < 0;
        // don't apply force if speed is max already (except when braking)
        if (Mathf.Abs(speed) >= MaxSpeed && !isBraking)
            return;

        // position on speed curve
        var normSpeed = Mathf.Sign(Thrust)*speed/MaxSpeed;
        // apply acceleration curve and select proper maximum value
        var acc = AccelerationBySpeed.Evaluate(normSpeed) *
                  (isBraking
                       ? MaxBrakingDeceleration
                       : thrust > 0 ? MaxForwardAcceleration : MaxReverseAcceleration);

        // drag should be added to the acceleration
        var dragForce = GetDrag(speed);
        var force = acc * thrust + dragForce;

        // reduce acceleration if we're close to vertical orientation and we're trying to go up
        var grav = -Physics.gravity.normalized;
        var y = Vector3.Dot(fwd, grav);
        if (MaxPitchAngle < 90 && y * thrust > 0)
        {
            if (isAutoBraking)
                return; // autobrakes flat out don't work in this case

            var pitch = Mathf.Asin(Mathf.Abs(y)) * Mathf.Rad2Deg;
            if (pitch > MaxPitchAngle)
            {
                var reduction = (pitch - MaxPitchAngle) / (90 - MaxPitchAngle) * VerticalReduction;
                force /= 1 + reduction;
            }
        }

        // apply total force, using acceleration mode so we don't depend on mass
        Rigidbody.AddForce(fwd * force, ForceMode.Acceleration);
    }

    private float GetDrag(float speed)
    {
        var sdd = m_Drag ? speed*m_Drag.DragCoeffs.z : 0;
        return sdd + Rigidbody.drag*speed;
    }
}
