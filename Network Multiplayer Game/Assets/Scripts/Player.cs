using Unity.Netcode;
using Unity.Netcode.Components;
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
    [SerializeField] private float grabRange = 3.5f;
    private GameObject heldObject;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform torchHoldPosition;
    [SerializeField] private float followSpeed = 30f;
    [SerializeField] private float maxHoldDistance = 0.5f;
    Transform objectHolder;
    private Transform activeHoldPosition;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("UI Prompts")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private GameObject pickupPrompt;

    public override void OnNetworkSpawn()
    {
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();

        Debug.Log($"[Player] OwnerClientId: {OwnerClientId} | LocalClientId: {NetworkManager.Singleton.LocalClientId} | IsOwner: {IsOwner} | IsServer: {IsServer}");
        Debug.Log($"[Player] CC: {cc} | CC enabled: {cc?.enabled} | PI: {pi} | PI enabled: {pi?.enabled}");
        Debug.Log($"[Player] Camera: {playerCamera} | CameraPivot: {cameraPivot} | HoldPosition: {holdPosition} | RayPoint: {rayPoint}");

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

        cc.enabled = false;
        playerCamera.enabled = true;
        StartCoroutine(EnableCCAfterDelay());

        Debug.Log($"[Player] Owner setup complete | Camera enabled: {playerCamera.enabled} | CC enabled: {cc.enabled}");
        //objectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;

        if(pi.playerIndex == 0)
        {
            animator.SetBool("IsPlayer1", true);
        }
        if (pi.playerIndex == 1)
        {
            animator.SetBool("IsPlayer2", true);
        }
        if (pi.playerIndex == 2)
        {
            animator.SetBool("IsPlayer3", true);
        }
        else if (pi.playerIndex == 3)
        {
            animator.SetBool("IsPlayer4", true);
        }
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

        animator.SetBool("IsWalking", true);

        if (context.canceled)
        {
            animator.SetBool("IsWalking", false);
        }
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
            HandleAllInteractions();
            Debug.Log("Oninteract is working");
        }
    }

    private System.Collections.IEnumerator EnableCCAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        cc.enabled = true;
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        HandlePrompts();
    }

    private void HandlePrompts()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        
        int pickupMask = LayerMask.GetMask("PickUp");
        if (Physics.Raycast(ray, out RaycastHit pickupHit, grabRange, pickupMask))
        {
            pickupPrompt.SetActive(true);
            interactPrompt.SetActive(false);
            return;
        }

       
        if (Physics.Raycast(ray, out RaycastHit interactHit, 3f))
        {
            bool isInteractable = interactHit.collider.CompareTag("Safe") ||
                                  interactHit.collider.CompareTag("LightSwitch");
            interactPrompt.SetActive(isInteractable);
            pickupPrompt.SetActive(false);
            return;
        }

        
        interactPrompt.SetActive(false);
        pickupPrompt.SetActive(false);
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (!isHolding || heldObject == null || activeHoldPosition == null) return;
        {
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb == null || rb.isKinematic) return;

            Vector3 targetPos = activeHoldPosition.position;
            float distance = Vector3.Distance(rb.position, targetPos);

            if (distance > maxHoldDistance)
            {
                rb.linearVelocity = Vector3.zero;
                rb.position = targetPos;
            }
            else
            {
                Vector3 direction = targetPos - rb.position;
                rb.linearVelocity = direction * followSpeed;
            }

            rb.MoveRotation(activeHoldPosition.rotation);
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void HandleMovement()
    {
        if (!cc.enabled) return;
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

                    Debug.Log("Pick Up working");
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

            // check if its a torch
            bool isTorch = netObj.GetComponent<Torch>() != null;
            Debug.Log("Is torch: " + isTorch + " | Object: " + netObj.name);
            activeHoldPosition = isTorch ? torchHoldPosition : holdPosition;
            Transform targetHoldPosition = isTorch ? torchHoldPosition : holdPosition;

            netObj.transform.position = targetHoldPosition.position;
            netObj.transform.rotation = targetHoldPosition.rotation;
            netObj.TrySetParent(playerNetObj.transform, true);

            NetworkTransform nt = netObj.GetComponent<NetworkTransform>();
            if (nt != null) nt.enabled = false;

            if (netObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
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
            activeHoldPosition = null;

            NetworkTransform nt = netObj.GetComponent<NetworkTransform>();
            if (nt != null) nt.enabled = true;

            if (netObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }
                
        }
    }

    private void HandleAllInteractions()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            Debug.Log("Hit: " + hit.collider.name + " | Tag: " + hit.collider.tag);

            if (hit.collider.CompareTag("Safe"))
            {
                Keypad keypad = hit.collider.GetComponentInParent<Keypad>();
                if (keypad == null) keypad = hit.collider.GetComponent<Keypad>();
                if (keypad != null) keypad.Open();
            }
            else if (hit.collider.CompareTag("LightSwitch"))
            {
                LightSwitch lightSwitch = hit.collider.GetComponent<LightSwitch>();
                if (lightSwitch != null) lightSwitch.Toggle();
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("RotateButton"))
            {
                Debug.Log("Rotate button hit");
                PhysicalRotateButton button = hit.collider.GetComponentInParent<PhysicalRotateButton>();
                if (button != null) button.Interact();
            }
        }
    }

}
