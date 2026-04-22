using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private GameObject heldObject;
    [SerializeField] private Transform holdPosition;
    Transform objectHolder;

    public override void OnNetworkSpawn()
    {
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();

        //Debug.Log($"[Player] OwnerClientId: {OwnerClientId} | LocalClientId: {NetworkManager.Singleton.LocalClientId} | IsOwner: {IsOwner} | IsServer: {IsServer}");
        //Debug.Log($"[Player] CC: {cc} | CC enabled: {cc?.enabled} | PI: {pi} | PI enabled: {pi?.enabled}");
        //Debug.Log($"[Player] Camera: {playerCamera} | CameraPivot: {cameraPivot} | HoldPosition: {holdPosition} | RayPoint: {rayPoint}");

        if (!IsOwner)
        {
            //Debug.Log("[Player] Not owner, destroying CC and disabling PI");
            if (pi) pi.enabled = false;
            Destroy(cc);
            playerCamera.enabled = false;
            AudioListener al = GetComponentInChildren<AudioListener>();
            if (al) al.enabled = false;
            enabled = false;
            return;
        }

        cc.enabled = true;
        playerCamera.enabled = true;

        //Debug.Log($"[Player] Owner setup complete | Camera enabled: {playerCamera.enabled} | CC enabled: {cc.enabled}");
        //objectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;
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
        if (!isHolding && context.performed)
        {
            HandlePickUp();
        }
        else if (isHolding && context.performed)
        {
            HandleDrop();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (isHolding && context.performed)
        {
            return;
        }
        else if (!isHolding && context.performed)
        {
            HandleInteract();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        //Debug.Log($"[Player] Update running | moveInput: {moveInput} | lookInput: {lookInput} | isGrounded: {cc.isGrounded}");
    }

    private void HandleMovement()
    {
        if (cc.isGrounded)
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

    private void HandlePickUp()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        int layerMask = LayerMask.GetMask("PickUp");

        if (Physics.Raycast(ray, out RaycastHit hit, grabRange, layerMask))
        {
            if (!isHolding)
            {
                NetworkObject netObj = hit.transform.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    ObjectInHandServerRpc(netObj.NetworkObjectId);
                    isHolding = true;
                    heldObject = hit.transform.gameObject;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ObjectInHandServerRpc(ulong networkObjectId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            netObj.ChangeOwnership(OwnerClientId);

            ObjectInHandClientRpc(networkObjectId, new NetworkObjectReference(gameObject.GetComponent<NetworkObject>()));
        }
    }

    [ClientRpc]
    void ObjectInHandClientRpc(ulong networkObjectId, NetworkObjectReference playerRef)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            playerRef.TryGet(out NetworkObject playerNetObj);

            netObj.transform.position = holdPosition.position;
            netObj.transform.rotation = holdPosition.rotation;
            netObj.TrySetParent(playerNetObj.transform, true);

            if (netObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }
        }
    }

    private void HandleDrop()
    {
        if (!isHolding)
            return;

        DropObjectServerRpc(heldObject.GetComponent<NetworkObject>().NetworkObjectId);
        isHolding = false;
        heldObject = null;
    }

    [ServerRpc(RequireOwnership = false)]
    void DropObjectServerRpc(ulong networkObjectId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            netObj.ChangeOwnership(0);
            DropObjectClientRpc(networkObjectId);
        }
    }

    [ClientRpc]
    void DropObjectClientRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            netObj.TryRemoveParent();

            if (netObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
                rb.isKinematic = false;
        }
    }

    private void HandleInteract()
    {
        Debug.Log("Interact is working");
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        int layerMask = LayerMask.GetMask("RotateButton");
        if (Physics.Raycast(ray, out RaycastHit hit, grabRange, layerMask))
        {
            
        }
    }
}
