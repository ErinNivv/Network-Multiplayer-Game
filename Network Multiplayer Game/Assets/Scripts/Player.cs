using Unity.Netcode;
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

    private PlayerInput pi;
    private InputAction moveAction;
    private InputAction lookAction;
    private CharacterController cc;

    private float pitch;

    public override void OnNetworkSpawn()
    {
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();

        if (!IsOwner)
        {
            if (playerCamera) playerCamera.enabled = false;
            if (pi) pi.enabled = false;
            enabled = true;
            return;
        }

        InvokeRepeating(nameof(SetupInput), 0.1f, 0.1f);
    }

    private void SetupInput()
    {
        if (pi == null || moveAction != null)
        {
            CancelInvoke(nameof(SetupInput));
            return;
        }

        moveAction = pi.actions["Move"];
        lookAction = pi.actions["Look"];

        if (moveAction == null || lookAction == null)
        {
            return;
        }

        moveAction.Enable();
        lookAction.Enable();
        if (playerCamera) playerCamera.enabled = true;

        CancelInvoke(nameof(SetupInput));
    }

    private void Update()
    {
        Vector2 m = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * m.x + transform.forward * m.y;
        cc.Move(move * moveSpeed * Time.deltaTime);

        Vector2 look = lookAction.ReadValue<Vector2>();
        transform.Rotate(0f, look.x, 0f);

        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
