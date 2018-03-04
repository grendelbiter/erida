using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    public class AutoCannon : Photon.PunBehaviour {
        public AudioClip shootSound;
        public AudioClip hitSound;
        public AudioClip missSound;
		public AudioSource audioSource;
        private int bulletDamageinHitpoints = 1;
        private float bulletsPerSecond = 16; // Changed from 10 in Pre-Alpha Planning Iteration
        private float range = 125; // * 4 = 500m
        private float deviationConeRadius = 1;
        private float maximumDConeRadius = 12f;
        public int fuelPerBullet = 1;
		private GameManager gameManager;
		//Start of the laser
		public Transform gunEnd;
        public GameObject muzzleFlash;
        bool shooting = false;
		//camera for firing
        public float simDelayInSeconds = 0.1f;

        private float lastSimTime = 0;

		//wait for second on laser
		private WaitForSeconds shotDuration = new WaitForSeconds(.07f);
		//line render for gun shots
		public LineRenderer laserLine;
		//next fire of laser
		private float nextFire;

        public bool debug = true;

        private float timeBetweenShots;
        private float nextFireTime;

        //private Vector3 screenCenter;
        private Transform targetPosition;

        public List<Material> teamColorMaterials;

        // Use this for initialization
        void Start() {
            laserLine = GetComponent<LineRenderer>();
            timeBetweenShots = 1f / bulletsPerSecond;
            nextFireTime = Time.time;
            if (photonView.isMine)
            {
                //screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0.0f);
            }
            Unit u = GetComponent<Unit>();
            if (u.unitTeam == PunTeams.Team.Red)
            {
                laserLine.material = teamColorMaterials[0];
            } else if (u.unitTeam == PunTeams.Team.Blue)
            {
                laserLine.material = teamColorMaterials[1];
            } else
            {
                laserLine.material = teamColorMaterials[2];
            }
            audioSource = GetComponent<AudioSource>();
        }

        private GameManager GetGameManager() {
			if (gameManager == null) {
				gameManager = FindObjectOfType<GameManager>();
			}
			return gameManager;
		}


		private IEnumerator ShotEffect()
		{
			laserLine.enabled = true;
			yield return shotDuration;
			laserLine.enabled = false;

		}

        [PunRPC]
        public void SetShooting(bool newShootingValue, float dcr) {
            if (!photonView.isMine) {
                shooting = newShootingValue;
                deviationConeRadius = dcr;
            }   
        }

        // Update is called once per frame
        void Update() {

            if (photonView.isMine) {
                CheckAndFire();
            }
            ShowFeedback();
        }

        private void SetAndSyncShooting(bool newValue) {
            if (shooting != newValue) {
                shooting = newValue;
                SyncShooting();
            }
        }


        private void CheckAndFire() {
            if ((GetComponent<CargoManager>() != null && GetComponent<CargoManager>().isDeploying) || 
                Cursor.visible || 
                GetComponent<PlayerManager>().isDead ||
                !GetComponent<FuelManager>().CanTakeFuel(fuelPerBullet))
            {
                SetAndSyncShooting(false);
                return;
            }

            if (InputEx.GetMouseButton(0) || InputEx.GetAxisRaw("Fire1") != 0) {
                if (Time.time < nextFireTime)
                    return;
                GetComponent<FuelManager>().TakeFuel(fuelPerBullet);
                nextFireTime = Time.time + timeBetweenShots;
                TargetController targetInfo = GetComponent<TargetController>();
                if (targetInfo != null && targetInfo.currentPlayerTarget != null)
                {
                    Vector3 targetDir = targetInfo.currentPlayerTarget.transform.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
                    deviationConeRadius = Vector3.Angle(targetDir, transform.forward);
                } else
                {
                    deviationConeRadius = maximumDConeRadius;
                }
                deviationConeRadius = Mathf.Clamp(deviationConeRadius, 0f, maximumDConeRadius);
                Vector3 shotPoint = (gunEnd.rotation * GetRandomPointInCircle()) + (gunEnd.position + transform.forward * range);
                RaycastHit objectHit;
                Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
                bool targetFound = Physics.Linecast(rayOrigin, shotPoint, out objectHit);
                if (targetFound && ValidTarget(objectHit.transform))
                {
                    objectHit.transform.GetComponent<Unit>().TellServerTakeDamage((int) Mathf.Ceil(bulletDamageinHitpoints));
                }
                SetAndSyncShooting(true);
                AudioSource.PlayClipAtPoint(shootSound, gunEnd.position, 0.1f);
            }
            else {
                SetAndSyncShooting(false);
            }
        }

        private bool ValidTarget(Transform t)
        {
            if (t.GetComponent<Unit>() != null && t.GetComponent<Unit>().unitTeam != this.gameObject.GetComponent<Unit>().unitTeam)
                return true;
            return false;
        }

        private void ShowFeedback() {
            if (shooting && (lastSimTime + simDelayInSeconds) < Time.time) {
                lastSimTime = Time.time;

                //Laser Effect
                StartCoroutine(ShotEffect());
                Vector3 rayOrigin;
                RaycastHit hit;
                rayOrigin = gunEnd.position; // Cast from per-unit gun position
                if (photonView.isMine) // To the local player the collision point appears in a more natural position when our ray is cast from the apparent crosshair position
                    rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));

                Vector3 bulletHitPoint;
                Vector3 targetPoint = (gunEnd.rotation * GetRandomPointInCircle()) + (gunEnd.position + transform.forward * range);
                if (Physics.Linecast(rayOrigin, targetPoint, out hit)) {
                    bulletHitPoint = hit.point;
                    if (hit.transform && ValidTarget(hit.transform))
                        AudioSource.PlayClipAtPoint(hitSound, hit.point);
                    else
                        AudioSource.PlayClipAtPoint(missSound, targetPoint);
                }
                else {
                    bulletHitPoint = targetPoint;
                    AudioSource.PlayClipAtPoint(missSound, targetPoint);
                }

                laserLine.SetPosition(0, gunEnd.position);
                laserLine.SetPosition(1, bulletHitPoint);
                muzzleFlash.GetComponent<ParticleSystem>().Emit(1);
                if (!photonView.isMine)
                    audioSource.PlayOneShot(shootSound, 1);
            }    
        }

        private void SyncShooting() {
            photonView.RPC("SetShooting", PhotonTargets.All, shooting, deviationConeRadius);
        }

        private Vector3 GetRandomPointInCircle() {
            Vector2 randomPoint = Random.insideUnitCircle * deviationConeRadius;
            return new Vector3(randomPoint.x, randomPoint.y, 0);
        }


    }

}
