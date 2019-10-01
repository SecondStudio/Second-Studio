using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// can find a distance of a line,
/// distance of an angle that looks from where the controller was originally to where it has been rotated to
/// (it finds rotations at the base right now, and should be moved to the top instead - this happens because origin)
/// and a curve distance, which is just a bunch of mini lines added together and adding up the length of each of these tiny lines
/// </summary>
public class MeasuringTool : ToolBase
{
    public Text valueText;
    public GameObject valueCard;
    public float distanceThreshold = 0.01f;

    Vector3 startPoint;
    Vector3 startVector;
    Quaternion startRot;
    Vector3 endPoint;
    Quaternion endRot;
    float endDistance = 0;
    bool isPermanent = false;
    float angle;
    List<Vector3> curveList;
    Vector3 prevPoint;
    float curveDist;
    enum MeasureMode {line, angle, curve};
    MeasureMode mode;


    public LineRenderer lineR;

    GameObject go;


    protected override void Start()
    {
        base.Start();
        valueText.text = "";
        mode = MeasureMode.line;
        trackerLetter = "Z";
    }

    protected override void Update()
    {
        base.Update();
        if (controller.triggerButtonDown)
        {
            startPoint = controller.transform.position;
            startVector =  -1 * controller.transform.forward;
            startRot = controller.transform.rotation;
            curveList = new List<Vector3>();
            endDistance = 0;
            curveDist = 0;
            prevPoint = startPoint;
        }

        if (controller.triggerButtonPressed)
        {
            endPoint = controller.transform.position;
            endRot = controller.transform.rotation;
            endDistance = Vector3.Distance(startPoint, endPoint);

            if(mode == MeasureMode.curve)
            {
                float shortDistance = Vector3.Distance(prevPoint, endPoint);
                if (shortDistance > distanceThreshold)
                {
                    curveList.Add(endPoint);
                    curveDist += shortDistance;
                    prevPoint = endPoint;
                }
                ShowCurveValue();
            }

            if(mode == MeasureMode.line)
            {
                ShowDistanceValue();
            }

            if(mode == MeasureMode.angle)
            {
                ShowAngleValue();
            }
        }

        if (controller.triggerButtonUp)
        {
            if (isPermanent)
            {

                if (mode == MeasureMode.curve)
                {
                    curveList.Clear();
                }

                if (mode == MeasureMode.line)
                {
                    GameObject card = Instantiate(valueCard, (startPoint + endPoint) / 2, endRot);
                    card.GetComponentInChildren<Text>().text = "" + endDistance;
                    card.GetComponentInChildren<LineRenderer>().positionCount = 2;
                    card.GetComponentInChildren<LineRenderer>().SetPosition(0, startPoint);
                    card.GetComponentInChildren<LineRenderer>().SetPosition(1, endPoint);
                }

                if (mode == MeasureMode.angle)
                {
                    angle = Vector3.Angle(startVector, -controller.transform.forward);
                    GameObject card = Instantiate(valueCard, (startPoint + endPoint) / 2, endRot);
                    card.GetComponentInChildren<Text>().text = angle.ToString("#.##") + "°";
                    card.GetComponentInChildren<LineRenderer>().positionCount = 3;
                    card.GetComponentInChildren<LineRenderer>().SetPosition(0, transform.position + startVector);
                    card.GetComponentInChildren<LineRenderer>().SetPosition(1, transform.position);
                    card.GetComponentInChildren<LineRenderer>().SetPosition(2, transform.position + controller.transform.forward *  -1f);
                }
            }
            else
            {
                lineR.positionCount = 0;
            }
        }
    }

    void ShowDistanceValue()
    {
        valueText.text = endDistance.ToString("#.##");
        lineR.positionCount = 2;
        lineR.SetPosition(0, startPoint);
        lineR.SetPosition(1, endPoint);
    }

    void ShowAngleValue()
    {
        angle = Vector3.Angle(startVector, -controller.transform.forward);
        valueText.text = "" + angle.ToString("###.##") + "°";
        lineR.positionCount = 3;
        lineR.SetPosition(0, transform.position + startVector * .5f);
        lineR.SetPosition(1, transform.position);
        lineR.SetPosition(2, transform.position + controller.transform.forward * -.5f);
    }

    void ShowCurveValue()
    {
        valueText.text = curveDist.ToString("#.##");
        lineR.positionCount = curveList.Count;
        lineR.SetPositions(curveList.ToArray());
    }

    public void TogglePermanent()
    {
        //isPermanent = !isPermanent;
    }

    public void isLine()
    {
        mode = MeasureMode.line;
    }

    public void isCurve()
    {
        mode = MeasureMode.curve;
    }

    public void isAngle()
    {
        mode = MeasureMode.angle;
    }

}
