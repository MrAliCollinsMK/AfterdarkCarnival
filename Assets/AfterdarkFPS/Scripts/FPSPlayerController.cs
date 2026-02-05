using UnityEngine;

namespace AfterdarkFPS
{
    public class FPSPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float sprintSpeed = 11f;
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float lookSensitivity = 2.2f;

        private CharacterController characterController;
        private Transform cameraTransform;
        private float verticalVelocity;
        private float cameraPitch;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cameraTransform = GetComponentInChildren<Camera>().transform;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void HandleLook()
        {
            var mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
            var desiredSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

            var movement = transform.TransformDirection(input) * desiredSpeed;

            if (characterController.isGrounded)
            {
                verticalVelocity = -1f;
                if (Input.GetButtonDown("Jump"))
                {
                    verticalVelocity = jumpForce;
                }
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            movement.y = verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
        }
    }
}
