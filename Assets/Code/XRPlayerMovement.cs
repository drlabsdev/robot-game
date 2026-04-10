using System;
using UnityEngine;
using UnityEngine.XR;

public class XRPlayerMove : MonoBehaviour
{
    int speed = 5;
    public XRNode moveStick = XRNode.LeftHand;
    public XRNode rotStick = XRNode.RightHand;

    public Transform body;
    Rigidbody _rigidbody;
    Transform camTrans;
    bool justClicked = false;
    int rot = 0;

    void Start()
    {
        camTrans = Camera.main.transform;
        _rigidbody = GetComponent<Rigidbody>();
        body.position = new Vector3(camTrans.position.x, body.position.y, camTrans.position.z);
    }

    void Update()
    {
        InputDevices.GetDeviceAtXRNode(rotStick).TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joyStick);
        if (!justClicked)
        {
            if (joyStick.x > .9f)
            {
                rot += 90;
                _rigidbody.MoveRotation(Quaternion.Euler(0, rot, 0));
                justClicked = true;
            }
            else if (joyStick.x < -.9f)
            {
                rot -= 90;
                _rigidbody.MoveRotation(Quaternion.Euler(0, rot, 0));
                justClicked = true;
            }
        }
        else if (Math.Abs(joyStick.x) < .1f)
        {
            justClicked = false;
        }
    }

    void FixedUpdate()
    {
        InputDevices.GetDeviceAtXRNode(moveStick).TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 direction);
        Vector3 moveDir = camTrans.forward * direction.y + camTrans.right * direction.x;
        moveDir = moveDir.normalized * speed;
        moveDir.y = _rigidbody.linearVelocity.y;
        _rigidbody.linearVelocity = moveDir;
    }
}