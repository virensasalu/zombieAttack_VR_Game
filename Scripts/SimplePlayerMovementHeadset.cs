using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//************************************************************************************************//
//************************THIS SCRIPT ENABLES STEERING LOCOMOTION IN VR***************************//
//************************************************************************************************//

//Use VR controller buttons for movement

public class SimplePlayerMovementHeadset : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 2.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;
    AudioSource footstepSource;

    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        footstepSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Get movement inputs from the left controller's thumbstick.
        var z = QuestInputs.Left.ThumbStick.y;

        //Move the player based on the controller input and camera direction.
        controller.Move((Camera.main.transform.forward - (Vector3.Dot(Camera.main.transform.forward, Vector3.up) 
                                    * Vector3.up)).normalized * Mathf.Clamp01(z) * playerSpeed * Time.deltaTime);

        if (z > 0 && !footstepSource.isPlaying) footstepSource.Play();
        if ((Mathf.Abs(z) <= 0.0001f) && footstepSource.isPlaying) footstepSource.Stop();

        // Makes the player jump when they pull the left controller's trigger.
        var jump = QuestInputs.Left.Trigger.Down;
        if (jump && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}