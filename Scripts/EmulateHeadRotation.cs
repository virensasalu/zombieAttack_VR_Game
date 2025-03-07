using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//********************************************************************************//
//***********THIS SCRIPT EMULATES HEAD ROTATION SIMILAR TO A VR HEADSET***********//
//********************************************************************************//

//Press Q while the Game View is active, rotate the camera with the left mouse button

public class EmulateHeadRotation : MonoBehaviour
{
    float headSpeedHorizontal = 1.5f;
    float headSpeedVertical = 1.5f;
    float headYaw = 0.0f;
    float headPitch = 0.0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            headYaw += headSpeedHorizontal * Input.GetAxis("Mouse X");
            headPitch += headSpeedVertical * Input.GetAxis("Mouse Y") * -1.0f;

            transform.eulerAngles = new Vector3(headPitch, headYaw, 0.0f);
        }
    }
}

