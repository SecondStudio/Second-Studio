using UnityEngine;
using System.Collections;
using System.IO;
public class CameraTool : ToolBase
{
    public Camera recorderCamera;
    public GameObject cameraBody;
    Vector3 originalUp;

    string filePath;
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        originalUp = transform.parent.parent.up;
        trackerLetter = "A";
    }

    [PunRPC]
    void ToggleSnapShot()
    {
        filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/Screenshots/" + System.DateTime.Now.ToLongTimeString().Replace(":" , "_").Replace(" " , "") + ".png";

        //for taking picture of the screen
        //Application.CaptureScreenshot(filePath, 2); 

        //for actually aiming the camera

        //StartCoroutine(TakePicture());
        TakePicture(recorderCamera); //not a coroutine for now

        StartCoroutine(LoadSnapshotData());
    }

    //IEnumerator TakePicture() //not a coroutine for now
    void TakePicture(Camera usingCamera)
    {
        //yield return new WaitForEndOfFrame();

        Camera cam = usingCamera;

        RenderTexture currentTexture = RenderTexture.active;

        RenderTexture.active = cam.targetTexture;
        cam.Render();
        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = currentTexture;

        byte[] bytes = image.EncodeToPNG();

        System.IO.FileInfo file = new System.IO.FileInfo(filePath);
        file.Directory.Create();
        System.IO.File.WriteAllBytes(filePath, bytes);
    }

    //loads the screenshot into the scene in front of the player
    IEnumerator LoadSnapshotData()
    {
        yield return new WaitForSeconds(2);
        Vector3 clipboardPos = controller.Head.transform.position + controller.Head.transform.forward * .5f;
        GameObject Clipboard = Instantiate(Resources.Load<GameObject>("Clipboard"), clipboardPos , Quaternion.LookRotation(controller.Head.transform.position - clipboardPos));
        Clipboard.transform.Rotate(90, 0, 0);
        ObjectManager.instance.AddObject(Clipboard);
        if (Clipboard != null)
        {
            Texture2D tex = new Texture2D(1920, 1080);
            byte[] fileData = File.ReadAllBytes(filePath);

            tex.LoadImage(fileData);
            Clipboard.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
        }
    }

    
    public void FlipCamera()
    {
        photonView.RPC("flipCamera", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void flipCamera()
    {
        cameraBody.transform.Rotate(0, 0, 180);
        print("CameraFlip");
    }

    //disabled some of the camera features such as making it further and closer because it didn't work properly
    /*public void ExtendCamera()
    {
        photonView.RPC("extendCamera", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void extendCamera()
    {
        cameraBody.transform.parent.parent.position += originalUp * .05f;
        print("extended");
    }

    public void ShortenCamera()
    {
        photonView.RPC("shortenCamera", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void shortenCamera()
    {
        cameraBody.transform.parent.parent.position += originalUp * -.05f;
        print("shortened");
    }*/

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!photonView.isMine)
            return;

        if (controller.triggerButtonDown)
        {
            photonView.RPC("ToggleSnapShot", PhotonTargets.AllBufferedViaServer);
        }

    }

}
