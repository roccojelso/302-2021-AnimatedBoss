using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jelsomeno
{



    public class CameraOrbit : MonoBehaviour
    {

        public PlayerMovement moveScript;
        private PlayerTargeting targetScript;
        private Camera cam;

        private float yaw = 0;
        private float pitch = 0;

        public float cameraSensitivityX = 10;
        public float cameraSensitivityY = 10;

        public float shakeIntensity = 0;

        // Start is called before the first frame update
        private void Start()
        {
            targetScript = moveScript.GetComponent<PlayerTargeting>();
            cam = GetComponentInChildren<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            PlayerOrbitCamera();

            transform.position = moveScript.transform.position;

            //if aiming, set camera's rotation to look at target
            RotateCamToLookAtTarget();

            // "zoom" in the camera 
            ZoomCamera();

            ShakeCamera();
        }

        public void Shake(float intensity = 1)
        {
            if (intensity > 1)
            {

            }
            else
            {
                shakeIntensity += intensity;
                if (shakeIntensity > 1) shakeIntensity = 1;
            }

        }

        private void ShakeCamera()
        {
            if (shakeIntensity < 0) shakeIntensity = 0;

            if (shakeIntensity > 0) shakeIntensity -= Time.deltaTime;
            else return;

            Quaternion targetRot = AnimMath.Lerp(Random.rotation, Quaternion.identity, .999f);

            //cam.transform.localRotation *= targetRot;
            cam.transform.localRotation = AnimMath.Lerp(cam.transform.localRotation, cam.transform.localRotation * targetRot, shakeIntensity * shakeIntensity);
        }

        private void ZoomCamera()
        {
            float dis = 10;
            if (IsTargeting()) dis = 3;

            cam.transform.localPosition = AnimMath.Slide(cam.transform.localPosition, new Vector3(0, 0, -dis), .001f);

        }

        private bool IsTargeting()
        {
            return (targetScript && targetScript.target != null && targetScript.wantsToTarget);
        }


        private void RotateCamToLookAtTarget()
        {

            if (IsTargeting())
            {
                // if targeting, set location to look at target

                Vector3 vToTarget = targetScript.target.position - cam.transform.position;

                Quaternion targetRot = Quaternion.LookRotation(vToTarget, Vector3.up);

                cam.transform.rotation = AnimMath.Slide(cam.transform.rotation, targetRot, .001f);
            }
            else
            {
                // if NOT targeting, reset rotation

                cam.transform.localRotation = AnimMath.Slide(cam.transform.localRotation, Quaternion.identity, .001f);



            }

        }

        private void PlayerOrbitCamera()
        {
            float mx = Input.GetAxisRaw("Mouse X");
            float my = Input.GetAxisRaw("Mouse Y");

            yaw += mx * cameraSensitivityX;
            pitch += my * cameraSensitivityY;

            if (IsTargeting())
            {
                pitch = Mathf.Clamp(pitch, 15, 60);
                //find player yaw
                float playerYaw = moveScript.transform.eulerAngles.y;
                // clamp camer rig yaw to player yaw +- 30
                yaw = Mathf.Clamp(yaw, playerYaw - 40, playerYaw + 40);
            }
            else
            {
                pitch = Mathf.Clamp(pitch, -5, 89);
            }

            transform.rotation = AnimMath.Slide(transform.rotation, Quaternion.Euler(pitch, yaw, 0), .001f);
        }
    }
}
