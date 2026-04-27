using UnityEngine;
using Unity.Netcode;

public class StatueSnap : NetworkBehaviour
{
    [SerializeField] private Transform pillarTransform;
    [SerializeField] private int correctRotationIndex = 1;
    [SerializeField] private StatueCount statueCount;

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

    public NetworkVariable<ulong> statueNetworkId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private NetworkObject statue;

    public override void OnNetworkSpawn()
    {
        currentRotationIndex.OnValueChanged += OnRotationIndexChanged;
        statueNetworkId.OnValueChanged += OnStatueIdChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentRotationIndex.OnValueChanged -= OnRotationIndexChanged;
        statueNetworkId.OnValueChanged -= OnStatueIdChanged;
    }

    private void OnStatueIdChanged(ulong previous, ulong current)
    {
        if(current == ulong.MaxValue)
        {
            statue = null;
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(current, out var obj))
            statue = obj;
    }

    public void OnCollisionEnter(Collision other)
    {
        if(!IsServer) return;
        if (!other.gameObject.CompareTag("Statue")) return;
        {
            statue = other.gameObject.GetComponent<NetworkObject>();
            statueNetworkId.Value = statue.NetworkObjectId;
            
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

        bool wasCorrect = isCorrect.Value;

        int nextIndex = (currentRotationIndex.Value +1) % rotationSteps.Length;
        bool nextIsCorrect = (nextIndex == correctRotationIndex);

        statue.transform.rotation = rotationSteps[nextIndex];
        currentRotationIndex.Value = nextIndex;
        isCorrect.Value = nextIsCorrect;

        if( nextIsCorrect != wasCorrect)
        {
            if (statueCount != null)
            {
                if (nextIsCorrect)
                {
                    Debug.Log("Is correctly placed");
                    statueCount.CorrectStatueCount();
                    
                }
                else
                {
                    Debug.Log("Is incorrectly placed");
                    statueCount.IncorrectStatueCount();
                    
                }
            }
        }
    }
    public void OnRotationIndexChanged(int previous, int current)
    {
        if (statue == null) return;

        statue.transform.rotation = rotationSteps[current];
    }
}
