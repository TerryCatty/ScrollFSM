using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SmoothScrollSnapping : MonoBehaviour
{
    [Header("Links")]
    public ScrollRect scrollRect;
    public RectTransform heightSource;
    [Tooltip("Объект, который служит 'прицелом' центра")]
    public RectTransform centerAnchor;

    [Header("Settings")]
    public float smoothTime = 0.2f;

    private bool isSnapping;
    private Vector2 targetPosition;
    private Vector2 currentVelocity;

    public void OnBeginDrag()
    {
        isSnapping = false;
    }

    public void SnapToNearest()
    {
        // Получаем cellSize.y + spacing.y
        float stepHeight = GetCellHeight();

        // Получаем чистую высоту ячейки (без отступа)
        float pureCellHeight = 0;
        var grid = scrollRect.content.GetComponent<GridLayoutGroup>();
        if (grid != null) pureCellHeight = grid.cellSize.y;
        else pureCellHeight = heightSource != null ? heightSource.rect.height : 0;

        if (stepHeight <= 0 || scrollRect == null || centerAnchor == null) return;

        RectTransform content = scrollRect.content;

        Vector3 anchorInContentSpace = content.InverseTransformPoint(centerAnchor.position);

        int nearestIndex = Mathf.RoundToInt((-anchorInContentSpace.y - (pureCellHeight / 2f)) / stepHeight);

        Vector3 anchorInViewportSpace = scrollRect.viewport.InverseTransformPoint(centerAnchor.position);
        float yOffset = anchorInViewportSpace.y;

        float targetY = (nearestIndex * stepHeight) + yOffset + (pureCellHeight / 2f);

        targetPosition = new Vector2(content.anchoredPosition.x, targetY);

        currentVelocity = Vector2.zero;
        isSnapping = true;
    }

    void Update()
    {
        if (isSnapping)
        {
            scrollRect.content.anchoredPosition = Vector2.SmoothDamp(
                scrollRect.content.anchoredPosition,
                targetPosition,
                ref currentVelocity,
                smoothTime
            );

            if (Vector2.Distance(scrollRect.content.anchoredPosition, targetPosition) < 0.1f)
            {
                scrollRect.content.anchoredPosition = targetPosition;
                isSnapping = false;
            }
        }
    }

    private float GetCellHeight()
    {
        var grid = scrollRect.content.GetComponent<GridLayoutGroup>();
        if (grid != null)
            return grid.cellSize.y + grid.spacing.y;

        return heightSource != null ? heightSource.rect.height : 0;
    }
}