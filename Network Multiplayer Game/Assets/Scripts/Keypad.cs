using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class Keypad : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI Ans;
    public string Answer = "4863";
    public GameObject safeDoor;
    public GameObject keyPad;

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
        }
        else
        {
            StartCoroutine(ClearText());
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