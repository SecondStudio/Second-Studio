using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class RecorderScript : MonoBehaviour
{
    public RenderTexture recorderTexture;
    Camera recorderCamera;
    public string path = "";
    public bool recording;
    float time = 0;

    void Start()
    {
        recorderCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        time += Time.deltaTime;
        if (time > 5)
        {
            StartCoroutine(TakeScreenShot());
            time = 0;
        }
    }

    public IEnumerator TakeScreenShot()
    {
        yield return new WaitForEndOfFrame();

        Camera cam = recorderCamera;

        RenderTexture currentTexture = RenderTexture.active;

        RenderTexture.active = cam.targetTexture;
        cam.Render();
        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = currentTexture;

        byte[] bytes = image.EncodeToPNG();

        string filename = "Recorded/" + DateTime.Now.ToLongTimeString() + ".png";
        path = filename;
        System.IO.FileInfo file = new System.IO.FileInfo(filename);
        file.Directory.Create();
        System.IO.File.WriteAllBytes(path, bytes);
    }
}