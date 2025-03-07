using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//********************************************************************************//
//****************THIS SCRIPT ENABLES CONTROLLER INTERACTION IN VR****************//
//********************************************************************************//

//Use Quest2 controller buttons for interaction

public class Grab : MonoBehaviour
{
    float controllerSpeedHorizontal = 1.5f;
    float controllerSpeedVertical = 1.5f;
    float controllerYaw = 0.0f;
    float controllerPitch = 0.0f;

    private bool isGrabbing = false;             
    private Transform grabbedTransform;          
    public float zSpeed = 4.5f;                  
    private Transform hitTransform;             
    
    void Update()
    {
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

        if (QuestInputs.Right.Trigger.Down) //If we are pulling the trigger of the right controller
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

        //If the trigger is released, we'll revert the changes to release the object
        if (isGrabbing && QuestInputs.Right.Trigger.Up) 
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
            float distance = QuestInputs.Right.ThumbStick.y; //We are storing the right controller's thumbstick value in the y-axis

            grabbedTransform.position += distance * zSpeed * Time.deltaTime * transform.forward;
            grabbedTransform.localPosition = new Vector3(grabbedTransform.localPosition.x, grabbedTransform.localPosition.y,
                                                         Mathf.Clamp(grabbedTransform.localPosition.z, 0.4f, 7.0f));
        }
    }

    void SetHighlight(Transform t, bool highlight) {
        if (highlight) {
            t.GetComponent<Renderer>().material.color = Color.cyan;
            t.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineAll;
            transform.GetComponent<LineRenderer>().material.color = new Color(1.0f, 1.0f, 0.0f, 1.0f); 
        }
        else {
            t.GetComponent<Renderer>().material.color = t.GetComponent<IsHit_S>().originalColorVar;
            t.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
            transform.GetComponent<LineRenderer>().material.color = new Color(1.0f, 1.0f, 0.0f, 0.5f); 
        }
    }
}
