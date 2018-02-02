using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3
{
    public class PlayerMovementManager : Photon.PunBehaviour
    {
        // Inspector Vars
        public AudioClip jumpSource;
        public AudioClip landSource;
        public AudioClip takeoffSource;
        [Tooltip("Delay, in seconds, before respawn map appears")]
        public float destroyDelayWhenDead = 3;
        public Transform[] meshList;
        public List<Mesh> availableColliders;
        public List<Texture2D> myIconTextures;
        public List<Transform> cargoDropPositions;
        public List<Transform> unitPlacePositions;
        public List<Transform> gunPositions;
        public List<Transform> firstPersonCameraPositions;
        public List<Transform> thirdPersonCameraPositions;

        private int fuelPerPulse = 135; // Reduced from 180 last
        private int minPropulsionFuel = 40; // Not Implemented
        private float thrustMultiplier = 0.5f;
        private float timeBetweenPulse = 3f;
        private float pulseShellFiringImpulse = 8f;
        private float maxVelocityX = 8f;
        private float maxVelocityZ = 12f;
        private float boostMultiplier = 1.85f;
        private float currentHeight = 1.2f;  // tank current (and starting) level above ground
        private float defaultHeight = 1.2f;
        private float maximumHeight = 4.0f;
        private float riseSpeed = 0.055f;
        private float lowerSpeed = 0.025f;
        private float sensitivityX = 0.2f;
        private float sensitivityY = 0.22f;
        private float minimumX = -360F;
        private float maximumX = 360F;
        private float minimumY = -56F;
        private float maximumY = 56F;

        // Internal vars
        [HideInInspector]
        public Transform onRepairPad;
        [HideInInspector]
        public bool isDead = false;
        [HideInInspector]
        public bool isSpawning = false;

        private GameManager gameManager;
        private GameObject tempShell;
        private IVehicleSetting mySettings;
        private Rigidbody myRigidbody;
        private FuelManager fuelManager;
        private Quaternion originalRotation;
        private PunTeams.Team initialTeam;
        private UnitType initialUnit;

        private int meshIndex = 0;

        private float pulseStamp;
        private float jumptimestamp;
        private float landRequestTime;
        private float thrustStamp;
        private float propulsionStamp;
        private float timeSinceDead = 0;
        private float strafeThrust = 1f;
        private float takeOffBumpForce = 0.5f; // A little extra umph
        private float rotationX = 0F;
        private float rotationY = 0F;
        private float lastRotationX = 0F;
        private float lastRotationY = 0F;
        private float inputX = 0f;
        private float inputZ = 0f;
        private float boost = 1f;

        private bool isLanded = false; // User controlled, request to land
        private bool isGrounded = false; // Script controlled, positive ground contact
        private bool requestJump = false;

        //[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        [HideInInspector]
        public static GameObject LocalPlayerInstance;
        [HideInInspector]
        public KGFMapIcon myMapIcon;
        [HideInInspector]
        public Unit myUnit;
        [HideInInspector]
        public Transform myMesh;
        [HideInInspector]
        public Transform gunEnd;
        [HideInInspector]
        public Transform firstPersonCamPos;
        [HideInInspector]
        public Transform thirdPersonCamPos;
        [HideInInspector]
        public Transform placePosition;
        [HideInInspector]
        public Transform dropPosition;
        [HideInInspector]
        public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
        [HideInInspector]
        public RotationAxes axes = RotationAxes.MouseXAndY;

        void Start()
        {
            myUnit = GetComponent<Unit>();
            if (myUnit != null)
            {
                myUnit.unitType = (UnitType)photonView.instantiationData[0];
                myUnit.unitTeam = (PunTeams.Team)photonView.instantiationData[1];
            } else
            {
                Debug.Log("*******[Player Error]*******: Unit.cs or Instantiation Data Not Found. This may occur if a scene was loaded with existing vehicles.");
            }
            if (!photonView.isMine)
            {
                myRigidbody.isKinematic = true;
                return;
            }
            myMesh = meshList[0];
            mySettings = VehicleSettingFactory.GetVehicleSetting(UnitType.Tank);
            originalRotation = transform.localRotation;
            PrepareForRespawn();
        }


        public void SetMesh(int i)
        {
            for (int n = 0; n < meshList.Length; n++)
            {
                meshList[n].gameObject.SetActive(false);
            }
            if (myUnit != null && myUnit.unitTeam == PunTeams.Team.Blue)
                myMapIcon.SetTextureIcon(myIconTextures[0]);
            else if (myUnit != null && myUnit.unitTeam == PunTeams.Team.Red)
                myMapIcon.SetTextureIcon(myIconTextures[1]);
            else
                Debug.Log("TODO: Set other team map icons. (Or Unit Script NULL)");
            meshIndex = i;
            meshList[i].gameObject.SetActive(true);
            myMesh = meshList[i];
            gunEnd = gunPositions[i];
            GetComponent<AutoCannon>().gunEnd = gunPositions[i];
            GetComponent<MeshCollider>().sharedMesh = availableColliders[i];
            mySettings = VehicleSettingFactory.GetVehicleSetting(myUnit.unitType);
            myUnit.maxHealth = mySettings.MaxHitPoints;
            if (!photonView.isMine)
                return;
            CameraManager cm = GetComponent<CameraManager>();
            cm.SetFirstPersonPosition(firstPersonCameraPositions[i]);
            cm.SetThirdPersonPosition(thirdPersonCameraPositions[i]);
            if (i == 0)
            {
                dropPosition = cargoDropPositions[0];
                placePosition = unitPlacePositions[0];
            } else if (i == 2)
            {
                dropPosition = cargoDropPositions[1];
                placePosition = unitPlacePositions[1];

            }
            strafeThrust = mySettings.StrafePercent * mySettings.BaseThrust;
        }

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            myRigidbody = GetComponent<Rigidbody>();
            if (photonView.isMine)
            {
                PlayerMovementManager.LocalPlayerInstance = gameObject;
                fuelManager = GetComponent<FuelManager>();
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        public int GetMeshIndex()
        {
            return meshIndex;
        }

        public bool GetIsGrounded()
        {
            return isGrounded;
        }

        public void Reset()
        {
            if (photonView.isMine)
            {
                if (myRigidbody != null)
                    myRigidbody.freezeRotation = false;
                
                isLanded = false;
                isGrounded = false;
                requestJump = false;
                timeSinceDead = 0;
                currentHeight = defaultHeight;
                gameManager.Respawn(this);
            }
        }

        [PunRPC]
        public void SetSelectedVehicle(int i)
        {
            if ((i == 1 || i == 3))
            {
                myUnit.unitType = UnitType.Scout;
            }
            else if ((i == 0 || i == 2))
            {
                myUnit.unitType = UnitType.Tank;
            }
            else
            {
                myUnit.unitType = UnitType.Other;
            }
            SetMesh(i);
        }

        [PunRPC]
        public void SetTeam(PunTeams.Team t)
        {
            myUnit.unitTeam = t;
        }

        [PunRPC]
        public void SetPosAndRotation(Vector3 pos, Quaternion rot)
        {
            CheckRespawn();
            if (!photonView.isMine)
                return;
            transform.position = pos;
            transform.rotation = rot;
        }

        void CheckRespawn()
        {
            if (isSpawning)
            {
                myMesh.gameObject.SetActive(true);
                GetComponent<AudioSource>().Play();
                GetComponent<Collider>().enabled = true;
                GetComponent<KGFMapIcon>().SetVisibility(true);
                isSpawning = false;
                myRigidbody.isKinematic = false;
            }

        }

        public void PrepareForRespawn()
        {
            isSpawning = true;
            myMesh.gameObject.SetActive(false);
            myRigidbody.isKinematic = true;
            GetComponent<Collider>().enabled = false;
            GetComponent<KGFMapIcon>().SetVisibility(false);
            GetComponent<AudioSource>().Stop();
            //gameManager.SpawnExplosion(transform.position);
            Reset();
        }

        public void CheckIsDead()
        {
            isDead = myUnit.health <= 0;
            if (isDead && !isSpawning)
            {
                if (timeSinceDead == 0)
                {
                    myRigidbody.freezeRotation = false;
                }
                timeSinceDead += Time.deltaTime;
                if (timeSinceDead >= destroyDelayWhenDead)
                {
                    PrepareForRespawn();
                }
            }
        }

        private void CheckMouseMotion()
        {
            float mx = InputEx.GetAxis("Mouse X");
            float my = InputEx.GetAxis("Mouse Y");
            if ((isGrounded || isLanded) && mx + my > 0)
            {
                TakeOff();
            }
            if (!isGrounded)
            {
                if (axes == RotationAxes.MouseXAndY)
                {
                    // Read the mouse input axis
                    rotationX += mx * sensitivityX;
                    rotationY += my * sensitivityY;
                    rotationX = ClampAngle(rotationX, minimumX, maximumX);
                    rotationY = ClampAngle(rotationY, minimumY, maximumY);
                    Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                    Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
                    transform.localRotation = originalRotation * xQuaternion * yQuaternion;
                }
                else if (axes == RotationAxes.MouseX)
                {
                    rotationX += mx * sensitivityX;
                    rotationX = ClampAngle(rotationX, minimumX, maximumX);
                    Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                    transform.localRotation = originalRotation * xQuaternion;
                }
                else
                {
                    rotationY += my * sensitivityY;
                    rotationY = ClampAngle(rotationY, minimumY, maximumY);
                    Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
                    transform.localRotation = originalRotation * yQuaternion;
                }
            }

        }

        private void CheckPropulsionControls()
        {
            inputX = InputEx.GetAxis("Strafe");
            inputZ = InputEx.GetAxis("Drive");
            boost = Mathf.Clamp(InputEx.GetAxisRaw("Boost") * boostMultiplier, 1f, boostMultiplier);
            if (Time.time >= thrustStamp)
            {
                if (InputEx.GetAxisRaw("ChangeThrust") > 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = Mathf.Clamp(thrustMultiplier + 0.01f, 0.1f, 1f);
                }
                else if (InputEx.GetAxisRaw("ChangeThrust") < 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = Mathf.Clamp(thrustMultiplier - 0.01f, 0.1f, 1f);
                }
                else if (InputEx.GetAxisRaw("SetSpeed1") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.1f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed2") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.2f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed3") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.3f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed4") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.4f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed5") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.5f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed6") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.6f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed7") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.7f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed8") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.8f;
                }
                else if (InputEx.GetAxisRaw("SetSpeed9") != 0)
                {
                    thrustStamp = Time.time + 0.3f;
                    thrustMultiplier = 0.9f;
                }
            }
        }

        private void CheckFireSecondary()
        {
            if (InputEx.GetAxisRaw("Fire2") != 0)
            {
                if (myUnit.unitType == UnitType.Tank && Time.time >= pulseStamp && fuelManager.TakeFuel(fuelPerPulse))
                {
                    CmdFirePulseShell();
                }
                else if (myUnit.unitType == UnitType.Scout)
                {
                    Debug.Log("PlayerMovementManager.cs (Line: 309) Scout Secondary Firing Detected.");
                }
            }
        }

        private void CheckAltitudeControls()
        {
            if (InputEx.GetAxisRaw("ChangeAltitude") > 0)
            {
                currentHeight = Mathf.Min(currentHeight + riseSpeed, maximumHeight);
            }
            else if (InputEx.GetAxisRaw("ChangeAltitude") < 0 && !isLanded && !isGrounded)
            {
                currentHeight = Mathf.Max(currentHeight - lowerSpeed, 0.2f);
            }
            if (currentHeight > 0.2f && (isLanded || isGrounded))
            {
                TakeOff();
            }
            else if (currentHeight <= 0.2f && !isLanded)
            {
                isLanded = true;
            }
        }

        private void CheckJumpjets()
        {
            if (myUnit.unitType != UnitType.Scout && Time.time >= jumptimestamp && InputEx.GetAxisRaw("Jump") != 0)
            {
                if (fuelManager.TakeFuel(mySettings.FuelPerJump))
                {
                    if (isLanded || isGrounded)
                    {
                        TakeOff();
                    }
                    requestJump = true;
                    jumptimestamp = Time.time + mySettings.TimeBetweenJumps;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            CheckIsDead();

            if (!photonView.isMine || isDead || isSpawning)
                return;

            if (!Cursor.visible)
            {
                myRigidbody.freezeRotation = false;
                myRigidbody.isKinematic = false;
                CheckMouseMotion();
                CheckPropulsionControls(); // WSAD, as well as speed setting keys
                CheckFireSecondary(); // Detects tank or scout, fires appropriate weapon
                CheckAltitudeControls(); // Altitude control before jump to allow cancel of landing attempts by jumping
                CheckJumpjets();
                if (isLanded)
                {
                    inputX = 0;
                    inputZ = 0;
                    RaycastHit hit;
                    if (!isGrounded && Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, GetComponent<Collider>().bounds.extents.z * 1.15f))
                        Land(hit);
                }
            }
            else
            { // In Menu, Freeze position and rotation.
                myRigidbody.freezeRotation = true;
                myRigidbody.isKinematic = true;
            }


        }

        private Vector3 CenteredLowestPoint()
        {
            return new Vector3(transform.position.x, GetComponent<Collider>().bounds.min.y, transform.position.z);
        }

        private void Land(RaycastHit hit)
        {
            if (hit.transform != null)
            {
                Unit u = hit.transform.GetComponent<Unit>();
                if (u != null && u.unitType == UnitType.RepairPad && u.unitTeam == GetComponent<Unit>().unitTeam)
                {
                    Physics.IgnoreCollision(hit.transform.GetComponent<Collider>(), GetComponent<Collider>(), true);
                    onRepairPad = hit.transform;
                }
            }
            /*
             *  TODO: Make this into an "animation" (Lerp)
            Vector3 fwd = transform.forward;
            Vector3 proj = fwd - (Vector3.Dot(fwd, hit.normal)) * hit.normal;
            transform.rotation = Quaternion.LookRotation(proj, hit.normal);
            */
            transform.Translate(hit.point - CenteredLowestPoint());
            //transform.Translate(Vector3.up * 0.03f); // prevent collision overlap TODO: smoothly move into position
            myRigidbody.freezeRotation = true;
            myRigidbody.isKinematic = true; //do not let physics forces affect this body
            isGrounded = true;
            AudioSource.PlayClipAtPoint(landSource, transform.position);
            GetComponent<AudioSource>().Stop();
        }

        private void TakeOff()
        {
            if (onRepairPad != null)
            {
                Physics.IgnoreCollision(onRepairPad.GetComponent<Collider>(), GetComponent<Collider>(), false);
                onRepairPad = null;
            }
            myRigidbody.isKinematic = false; //let physics forces affect this body again
            myRigidbody.freezeRotation = false;
            isLanded = false;
            isGrounded = false;
            currentHeight = defaultHeight;
            //myRigidbody.AddForce(Vector3.up * (takeOffBumpForce * myRigidbody.mass), ForceMode.Impulse);
            AudioSource.PlayClipAtPoint(takeoffSource, transform.position);
            GetComponent<AudioSource>().Play();
        }

        void CmdFirePulseShell()
        {
            object[] args = new object[5];
            args[0] = gunEnd.position;
            args[1] = gunEnd.rotation;
            args[2] = transform.GetComponent<Unit>().unitTeam;
            args[3] = myRigidbody.velocity;
            args[4] = this.photonView.viewID;
            tempShell = Instantiate(Resources.Load("Prefabs/Weapons/DummyPulse_" + myUnit.unitTeam), gunEnd.position, gunEnd.rotation) as GameObject;
            tempShell.GetComponent<Rigidbody>().velocity = myRigidbody.velocity + (tempShell.transform.forward * SplashProjectileController.PulseVelocity);
            gameManager.photonView.RPC("SpawnPulseShell", PhotonTargets.MasterClient, args);
            if (!isGrounded)
            {
                myRigidbody.AddForce(-transform.forward * pulseShellFiringImpulse, ForceMode.Impulse);
            }
            pulseStamp = Time.time + timeBetweenPulse;
        }

        [PunRPC]
        public void DestroyLocalShell()
        {
            if (tempShell != null)
            {
                Destroy(tempShell);
                tempShell = null;
            }
        }

        public void FixedUpdate()
        {
            if (!photonView.isMine || isDead)
                return;


            if (!isLanded && !isGrounded)
            {
                /*
                 *  Start Hover Code
                 *  - This could likely be moved to a separate script for use on other things
                 *  - Provides mass relative upward force needed to maintain {height}
                 *  - TODO: Could handle high velocities a little better
                 */
                //Ray ray = new Ray(transform.position, -Vector3.up);
                //Debug.Log(rotationX + " " + rotationY);
                Vector3 clp = CenteredLowestPoint();
                Ray ray = new Ray(clp, -Vector3.up);
                Ray checkRay = new Ray(new Vector3(clp.x, transform.position.y, clp.z), -Vector3.up); // This double check helps make sure the tank doesn't get stuck in the ground
                RaycastHit hit;
                RaycastHit groundCheck;
                if (Physics.Raycast(ray, out hit, currentHeight) || (Physics.Raycast(checkRay, out groundCheck, currentHeight) && groundCheck.distance != 0 && hit.distance == 0))
                {
                    if (hit.distance < currentHeight)
                    {
                        float ftcGravity = Physics.gravity.y * myRigidbody.mass;
                        float ftcVelocity = myRigidbody.velocity.y * myRigidbody.mass;
                        float multi = (currentHeight - hit.distance) / currentHeight;
                        float force = (ftcGravity + ftcVelocity) * (multi * myRigidbody.mass);
                        myRigidbody.AddForce(new Vector3(0f, -force, 0f));
                    }
                }
                // End Hover Code
            }

            // Tank Jump
            if (requestJump)
            {
                AudioSource.PlayClipAtPoint(jumpSource, transform.position);
                myRigidbody.AddForce(transform.up * mySettings.JumpForce * myRigidbody.mass, ForceMode.Impulse);
                requestJump = false;
            }

            // Propulsion
            if (isLanded && (inputX != 0f || inputZ != 0f))
                TakeOff();
            float localXVelocity = Mathf.Abs(transform.InverseTransformDirection(myRigidbody.velocity).x);
            float localZVelocity = Mathf.Abs(transform.InverseTransformDirection(myRigidbody.velocity).z);
            float localXLimit = maxVelocityX * thrustMultiplier;
            float localZLimit = maxVelocityZ * thrustMultiplier;
            Vector3 relativeFwd = Vector3.Cross(Vector3.up, transform.right);
            if (localXVelocity > localXLimit)
                inputX = 0;
            if (localZVelocity > localZLimit)
                inputZ = 0;
            Vector3 totalSidewaysForce = transform.right * inputX * strafeThrust * myRigidbody.mass * boost;
            Vector3 totalForwardForce = relativeFwd * inputZ * mySettings.BaseThrust * myRigidbody.mass * boost;
            myRigidbody.AddForce(-totalForwardForce);
            myRigidbody.AddForce(totalSidewaysForce);
            //myRigidbody.AddRelativeForce(new Vector3((x * strafeThrust * myRigidbody.mass) * thrustMultiplier, 0, (z * baseThrust * myRigidbody.mass) * thrustMultiplier));
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
