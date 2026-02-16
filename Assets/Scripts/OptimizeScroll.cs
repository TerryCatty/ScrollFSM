using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class OptimizedGridScroll : MonoBehaviour
{
    [Header("Components")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject cellPrefab;

    [Header("Adaptive Settings")]
    public int minColumns = 1;
    public int maxColumns = 10;

    private List<PictureCellInfo> dataList = new List<PictureCellInfo>();
    private List<PictureCell> pool = new List<PictureCell>();
    private List<RectTransform> poolRects = new List<RectTransform>();

    private GridLayoutGroup grid;
    private int columns;
    private float rowHeight;
    private int headRowIndex = 0;

    private float cellWidth;
    private float spacingX;
    private float spacingY;
    private int paddingTop;
    private float paddingLeft;

    private Vector2 lastViewportSize;
    private bool isInitialLoaded = false;

    void Awake()
    {
        Application.targetFrameRate = 120;
        grid = content.GetComponent<GridLayoutGroup>();

        cellWidth = grid.cellSize.x;
        spacingX = grid.spacing.x;
        spacingY = grid.spacing.y;
        rowHeight = grid.cellSize.y + spacingY;
        paddingTop = grid.padding.top;

        SetupContentTransform();
        CalculateGrid();

        grid.enabled = false;
    }

    void Start()
    {
        scrollRect.onValueChanged.AddListener(OnScroll);
        // Для бесконечного повторения нужно разрешить скроллу двигаться за границы
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        lastViewportSize = scrollRect.viewport.rect.size;

        StartCoroutine(DelayedRefresh(0.01f));
    }

    private IEnumerator DelayedRefresh(float delay)
    {
        yield return new WaitForSeconds(delay);
        Canvas.ForceUpdateCanvases();
        if (dataList.Count > 0) RefreshGridLayout();
    }

    private void RefreshGridLayout()
    {
        CalculateGrid();
        InitializePool();
        RefreshContentHeight();
        ResetPoolToTop();
    }

    void Update()
    {
        if (scrollRect.viewport.rect.size != lastViewportSize)
        {
            lastViewportSize = scrollRect.viewport.rect.size;
            OnViewportResized();
        }
    }

    private void OnViewportResized()
    {
        int oldColumns = columns;
        CalculateGrid();
        if (oldColumns != columns && isInitialLoaded) InitializePool();
        RefreshContentHeight();
        ResetPoolToTop();
    }

    public void SetDataList(List<PictureCellInfo> newData)
    {
        dataList.Clear();
        content.anchoredPosition = Vector2.zero;
        headRowIndex = 0;
        UpdateDataList(newData);
        isInitialLoaded = true;
    }

    public void UpdateDataList(List<PictureCellInfo> newData)
    {
        var existingLinks = new HashSet<string>(dataList.Select(x => x.id));
        var filteredNewData = newData.Where(x => !existingLinks.Contains(x.id)).ToList();
        dataList.AddRange(filteredNewData);
        ApplyChangesData();
    }

    private void ApplyChangesData()
    {
        if (pool.Count == 0) InitializePool();
        else ResetPoolToTop();

        RefreshContentHeight();
    }

    private void ResetPoolToTop()
    {
        headRowIndex = Mathf.FloorToInt(content.anchoredPosition.y / rowHeight);
        // Убрали Clamp(0, max), чтобы можно было крутить в минус
        for (int i = 0; i < pool.Count; i++)
        {
            UpdateCell(pool[i], headRowIndex * columns + i);
        }
    }

    void CalculateGrid()
    {
        float viewportWidth = scrollRect.viewport.rect.width;
        float effectiveWidth = viewportWidth - grid.padding.left - grid.padding.right;
        int calcCols = Mathf.FloorToInt((effectiveWidth + spacingX) / (cellWidth + spacingX));
        columns = Mathf.Clamp(calcCols, minColumns, maxColumns);
        float totalGridWidth = (columns * cellWidth) + ((columns - 1) * spacingX);
        paddingLeft = (viewportWidth - totalGridWidth) / 2f;
    }

    void UpdateCell(PictureCell cell, int index)
    {
        if (dataList.Count == 0) return;

        int poolIdx = pool.IndexOf(cell);
        RectTransform rt = poolRects[poolIdx];

        // --- ЛОГИКА ПОВТОРЕНИЯ ---
        int dataIdx = index % dataList.Count;
        if (dataIdx < 0) dataIdx += dataList.Count;

        cell.gameObject.SetActive(true);
        cell.ConfigureCell(dataList[dataIdx]);

        // Используем FloorToInt для корректного ряда при отрицательном индексе
        int row = Mathf.FloorToInt((float)index / columns);
        int col = index % columns;
        if (col < 0) col += columns;

        float x = paddingLeft + col * (cellWidth + spacingX);
        float y = -(paddingTop + row * rowHeight);

        rt.anchoredPosition = new Vector2(x, y);
        cell.SetImage(dataList[dataIdx].sprite);
    }

    void InitializePool()
    {
        foreach (var c in pool) if (c != null) Destroy(c.gameObject);
        pool.Clear();
        poolRects.Clear();

        int rowsToVisible = Mathf.CeilToInt(scrollRect.viewport.rect.height / rowHeight);
        int totalToSpawn = (rowsToVisible + 2) * columns;

        for (int i = 0; i < totalToSpawn; i++)
        {
            var cell = Instantiate(cellPrefab, content).GetComponent<PictureCell>();
            var rt = cell.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            pool.Add(cell);
            poolRects.Add(rt);
        }
    }

    void OnScroll(Vector2 pos)
    {
        if (dataList.Count == 0) return;

        // --- ТЕЛЕПОРТАЦИЯ ДЛЯ БЕСКОНЕЧНОСТИ ---
        float totalDataHeight = Mathf.CeilToInt((float)dataList.Count / columns) * rowHeight;
        if (content.anchoredPosition.y > totalDataHeight)
            content.anchoredPosition -= new Vector2(0, totalDataHeight);
        else if (content.anchoredPosition.y < 0)
            content.anchoredPosition += new Vector2(0, totalDataHeight);

        int currentHeadRow = Mathf.FloorToInt(content.anchoredPosition.y / rowHeight);
        // Убрали Clamp, чтобы разрешить расчет любых рядов
        if (currentHeadRow != headRowIndex) UpdatePool(currentHeadRow);
    }

    private void UpdatePool(int newHeadRow)
    {
        bool scrollingDown = newHeadRow > headRowIndex;
        while (newHeadRow != headRowIndex)
        {
            if (scrollingDown)
            {
                for (int i = 0; i < columns; i++)
                {
                    var cell = pool[0]; pool.RemoveAt(0); pool.Add(cell);
                    var rect = poolRects[0]; poolRects.RemoveAt(0); poolRects.Add(rect);
                    UpdateCell(cell, (headRowIndex + (pool.Count / columns)) * columns + i);
                }
                headRowIndex++;
            }
            else
            {
                headRowIndex--;
                for (int i = columns - 1; i >= 0; i--)
                {
                    var cell = pool[pool.Count - 1]; pool.RemoveAt(pool.Count - 1); pool.Insert(0, cell);
                    var rect = poolRects[poolRects.Count - 1]; poolRects.RemoveAt(poolRects.Count - 1); poolRects.Insert(0, rect);
                    UpdateCell(cell, headRowIndex * columns + i);
                }
            }
        }
    }

    void SetupContentTransform()
    {
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
    }

    void RefreshContentHeight()
    {
        // Высота теперь просто равна одному циклу элементов
        int totalRows = Mathf.CeilToInt((float)dataList.Count / columns);
        float totalHeight = (totalRows * rowHeight) - spacingY + paddingTop + grid.padding.bottom;
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
    }
}