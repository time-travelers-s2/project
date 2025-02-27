using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class knightCamController : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;


    [Header("Other")]
    public float rotationSpeed;
    public bool isDead;
    


    public CameraStyle currentStyle;
    public enum CameraStyle 
    {
        Basic
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isDead = false;
    }

    private void Update() 
    {
        
        {
            
            Vector3 viewDirection = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = viewDirection.normalized;
            if(playerObj.forward != viewDirection)
            {
                playerObj.forward = viewDirection;
            }

            if(currentStyle == CameraStyle.Basic)
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
                if(inputDirection != Vector3.zero && verticalInput == 1)
                {
                    playerObj.forward = Vector3.Slerp(playerObj.forward, inputDirection.normalized, Time.deltaTime * rotationSpeed);
                }
            }
        }
        
    }
}
