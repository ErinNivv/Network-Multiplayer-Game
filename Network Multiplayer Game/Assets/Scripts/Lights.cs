using UnityEngine;
using Unity.Netcode;

public class LightSwitch : NetworkBehaviour
{
    [SerializeField] private Light[] lights;

    public bool isOn = true;

    public void Toggle()
    {
        ToggleServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleServerRpc()
    {
        ToggleClientRpc();
    }

    [ClientRpc]
    private void ToggleClientRpc()
    {
        isOn = !isOn;
        foreach (Light light in lights)
        {
            light.enabled = isOn;
        }
    }
}