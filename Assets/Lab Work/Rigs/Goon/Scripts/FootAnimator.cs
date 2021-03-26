using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// this script animates the foot/legs by changing the local position of the object/ik target.
/// </summary>
/// 
public class FootAnimator : MonoBehaviour
{
    /// <summary>
    /// The local-space starting position of the object
    /// </summary>
    private Vector3 startingPos;

    /// <summary>
    /// the local space starting rotation for the object
    /// </summary>
    private Quaternion startingRot;

    /// <summary>
    /// an offset value to use for timing of the Sin wave that controle the walk animation.
    /// 
    /// a value of Mathf.PI woulbe be half a period
    /// </summary>
    public float stepOffset = 0;

    GoonController goon;

    private Vector3 targetPos;
    private Quaternion targetRot;

    void Start()
    {
        startingPos = transform.localPosition;
        startingRot = transform.localRotation;

        goon = GetComponentInParent<GoonController>();
    }


    void Update()
    {
        switch (goon.state)
        {
            case GoonController.States.Idle:
                AnimateIdle();
                break;
            case GoonController.States.Walk:
                AnimateWalk();
                break;
        }

        //transform.position = AnimMath.Slide(transform.position, targetPos, .01f);
        //transform.rotation = AnimMath.Slide(transform.rotation, targetRot, .01f);

    }

    void AnimateWalk()
    {
        Vector3 finalPos = startingPos;

        float time = ((Time.time + stepOffset) * goon.stepSpeed);

        //lateral movement
        float frontToBack = Mathf.Sin(time);
        finalPos += goon.moveDir * frontToBack * goon.walkScale.z;

        //verticl movement
        finalPos.y += Mathf.Cos(time) * goon.walkScale.y;

        //finalPos.x *= goon.walkScale.x;

        bool isOnGround = (finalPos.y < startingPos.y);

        if (isOnGround) finalPos.y = startingPos.y;


        // convert from z (-1 to 1) to p (0 to 1 to 0)
        float p = 1 - Mathf.Abs(frontToBack);

        float anklePitch = isOnGround ? 0 : -p * 20;

        transform.localPosition = finalPos;

        //targetPos = transform.TransformPoint(finalPos);

        transform.localRotation = startingRot * Quaternion.Euler(0, 0, anklePitch);

        //targetRot = transform.parent.rotation * startingRot * Quaternion.Euler(0, 0, anklePitch);
    }

    void AnimateIdle()
    {
        transform.localPosition = startingPos;
        transform.localRotation = startingRot;

        //targetPos = transform.TransformPoint(startingPos);
        //targetRot = transform.parent.rotation * startingRot;


        FindGround();
    }

    void FindGround()
    {

        Ray ray = new Ray(transform.position + new Vector3(0, .5f, 0), Vector3.down * 2);

        Debug.DrawRay(ray.origin, ray.direction);

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            transform.position = hit.point;

            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        }
        else
        {

        }
    }
}
