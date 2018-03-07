using UnityEngine;

/// <summary>
/// Keeps track of hover rotation, including turning
/// </summary>
public class HoverOrientation : MonoBehaviour
{
    /// <summary>
    /// Turn value (i.e. input axis or AI-supplied turn). -1 for full left, 1 for full right.
    /// </summary>
    public float Turn;

    /// <summary>
    /// Maximum turn torque
    /// </summary>
    public float MaxTorque=2000;
    /// <summary>
    /// "Braking" torque that is applied when Turn==0 (to stop turning when no buttons are pressed)
    /// </summary>
    public float BrakingTorque=1000;

    /// <summary>
    /// Roll angle to simulate "leaning-in" on turns
    /// </summary>
    public float RollOnTurns=10;
    /// <summary>
    /// Torque value to apply to roll angle when turning
    /// </summary>
    public float RollOnTurnsTorque=20;

    /// <summary>
    /// Auto-level torque for roll. When >0, hover tries to automatically keep level when near ground
    /// </summary>
    public float RollCompensationTorque=10;
    /// <summary>
    /// Auto-level torque for pitch. When >0, hover tries to automatically keep level when near ground
    /// </summary>
    public float PitchCompensationTorque = 10;

    /// <summary>
    /// Auto-level engines. Auto-level is active only when all these engines see ground (i.e. have non-zero power).
    /// If the array is empty, auto-leveling works always.
    /// </summary>
    public HoverEngine[] HoverEngines;

    void FixedUpdate()
    {
        ApplyTurn();
        Level();
    }

    private void Level()
    {
        // Do not auto-level if hover is in the air (i.e. engines do not see ground)
        foreach (var hoverEngine in HoverEngines) 
        {
            if(!hoverEngine.HasGround)
                return;
        }

        // find current pitch and roll. We need them in local space, not world space!
        var grav = -Physics.gravity.normalized;
        var pitch = Mathf.Asin(Vector3.Dot(transform.forward,grav)) * Mathf.Rad2Deg;
        var roll = Mathf.Asin(Vector3.Dot(transform.right, grav)) * Mathf.Rad2Deg;
        pitch = Mathf.DeltaAngle(pitch, 0); 
        roll = Mathf.DeltaAngle(roll, 0);

        // apply compensation torque
        var pt = -pitch*PitchCompensationTorque;
        var rt = roll*RollCompensationTorque;
        GetComponent<Rigidbody>().AddRelativeTorque(pt, 0, rt);
    }

    private void ApplyTurn()
    {
        if (Mathf.Approximately(Turn, 0))
        {
            // if we're not turning, apply "braking" torque counter to current rotation
            var localR = Vector3.Dot(GetComponent<Rigidbody>().angularVelocity, transform.up);
            GetComponent<Rigidbody>().AddRelativeTorque(0, -localR*BrakingTorque, 0);
        }
        else
        {
            // calculate roll torque to simulate "leaning in" on turns
            var targetRoll = -RollOnTurns*Turn;
            var roll = Mathf.Asin(transform.right.y)*Mathf.Rad2Deg;
            // only apply additional roll if we're not "overrolled"
            roll = Mathf.Abs(roll) > Mathf.Abs(targetRoll) ? 0 : Mathf.DeltaAngle(roll, targetRoll);

            // apply turning and rolling torque 
            GetComponent<Rigidbody>().AddRelativeTorque(0, Turn*MaxTorque, roll*RollOnTurnsTorque);
        }
    }

}
