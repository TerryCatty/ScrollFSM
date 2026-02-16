using Unity.VisualScripting;
using UnityEngine;

public class AutoScroll : MonoBehaviour
{
    [SerializeField]
    private RectTransform scrollTransform;

    [SerializeField]
    private float maxSpeedScroll;

    [SerializeField]
    private float speedChangeSpeedScroll;

    private float speedScroll;
    public bool isOn { private set; get; }
    private bool isOnLast;

    private SmoothScrollSnapping smoothScrollSnapping;

    private void Start()
    {
        smoothScrollSnapping = GetComponent<SmoothScrollSnapping>();
    }

    private void Update()
    {
        if (isOn)
        {
            speedScroll = Mathf.Lerp(speedScroll, maxSpeedScroll, speedChangeSpeedScroll * Time.deltaTime);
        }
        else
        {
            speedScroll = Mathf.Lerp(speedScroll, 0, speedChangeSpeedScroll * Time.deltaTime);
        }

        if(speedScroll <= 0.1 && isOnLast != isOn)
        {
            smoothScrollSnapping.SnapToNearest();
            isOnLast = isOn;
        }

        Vector3 positionScroll = scrollTransform.position;
        positionScroll.y += Time.deltaTime * speedScroll;

        scrollTransform.position = positionScroll;
    }

    public void ChangeScroll()
    {
        isOnLast = isOn;
        isOn = !isOn;

        if (isOn)
        {
            smoothScrollSnapping.OnBeginDrag();
        }
    }
}
