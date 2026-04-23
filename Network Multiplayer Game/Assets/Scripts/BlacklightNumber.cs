using UnityEngine;

public class BlacklightNumber : MonoBehaviour
{
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material blacklightMaterial;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = normalMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TorchLight"))
        {
            rend.material = blacklightMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TorchLight"))
        {
            rend.material = normalMaterial;
        }
    }
}