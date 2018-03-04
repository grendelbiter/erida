using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3
{
    [RequireComponent(typeof(Unit))]
    public class GunTurretController : Photon.PunBehaviour
    {

        private int bulletDamageinHitpoints = 1;
        private float turnSpeed = 7f;
        private float bulletsPerSecond = 8f;
        public Transform gunEnd;
        public SphereCollider targetTrigger;

        private bool started = false;
        private bool shooting = false;
        private float rangeMax;
        private float timeBetweenShots;
        private float accumulatedDamage;
        private float damageSendLimit = 7f;
        private float timeAtNextEffect = 0;
        private float deviationConeRadius = 2.2f;
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

        void Update()
        {
            if (myUnit.hasPower) {
                DisplayEffectsIfShooting();
                if (PhotonNetwork.isMasterClient)
                {
                    TurnToCurrentTarget();
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
                                    Unit u = hit.transform.GetComponent<Unit>();
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
                }
            }
        }

        private void DisplayEffectsIfShooting()
        {
            if (shooting && Time.time >= timeAtNextEffect)
            {
                timeAtNextEffect = Time.time + timeBetweenShots;
                ShowFeedback();
            }
        }

        private void TurnToCurrentTarget()
        {
            if (currentTarget != null)
            {
                Vector3 lookPos = currentTarget.transform.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);
            }
        }

        private IEnumerator ShotEffect()
        {
            laserLine.enabled = true;
            yield return shotDuration;
            laserLine.enabled = false;
        }

        [PunRPC]
        private void SetAndSyncShooting(bool newValue)
        {
            if (shooting != newValue)
            {
                if (PhotonNetwork.isMasterClient)
                    photonView.RPC("SetAndSyncShooting", PhotonTargets.Others, shooting);
                shooting = newValue;
            }
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

        void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && !targetList.Contains(other.gameObject) && ValidTarget(other.gameObject.transform) && !other.isTrigger)
                targetList.Add(other.gameObject);
        }

        void OnTriggerStay(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && ValidTarget(other.gameObject.transform) && !targetList.Contains(other.gameObject) && !other.isTrigger)
                targetList.Add(other.gameObject);
        }

        void OnTriggerLeave(Collider other)
        {
            if (PhotonNetwork.isMasterClient && targetList.Contains(other.gameObject))
                targetList.Remove(other.gameObject);
        }
    }
}

