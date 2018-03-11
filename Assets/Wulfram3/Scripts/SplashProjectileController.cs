using Assets.Wulfram3.Scripts.InternalApis.Classes;
using UnityEngine;

namespace Com.Wulfram3 {
    public class SplashProjectileController : Photon.PunBehaviour {
        /* This script facilitates control of our direct fire splash damage projectiles
         * 
         * Static variables are used in Start() to initialize each projectile with the proper
         * information and mesh     */

        public static int PulseVelocity = 50;
        public static int FlakVelocity = 75; // To intercept, flak must be faster than pulse

        public static int PulseDirectDamage = 300;
        public static int FlakDirectDamage = 120;

        public static int PulseSplashRadius = 14;
        public static int FlakSplashRadius = 18;

        public float lifetime; // This is used by pulse shells to control self detonation. Flak turret pass a "fuse" time when creating shells

        // Each projectile mesh should be a child transform of the main projectile and linked to these vars in the inspector.
        public Transform redPulse;
        public Transform bluePulse;
        public Transform flakShell;

        // These vars are used internally by the script, overwritten in Start() as needed.
        private Vector3 velocity;
        private int directDamage = 100;
        private int splashRadius = 6;
        private GameManager gameManager;
        [HideInInspector]
        public PunTeams.Team team;

        private UnitType firedBy;

        private bool Collided = false;

        // Use this for initialization
        void Start() {
            object[] instanceData = transform.GetComponent<PhotonView>().instantiationData;
            team = (PunTeams.Team) instanceData[0];
            firedBy = (UnitType) instanceData[1];

            if (firedBy == UnitType.FlakTurret)
            {
                velocity = FlakVelocity * transform.forward;
                directDamage = FlakDirectDamage;
                splashRadius = FlakSplashRadius;
                lifetime = Time.time + (float) instanceData[2]; // This value is determined by the turret
                redPulse.gameObject.SetActive(false);
                bluePulse.gameObject.SetActive(false);
                flakShell.gameObject.SetActive(true);
            }
            else
            {
                velocity = (Vector3)instanceData[2];
                directDamage = PulseDirectDamage;
                splashRadius = PulseSplashRadius;
                lifetime = Time.time + (262.5f / velocity.magnitude);
                flakShell.gameObject.SetActive(false);
                if (team == PunTeams.Team.Blue)
                {
                    redPulse.gameObject.SetActive(false);
                    bluePulse.gameObject.SetActive(true);
                }
                else if (team == PunTeams.Team.Red)
                {
                    redPulse.gameObject.SetActive(true);
                    bluePulse.gameObject.SetActive(false);
                }
            }
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = velocity;
            if (PhotonNetwork.isMasterClient)
                gameManager = FindObjectOfType<GameManager>();
        }

        void Update() {
            if (PhotonNetwork.isMasterClient && !Collided) // Force masterclient control of time based detonation
            {
                if (Time.time >= lifetime)
                    Detonate(null);
            }
        }

        private void FixedUpdate()
        {
            if (PhotonNetwork.isMasterClient && !Collided) // Force masterclient control of velocity base detonation
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb.velocity != velocity)
                    Detonate(null);
            }
        }

        void DoEffects(Vector3 pos)
        {
            // We should not get here unless we are masterclient (See Update(), OnCollisionEnter())
            gameManager.SpawnExplosion(pos, UnitType.Other);
            PhotonNetwork.Destroy(gameObject);
        }

        void DoDamage(Transform target, int amount)
        {
            // We should not get here unless we are masterclient (See OnCollisionEnter())
            Unit unit = target.GetComponent<Unit>();
            if (unit != null && unit.unitTeam != team)
                unit.TellServerTakeDamage(amount);
        }

        void Detonate(Transform originalHit)
        {
            if (originalHit != null)
                DoDamage(originalHit, directDamage);
            Collider[] splashedObjects = Physics.OverlapSphere(transform.position, splashRadius);
            foreach (Collider c in splashedObjects)
            {
                if (!c.transform.Equals(originalHit))
                {
                    float pcnt = (splashRadius - Vector3.Distance(transform.position, c.transform.position)) / splashRadius;
                    if (pcnt > 0)
                        DoDamage(c.transform, (int)Mathf.Ceil(directDamage * pcnt));
                }
            }
            DoEffects(transform.position);
        }

        void OnCollisionEnter(Collision col) {
            if (PhotonNetwork.isMasterClient && !Collided) { // Force masterclient handling of damage and effects
                Collided = true;
                Detonate(col.transform);
            }
        }
    }
}
