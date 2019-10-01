using UnityEngine;
using System;
//using System.Collections;
using System.Collections.Generic;


public class LicenseController : MonoBehaviour
{
    //private string secretKey = "fuckservers"; // Edit this value and make sure it's the same as the one stored on the server
    public string postURL = "http://www.secondstudio.la/scripts/initialpost.php?"; //be sure to add a ? to your url
    public string getURL = "http://www.secondstudio.la/scripts/display.php";
    private bool check = true;
    string serial_num = "";
    string client_name = "";
    string client_contact = "";
    string old_physical_address = "";
    string new_physical_address = "";
    List<Data> values;
    char[] delim1 = { '|' };
    char[] delim2 = { ';' };
    public Data data;
    public DateTime expDate;
    private string extID;
    private string newID;
    private string msg;

    void Awake()
    {
        CheckExpiration();
    }
    void Start()
    {

    }

    /* remember to use StartCoroutine when calling this function!
    IEnumerator PostInfo(string serial, string name, string contact, string address)
    {
        WWWForm form = new WWWForm();
        form.AddField("serial_num", serial);
        form.AddField("client_name", name);
        form.AddField("client_contact", contact);
        form.AddField("physical_address", name);
        // Post the URL to the site and create a download object to get the result.
        WWW _post = new WWW(postURL, form);
        yield return _post; // Wait until the download is done

        if (_post.error != null)
        {
            print("There was an error posting the high score: " + _post.error);
        }
    }

    // Get the scores from the MySQL DB to display in a GUIText.
    // remember to use StartCoroutine when calling this function!
    IEnumerator Getinfo()
    {
        WWW _get = new WWW(getURL);
        yield return _get;

        if (_get.error != null)
        {
            print("There was an error getting the high score: " + _get.error);
        }
        else
        {
            
            msg = _get.text;
            Debug.Log(msg); // this will display the scores in game.
            string[] players = msg.Split(delim1);
            Debug.Log("The stats for the first player are " + players[0]);
            foreach (string player in players)
            {
                string[] data = player.Split(delim2);
                //values.Add(new Data { Name = ripples[0], Score = ripples[1] });
                Debug.Log("Player name is " + data[1]);
                Debug.Log("Player score is " + data[2]);
            }


        }
    }
    */

    void CheckExpiration()
    {
        expDate = new DateTime(2017, 4, 15, 0, 0, 0);
        int result = DateTime.Compare(DateTime.Today, expDate);

        if (result > 0)
        {
            Application.Quit();
        }
    }
    /*
    //update from playerprefs to online db
    void CheckPhysical()
    {
        extID = PlayerPrefs.GetString("Machine ID");
        if (extID == null)
        {
            newID = Environment.MachineName;
            PlayerPrefs.SetString("Machine ID", newID);
        }
        else
        {
            newID = Environment.MachineName;
            bool result = extID.Equals(newID, StringComparison.Ordinal);
            Debug.Log(result);
            if (result = false)
            {
                Application.Quit();
            }

        }
    }
    */

}

public class Data
{
    public string SerialNumber { get; set; }
    public string ClientName { get; set; }
    public string ClientContact { get; set; }
    public string PhysicalAddress { get; set; }


}


