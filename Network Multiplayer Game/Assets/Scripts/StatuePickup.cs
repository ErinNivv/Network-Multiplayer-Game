using UnityEngine;

public class StatuePickup : MonoBehaviour
{
    private void Start()
    {
        
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void Update()
    {
        if (Keypad.isSafeOpen)
        {
            gameObject.layer = LayerMask.NameToLayer("PickUp");
        }
    }
}