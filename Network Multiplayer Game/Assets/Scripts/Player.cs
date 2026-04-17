using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : NetworkBehaviour
{
    [Header("Player Components")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Player Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float verticalVelocity;

    private PlayerInput pi;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private CharacterController cc;

    private float pitch;

    [Header("PickUp")]
    private bool isHolding;
    [SerializeField] private Transform rayPoint;
    [SerializeField] private float grabRange = 2.5f;
    private NetworkObject heldObject;
    [SerializeField] private Transform holdPosition;

    public override void OnNetworkSpawn()
    {
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();
        if (playerCamera) playerCamera.enabled = false;

        if (!IsOwner)
        {
            if (pi) pi.enabled = false;
            enabled = false;
            return;
        }

        if (playerCamera) playerCamera.enabled = true;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    //public override void OnNetworkDespawn()
    //{
    //    if (!IsOwner) return;
    //    Cursor.lockState = CursorLockMode.None;
    //    Cursor.visible = true;
    //}
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>() * lookSensitivity;
    }

    public void OnPickUp(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
           // HandlePickUp();
        }
    }
    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        if(cc.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move.y = verticalVelocity;

        cc.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleLook()
    {
        Quaternion yRotation = Quaternion.Euler(0f, lookInput.x, 0f);
        transform.rotation *= yRotation;

        pitch -= lookInput.y;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
