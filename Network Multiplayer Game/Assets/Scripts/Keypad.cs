using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class Keypad : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI Ans;
    [SerializeField] private NetworkObject doorToDespawn;
    public string Answer = "4863";
    public GameObject safeDoor;
    public GameObject keyPad;

    public static bool isSafeOpen = false;
    public void Number(int number)
    {
        Ans.text += number.ToString();
    }

    public void Execute()
    {
        if (Ans.text == Answer)
        {
            Ans.text = "CORRECT";
            safeDoor.SetActive(false);
            keyPad.SetActive(false);
            isSafeOpen = true; // safe is now open
            DespawnDoorServerRpc();
        }
        else
        {
            StartCoroutine(ClearText());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnDoorServerRpc()
    {
        if (doorToDespawn != null)
        {
            doorToDespawn.Despawn(false);
        }
    }
    private IEnumerator ClearText()
    {
        Ans.text = "INVALID";
        yield return new WaitForSeconds(2f);
        Ans.text = "";
    }

    public void Open()
    {
        Debug.Log("Open called! keyPad is: " + keyPad);
        keyPad.SetActive(true);
    }

    public void Close()
    {
        keyPad.SetActive(false);
    }
}