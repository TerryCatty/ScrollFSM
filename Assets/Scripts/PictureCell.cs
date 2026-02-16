using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PictureCell : MonoBehaviour
{
    public Image photoDisplay;
    public PictureCellInfo Info { get; private set; }

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

}
