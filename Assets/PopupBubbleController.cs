using TMPro;
using UnityEngine;

public class PopupBubbleController : MonoBehaviour {
    public TextMeshProUGUI Text; // Reference to the text element.
    private float lifetime = 3f; // Time before the popup disappears.

    public void Setup(string message) {
        Text.text = message;
        Invoke(nameof(DestroyBubble), lifetime); // Schedule destruction.
    }

    private void DestroyBubble() {
        Destroy(gameObject); // Destroy the popup bubble.
    }
}
