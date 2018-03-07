using UnityEngine;

/// <summary>
/// Auto-resets hover in case it flips over
/// </summary>
public class FlipHelper : MonoBehaviour
{
    /// <summary>
    /// Time to spend flipped until resel kicks in
    /// </summary>
    public float MaxFlippedTime = 5;
    private float m_LastTimeUp;

    void Update()
    {
        if (transform.up.y > 0)
        {
            // if we're not flipped, remember time
            m_LastTimeUp = Time.time;
        }
        else if (Time.time - m_LastTimeUp > MaxFlippedTime)
        {
            // if we've been upside down long enough, restore default orientation (and move hover up a bit)
            transform.Translate(Vector3.up * 2, Space.World);
            var r = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, r.y, 0);
        }
    }
}
