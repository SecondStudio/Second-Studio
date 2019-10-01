using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// global constraint settings
/// </summary>
public class ConstraintManager : Photon.MonoBehaviour {

    public static bool ConstrainX = false, ConstrainY = false, ConstrainZ = false;
    public static float LengthConstraint = 1;
    public Text[] LengthText;
    public static bool ConstrainLength = false;

    public ColorSwapper[] Xlisteners, Ylisteners, Zlisteners;

    public void ToggleConstrainX()
    {
        photonView.RPC("ToggleX", PhotonTargets.AllBufferedViaServer);
    }

    public void ToggleConstrainY()
    {
        photonView.RPC("ToggleY", PhotonTargets.AllBufferedViaServer);
    }

    public void ToggleConstrainZ()
    {
        photonView.RPC("ToggleZ", PhotonTargets.AllBufferedViaServer);
    }


    [PunRPC]
    void ToggleX()
    {
        ConstrainX = !ConstrainX;
        ToolTracker.net[2] = 2;
        ToolTracker.net[3] = 1;
        if (ConstrainX)
            ToolTracker.value[0] = "2";
        else
            ToolTracker.value[0] = "1";
        ToolTracker.setEmpty(1);
        TrackerScript.AddAction();
        foreach (var s in Xlisteners)
        {
            if (ConstrainX) s.Enable(); else s.Disable();
        }
    }
    [PunRPC]
    void ToggleY()
    {
        ConstrainY = !ConstrainY;
        ToolTracker.net[2] = 2;
        ToolTracker.net[3] = 2;
        if (ConstrainY)
            ToolTracker.value[0] = "2";
        else
            ToolTracker.value[0] = "1";
        ToolTracker.setEmpty(1);
        TrackerScript.AddAction();
        foreach (var s in Ylisteners) {
            if (ConstrainY) s.Enable(); else s.Disable();
        }

    }
    [PunRPC]
    void ToggleZ()
    {
        ConstrainZ = !ConstrainZ;
        ToolTracker.net[2] = 2;
        ToolTracker.net[3] = 3;
        if (ConstrainZ)
            ToolTracker.value[0] = "2";
        else
            ToolTracker.value[0] = "1";
        ToolTracker.setEmpty(1);
        TrackerScript.AddAction();
        foreach (var s in Zlisteners)
        {
            if (ConstrainZ) s.Enable(); else s.Disable();
        }
    }

    public void IncreaseLength(float amount)
    {
        LengthConstraint += amount;
        photonView.RPC("UpdateLengthConstraint", PhotonTargets.AllBufferedViaServer, ConstrainLength, LengthConstraint);

        ToolTracker.net[2] = 5;
        ToolTracker.net[3] = 2;
        ToolTracker.value[0] = "f";
        ToolTracker.value[1] = "" + LengthConstraint;
    }

    public void DecreaseLength(float amount)
    {
        LengthConstraint = LengthConstraint - amount > 0 ? LengthConstraint - amount : 0;
        photonView.RPC("UpdateLengthConstraint", PhotonTargets.AllBufferedViaServer, ConstrainLength, LengthConstraint);

        ToolTracker.net[2] = 5;
        ToolTracker.net[3] = 3;
        ToolTracker.value[0] = "f";
        ToolTracker.value[1] = "" + LengthConstraint;
    }



    [PunRPC]
    void UpdateLengthConstraint(bool active , float amount)
    {
        ConstrainLength = active;
        LengthConstraint = amount;
        foreach (var t in LengthText) t.text = "" + LengthConstraint.ToString("0.00");
    }

    public void ToggleLengthConstraint()
    {
        ConstrainLength = !ConstrainLength;
        photonView.RPC("UpdateLengthConstraint", PhotonTargets.AllBufferedViaServer, ConstrainLength, LengthConstraint);

        ToolTracker.net[2] = 5;
        ToolTracker.net[3] = 1;
        if (ConstrainLength)
        {
            ToolTracker.value[0] = "2";
            ToolTracker.value[1] = "f";
            ToolTracker.value[2] = ""+LengthConstraint;
            ToolTracker.setEmpty(2);
        }
        else
        {
            ToolTracker.value[0] = "1";
            ToolTracker.setEmpty(1);
        }
        
        TrackerScript.AddAction();
    }
    public void ResetConstraints()
    {
        photonView.RPC("Reset", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]

    void Reset()
    {
        ConstrainLength = ConstrainX = ConstrainY = ConstrainZ = false;
    }
}
