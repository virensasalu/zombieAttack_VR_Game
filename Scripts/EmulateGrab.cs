using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//********************************************************************************//
//******THIS SCRIPT EMULATES CONTROLLER ROTATION SIMILAR TO A VR CONTROLLER*******//
//********************************************************************************//

//Press the 'C' key (C for Controller) while the Game View is active and rotate the Controller with the left mouse button
//Grab objects by pointing and clicking down the left mouse button
//Scroll up and down the mouse wheel to move the grabbed object in z-axis (forward/backward)

public class EmulateGrab : MonoBehaviour
{
    float controllerSpeedHorizontal = 1.5f;
    float controllerSpeedVertical = 1.5f;
    float controllerYaw = 0.0f;
    float controllerPitch = 0.0f;

    private bool isGrabbing = false;             //A control variable that stores if the user is grabbing anything
    private Transform grabbedTransform;          //A variable to hold the grabbed object's transform
    public float zSpeed = 4.5f;                  //A variable to control the speed of movement in z-axis (forward/backward)
    public float rotationSpeedMultiplier = 2.0f; //A variable to increase the rotational speed for the controller
    private Transform hitTransform;              //A variable to hold the hit object's transform
    
    void Update()
    {
        if (Input.GetKey(KeyCode.C)) //When the C key is pressed, we'll rotate the controller with the mouse movements
        {
            controllerYaw += controllerSpeedHorizontal * Input.GetAxis("Mouse X") * rotationSpeedMultiplier;
            controllerPitch += controllerSpeedVertical * Input.GetAxis("Mouse Y") * -rotationSpeedMultiplier;
            transform.localRotation = Quaternion.Euler(controllerPitch, controllerYaw, 0.0f);
        }

        //We are casting a ray from the controller to check for hits with grabbable objects
        RaycastHit hitInfo2;
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out hitInfo2))
        {
            //If we are pointing at a grabbable object, but not grabbing
            if (hitInfo2.transform.tag == "Grabbable" && !isGrabbing)
            {
                if (hitTransform != null)
                    SetHighlight(hitTransform, false); //We dim the previously pointed object

                hitTransform = hitInfo2.transform; //We update the hitTransform as the currently pointed object's transform
                SetHighlight(hitTransform, true); //We highlight the currently pointed object
            }

            else
            {
                if (hitTransform != null && !isGrabbing) //If we are hitting a non-grabbable object
                    SetHighlight(hitTransform, false); //We dim the previously pointed object
            }
        }

        else //If we are not hitting any object
        {
            if (hitTransform != null && !isGrabbing)
            {
                SetHighlight(hitTransform, false); //We dim the previously pointed object
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) //If we are pressing down the left mouse button
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(new Ray(transform.position, transform.forward), out hitInfo))
            {
                if (hitInfo.transform.tag == "Grabbable") //If we are hitting a grabbable object
                {
                    isGrabbing = true; //We turn the control variable to true indicating that we are grabbing

                    grabbedTransform = hitInfo.transform;
                    //We are setting isKinematic as true to control the movement via code
                    grabbedTransform.GetComponent<Rigidbody>().isKinematic = true;
                    //We are setting useGravity as false to control the movement via code
                    grabbedTransform.GetComponent<Rigidbody>().useGravity = false;
                    //We are setting the gameobject to which this script is attached as the parent of the grabbed 
                    //gameobject so that the movement of the gameobject to which this script is attached is reflected
                    //to the child (the grabbed game object is anchored to this gameobject)
                    grabbedTransform.parent = transform; 
                }
            }
        }

        //If the left mouse button is released, we'll revert the changes to release the object
        if (isGrabbing && Input.GetKeyUp(KeyCode.Mouse0)) 
        {
            if (grabbedTransform != null)
            {
                //We are setting isKinematic as false for the object to be controlled via physics
                grabbedTransform.GetComponent<Rigidbody>().isKinematic = false;
                //We are setting useGravity as true for the object to be controlled via physics
                grabbedTransform.GetComponent<Rigidbody>().useGravity = true;
                //We are breaking the hierarchy so that the previously grabbed object has no parent and can act independently
                grabbedTransform.parent = null; 
            }
            isGrabbing = false; //We turn the control variable to false indicating that we are not grabbing anything
        }

        //We'll take care of moving the grabbed object in z-axis here
        if (isGrabbing)
        {
            float distance = Input.mouseScrollDelta.y; //We are storing mouse scroll delta in y-axis

            //We are moving the grabbed object in z-axis forward/backward
            grabbedTransform.position += distance * zSpeed * Time.deltaTime * transform.forward;

            //We want to restrict the movement between 0.4 and 7.0, hence we are clamping the z of the localPosition
            grabbedTransform.localPosition = new Vector3(grabbedTransform.localPosition.x, grabbedTransform.localPosition.y,
                                                         Mathf.Clamp(grabbedTransform.localPosition.z, 0.4f, 7.0f));
        }
    }

    //This method sets/dims highlight through the use of the Outline and IsHit_S scripts
    //(attached to grabbable objects in the scene)
    void SetHighlight(Transform t, bool highlight)
    {
        if (highlight)
        {
            //We are setting the material color of the highlighted object as cyan
            t.GetComponent<Renderer>().material.color = Color.cyan;
            //We are setting the outline mode as OutlineAll
            t.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineAll;
            //We are setting the color of the linerenderer as fully opaque
            transform.GetComponent<LineRenderer>().material.color = new Color(1.0f, 1.0f, 0.0f, 1.0f); 
        }
        else
        {
            //We are reverting the material color of the highlighted object to the original color
            t.GetComponent<Renderer>().material.color = t.GetComponent<IsHit_S>().originalColorVar;
            //We are setting the outline mode as OutlineHidden
            t.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
            //We are setting the material color of the linerenderer as half transparent
            transform.GetComponent<LineRenderer>().material.color = new Color(1.0f, 1.0f, 0.0f, 0.5f); 
        }
    }
}
