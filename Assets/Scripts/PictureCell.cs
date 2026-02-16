using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PictureCell : MonoBehaviour
{
    [SerializeField]
    private Image photoDisplay;

    [field: SerializeField]
    public PictureCellInfo Info { get; private set; }

    [SerializeField]
    private ParticleSystem particles;

    public void ConfigureCell(PictureCellInfo info)
    {
        Info = info;

        photoDisplay.gameObject.SetActive(false);
    }


    public void SetImage(Sprite sprite)
    {
        photoDisplay.sprite = sprite;

        photoDisplay.gameObject.SetActive(true);
    }

    public void OnChoosen()
    {
        particles.Play();
    }

}
