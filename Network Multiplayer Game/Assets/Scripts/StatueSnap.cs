using UnityEngine;
using Unity.Netcode;

public class StatueSnap : NetworkBehaviour
{
    [SerializeField] private Transform pillarTransform;
    [SerializeField] private int correctRotationIndex = 1;

    private readonly Quaternion[] rotationSteps = new Quaternion[]
    {
        Quaternion.Euler(0f, 0f, 0f),
        Quaternion.Euler(0f, 90f, 0f),
        Quaternion.Euler(0f, 180f, 0f),
        Quaternion.Euler(0f, 270f, 0f),
    };

    private NetworkVariable<int> currentRotationIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    
    public NetworkVariable<bool> isPlaced = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> isCorrect = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    private NetworkObject statue;

    public override void OnNetworkSpawn()
    {
        currentRotationIndex.OnValueChanged += OnRotationIndexChanged;
    }
    public void OnCollisionEnter(Collision other)
    {
        if(!IsServer) return;
        if (other.gameObject.CompareTag("Statue"))
        {
            statue = other.gameObject.GetComponent<NetworkObject>();
            
            statue.transform.position = pillarTransform.position;
            statue.transform.rotation = rotationSteps[0];

            isPlaced.Value = true;
            currentRotationIndex.Value = 0;
            isCorrect.Value = (0 == correctRotationIndex);
            Debug.Log("statue placed");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StatueRotateServerRpc()
    {
        if (!isPlaced.Value) return;

        int nextIndex = (currentRotationIndex.Value +1) % rotationSteps.Length;
        statue.transform.rotation = rotationSteps[nextIndex];

        currentRotationIndex.Value = nextIndex;
        isCorrect.Value = (nextIndex == correctRotationIndex);

        if( isCorrect.Value )
        {
            Debug.Log("Is correctly placed");
        }
    }
    public void OnRotationIndexChanged(int previous, int current)
    {
        if (statue == null) return;

        statue.transform.rotation = rotationSteps[current];
    }
}
