using UnityEngine;
using UnityEngine.Events;

public class ButtonTrigger : MonoBehaviour
{
    public UnityEvent onButtonPressed; // Assign in Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            onButtonPressed.Invoke(); // Trigger assigned actions
            // Optional: Change sprite color or animation
        }
    }
}
