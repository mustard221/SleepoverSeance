using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhysicsPickup : MonoBehaviour
{
    //distance from the camera the item is carried
    [Tooltip("How far object is held from self")]
    public float dist = 6.0f;

    [Tooltip("How high a held object will be")]
    public float height = 3.0f;

    //grab icon images
    public GameObject grabIMG;
    public GameObject grabbedIMG;

    //sound effects
    public AudioSource grab;

    //the object being held
    private GameObject curObject;
    private Rigidbody curBody;

    //the rotation of the curObject at pickup relative to the camera
    private Quaternion relRot;

    // Update is called once per frame
    void Update ()
    {
        // detect hover only when not holding an object
        RaycastHit hitInfo;
        bool hovering = false;
        if (curObject == null)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 5f) && hitInfo.rigidbody != null)
            {
                hovering = true;
            }
        }

        if (curObject != null)
        {
            grabIMG.SetActive(false);
            grabbedIMG.SetActive(true);
        }
        else if (hovering)
        {
            grabIMG.SetActive(true);
            grabbedIMG.SetActive(false);
        }
        else
        {
            grabIMG.SetActive(false);
            grabbedIMG.SetActive(false);
        }

        // on mouse click, either pickup or drop an item
        if (Input.GetMouseButtonDown(0))
        {
            if (curObject == null)
            {
                // only pick up if we were hovering a valid rigidbody
                if (hovering)
                {
                    PickupItem();

                    // immediately update icons when pickup succeeds
                    if (curObject != null)
                    {
                        grabIMG.SetActive(false);
                        grabbedIMG.SetActive(true);
                    }
                }
            }
            else
            {
                DropItem();

                // ensure icons are hidden after dropping
                grabIMG.SetActive(false);
                grabbedIMG.SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {
        if(curObject != null)
        {
            //keep the object in front of the camera
            ReposObject();
        }
    }

    //calculates the new rotation and position of the curObject
    void ReposObject()
    {
        //calculate the target position and rotation of the curbody
        Vector3 targetPos = transform.position + transform.forward + (transform.up / height) * dist;
        Quaternion targetRot = transform.rotation * relRot;

        //interpolate to the target position using velocity
        curBody.linearVelocity = (targetPos - curBody.position) * 10;

        //keep the relative rotation the same
        curBody.rotation = targetRot;

        //no spinning around
        curBody.angularVelocity = Vector3.zero;
    }

    //attempts to pick up an item straight ahead
    public void PickupItem()
    {
        grab.Play();

        //raycast to find an item
        RaycastHit hitInfo;
        Physics.Raycast(transform.position, transform.forward, out hitInfo, 5f);

        if(hitInfo.rigidbody == null)
            return;

        if (hitInfo.transform.CompareTag("Weapon")) //if the object is tagged as a weapon, switch to weapon script and return
        {
            var weaponComp = hitInfo.transform.GetComponent<WeaponObject>();
            if (weaponComp != null)
            {
                weaponComp.PickUpWeapon();
            }

            return;
        }

        curBody = hitInfo.rigidbody;
        curBody.useGravity = false;

        curObject = hitInfo.rigidbody.gameObject;

        //hack w/ parenting & unparenting to get the relative rotation
        curObject.transform.parent = transform;
        relRot = curObject.transform.localRotation;
        curObject.transform.parent = null;
    }

    //drops the current item
    public void DropItem()
    {
        grabbedIMG.SetActive(false);

        curBody.useGravity = true;
        curBody = null;
        curObject = null;
    }
}
