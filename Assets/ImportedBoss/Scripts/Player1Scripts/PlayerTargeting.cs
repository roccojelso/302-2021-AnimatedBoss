using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Jelsomeno
{

    public class PlayerTargeting : MonoBehaviour
    {

        public Transform target;
        public bool wantsToTarget = false;
        public bool wantsToAttack = false;
        public float visionDistance = 10;
        public float visionAngle = 45;
        private List<TargetableThing> potentialTargets = new List<TargetableThing>();

        float cooldownScan = 0;
        float cooldownPick = 0;

        float cooldownShoot = 0;

        public float roundsPerSecond = 10;

        public Transform armL;
        public Transform armR;

        private Vector3 startPosArmL;
        private Vector3 startPosArmR;

        public ParticleSystem prefabMuzzleFlash;
        public Transform handR;
        public Transform handL;

        CameraOrbit camOrbit;

        private AudioSource soundPlayer;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            startPosArmL = armL.localPosition;
            startPosArmR = armR.localPosition;

            soundPlayer = GetComponentInChildren<AudioSource>();

            camOrbit = Camera.main.GetComponentInParent<CameraOrbit>();
        }

        void Update()
        {

            wantsToTarget = Input.GetButton("Fire2");

            wantsToAttack = Input.GetButton("Fire1");

            if (!wantsToTarget) target = null;

            cooldownScan -= Time.deltaTime; // counting down
            if (cooldownScan <= -0 || (target == null && wantsToTarget)) ScanForTargets(); // do this when countdown finsished

            cooldownPick -= Time.deltaTime;
            if (cooldownPick <= 0) PickATarget();

            if (cooldownShoot > 0) cooldownShoot -= Time.deltaTime;

            if (target && !CanSeeThing(target)) target = null;

            SlideArmsHome();

            DoAttack();



        }

        private void SlideArmsHome()
        {
            armL.localPosition = AnimMath.Slide(armL.localPosition, startPosArmL, .01f);
            armR.localPosition = AnimMath.Slide(armR.localPosition, startPosArmR, .01f);
        }

        private void DoAttack()
        {
            if (cooldownShoot > 0) return;
            if (!wantsToTarget) return; // not targeting
            if (!wantsToAttack) return; // not shooting
            if (target == null) return; // no target
            if (!CanSeeThing(target)) return;

            HealthSystem health = target.GetComponent<HealthSystem>();

            if (health)
            {
                health.TakeDamage(20);
            }

            cooldownShoot = 1 / roundsPerSecond;
            soundPlayer.Play();
            // attack

            camOrbit.Shake(.5f);

            Instantiate(prefabMuzzleFlash, handL.position, handL.rotation);
            Instantiate(prefabMuzzleFlash, handR.position, handR.rotation);

            // trigger/arm animation 
            //this rotates the arms up/recoil effect 1
            armL.localEulerAngles += new Vector3(-20, 0, 0);
            armR.localEulerAngles += new Vector3(-20, 0, 0);

            // moves the arms backwards/ recoil effect 2
            armL.position += -armL.forward * .1f;
            armR.position += -armR.forward * .1f;
        }

        private bool CanSeeThing(Transform thing)
        {

            if (!thing) return false;

            Vector3 vToThing = thing.position - transform.position;

            if (vToThing.sqrMagnitude > visionDistance * visionDistance) return false;

            if (Vector3.Angle(transform.forward, vToThing) > visionAngle) return false;

            // TODO: check occlusion

            return true;



        }

        private void ScanForTargets()
        {
            cooldownScan = 1; // do the next scan in 2 seconds


            potentialTargets.Clear(); // empty list

            // refill list

            TargetableThing[] things = GameObject.FindObjectsOfType<TargetableThing>();
            foreach (TargetableThing thing in things)
            {
                // check how far away thing is

                //if we can see it

                if (CanSeeThing(thing.transform))
                {
                    potentialTargets.Add(thing);
                }

            }
        }

        void PickATarget()
        {

            cooldownPick = .25f;

            //if (target) return; // we already have a target
            target = null;


            float closestDistanceSoFar = 0;

            //find closest targetable thing and sets it as our target
            foreach (TargetableThing pt in potentialTargets)
            {
                float dd = (pt.transform.position - transform.position).sqrMagnitude;

                if (dd < closestDistanceSoFar || target == null)
                {
                    target = pt.transform;
                    closestDistanceSoFar = dd;
                }

            }


        }
    }
}
