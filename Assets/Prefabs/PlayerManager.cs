﻿using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3
{
    public class PlayerManager : Photon.PunBehaviour
    {
        // Inspector Vars
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
        private float timeBetweenPulse = 3f;
        private float pulseShellFiringImpulse = 8f;

        // Internal vars
        [HideInInspector]
        public bool isDead = false;
        [HideInInspector]
        public bool isSpawning = false;

        private GameManager gameManager;
        private GameObject tempShell;
        private GameObject serverShell;
        private IVehicleSetting mySettings;
        private Rigidbody myRigidbody;
        private FuelManager fuelManager;
        private PunTeams.Team initialTeam;
        private UnitType initialUnit;
        private int meshIndex = 0;
        private float pulseStamp;
        private float timeSinceDead = 0;

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

        private PlayerMotionController PMC;

        void Start() { }

        private void Awake()
        {
            myUnit = GetComponent<Unit>();
            if (myUnit != null)
                myUnit.unitTeam = (PunTeams.Team)photonView.instantiationData[0];
            else
                Debug.Log("*******[Player Error]*******: Unit.cs or Instantiation Data Not Found. This may occur if a scene was loaded with existing vehicles.");
            gameManager = FindObjectOfType<GameManager>();
            myRigidbody = GetComponent<Rigidbody>();
            if (photonView.isMine)
            {
                PlayerManager.LocalPlayerInstance = gameObject;
                fuelManager = GetComponent<FuelManager>();
                PMC = GetComponent<PlayerMotionController>();
            }
            PrepareForRespawn();
            DontDestroyOnLoad(gameObject);
        }

        public void SetMesh(int i)
        {
            for (int n = 0; n < meshList.Length; n++)
                meshList[n].gameObject.SetActive(false);
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
            }
            else if (i == 2)
            {
                dropPosition = cargoDropPositions[1];
                placePosition = unitPlacePositions[1];

            }
            //strafeThrust = mySettings.StrafePercent * mySettings.BaseThrust;
        }

        public int GetPlayerMeshIndexFromType(PunTeams.Team t, UnitType u)
        {
            if (t == PunTeams.Team.Blue && u == UnitType.Tank)
                return 0;
            else if (t == PunTeams.Team.Blue && u == UnitType.Scout)
                return 1;
            else if (t == PunTeams.Team.Red && u == UnitType.Tank)
                return 2;
            else if (t == PunTeams.Team.Red && u == UnitType.Scout)
                return 3;
            return -1;
        }


        public int GetMeshIndex()
        {
            return meshIndex;
        }

        public void Reset()
        {
            if (photonView.isMine)
            {
                myRigidbody.freezeRotation = false;
                PMC.Reset();
                timeSinceDead = 0;
                gameManager.Respawn(this);
            }
        }

        public void SetSelectedVehicle(int i)
        {
            if ((i == 1 || i == 3))
                myUnit.unitType = UnitType.Scout;
            else if ((i == 0 || i == 2))
                myUnit.unitType = UnitType.Tank;
            else
                myUnit.unitType = UnitType.Other;
            SetMesh(i);
        }

        [PunRPC]
        public void SetTeam(PunTeams.Team t)
        {
            myUnit.unitTeam = t;
        }

        [PunRPC]
        public void SetPosAndRotation(Vector3 pos, Quaternion rot, int meshIndex)
        {
            SetSelectedVehicle(meshIndex);
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
                if (myMesh != null)
                {
                    myMesh.gameObject.SetActive(true);
                    GetComponent<Collider>().enabled = true;
                    GetComponent<AudioSource>().Play();
                }
                GetComponent<KGFMapIcon>().SetVisibility(true);
                isDead = false;
                isSpawning = false;
                if (photonView.isMine)
                    myRigidbody.isKinematic = false;
            }

        }

        public void PrepareForRespawn()
        {
            isSpawning = true;
            if (myMesh != null)
            {
                myMesh.gameObject.SetActive(false);
                myRigidbody.isKinematic = true;
                myRigidbody.freezeRotation = true;
                GetComponent<Collider>().enabled = false;
            }
            GetComponent<KGFMapIcon>().SetVisibility(false);
            GetComponent<AudioSource>().Stop();
            Reset();
        }

        public void CheckIsDead()
        {
            if (myUnit.isDead && !isSpawning)
            {
                if (timeSinceDead == 0)
                {
                    isDead = true;
                    myRigidbody.freezeRotation = false;
                    myRigidbody.isKinematic = false;
                    if (PMC != null)
                        PMC.receiveInput = false;
                }
                timeSinceDead += Time.deltaTime;
                if (timeSinceDead >= destroyDelayWhenDead)
                {
                    Debug.Log("Player ready for respawn.");
                    PrepareForRespawn();
                }
            }
        }

        private void CheckFireSecondary()
        {
            if (InputEx.GetAxisRaw("Fire2") != 0)
            {
                if (myUnit.unitType == UnitType.Tank && Time.time >= pulseStamp && fuelManager.TakeFuel(fuelPerPulse))
                    CmdFirePulseShell();
                else if (myUnit.unitType == UnitType.Scout)
                    Debug.Log("PlayerMovementManager.cs (Line: 309) Scout Secondary Firing Detected.");
            }
        }

        void Update()
        {
            CheckIsDead();

            if (!photonView.isMine || isDead || isSpawning || PMC == null)
            {
                if (PMC != null && PMC.receiveInput)
                    PMC.receiveInput = false;
                return;
            }

            if (!Cursor.visible)
            {
                if (!PMC.receiveInput)
                    PMC.receiveInput = true;
                CheckFireSecondary(); // Detects tank or scout, fires appropriate weapon
            }
            else
            {
                if (PMC.receiveInput)
                    PMC.receiveInput = false;

            }

            if (tempShell != null && serverShell != null)
            {
                tempShell.transform.position = Vector3.Slerp(tempShell.transform.position, serverShell.transform.position, 50 * Time.deltaTime);
                if (Vector3.Distance(tempShell.transform.position, serverShell.transform.position) <= 0.05f)
                {
                    serverShell.GetComponent<SplashProjectileController>().Show();
                    Destroy(tempShell);
                    tempShell = null;
                    serverShell = null;
                }
            }

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
            if (!PMC.isGrounded)
                myRigidbody.AddForce(-transform.forward * pulseShellFiringImpulse, ForceMode.Impulse);
            pulseStamp = Time.time + timeBetweenPulse;
        }

        [PunRPC]
        public void DestroyLocalShell(int serverShellViewID)
        {
            PhotonView pv = PhotonView.Find(serverShellViewID);
            if (pv != null)
            {
                Debug.Log("+++++++++++++++++++++ Got server shell photonView +++++++++++++++++++++++++++++++++");
                serverShell = pv.gameObject;
                serverShell.GetComponent<SplashProjectileController>().Hide();
            } else
            {
                Debug.LogError("Failed to find server shell photonView");
            }
        }

        public void FixedUpdate()
        {
        }
    }
}
