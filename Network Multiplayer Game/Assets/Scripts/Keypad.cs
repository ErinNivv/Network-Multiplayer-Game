using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


public class Keypad : MonoBehaviour
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
        if(Ans.text == Answer)
        {
            Ans.text = "CORRECT";
            safeDoor.SetActive(false);
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

    public void Close()
    {
        keyPad.SetActive(false);
    }
}
