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
    private NetworkRigidbody heldObject;
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
            HandlePickUp();
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

        Vector3 move = transform.forward * moveInput.x + transform.right * moveInput.y;
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

    private void HandlePickUp()
    {
        Debug.Log("Pick up input works");

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        int layerMask = LayerMask.GetMask("PickUp");
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray, out hit, grabRange, layerMask);
        Debug.DrawRay(ray.origin, ray.direction * grabRange, Color.red, 3f);
        if (didHit)
        {
            if(hit.collider != null)
            {
                NetworkRigidbody networkRb = hit.collider.GetComponent<NetworkRigidbody>();
                if (networkRb != null)
                {
                    isHolding = true;
                    heldObject = networkRb;
                    networkRb.SetIsKinematic(true);
                    networkRb.transform.position = holdPosition.transform.position;
                    networkRb.transform.rotation = holdPosition.transform.rotation;
                    networkRb.transform.parent = holdPosition;
                }
            }
            
        }
    }

    private void HandleDrop()
    {
        Debug.Log("dropping input works");
    }
}
