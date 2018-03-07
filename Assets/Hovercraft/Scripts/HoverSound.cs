using System.Linq;
using UnityEngine;

/// <summary>
/// Simple engine sound behaviour. Adjusts sound volume and pitch based on engine power.
/// </summary>
public class HoverSound : MonoBehaviour
{
    /// <summary>
    /// Source actually playing engine sound
    /// </summary>
    public AudioSource SoundSource;
    /// <summary>
    /// Hover engines to use
    /// </summary>
    private HoverEngine[] m_Engines;
    /// <summary>
    /// Movement engines to use
    /// </summary>
    private MovementEngine[] m_MoveEngines;

    /// <summary>
    /// How much is volume and pitch affected by movement, as opposed to hover (0 - hover only, 1-movement only)
    /// </summary>
    public float MovementEngineWeight;

    /// <summary>
    /// Minimum volume (when all engines are unpowered)
    /// </summary>
    public float MinVolume = 0.5f;
    /// <summary>
    /// Max volume (when engines are firing at max power)
    /// </summary>
    public float MaxVolume = 1;

    /// <summary>
    /// Minimum pitch (when all engines are unpowered)
    /// </summary>
    public float MinPitch = 0.5f;
    /// <summary>
    /// Maximum pitch (when engines are firing at max power)
    /// </summary>
    public float MaxPitch = 1.2f;

    void Start()
    {
        m_Engines = GetComponentsInChildren<HoverEngine>();
        m_MoveEngines = GetComponentsInChildren<MovementEngine>();
    }

    void Update()
    {
        // take max power hover and movement engine
        var hp = m_Engines.Max(e => e.Power);
        var mp = m_MoveEngines.Max(e => Mathf.Abs(e.Thrust));
        // apply weighting
        var power = Mathf.Lerp(hp, mp, MovementEngineWeight);
        // set pitch&volume
        SoundSource.volume = Mathf.Lerp(MinVolume, MaxVolume, power);
        SoundSource.pitch = Mathf.Lerp(MinPitch, MaxPitch, power);
    }
}
