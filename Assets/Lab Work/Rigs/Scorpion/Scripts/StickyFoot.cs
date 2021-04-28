using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class StickyFoot : MonoBehaviour
{
    public Transform stepPosition;

    public AnimationCurve verticleStepMovement;

    private Vector3 previousPlantedPosition;
    private Quaternion previousPlantedRotation = Quaternion.identity;

    private Vector3 plantedPosition;
    private Quaternion plantedRotation = Quaternion.identity;

    private float timeLength = .25f;
    private float timeCurrent = 0;

    void Start()
    {

    }


    void Update()
    {
        if (CheckIfCanStep())
        {
            DoRayCast();
        }

        if (timeCurrent < timeLength)
        { 

            timeCurrent += Time.deltaTime; 

            float p = timeCurrent / timeLength;

            plantedPosition.y = Mathf.Clamp(plantedPosition.y, 2, 100);

            Vector3 finalPosition = AnimMath.Lerp(previousPlantedPosition, plantedPosition, p);

            finalPosition.y += verticleStepMovement.Evaluate(p);

            transform.position = finalPosition;

        }
        else
        { 
            plantedPosition.y = Mathf.Clamp(plantedPosition.y, 2, 100);
            transform.position = plantedPosition;

        }


    }

    bool CheckIfCanStep()
    {

        Vector3 vBetween = transform.position - stepPosition.position;
        float threshold = 6;

        return (vBetween.sqrMagnitude > threshold * threshold);
    }

    void DoRayCast()
    {

        Ray ray = new Ray(stepPosition.position + Vector3.up, Vector3.down);

        Debug.DrawRay(ray.origin, ray.direction * 3);

        if (Physics.Raycast(ray, out RaycastHit hit, 3))
        {


            previousPlantedPosition = transform.position;
            previousPlantedRotation = transform.rotation;

            plantedPosition = hit.point;
            plantedRotation = Quaternion.FromToRotation(transform.up, hit.normal);

            timeCurrent = 0;
        }

    }
}
