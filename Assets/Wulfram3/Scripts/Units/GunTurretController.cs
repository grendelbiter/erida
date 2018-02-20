using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3
{
    [RequireComponent(typeof(Unit))]
    public class GunTurretController : Photon.PunBehaviour
    {

        public int bulletDamageinHitpoints = 1;
        public float turnSpeed = 12;
        public float bulletsPerSecond = 2;
        public Transform gunEnd;
        public SphereCollider targetTrigger;

        private bool started = false;
        private bool shooting = false;
        private float rangeMax;
        private float timeBetweenShots;
        private float accumulatedDamage;
        private float damageSendLimit = 5f;
        private float timeSinceLastEffect = 0;
        private float deviationConeRadius = 1;
        private Unit myUnit;
        private Transform currentTarget = null;
        private LineRenderer laserLine;
        private WaitForSeconds shotDuration = new WaitForSeconds(.07f);
        private List<GameObject> targetList = new List<GameObject>();

        void Start()
        {
            timeBetweenShots = 1f / bulletsPerSecond;
            laserLine = GetComponent<LineRenderer>();
            myUnit = GetComponent<Unit>();
            rangeMax = targetTrigger.radius;
            started = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (myUnit.hasPower) { 
                if (shooting)
                {
                    timeSinceLastEffect += Time.deltaTime;
                    if (timeSinceLastEffect >= timeBetweenShots)
                    {
                        timeSinceLastEffect = 0f;
                        ShowFeedback();
                    }
                }
                if (PhotonNetwork.isMasterClient)
                {
                    if (currentTarget != null)
                    {
                        Vector3 lookPos = currentTarget.transform.position - transform.position;
                        Quaternion rotation = Quaternion.LookRotation(lookPos);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);
                    }
                    bool hadVisibleTarget = false;
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
                            if (hit.transform == currentTarget)
                            {
                                SetAndSyncShooting(true);
                                hadVisibleTarget = true;
                                accumulatedDamage += bulletsPerSecond * bulletDamageinHitpoints * Time.deltaTime;
                                if (accumulatedDamage >= damageSendLimit)
                                {
                                    Unit u = hit.transform.GetComponent<Unit>();
                                    if (u != null)
                                    {
                                        u.TellServerTakeDamage((int) damageSendLimit);
                                        accumulatedDamage -= damageSendLimit;
                                    }
                                }
                            }
                            else
                            {
                                SetAndSyncShooting(true);
                                hadVisibleTarget = true;
                                if (accumulatedDamage >= 1f)
                                {
                                    Unit u = currentTarget.GetComponent<Unit>();
                                    if (u != null)
                                    {
                                        u.TellServerTakeDamage((int)accumulatedDamage);
                                        accumulatedDamage = 0f;
                                    }
                                }
                                currentTarget = hit.transform;
                                accumulatedDamage += bulletsPerSecond * bulletDamageinHitpoints * Time.deltaTime;
                            }
                        }
                    }
                    if (!hadVisibleTarget && currentTarget != null && accumulatedDamage > 0f)
                    {
                        Unit u = currentTarget.GetComponent<Unit>();
                        if (u != null)
                        {
                            u.TellServerTakeDamage((int)accumulatedDamage);
                            accumulatedDamage = 0f;
                            currentTarget = null;
                            SetAndSyncShooting(false);
                        }
                    }


                    /*
                    FindTarget();
                    TurnTowardsCurrentTarget();
                    if (shooting)
                    {
                        timeSinceLastEffect += Time.deltaTime;
                        if (timeSinceLastEffect >= timeBetweenShots)
                        {
                            timeSinceLastEffect = 0f;
                            ShowFeedback();
                        }
                    }
                    if (PhotonNetwork.isMasterClient)
                    {
                        CheckTargetOnSight();
                        FireAtTarget();
                    }
                    */
                }
            }
        }

        private IEnumerator ShotEffect()
        {
            laserLine.enabled = true;
            yield return shotDuration;
            laserLine.enabled = false;
        }

        private void SetAndSyncShooting(bool newValue)
        {
            if (shooting != newValue)
            {
                shooting = newValue;
                SyncShooting();
            }
        }

        private void SyncShooting()
        {
            if (PhotonNetwork.isMasterClient)
                photonView.RPC("SetShooting", PhotonTargets.All, shooting);
        }

        private void ShowFeedback()
        {
            if (currentTarget != null && currentTarget.transform != null)
            {
                StartCoroutine(ShotEffect());
                laserLine.SetPosition(0, gunEnd.position);
                laserLine.SetPosition(1, currentTarget.transform.position);
            }
        }

        private Vector3 GetRandomPointInCircle()
        {
            Vector2 randomPoint = Random.insideUnitCircle * deviationConeRadius;
            return new Vector3(randomPoint.x, randomPoint.y, 0);
        }

        /*
        private void FireAtTarget()
        {
            if (currentTarget == null || targetOnSight == false)
            {
                targetOnSight = false;
                return;
            }
            timeSinceLastDamage += Time.deltaTime;
            if (timeSinceLastDamage >= 1f)
            {
                Vector3 pos = transform.position + (transform.forward * 3.0f + transform.up * 0.2f);
                Quaternion rotation = transform.rotation;
                RaycastHit objectHit;
                targetOnSight = Physics.Raycast(pos, transform.forward, out objectHit, rangeMax) && ValidTarget(objectHit.collider.transform);
                if (objectHit.transform != null)
                {
                    Unit hitUnit = objectHit.transform.GetComponent<Unit>();
                    if (targetOnSight && hitUnit.unitTeam != this.gameObject.GetComponent<Unit>().unitTeam)
                        hitUnit.TellServerTakeDamage((int)Mathf.Ceil(bulletDamageinHitpoints * bulletsPerSecond));
                    timeSinceLastDamage = 0;
                }
            }
        }*/

        private bool ValidTarget(Transform t)
        {
            if (t != null) {
                Unit u = t.GetComponent<Unit>();
                SplashProjectileController spc = t.GetComponent<SplashProjectileController>();
                if (u != null && u.unitTeam != myUnit.unitTeam)
                    return true;
                else if (spc != null && spc.team != myUnit.unitTeam)
                    return true;
            }
            return false;
        }
        /*
        private void CheckTargetOnSight()
        {
            if (currentTarget == null)
            {
                SetAndSyncShooting(false);
                targetOnSight = false;
                return;
            }

            RaycastHit objectHit;
            targetOnSight = Physics.Raycast(gunEnd.position, gunEnd.forward, out objectHit, rangeMax) && ValidTarget(objectHit.collider.transform);
            if (!targetOnSight)
            {
                SetAndSyncShooting(false);
            } else 
            {
                SetAndSyncShooting(true);
            }
        }*/
        /*
        private void TurnTowardsCurrentTarget()
        {
            if (currentTarget != null)
            {
                var distance = Vector3.Distance(currentTarget.transform.position, transform.position);
                if (distance >= rangeMax)
                {
                    targetOnSight = false;
                    currentTarget = null;
                    SetAndSyncShooting(false);
                    return;
                }
                else
                {
                    Vector3 lookPos = currentTarget.transform.position - transform.position;
                    Quaternion rotation = Quaternion.LookRotation(lookPos);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);
                }
            }
        }

        private void FindTarget()
        {

            timeSinceLastScan += Time.deltaTime;
            if (timeSinceLastScan >= scanInterval)
            {
                currentTarget = null;
                Transform closestTarget = null;
                float minDistance = rangeMax + 1f;
                Collider[] cols = Physics.OverlapSphere(transform.position, rangeMax);
                foreach (var col in cols)
                {
                    if (ValidTarget(col.transform))
                    {
                        float distance = Vector3.Distance(transform.position, col.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestTarget = col.transform;
                        }
                    }
                }
                currentTarget = closestTarget;
                timeSinceLastScan = 0;
            }
            if (currentTarget != null && currentTarget.transform == null)
                currentTarget = null;
        }*/

        void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && !targetList.Contains(other.gameObject) && ValidTarget(other.gameObject.transform))
                targetList.Add(other.gameObject);
        }

        void OnTriggerStay(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && ValidTarget(other.gameObject.transform) && !targetList.Contains(other.gameObject))
                targetList.Add(other.gameObject);
        }

        void OnTriggerLeave(Collider other)
        {
            if (PhotonNetwork.isMasterClient && targetList.Contains(other.gameObject))
                targetList.Remove(other.gameObject);
        }
    }
}

