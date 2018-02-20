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
                    DoEffects(transform.position);
            }
        }

        private void FixedUpdate()
        {
            if (PhotonNetwork.isMasterClient && !Collided) // Force masterclient control of velocity base detonation
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb.velocity != velocity)
                    SplashDetonation();
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

        void SplashDetonation()
        {
            /*
             * Desired Effect When Hit Here
             * 
             */
            DoEffects(transform.position);
        }

        void SplashDamage(Collider[] hitObjects, Vector3 hitPos, Transform originalHit)
        {
            // We should not get here unless we are masterclient (See OnCollisionEnter())
            foreach (Collider c in hitObjects)
            {
                if (!c.transform.Equals(originalHit)) // Ignore the object hit directly, it has already been damaged
                {
                    float pcnt = (splashRadius - Vector3.Distance(hitPos, c.transform.position)) / splashRadius;
                    DoDamage(c.transform, (int)Mathf.Ceil(directDamage * pcnt));
                }
            }
        }

        void OnCollisionEnter(Collision col) {
            if (PhotonNetwork.isMasterClient && !Collided) { // Force masterclient handling of damage and effects
                Collided = true;
                Vector3 hitPosition = col.contacts[0].point;
                Collider[] splashedObjects = Physics.OverlapSphere(hitPosition, splashRadius);
                DoEffects(hitPosition);
                DoDamage(col.transform, directDamage);
                SplashDamage(splashedObjects, hitPosition, col.transform);
            }
        }
    }
}
