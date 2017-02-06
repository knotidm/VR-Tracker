using UnityEngine;
using WebSocketSharp;

public class VRTracker : MonoBehaviour
{
    private WebSocket webSocket;
    private Vector3 cameraPosition;
    private Vector3 cameraOrientation;
    private Vector3 cubePosition;
    private Vector3 cubeOrientation;
    public int cameraOrientationEnabled = 0;
    public Transform CameraTransform;
    public Vector3 cameraPositionOffset;
    public Vector3 cameraOrientationOffset;
    private int counter = 0;

    public GameObject cube;

    public string cameraTagID;
    public string cubeTagID;
    public string userID;

    private bool orientationEnablingSent = false;
    void Start()
    {
        openWebsocket();
    }

    void Update()
    {
        CameraTransform.transform.position = cameraPosition;
        cube.transform.position = cubePosition;
        if (cameraOrientationEnabled == 1)
        {
            if (!orientationEnablingSent)
            {
                Debug.Log("VR Tracker : asking for orientation");
                orientationEnablingSent = true;

                webSocket.SendAsync("cmd=mac&uid=" + userID, OnSendComplete);
                assignTag(cameraTagID);
                assignTag(cubeTagID);
                TagOrientation(cameraTagID, true);
                TagOrientation(cubeTagID, true);
            }
            CameraTransform.transform.rotation = Quaternion.Euler(cameraOrientation);
            cube.transform.rotation = Quaternion.Euler(cubeOrientation);
        }
        else if (counter < 100)
        {
            counter++;
        }
        else if (counter == 100)
        {
            cameraOrientationEnabled = 1;
            counter++;
        }
    }

    private void openWebsocket()
    {
        webSocket = new WebSocket("ws://192.168.42.1:7777/user/");
        webSocket.OnOpen += OnOpenHandler;
        webSocket.OnMessage += OnMessageHandler;
        webSocket.OnClose += OnCloseHandler;
        webSocket.ConnectAsync();
    }

    private void OnOpenHandler(object sender, System.EventArgs e)
    {
        Debug.Log("VR Tracker : connection established");
        webSocket.SendAsync("cmd=mac&uid=" + userID, OnSendComplete);
        assignTag(cameraTagID);
        assignTag(cubeTagID);
    }

    private void OnMessageHandler(object sender, MessageEventArgs e)
    {
        if (e.Data.Contains("cmd=position"))
        {
            string[] datas = e.Data.Split('&');

            for (int i = 2; i < 8; i++)
            {
                string[] datasplit = datas[i].Split('=');

                switch (datasplit[0])
                {
                    case "x":
                        cameraPosition.x = float.Parse(datasplit[1]) + cameraPositionOffset.x;
                        break;
                    case "z":
                        cameraPosition.y = float.Parse(datasplit[1]) + cameraPositionOffset.y;
                        break;
                    case "y":
                        cameraPosition.z = float.Parse(datasplit[1]) + cameraPositionOffset.z;
                        break;
                    case "ox":
                        cameraOrientation.y = -float.Parse(datasplit[1]) + cameraOrientationOffset.y;
                        break;
                    case "oy":
                        cameraOrientation.z = -float.Parse(datasplit[1]) + cameraOrientationOffset.z;
                        break;
                    case "oz":
                        cameraOrientation.x = -float.Parse(datasplit[1]) + cameraOrientationOffset.x;
                        break;
                }
            }

            for (int i = 9; i < 15; i++)
            {
                string[] datasplit = datas[i].Split('=');

                switch (datasplit[0])
                {
                    case "x":
                        cubePosition.x = float.Parse(datasplit[1]);
                        break;
                    case "z":
                        cubePosition.y = float.Parse(datasplit[1]);
                        break;
                    case "y":
                        cubePosition.z = float.Parse(datasplit[1]);
                        break;
                    case "ox":
                        cubeOrientation.y = -float.Parse(datasplit[1]);
                        break;
                    case "oy":
                        cubeOrientation.z = -float.Parse(datasplit[1]);
                        break;
                    case "oz":
                        cubeOrientation.x = -float.Parse(datasplit[1]);
                        break;
                }
            }
        }
        else if (e.Data.Contains("cmd=error"))
        {
            webSocket.SendAsync("cmd=mac&uid=" + userID, OnSendComplete);
            assignTag(cameraTagID);
            assignTag(cubeTagID);
        }
        else
        {
            Debug.Log("VR Tracker : Unknown data received : " + e.Data);
        }
    }

    private void OnCloseHandler(object sender, CloseEventArgs e)
    {
        Debug.Log("VR Tracker : connection closed for this reason: " + e.Reason);
    }

    private void OnSendComplete(bool success)
    {
        Debug.Log("VR Tracker : Send Complete");
    }

    private void closeWebsocket()
    {
        Debug.Log("VR Tracker : closing websocket connection");
        webSocket.Close();
    }

    public void assignTag(string TagID)
    {
        webSocket.SendAsync("cmd=tagassign&uid=" + TagID, OnSendComplete);
    }

    public void TagOrientation(string TagID, bool enable)
    {
        string en = "";
        if (enable)
        {
            cameraOrientationEnabled = 1;
            en = "true";
        }
        else
        {
            cameraOrientationEnabled = 0;
            en = "false";
        }

        webSocket.SendAsync("cmd=orientation&orientation=" + en + "&uid=" + TagID, OnSendComplete);
    }

    void OnApplicationQuit()
    {
        TagOrientation(cameraTagID, false);
        TagOrientation(cubeTagID, false);
        closeWebsocket();
    }
}