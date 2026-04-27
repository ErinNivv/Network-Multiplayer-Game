using UnityEngine;

public class BlacklightNumber : MonoBehaviour
{
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material glowMaterial;

    private MeshRenderer rend;

    private void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        Debug.Log("Awake! Renderer: " + rend);
    }

    private void Start()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        Debug.Log("Total renderers found: " + allRenderers.Length);
        foreach (Renderer r in allRenderers)
        {
            Debug.Log("Renderer: " + r.gameObject.name);
        }
        rend = GetComponent<MeshRenderer>();
    }

    public void Reveal()
    {
        rend.material = glowMaterial;
    }

    public void Hide()
    {
        rend.material = normalMaterial;
    }
}