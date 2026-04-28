using UnityEngine;

public class CommsUI : MonoBehaviour
{
    [SerializeField] private PlayerComms playerComms;

    private void Start()
    {
        // find the local player's PlayerComms
        foreach (PlayerComms pc in FindObjectsOfType<PlayerComms>())
        {
            if (pc.IsOwner)
            {
                playerComms = pc;
                break;
            }
        }
        Debug.Log("PlayerComms found: " + playerComms);
    }
    public void PressYes() 
    { 
        playerComms.SendComms(0); 
    }
    public void PressNo() 
    { 
        playerComms.SendComms(1); 
    }
    public void PressLeftArrow() 
    { 
        playerComms.SendComms(2); 
    }
    public void PressRightArrow() 
    { 
        playerComms.SendComms(3); 
    }
    public void PressExclamation() 
    { 
        playerComms.SendComms(4); 
    }
    public void PressLiftHand() 
    { 
        playerComms.SendComms(5); 
    }
}