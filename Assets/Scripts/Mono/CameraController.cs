using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset;
    public float Smooth;
    private Vector3 velocity;
    public  Vector2 xRange;
    public Vector2 yRange;


    // Update is called once per frame
    void Update()
    {
        if(Target!=null)
        {
            Vector3 pos = Vector3.SmoothDamp(transform.position, Target.position + Offset, ref velocity, Time.deltaTime * Smooth);
            SetPosition(pos);
        }
    }

    private void SetPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, xRange.x, xRange.y);
        pos.y = Mathf.Clamp(pos.y, yRange.x, yRange.y);
        pos.z = -10;
        transform.position = pos;

    }
}
