using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    public RectTransform rectTransform; // Assign your RectTransform in the inspector
    public Vector2 scrollSpeed = new Vector2(10f, 0f); // Scrolling speed

    private Vector2 startPosition;

    void Start()
    {
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Move the RectTransform
        Vector2 newPosition = rectTransform.anchoredPosition + scrollSpeed * Time.deltaTime;

        // Reset the position if it exceeds a threshold to create an infinite effect
        if (newPosition.x >= rectTransform.sizeDelta.x || newPosition.x <= -rectTransform.sizeDelta.x)
        {
            newPosition.x = startPosition.x;
        }
        if (newPosition.y >= rectTransform.sizeDelta.y || newPosition.y <= -rectTransform.sizeDelta.y)
        {
            newPosition.y = startPosition.y;
        }

        rectTransform.anchoredPosition = newPosition;
    }
}
