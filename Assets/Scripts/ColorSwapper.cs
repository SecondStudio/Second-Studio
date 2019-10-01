using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ColorSwapper : MonoBehaviour {

    Graphic text;
    public Color EnabledColor, DisabledColor;
    bool enabled = false;
	void Start () {
        text = GetComponent<Graphic>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Toggle()
    {
        enabled = !enabled;
        text.color = enabled ? EnabledColor : DisabledColor;
    }

    public void Reset()
    {
        enabled = false;
        if(text)
            text.color = DisabledColor;
    }

    public void Enable()
    {
        enabled = true;
        if (text == null) text = GetComponent<Graphic>();
        text.color = enabled ? EnabledColor : DisabledColor;
    }

    public void Disable()
    {
        enabled = false;
        if(text == null) text = GetComponent<Graphic>();
        text.color = enabled ? EnabledColor : DisabledColor;
    }
}
