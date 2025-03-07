using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//************************************************************************************************//
//************************THIS SCRIPT EMULATES STEERING LOCOMOTION IN VR**************************//
//************************************************************************************************//

//Use the W key to continuously move forward in the direction you look at

public class SimplePlayerMovement : MonoBehaviour
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

        controller.Move(transform.forward * Mathf.Clamp01(Input.GetAxis("Vertical")) * playerSpeed * Time.deltaTime);

        if (Input.GetAxis("Vertical") > 0 && !footstepSource.isPlaying) footstepSource.Play();
        if (Input.GetAxis("Vertical") == 0 && footstepSource.isPlaying) footstepSource.Stop();


        // Changes the height position of the player.
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}