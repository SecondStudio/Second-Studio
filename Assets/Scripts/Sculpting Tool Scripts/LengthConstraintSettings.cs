using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LengthConstraintSettings : ToolSettings
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DecreaseLength(float size)
    {
        GetComponentInParent<ConstraintManager>().DecreaseLength(size);
    }

    public void IncreaseLength(float size)
    {
        GetComponentInParent<ConstraintManager>().IncreaseLength(size);
    }

    public void ToggleConstraint()
    {
        GetComponentInParent<ConstraintManager>().ToggleLengthConstraint();
    }
}
