using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jelsomeno
{

    public class PointAt : MonoBehaviour
    {

        private PlayerTargeting playerTargeting;


        private Quaternion startingRotation;

        public bool lockRotationX;
        public bool lockRotationY;
        public bool lockRotationZ;

        // Start is called before the first frame update
        void Start()
        {
            startingRotation = transform.localRotation;
            playerTargeting = GetComponentInParent<PlayerTargeting>();

        }

        // Update is called once per frame
        void Update()
        {
            TurnTowardsTarget();

        }

        private void TurnTowardsTarget()
        {

            if (playerTargeting && playerTargeting.target && playerTargeting.wantsToTarget)
            {
                Vector3 disToTarget = playerTargeting.target.position - transform.position;


                Quaternion targetRotation = Quaternion.LookRotation(disToTarget, Vector3.up);

                Vector3 euler1 = transform.localEulerAngles;
                Quaternion prevRot = transform.rotation;
                transform.rotation = targetRotation;
                Vector3 euler2 = transform.localEulerAngles;

                if (lockRotationX) euler2.x = euler1.x; //revert x to previous value 
                if (lockRotationY) euler2.y = euler1.y; //revert x to previous value 
                if (lockRotationZ) euler2.z = euler1.z; //revert x to previous value 

                transform.rotation = prevRot;
                transform.localRotation = AnimMath.Slide(transform.localRotation, Quaternion.Euler(euler2), .01f);
            }
            else
            {



                transform.localRotation = AnimMath.Slide(transform.localRotation, startingRotation, .05f);
            }





        }

    }
}
