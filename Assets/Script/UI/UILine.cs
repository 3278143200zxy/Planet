using UnityEngine;
using UnityEngine.UI;

public class UILine : MonoBehaviour
{
    public Vector2 startPoint;
    public Vector2 endPoint;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    /*
    public void SetLine(Vector3 start, Vector3 end)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        startPoint = start;
        endPoint = end;

        rectTransform.position = (startPoint + endPoint) / 2f;

        float distance = Vector2.Distance(startPoint, endPoint);
        rectTransform.sizeDelta = new Vector2(distance * 2, rectTransform.sizeDelta.y);

        Vector2 direction = endPoint - startPoint;
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    */
    public void SetLine(Vector3 start, Vector3 end)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        rectTransform.position = (start + end) / 2f;

        Vector3 direction = end - start;
        float worldDistance = direction.magnitude;

        rectTransform.right = direction;

        float localWidth = worldDistance / rectTransform.lossyScale.x;
        rectTransform.sizeDelta = new Vector2(localWidth, rectTransform.sizeDelta.y);
    }
}