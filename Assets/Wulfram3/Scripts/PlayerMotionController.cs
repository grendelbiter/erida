using Assets.Wulfram3.Scripts.InternalApis.Classes;
using UnityEngine;

namespace Com.Wulfram3
{
    [RequireComponent(typeof(FuelManager))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Unit))]
    public class PlayerMotionController : Photon.PunBehaviour
    {
        public AudioClip takeoffSound;
        public AudioClip landingSound;
        public AudioClip jumpSound;
        public AudioClip tankInternalSound;
        public AudioClip tankExternalSound;
        public AudioClip scoutInternalSound;
        public AudioClip scoutExternalSound;

        [HideInInspector]
        public UnitType myUnitType;
        private IVehicleSetting vehicleSettings;
        private Unit unitManager;
        private FuelManager fuelManager;
        private Rigidbody rigidBody;
        [HideInInspector]
        public bool receiveInput = true;
        [HideInInspector]
        public Transform isLandedOnRepairPad;

        private float inputX; // Strafe
        private float inputZ; // Drive
        private float rX; // L/R Rotation
        private float rY; // U/D Rotation

        private float takeoffBumpForce = 0.25f;

        private float boost = 1f;
        private float boostMultiplier = 2.8f;
        private float thrustMultiplier = 0.5f;

        public float healingBoost = 1f;
        private float landedBoost = 2.65f;
        private float repPadBoost = 75f;

        private float thrustChangedStamp;
        private float jumpjetStamp;
        private float tryLandStamp;
        [HideInInspector]
        public float currentHeight = 1.2f;
        private float defaultHeight = 1.2f;
        private float maximumHeight = 4f;
        private float riseSpeed = 0.075f;
        private float sinkSpeed = 0.03f;


        private bool isLanding = false;
        [HideInInspector]
        public bool isGrounded = false;
        private bool jump = false;

        public float xSens = 0.4f;//0.2f;
        public float ySens = 0.5f;//0.22f;

        private float minXAngle = -360f;
        private float maxXAngle = 360f;
        private float minYAngle = -55f;
        private float maxYAngle = 58f;

        private Quaternion oRot = Quaternion.identity;

        private void Start()  { }

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            if (photonView.isMine)
            {
                unitManager = GetComponent<Unit>();
                fuelManager = GetComponent<FuelManager>();
            } else
            {
                rigidBody.isKinematic = true;
            }
        }

        void Update()
        {
            if (!photonView.isMine)
                return;
            if (unitManager == null || unitManager.isDead)
                return;
            if (unitManager.unitType != myUnitType || vehicleSettings == null)
                SetUnitType(GetComponent<Unit>().unitType);
            if (receiveInput && vehicleSettings != null)
            {
                if (rigidBody.isKinematic && !isGrounded)
                {
                    rigidBody.isKinematic = false;
                    rigidBody.freezeRotation = false;
                }
                HandleSpeedControls(); // Boost is calculated here, as well as user set thrust control
                HandleMouseMotion(); // Mouse controls first, can cause liftoff from landed
                HandleDriveControls(); // This method sets the "drive" forces
                HandleAltitudeControls(); // Must be after "drive" forces to allow landing to cancel motion
                if (myUnitType == UnitType.Tank)
                    HandleJumpjetControls(); // Jump can cancel landing
            } else
            {
                inputX = 0f;
                inputZ = 0f;
                rigidBody.freezeRotation = true;
                if (rigidBody.velocity.magnitude <= 0.01f)
                    rigidBody.isKinematic = true;
            }
        }

        private void FixedUpdate()
        {
            if (!photonView.isMine || isGrounded || vehicleSettings == null)
                return;
            if (unitManager.isDead)
                return;
            if (!isLanding && !isGrounded && !jump)
                AddHoverForce();
            if (jump)
                AddJumpForce();
            Vector3 localVelocity = transform.InverseTransformDirection(rigidBody.velocity);
            Vector3 relativeFwd = Vector3.Cross(Vector3.up, transform.right);
            float lVX = Mathf.Abs(localVelocity.x);
            float lVZ = Mathf.Abs(localVelocity.z);
            float limitX = vehicleSettings.MaxVelocityX * thrustMultiplier;
            float limitZ = vehicleSettings.MaxVelocityZ * thrustMultiplier;
            if (lVX > (limitX * boost))
                inputX = 0;
            if (lVZ > (limitZ * boost))
                inputZ = 0;
            Vector3 totalSidewaysForce = transform.right * inputX * (vehicleSettings.BaseThrust * vehicleSettings.StrafePercent) * rigidBody.mass * boost;
            Vector3 totalForwardForce = relativeFwd * inputZ * vehicleSettings.BaseThrust * rigidBody.mass * boost;
            rigidBody.AddForce(-totalForwardForce);
            rigidBody.AddForce(totalSidewaysForce);
        }

        private void AddHoverForce()
        {
            Vector3 clp = CenteredLowestPoint();
            Ray ray = new Ray(clp, -Vector3.up);
            Ray checkRay = new Ray(new Vector3(clp.x, transform.position.y, clp.z), -Vector3.up);
            RaycastHit lowHit;
            RaycastHit highHit;
            if (Physics.Raycast(ray, out lowHit, currentHeight) || (Physics.Raycast(checkRay, out highHit, currentHeight) && highHit.distance != 0 && lowHit.distance == 0))
            {
                if (lowHit.distance < currentHeight)
                {
                    float gravityCancel = Physics.gravity.y * rigidBody.mass;
                    float velocityCancel = rigidBody.velocity.y * rigidBody.mass;
                    float heightMultiplier = (currentHeight - lowHit.distance) / currentHeight;
                    float totalForce = (gravityCancel + velocityCancel) * (heightMultiplier * rigidBody.mass);
                    rigidBody.AddForce(new Vector3(0f, -totalForce, 0f));
                }
            }
        }

        private void AddJumpForce()
        {
            AudioSource.PlayClipAtPoint(jumpSound, CenteredLowestPoint());
            rigidBody.AddForce(transform.up * vehicleSettings.JumpForce * rigidBody.mass, ForceMode.Impulse);
            jump = false;
        }

        #region Input Handlers
        private void HandleMouseMotion()
        {
            float mx = InputEx.GetAxis("Mouse X");
            float my = InputEx.GetAxis("Mouse Y");
            if ((isGrounded || isLanding) && (mx != 0 || my != 0))
            {
                TakeOff();
                return;
            }
            rX = AngleClamp(rX + (mx * (xSens * boost)), minXAngle, maxXAngle);
            rY = AngleClamp(rY + (my * (ySens * boost)), minYAngle, maxYAngle);
            Quaternion xQuaternion = Quaternion.AngleAxis(rX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rY, -Vector3.right);
            transform.localRotation = oRot * xQuaternion * yQuaternion;
        }

        private void HandleSpeedControls() { 
            boost = Mathf.Clamp(InputEx.GetAxisRaw("Boost") * boostMultiplier, 1f, boostMultiplier);
            if (Time.time >= thrustChangedStamp)
            {
                if (InputEx.GetAxisRaw("ChangeThrust") > 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = Mathf.Clamp(thrustMultiplier + 0.01f, 0.1f, 1f);
                }
                else if (InputEx.GetAxisRaw("ChangeThrust") < 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = Mathf.Clamp(thrustMultiplier - 0.01f, 0.1f, 1f);
                }
                else if (InputEx.GetAxisRaw("SetSpeed1") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.1f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed2") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.2f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed3") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.3f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed4") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.4f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed5") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.5f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed6") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.6f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed7") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.7f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed8") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.8f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed9") != 0)
                {
                    thrustChangedStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.9f;
                }
            }
        }

        private void HandleDriveControls()
        {
            inputX = InputEx.GetAxis("Strafe");
            inputZ = InputEx.GetAxis("Drive");
        }

        private void HandleAltitudeControls()
        {
            if (InputEx.GetAxisRaw("ChangeAltitude") > 0)
                currentHeight = Mathf.Min(currentHeight + (riseSpeed * boost), maximumHeight);
            else if (InputEx.GetAxisRaw("ChangeAltitude") < 0 && !isLanding && !isGrounded)
                currentHeight = Mathf.Max(currentHeight - (sinkSpeed * boost), 0.2f);
            if (currentHeight > 0.2f && (isLanding || isGrounded))
                TakeOff();
            else if (currentHeight <= 0.2f && !isLanding)
                isLanding = true;
            if (isLanding)
            {
                inputX = 0;
                inputZ = 0;
                if (!isGrounded && Time.time > tryLandStamp)
                    Land();
            }
        }

        private void HandleJumpjetControls()
        {
            if (Time.time >= jumpjetStamp && InputEx.GetAxisRaw("Jump") != 0 && fuelManager.TakeFuel(vehicleSettings.FuelPerJump))
            {
                if (isLanding || isGrounded)
                    TakeOff();
                jump = true;
                jumpjetStamp = Time.time + vehicleSettings.TimeBetweenJumps;
            }
        }
        #endregion

        private void Land()
        {
            tryLandStamp = Time.time + 0.05f;
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, GetComponent<Collider>().bounds.extents.z * 1.15f))
            {
                healingBoost = landedBoost;
                if (hit.transform != null)
                {
                    Unit u = hit.transform.GetComponent<Unit>();
                    if (u != null && u.unitType == UnitType.RepairPad && u.unitTeam == GetComponent<Unit>().unitTeam)
                    {
                        Physics.IgnoreCollision(hit.transform.GetComponent<Collider>(), GetComponent<Collider>(), true);
                        isLandedOnRepairPad = hit.transform;
                        healingBoost = repPadBoost;
                    }
                }
                fuelManager.SetLandedBoost(true);
                transform.Translate(hit.point - CenteredLowestPoint());
                rigidBody.freezeRotation = true;
                rigidBody.isKinematic = true;
                isGrounded = true;
                AudioSource.PlayClipAtPoint(landingSound, transform.position);
                GetComponent<AudioSource>().Stop();
            }
        }

        private void TakeOff()
        {
            if (isLandedOnRepairPad != null)
            {
                Physics.IgnoreCollision(isLandedOnRepairPad.GetComponent<Collider>(), GetComponent<Collider>(), false);
                isLandedOnRepairPad = null;
            }
            healingBoost = 1f;
            fuelManager.SetLandedBoost(false);
            rigidBody.isKinematic = false; 
            rigidBody.freezeRotation = false;
            isLanding = false;
            isGrounded = false;
            currentHeight = defaultHeight;
            rigidBody.AddForce(Vector3.up * (takeoffBumpForce * rigidBody.mass), ForceMode.Impulse);
            AudioSource.PlayClipAtPoint(takeoffSound, transform.position);
            GetComponent<AudioSource>().Play();
        }

        public void SetUnitType(UnitType u)
        {
            myUnitType = u;
            vehicleSettings = VehicleSettingFactory.GetVehicleSetting(u);
            if (u == UnitType.Tank)
            {
                if (photonView.isMine)
                    GetComponent<AudioSource>().clip = tankInternalSound;
                else
                    GetComponent<AudioSource>().clip = tankExternalSound;
            } else
            {
                if (photonView.isMine)
                    GetComponent<AudioSource>().clip = scoutInternalSound;
                else
                    GetComponent<AudioSource>().clip = scoutExternalSound;
            }
            GetComponent<AudioSource>().Play();
            rigidBody.drag = vehicleSettings.RigidbodyDrag;
        }


        #region Utility Functions
        private Vector3 CenteredLowestPoint()
        {
            return new Vector3(transform.position.x, GetComponent<Collider>().bounds.min.y, transform.position.z);
        }

        private Vector3 OffsetLowestPoint(float offset)
        {
            return new Vector3(transform.position.x, GetComponent<Collider>().bounds.min.y, transform.position.z + offset);
        }

        public float AngleClamp(float a, float m, float x)
        {
            if (a < -360f)
                a += 360f;
            if (a > 360f)
                a -= 360f;
            return Mathf.Clamp(a, m, x);
        }
        #endregion
    }
}