using UnityEngine;

public class AutoScrollControl : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Button buttonRoll;

    [SerializeField]
    private UnityEngine.UI.Button buttonStop;

    [SerializeField]
    private AutoScroll autoScroll;

    [SerializeField]
    private float timeToCanStop;

    private float timer;

    public void Start()
    {
        buttonRoll.onClick.AddListener(() => StartRoll());
        buttonStop.onClick.AddListener(() => StopRoll());


        buttonStop.interactable = false;
    }

    private void Update()
    {
        if(autoScroll.isOn == true && timer >= 0)
        {
            timer -= Time.deltaTime;
        }

        if(autoScroll.isOn == true && timer <= 0)
        {
            buttonStop.interactable = true;
        }
    }

    public void StartRoll()
    {
        if(autoScroll.isOn == false)
        {
            autoScroll.ChangeScroll();

            buttonRoll.interactable = false;
            buttonStop.interactable = false;

            timer = timeToCanStop;
        }
    }

    public void StopRoll()
    {
        if (autoScroll.isOn == true)
        {
            autoScroll.ChangeScroll();
            buttonStop.interactable = false;
            buttonRoll.interactable = true;
        }
    }
}
