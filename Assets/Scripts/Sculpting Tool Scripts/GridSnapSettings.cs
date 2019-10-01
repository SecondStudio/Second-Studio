using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSnapSettings : ToolSettings
{
    public Text sizetext;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DecreaseGrid()
    {
        GetComponentInParent<GridSnapTool>().DecreaseGrid();
        GetComponentInParent<GridSnapTool>().SizeIndicatorText = sizetext;
    }

    public void IncreaseGrid()
    {
        GetComponentInParent<GridSnapTool>().IncreaseGrid();
        GetComponentInParent<GridSnapTool>().SizeIndicatorText = sizetext;
    }

    public void ToggleGrid()
    {
        GetComponentInParent<GridSnapTool>().ToggleGridSnap();
    }

    public void NextElement()
    {
        FindObjectOfType<GridSnapTool>().NextElementType();
    }

    public void PrevElement()
    {
        FindObjectOfType<GridSnapTool>().PrevElementType();
    }

    public void ToggleElement()
    {
        FindObjectOfType<GridSnapTool>().ToggleElementSnap();
    }
}
