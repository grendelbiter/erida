using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankVehicleSetting : IVehicleSetting
{

    public int MaxHitPoints
    {
        get
        {
            return 500;
        }
    }

    public float RigidbodyDrag
    {
        get
        {
            return 0.765f;
        }
    }

    public float BaseThrust
    {
        get
        {
            return 3f;
        }
    }

    public float StrafePercent
    {
        get
        {
            return 0.6f;
        }
    }

    public float JumpForce
    {
        get
        {
            return 17.6f;
        }
    }

    public int FuelPerJump
    {
        get
        {
            return 60;
        }
    }

    public float TimeBetweenJumps
    {
        get
        {
            return 3.6f;
        }
    }

    public float MaxVelocityX
    {
        get
        {
            return 8f;
        }
    }

    public float MaxVelocityZ
    {
        get
        {
            return 12f;
        }
    }

    public float BoostMultiplier
    {
        get
        {
            return 2.2f;
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
            return 1.2f;
        }
    }

    public float MaximumHeight
    {
        get
        {
            return 4.2f;
        }
    }

    public float RiseSpeed
    {
        get
        {
            return 0.04f;
        }
    }

    public float LowerSpeed
    {
        get
        {
            return 0.02f;
        }
    }

    public float LandedHealingBoost
    {
        get
        {
            return 2.65f;
        }
    }

    public float RepairPadHealingBoost
    {
        get
        {
            return 65f;
        }
    }

    public float PowerCellHealingMultiplier
    {
        get
        {
            return 1.1f;
        }
    }


    public List<WeaponTypes> AvailableWeapons
    {
        get
        {
            return new List<WeaponTypes>
            {
                WeaponTypes.Autocannon,
                WeaponTypes.Pulse
            };
        }
    }
}
