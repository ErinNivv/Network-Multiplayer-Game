using UnityEngine;

public class TorchTrigger : MonoBehaviour
{
    [SerializeField] private float range = 5f;
    private BlacklightNumber currentNumber;

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range))
        {
            Debug.Log("Torch ray hit: " + hit.collider.name);
            BlacklightNumber number = hit.collider.GetComponent<BlacklightNumber>();
            if (number != null && currentNumber != number)
            {
                if (currentNumber != null) currentNumber.Hide();
                currentNumber = number;
                currentNumber.Reveal();
            }
            else if (number == null && currentNumber != null)
            {
                currentNumber.Hide();
                currentNumber = null;
            }
        }
        else if (currentNumber != null)
        {
            currentNumber.Hide();
            currentNumber = null;
        }
    }
}