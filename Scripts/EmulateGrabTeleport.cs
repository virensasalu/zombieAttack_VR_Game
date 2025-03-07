using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//************************************************************************************************//
//*********************THIS SCRIPT EMULATES TELEPORTATION LOCOMOTION IN VR************************//
//************************************************************************************************//

//Click down the right mouse button to activate the teleportation destination visual and move the
//mouse to point to desired destination. Press the 'T' key (T for Teleport) to get teleported there.

//Press the 'C' key (C for Controller) while the Game View is active and rotate the Controller 
//with the left mouse button. Grab objects by pointing and clicking down the left mouse button
//Scroll up and down the mouse wheel to move the grabbed object in z-axis (forward/backward).


public class EmulateGrabTeleport : MonoBehaviour
{
    float controllerSpeedHorizontal = 1.5f;
    float controllerSpeedVertical = 1.5f;
    float controllerYaw = 0.0f;
    float controllerPitch = 0.0f;

    private bool isGrabbing = false;
    private Transform grabbedTransform;
    public float zSpeed = 4.0f;
    public float rotationSpeedMultiplier = 2.0f;
    private Transform hitTransform;

    public GameObject playerController; //Holds the gameobject to be moved when there is a teleportation
    public GameObject targetVisual;     //Holds the teleportation target visual
    private Material targetMaterial;    //Holds the teleportation target visual's material
    private bool waitTeleportation;     //A control variable to prevent multiple teleportation triggers
    private Vector3 hitPos;             //A variable that holds the position to be teleported to

    void Start()
    {
        targetMaterial = targetVisual.GetComponent<Renderer>().material;
        waitTeleportation = false;
        targetVisual.transform.GetComponent<Renderer>().enabled = false;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            controllerYaw += controllerSpeedHorizontal * Input.GetAxis("Mouse X") * rotationSpeedMultiplier;
            controllerPitch += controllerSpeedVertical * Input.GetAxis("Mouse Y") * -rotationSpeedMultiplier;
            transform.localRotation = Quaternion.Euler(controllerPitch, controllerYaw, 0.0f);
        }

        RaycastHit hitInfo2;
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out hitInfo2))
        {
            if (hitInfo2.transform.tag == "Grabbable" && !isGrabbing)
            {
                if (hitTransform != null)
                    SetHighlight(hitTransform, false);

                hitTransform = hitInfo2.transform;
                SetHighlight(hitTransform, true);
            }
            else
            {
                if (hitTransform != null && !isGrabbing)
                    SetHighlight(hitTransform, false);
            }
        }
        else
        {
            if (hitTransform != null && !isGrabbing)
            {
                SetHighlight(hitTransform, false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(new Ray(transform.position, transform.forward), out hitInfo))
            {
                if (hitInfo.transform.tag == "Grabbable")
                {
                    isGrabbing = true;
                    grabbedTransform = hitInfo.transform;
                    grabbedTransform.GetComponent<Rigidbody>().isKinematic = true;
                    grabbedTransform.GetComponent<Rigidbody>().useGravity = false;
                    grabbedTransform.parent = transform;
                }
            }
        }
        if (isGrabbing && Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (grabbedTransform != null)
            {
                grabbedTransform.GetComponent<Rigidbody>().isKinematic = false;
                grabbedTransform.GetComponent<Rigidbody>().useGravity = true;
                grabbedTransform.parent = null;
            }
            isGrabbing = false;
        }

        if (isGrabbing)
        {
            float distance = Input.mouseScrollDelta.y;

            grabbedTransform.position += distance * Time.deltaTime * zSpeed * transform.forward;
            grabbedTransform.localPosition = new Vector3(grabbedTransform.localPosition.x, grabbedTransform.localPosition.y,
                                                         Mathf.Clamp(grabbedTransform.localPosition.z, 0.4f, 7.0f));
        }

        //If the user clicks down the right mouse button, we enable the destination target visual
        //And update its position based on where the user is pointing at
        if (Input.GetKey(KeyCode.Mouse1))
        {
            targetVisual.transform.GetComponent<Renderer>().enabled = true;

            RaycastHit hitInfo3;
            if (Physics.Raycast(new Ray(transform.position, transform.forward), out hitInfo3))
            {
                if (hitInfo3.transform.tag == "Ground") //If there is a hit with the ground
                {
                    //We update the position of the target visual and rotation to conform to the terrain's shape
                    targetVisual.transform.position = new Vector3(hitInfo3.point.x,
                                                      hitInfo3.point.y + 0.01f, hitInfo3.point.z);
                    targetVisual.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo3.normal);

                    //If the user presses down the T key, we store the position to be teleported and call
                    //the Teleportation coroutine
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        hitPos = new Vector3(hitInfo3.point.x, hitInfo3.point.y + 1.02f, hitInfo3.point.z);

                        if (!waitTeleportation)
                            StartCoroutine(Teleportation());

                    }
                }
            }
        }

        //If the user doesn't click the right mouse button anymore, we hide the teleportation target visual
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            targetVisual.transform.GetComponent<Renderer>().enabled = false;
        }
    }

    //This coroutiune enables teleportation (instant position update)
    IEnumerator Teleportation()
    {
        //We make the control variable true to indicate that teleportation is in progress
        waitTeleportation = true;
        targetMaterial.color = new Color(0.0f, 1.0f, 0.0f, 0.7f);
        targetVisual.GetComponent<AudioSource>().Play();

        //We wait for 0.35 seconds for the color change of the target visual to be noticable
        yield return new WaitForSeconds(0.35f);

        //We update the position of the playerController with the new position (teleport)
        playerController.transform.position = hitPos; 
        targetMaterial.color = new Color(1.0f, 1.0f, 0.0f, 0.7f);
        targetVisual.transform.GetComponent<Renderer>().enabled = false;
        //We make the control variable false to indicate that teleportation is finished
        waitTeleportation = false; 
    }

    void SetHighlight(Transform t, bool highlight)
    {
        if (highlight)
        {
            t.GetComponent<Renderer>().material.color = Color.cyan;
            t.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineAll;
            transform.GetComponent<LineRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
        else
        {
            t.GetComponent<Renderer>().material.color = t.GetComponent<IsHit_S>().originalColorVar;
            t.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
            transform.GetComponent<LineRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 0.6f);
        }
    }
}