using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    [RequireComponent(typeof(Unit))]
    public class FlakTurretController : Photon.PunBehaviour {

        // Settings
        public Transform gunEnd;  // Needs to be set in inspector, empty transform, where projectiles spawn
        public SphereCollider rangeTrigger;
        private float rangeMax; // Set by sphere collider attached to turret
        private float rangeMin = 18f;  // Targets below this range will be ignored
        private float fireDelay = 2.8f; // Delay between each three round burst
        private float shellDelay = 0.43f; // Delay between shells
        private float turnSpeed = 80; // This value should be high to make sure turrets can intercept fast moving targets
        private int shellCount = 3; // Shells per burst

        // Internal vars
        private List<GameObject> targetList = new List<GameObject>();
        private int shellCountCurrent = 0;
        private float fireStamp;
        private GameManager gameManager;
        private Quaternion currentTargetRotation;
        private float interceptTime;
        private PunTeams.Team team;

        private Unit myUnit;
        private bool started = false;

        private float defaultInterceptTime;

        // Use this for initialization
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            myUnit = GetComponent<Unit>();
            team = transform.GetComponent<Unit>().unitTeam;
            rangeMax = rangeTrigger.radius;
            defaultInterceptTime = rangeMax / SplashProjectileController.FlakVelocity;
        }

        void Start() {
            fireStamp = Time.time;
            started = true;
            currentTargetRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update() {
            if (PhotonNetwork.isMasterClient && myUnit.hasPower)
            {
                Transform closestVisibleTarget = FindClosestVisibleTarget();
                if (closestVisibleTarget != null)
                {
                    currentTargetRotation = Quaternion.LookRotation(GetInterceptPoint(closestVisibleTarget) - transform.position);
                }
                else
                {
                    currentTargetRotation = transform.rotation;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, currentTargetRotation, Time.deltaTime * turnSpeed);
                if (Quaternion.Angle(currentTargetRotation, transform.rotation) < 2f && (closestVisibleTarget != null || shellCountCurrent > 0))
                {
                    FireAtTarget();
                }
            }
        }

        private Transform FindClosestVisibleTarget()
        {
            Transform returnObject = null;
            if (shellCountCurrent == 0)
            {
                int currentTargetPriority = 0;
                float minDistance = rangeMax * rangeMax; // We'll save some overhead comparing square distance instead of true distance
                for (int i = targetList.Count - 1; i >= 0; i--)
                {
                    if (targetList[i] == null || targetList[i].transform == null || !ValidTarget(targetList[i].transform))
                    {
                        targetList.RemoveAt(i);
                        continue;
                    }

                    float tgtSqrDistance = (targetList[i].transform.position - transform.position).sqrMagnitude;
                    if (tgtSqrDistance > rangeMax * rangeMax)
                    {
                        targetList.RemoveAt(i);
                        continue;
                    }
                    RaycastHit hit;
                    Physics.Raycast(transform.position, targetList[i].transform.position - transform.position, out hit, rangeMax);
                    if (hit.transform != null && ValidTarget(hit.transform))
                    {
                        tgtSqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
                        int tgtPriority = GetTargetPriority(hit.transform);
                        if (tgtSqrDistance > (rangeMin * rangeMin) && (tgtSqrDistance < minDistance || tgtPriority > currentTargetPriority))
                        {
                            currentTargetPriority = tgtPriority;
                            minDistance = tgtSqrDistance;
                            returnObject = targetList[i].transform;
                        }
                    }
                }
            }
            return returnObject;
        }

        private Vector3 GetRandomPointInCircle()
        {
            Vector2 randomPoint = Random.insideUnitCircle * 0.01f;
            return new Vector3(randomPoint.x, randomPoint.y, 0);
        }

        private int GetTargetPriority(Transform t)
        {
            int rv = 2;
            Unit u = t.GetComponent<Unit>();
            if (u != null)
            {
                rv = u.targetPriority;
            }
            return rv;
        }


        private void FireAtTarget()
        {
            if (Time.time > fireStamp) // "Loaded"
            {
                if (interceptTime <= 0.01f)
                    interceptTime = defaultInterceptTime;
                if (shellCountCurrent < shellCount) // And have ammo
                {
                    gameManager.SpawnFlakShell(gunEnd.position, Quaternion.LookRotation((gunEnd.rotation * GetRandomPointInCircle()) + gunEnd.forward), team, interceptTime);
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

        private Vector3 GetInterceptPoint(Transform t)
        {
            Vector3 targetRVel = t.GetComponent<Rigidbody>().velocity - GetComponent<Rigidbody>().velocity; // Relative velocity
            interceptTime = GetInterceptTime(t);
            return t.position + targetRVel * interceptTime; // Current position + Velocity * Time = Predicted future position
        }

        private float GetInterceptTime(Transform tgt)
        {
            Vector3 targetRPos = tgt.position - gunEnd.position;
            Vector3 targetRVel = tgt.GetComponent<Rigidbody>().velocity - GetComponent<Rigidbody>().velocity; // Relative velocity
            float targetSVel = targetRVel.sqrMagnitude;
            float targetSPos = targetRPos.sqrMagnitude;
            if (targetSVel < 0.001f)
                return 0f;
            float a = targetSVel - SplashProjectileController.FlakVelocity * SplashProjectileController.FlakVelocity;
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
            if (PhotonNetwork.isMasterClient && started && !targetList.Contains(other.gameObject) && ValidTarget(other.gameObject.transform) && !other.isTrigger)
                targetList.Add(other.gameObject);
        }

        void OnTriggerStay(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && ValidTarget(other.gameObject.transform) && !other.isTrigger)
            {
                if (!targetList.Contains(other.gameObject) && (other.transform.position - transform.position).sqrMagnitude > (rangeMin * rangeMin))
                    targetList.Add(other.gameObject);
                else if ((other.transform.position - transform.position).sqrMagnitude <= (rangeMin * rangeMin))
                    targetList.Remove(other.gameObject);
            }
        }

        void OnTriggerLeave(Collider other)
        {
            if (PhotonNetwork.isMasterClient && targetList.Contains(other.gameObject))
                targetList.Remove(other.gameObject);
        }
    }
}
