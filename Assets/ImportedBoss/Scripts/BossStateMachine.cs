using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Jelsomeno
{


    public class BossStateMachine : MonoBehaviour
    {
        static class States
        {
            public class State
            {

                protected BossStateMachine bossState;

                virtual public State Update()
                {
                    return null;
                }

                virtual public void OnStart(BossStateMachine bossState)
                {
                    this.bossState = bossState;
                }

                virtual public void OnEnd()
                {

                }
            }

            ///////// Child Classes (Boss States):
            ///

            public class Idle : State
            {

                float idleTime = 5;

                public Idle()
                {
                    idleTime = 5;
                }

                public override State Update()
                {
                    // behaviour:
                    bossState.IdleAnim();

                    // transition: 
                    if (bossState.health <= 0)
                    {

                        bossState.bossNav.enabled = false;
                        return new States.DeathAnim();
                    }

                    if(bossState.bulletAmount > 0 && bossState.CanSeePlayer(bossState.player, bossState.viewingDistance))
                    {
                        bossState.bossNav.isStopped = false;
                        return new States.Attack();
                    }

                    idleTime -= Time.deltaTime;
                    if (!bossState.CanSeePlayer(bossState.player, bossState.viewingDistance) && idleTime <= 0)
                        return new States.Roaming(true); 

                    return null;
                }
            }

            public class Roaming : State
            {

                bool patrollingPoint = true;

                public Roaming(bool patrolling)
                {
                    patrollingPoint = patrolling;
                }

                public override State Update()
                {
                    // behaviour:
                    if (patrollingPoint)
                    { 
                        bossState.PatrolingPointsSetter(); 
                        patrollingPoint = false; 
                    }

                    // transitions:
                    if (bossState.health <= 0)
                    {

                        bossState.bossNav.enabled = false;
                        return new States.DeathAnim();
                    }

                    if (!bossState.bossNav.pathPending && bossState.bossNav.remainingDistance <= 2f) 
                        return new States.Idle(); 

                    if (bossState.CanSeePlayer(bossState.player, bossState.viewingDistance))
                    {
                        bossState.bossNav.isStopped = false;
                        return new States.Attack();
                    }

                    return null;
                }
            }

            public class Attack : State
            {

                public override State Update()
                {
                    // behaviour:

                    bossState.ShootAt(); // use the the method for shooting
                    bossState.LookAtPlayer(); // points at the player by using this method

                    // transition:
                    if (bossState.health <= 0)
                    {

                        bossState.bossNav.enabled = false;
                        return new States.DeathAnim();
                    }

                    if (bossState.bulletAmount <= 0) return new States.Reload(bossState.reloadingTime); // reload 

                    if (bossState.bulletAmount > 0 && bossState.CanSeePlayer(bossState.player, bossState.viewingDistance))
                    {
                        bossState.bossNav.isStopped = true;
                        return new States.Idle();
                    }

                    return null;
                }
            }

            public class Reload : State
            {

                float reloadTime = 5; // how long it takes to reload

                public Reload(float reload)
                {
                    reloadTime = reload; // timer is reset
                }

                public override State Update()
                {

                    reloadTime -= Time.deltaTime; // reload timer

                    if (reloadTime <= 0) return new States.Idle(); // goes to idle well reloading

                    return null;
                }

                // end of this state
                public override void OnEnd()
                {
                    bossState.bulletAmount = bossState.maxRounds; // reload back to max
                }
            }

            public class DeathAnim : State
            {

                public override State Update()
                {
                    // behaviour:
                    bossState.DeathAnimation();

                    return null;
                }
            }


        }

        private States.State state;

        public Transform hoverBody;

        private NavMeshAgent bossNav;

        public Transform[] patrolingPoints;

        int currentPointPatrolling;

        public Transform player;

        public Projectile bullet;

        public float viewingDistance = 10;

        public float AttackDistance = 35;

        public float reloadingTime = 0;

        public int maxRounds = 50;

        public float roundPerSec = 20;

        public Transform bulletSpawn;

        private int bulletAmount = 50;

        private float bulletAmountTime = 0;

        private Quaternion startingRotation;

        public bool lockRotationX;

        public bool lockRotationY;

        public bool lockRotationZ;

        bool deathAnimLiftLegs = false;

        public float health = 10;

        void Start()
        {
            bossNav = GetComponent<NavMeshAgent>();

        }


        void Update()
        {
            if (state == null) SwitchStates(new States.Idle());

            if (state != null) SwitchStates(state.Update());

            if (bulletAmountTime > 0) bulletAmountTime -= Time.deltaTime;
        }

        void SwitchStates(States.State stateSwitched)
        {
            if (stateSwitched == null) return;

            if (state != null) state.OnEnd();
            state = stateSwitched;
            state.OnStart(this);
        }

        void IdleAnim()
        {
            hoverBody.localPosition = Vector3.down * .25f * Mathf.Cos(Time.time);
            hoverBody.localRotation = Quaternion.Euler(2f * Mathf.Sin(Time.time), 4f * Mathf.Sin(Time.time), 2f * Mathf.Cos(Time.time));
        }

        private bool CanSeePlayer(Transform player, float viewingDis)
        {

            if (!player) return false;

            Vector3 disToTarget = player.position - transform.position;

            if (disToTarget.sqrMagnitude > viewingDis * viewingDis)
            {
                return false;
            }

            return true;
        }

        void LookAtPlayer()
        {
            if (player) // player is the target
            {
                Vector3 disToTarget = player.position - transform.position; // how far away is the player

                Quaternion targetRotation = Quaternion.LookRotation(disToTarget, Vector3.up); // how the player is rotated
                Vector3 euler1 = transform.localEulerAngles; // get local angles 
                Quaternion prevRot = transform.rotation;
                transform.rotation = targetRotation; // Set Rotation
                Vector3 euler2 = transform.localEulerAngles; // get local angles again

                if (lockRotationX) euler2.x = euler1.x; //revert to  previous value;
                if (lockRotationY) euler2.y = euler1.y; //revert to previous value;
                if (lockRotationZ) euler2.z = euler1.z; //revert to previous value;

                transform.rotation = prevRot; // go back to previous Rotaion

                transform.localRotation = AnimMath.Slide(transform.localRotation, Quaternion.Euler(euler2), .5f); // slides into a smoother rotation
            }
            else
            {

                transform.localRotation = AnimMath.Slide(transform.localRotation, startingRotation, .05f);
            }
        }

        void ShootAt()
        {
            if (bulletAmountTime > 0) return; // how fast the tank shoots

            Projectile Bullets = Instantiate(bullet, bulletSpawn.position, bulletSpawn.transform.rotation); // spawns bullet object
            Bullets.InitBullet(transform.forward * 30); // speed of object

            bulletAmount--; // removes a bullet from the tanks current ammo count
            bulletAmountTime = 1 / roundPerSec; // causes the rate of fire
        }


        void DeathAnimation()
        {

            if (!deathAnimLiftLegs)
            {
                hoverBody.localPosition = Vector3.Slerp(hoverBody.localPosition, Vector3.up * 1f, 1f * Time.deltaTime);

                if (hoverBody.localPosition.y >= .95f) deathAnimLiftLegs = true;

                return;
            }

            if (hoverBody.localPosition.y > -2.5f)
            {
                hoverBody.localPosition = Vector3.Slerp(hoverBody.localPosition, Vector3.down * 2.8f, 1f * Time.deltaTime);
                hoverBody.localRotation = Quaternion.Euler(1f * Mathf.Sin(5 * Time.time), 1f * Mathf.Sin(5 * Time.time), .8f * Mathf.Cos(5 * Time.time));
            }

        }


        void PatrolingPointsSetter()
        {
            bossNav.updatePosition = true; 
            bossNav.destination = patrolingPoints[currentPointPatrolling].position; 
            currentPointPatrolling = (currentPointPatrolling + 1) % patrolingPoints.Length;
        }
    }
}
