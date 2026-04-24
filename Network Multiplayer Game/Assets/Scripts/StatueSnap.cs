using UnityEngine;
using Unity.Netcode;

public class StatueSnap : NetworkBehaviour
{
    [SerializeField] private Transform pillarTransform;
    //[SerializeField] private NetworkObject statue;

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Statue"))
        {
            //statue = other.gameObject.GetComponent<NetworkObject>();
            other.transform.position = pillarTransform.position;
            other.transform.rotation = pillarTransform.rotation;
            other.transform.parent = pillarTransform;
        }
    }
}
