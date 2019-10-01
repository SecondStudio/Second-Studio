using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class UploadAnalyitics : MonoBehaviour {

    // Use this for initialization


    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	}

    
    public static void Upload(System.Object payload, System.Object payload2)
    {
        print("sendingAPI request");
        string json = JsonUtility.ToJson(payload);
        string json2 = JsonUtility.ToJson(payload2);
        print(json);
        print(json2);
        var data = System.Text.Encoding.UTF8.GetBytes(json);
        var data2 = System.Text.Encoding.UTF8.GetBytes(json2);
        Dictionary<string, string> postHeader = new Dictionary<string, string>();
        postHeader.Add("Content-Type", "application/json");

        WWW apiRequest = new WWW("https://ro0zmt1nvh.execute-api.us-west-2.amazonaws.com/prod/UploadAnalytics", data , postHeader);

        while (!apiRequest.isDone)
        {
            continue;
        }
        print(apiRequest.error);
        print(apiRequest.text);

        WWW apiRequest2 = new WWW("https://ro0zmt1nvh.execute-api.us-west-2.amazonaws.com/prod/UploadRawData", data2, postHeader);

        while (!apiRequest2.isDone)
        {
            continue;
        }
        print(apiRequest2.error);
        print(apiRequest2.text);
    }
}
