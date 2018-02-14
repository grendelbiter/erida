using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    [RequireComponent(typeof(Unit))]
    public class FlakTurretController : Photon.PunBehaviour {

        // Settings
        public Transform gunEnd;  // Needs to be set in inspector, empty transform, where projectiles spawn
        private float rangeMax = 100f; // Targets beyond this range will be discarded (Note: FlakTurret prefabs also employ a sphere trigger to track targetList)
        private float rangeMin = 10f;  // Targets below this range will be ignored
        private float fireDelay = 1.8f; // Delay between each three round burst
        private float shellDelay = 0.4f; // Delay between shells
        private float turnSpeed = 60; // This value should be high to make sure turrets can intercept fast moving targets
        private int shellCount = 3; // Shells per burst

        // Internal vars
        private List<GameObject> targetList = new List<GameObject>();
        private int shellCountCurrent = 0;
        private float fireStamp;
        private GameManager gameManager;
        private Transform currentTarget = null;
        private Vector3 currentIntercept = new Vector3(99999f, 99999f, 99999f);
        private float interceptTime;
        //private bool targetOnSight = false;
        private PunTeams.Team team;

        private Unit myUnit;
        private bool started = false;

        // Use this for initialization
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            myUnit = GetComponent<Unit>();
            team = transform.GetComponent<Unit>().unitTeam;
        }

        void Start() {
            fireStamp = Time.time;
            started = true;
        }

        // Update is called once per frame
        void Update() {
            if (myUnit.hasPower)
            {
                FindTarget();
                if (currentTarget != null)
                {
                    TurnTowardsCurrentTarget();
                    if (PhotonNetwork.isMasterClient)
                        FireAtTarget();
                }
            }
        }

        private void ResetTarget()
        {
            currentIntercept = new Vector3(99999f, 99999f, 99999f);
            interceptTime = -1f;
            currentTarget = null;
            //targetOnSight = false;

        }

        private void FindTarget() {
            if (currentTarget != null && shellCountCurrent > 0)
                return;
            // Clean target list, find new nearest target
            Transform closestVisibleTarget = null;
            Unit closestType = null;
            float minDistance = rangeMax * rangeMax; // We'll save some overhead comparing square distance instead of true distance
            for (int i = targetList.Count-1; i>=0; i--) {
                float tgtSqrDistance = (targetList[i].transform.position - transform.position).sqrMagnitude;
                if (targetList[i] == null || !ValidTarget(targetList[i].transform) || tgtSqrDistance > minDistance)
                {
                    targetList.RemoveAt(i);
                }
                else
                {
                    RaycastHit hit;
                    Physics.Raycast(gunEnd.position, gunEnd.forward, out hit, rangeMax);
                    if (hit.transform != null && ValidTarget(hit.transform))
                    {
                        tgtSqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
                        if (tgtSqrDistance > (rangeMin * rangeMin) && tgtSqrDistance < minDistance)
                        {
                            minDistance = tgtSqrDistance;
                            closestVisibleTarget = targetList[i].transform;
                        }
                    }
                }
            }
            currentTarget = closestVisibleTarget;
        }

        private void TurnTowardsCurrentTarget()
        {
            currentIntercept = getInterceptPoint(gunEnd.position, GetComponent<Rigidbody>().velocity, SplashProjectileController.FlakVelocity, currentTarget.position, currentTarget.GetComponent<Rigidbody>().velocity);
            Vector3 lookPos = currentIntercept - transform.position;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);
        }

        private void FireAtTarget()
        {
            if (currentTarget != null && shellCountCurrent > 0)
            {
                if (Time.time > fireStamp) // And have "reloaded"
                {
                    if (interceptTime <= 0.01f)
                        interceptTime = 12f;
                    if (shellCountCurrent < shellCount) // And have ammo
                    {
                        gameManager.SpawnFlakShell(gunEnd.position, gunEnd.rotation, team, interceptTime);
                        shellCountCurrent += 1;
                        fireStamp = Time.time + shellDelay;
                    }
                    else if (shellCountCurrent == shellCount) // End of salvo, reset
                    {
                        fireStamp = Time.time + fireDelay;
                        shellCountCurrent = 0;
                    }
                }
            }
        }


        private bool ValidTarget(Transform t)
        {
            // Check for both units and projectiles, further checks will probably be needed for other projectiles
            // Does not affect target priority
            Unit u = t.GetComponent<Unit>();
            SplashProjectileController p = t.GetComponent<SplashProjectileController>();
            if ((u != null && u.unitTeam != team) || (p != null && p.team != team))
                return true;
            return false;
        }

        Vector3 getInterceptPoint(Vector3 turretPos, Vector3 turretVel, float pSpeed, Vector3 targetPos, Vector3 targetVel)
        {
            Vector3 targetRPos = targetPos - turretPos; // Relative position
            Vector3 targetRVel = targetVel - turretVel; // Relative velocity
            interceptTime = getInterceptTime(pSpeed, targetRPos, targetRVel); // Find best intercept time / path
            return targetPos + targetRVel * interceptTime; // Current position + Velocity * Time = Predicted future position
        }

        float getInterceptTime(float pSpeed, Vector3 targetRPos, Vector3 targetRVel)
        {
            float targetSVel = targetRVel.sqrMagnitude;
            float targetSPos = targetRPos.sqrMagnitude;
            if (targetSVel < 0.001f)
                return 0f;
            float a = targetSVel - pSpeed * pSpeed;
            if (Mathf.Abs(a) < 0.001f)
            {
                float t = -targetSPos / (2f * Vector3.Dot(targetRVel, targetRPos));
                return Mathf.Max(t, 0f);
            }
            float b = 2f * Vector3.Dot(targetRVel, targetRPos);
            float d = b * b - 4f * a * targetSPos;
            if (d > 0f)
            {
                float t1 = (-b + Mathf.Sqrt(d)) / (2f * a), t2 = (-b - Mathf.Sqrt(d)) / (2f * a);
                if (t1 > 0f)
                {
                    if (t2 > 0f)
                        return Mathf.Min(t1, t2);
                    else
                        return t1;
                }
                else
                {
                    return Mathf.Max(t2, 0f);
                }
            }
            else if (d < 0f)
            {
                return 0f;
            }
            else
            {
                return Mathf.Max(-b / (2f * a), 0f);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && !targetList.Contains(other.gameObject) && ValidTarget(other.gameObject.transform))
                targetList.Add(other.gameObject);
        }

        void OnTriggerStay(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && !targetList.Contains(other.gameObject) && ValidTarget(other.gameObject.transform))
                targetList.Add(other.gameObject);
        }

        void OnTriggerLeave(Collider other)
        {
            if (PhotonNetwork.isMasterClient && targetList.Contains(other.gameObject))
                targetList.Remove(other.gameObject);
        }
    }
}
