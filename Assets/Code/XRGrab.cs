using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class XRGrab : MonoBehaviour
{
    private float launchForce = 5; //how moch force to apply to a throw

    // Far Grab
    private float raycastDist = 50; //how far away can an object be grabbed

    //Close Grab
    private float grabRaduis = .2f; //close grab raduis
    Collider[] hitColliders = new Collider[1];

    public Transform holdPoint; // where the players hand would be
    public LayerMask grabbableLayer; // What layers can be grabbed

    private Transform hoverObject = null;
    public Material glowMat;
    Material baseMat;

    private Transform heldObject = null; // The held object's transform if a object is held
    private Rigidbody heldRigidbody = null; // The held object's Rigidbody
    public XRNode handRole = XRNode.LeftHand;
    bool gripState = false;

    void Update()
    {
        InputDevices.GetDeviceAtXRNode(handRole).TryGetFeatureValue(CommonUsages.gripButton, out bool grip);

        if (grip && !gripState) // on grip down
        {
            //On grip, check for an object to pick up. If an object is already held throw it.
            if (heldObject == null)
            {
                FarGrab();
                //CloseGrab(); //swap this for FarGrab to change the pickup behavior
            }
        }
        else if (!grip && gripState && heldObject != null)
        {
            LaunchObject();
        }

        gripState = grip;
    }

    void FarGrab()
    {
        //Cast a ray from the controller position in the direction the controller is facing for a distance of raycastDist.
        //Return a collision with an object on a grabbable layer if one is detected.

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDist, grabbableLayer))
        {
            if (hit.transform.parent == null) // Make sure it is not already held
            {
                StartCoroutine(PickUpObject(hit.transform)); //pass in the transform of the collider that was hit
            }
        }
    }

    void CloseGrab()
    {
        //Create a sphere in a radius around the controller.
        if (Physics.OverlapSphereNonAlloc(transform.position, grabRaduis, hitColliders, grabbableLayer) > 0)
        {
            if (hitColliders[0].transform.parent == null) // Make sure it is not already held
            {
                StartCoroutine(PickUpObject(hitColliders[0].transform)); //Return the first object on the grabbable layer that is detected
            }
        }
    }

    IEnumerator PickUpObject(Transform _trans)
    {
        heldObject = _trans;
        heldRigidbody = heldObject.GetComponent<Rigidbody>();
        heldRigidbody.isKinematic = true; //ignore gravity and other physics while held

        float t = 0;
        while (t < .5f)
        {
            //lerp the position of the object to the held position for .5 sec
            heldRigidbody.MovePosition(Vector3.Lerp(heldRigidbody.position, holdPoint.position, t));
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        SnapToHand(); //When it is close snap it into place
    }

    void SnapToHand()
    {
        heldObject.position = holdPoint.position;
        heldObject.parent = holdPoint; // make it a child of the hold position so it inharits the position and rotation
    }

    void LaunchObject()
    {
        StopAllCoroutines(); //if the grab coroutine is still running, stop it and skip to the end
        SnapToHand();

        heldRigidbody.isKinematic = false; //regular physics like gravity is active again
        heldRigidbody.linearVelocity = Vector3.zero; //reset the velocity when the rigidbody becomes active again
        heldRigidbody.AddForce(transform.forward * launchForce, ForceMode.VelocityChange);  //throw in the direction the controller is facing
        //ForceMode.VelocityChange means add an instant velocity, and the same for any object regardless of mass

        heldObject.parent = null; //remove it as a child and set it back on the root level of the hierarchy
        StartCoroutine(LetGo());
    }

    IEnumerator LetGo()
    {
        yield return new WaitForSeconds(.1f);
        heldObject = null; //remove the reference to the object 
    }

    private void FixedUpdate()
    {
        //If you change to close grab uptate this to match, using OverlapSphereNonAlloc as well
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, raycastDist, grabbableLayer))
        {
            if (hoverObject != hit.transform)
            {
                if (hoverObject != null)
                {
                    hoverObject.GetComponent<Renderer>().material = baseMat;
                }
                hoverObject = hit.transform;
                baseMat = hoverObject.GetComponent<Renderer>().material;
                hoverObject.GetComponent<Renderer>().material = glowMat;
            }
        }
        else
        {
            if (hoverObject != null)
            {
                hoverObject.GetComponent<Renderer>().material = baseMat;
                hoverObject = null;
            }
        }
    }

}
