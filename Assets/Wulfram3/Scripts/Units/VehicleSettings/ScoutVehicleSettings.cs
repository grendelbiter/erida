using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutVehicleSettings : IVehicleSetting
{
    public float BaseThrust
    {
        get
        {
            return 5f;
        }
    }

    public float StrafePercent
    {
        get
        {
            return 0.8f;
        }
    }

    public float ThrustMultiplier
    {
        get
        {
            return 0.7f;
        }
    }

    public float JumpForce
    {
        get
        {
            return 0f;
        }
    }

    public int FuelPerJump
    {
        get
        {
            return 0;
        }
    }

    public float TimeBetweenJumps
    {
        get
        {
            return 0f;
        }
    }

    public float MaxVelocityX
    {
        get
        {
            return 12f;
        }
    }

    public float MaxVelocityZ
    {
        get
        {
            return 16f;
        }
    }

    public float BoostMultiplier
    {
        get
        {
            return 3f;
        }
    }

    public int MinPropulsionFuel
    {
        get
        {
            return 40;
        }
    }

    public float DefaultHeight
    {
        get
        {
            return 1.8f;
        }
    }

    public float MaximumHeight
    {
        get
        {
            return 5.2f;
        }
    }

    public float RiseSpeed
    {
        get
        {
            return 0.07f;
        }
    }

    public float LowerSpeed
    {
        get
        {
            return 0.04f;
        }
    }

    public List<WeaponTypes> AvailableWeapons
    {
        get
        {
            return new List<WeaponTypes>
            {
                WeaponTypes.MachineGun,
                WeaponTypes.RepairBeam
            };
        }
    }
}