using UnityEngine;

/// <summary>
/// Rotates based on current engine power. Used to animate fans, turbines etc
/// </summary>
public class TurbineRotation : MonoBehaviour
{
    /// <summary>
    /// hover engine to take power from
    /// </summary>
    public HoverEngine HoverEngine;
    /// <summary>
    /// movement engine to take power from
    /// </summary>
    public MovementEngine MoveEngine;

    /// <summary>
    /// Rotation per minute when power==0
    /// </summary>
    public float MinRPM;
    /// <summary>
    /// Rotation per minute when power==1
    /// </summary>
    public float MaxRPM;

    void Update()
    {
        // get power
        var hp = HoverEngine ? HoverEngine.Power : 0;
        var mp = MoveEngine ? Mathf.Abs(MoveEngine.Thrust) : 0;
        // use max if both engines are set
        var power = Mathf.Max(hp, mp);
        // find rpm
        var rpm = Mathf.Lerp(MinRPM, MaxRPM, power);
        // rotate
        transform.Rotate(0, rpm * Time.deltaTime * 6, 0, Space.Self);
    }
}
