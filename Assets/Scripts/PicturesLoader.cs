using System;
using System.Collections.Generic;
using UnityEngine;

public class PicturesLoader : MonoBehaviour
{
    [SerializeField]
    private List<PictureCellInfo> _loadCells;

    [SerializeField] 
    private OptimizedGridScroll scroll;

    private void Start()
    {
        scroll.SetDataList(_loadCells);
    }
}
