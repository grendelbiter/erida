using UnityEngine;

/// <summary>
/// Positions and scales particle emission based on hover engine, so that particles are always on the ground
/// </summary>
public class DustParticles : MonoBehaviour
{
    /// <summary>
    /// Particles simulating dust
    /// </summary>
    public ParticleSystem ParticleSystem;
    /// <summary>
    /// Hover engine causing that dust
    /// </summary>
    public HoverEngine Engine;
    /// <summary>
    /// Particle emission when engine is on ground (i.e. has max power)
    /// </summary>
    public float MaxEmission=250;
    /// <summary>
    /// Fudge factor - particle emitter moved above ground by this value, so that particles don't clip as much
    /// </summary>
    public float DustHeightFudge = 0.5f;

    void Update()
    {
        if (ParticleSystem&&Engine)
        {
            ParticleSystem.emissionRate = MaxEmission * Engine.Power;
            ParticleSystem.transform.position = Engine.Ground.point + Engine.Ground.normal*DustHeightFudge;
            ParticleSystem.transform.LookAt(Engine.Ground.point + Engine.Ground.normal*10);
        }
    }
}
