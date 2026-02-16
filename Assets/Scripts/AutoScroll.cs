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
    private bool isOn;

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

        if(speedScroll <= 0.1)
        {
            GetComponent<SmoothScrollSnapping>().SnapToNearest();
        }

        Vector3 positionScroll = scrollTransform.position;
        positionScroll.y += Time.deltaTime * speedScroll;

        scrollTransform.position = positionScroll;
    }

    public void ChangeScroll()
    {
        isOn = !isOn;

        if (isOn)
        {
            GetComponent<SmoothScrollSnapping>().OnBeginDrag();
        }
    }
}
