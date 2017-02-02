using UnityEngine;
using WebSocketSharp;

public class VRTracker : MonoBehaviour
{
    private WebSocket webSocket;
    private Vector3 cameraPosition;
    private Vector3 cameraOrientation;
    public int cameraOrientationEnabled = 0;
    public Transform CameraTransform;
    public Vector3 cameraPositionOffset;
    public Vector3 cameraOrientationOffset;
    private int counter = 0;

    public string tagID;
    public string userID;

    private bool orientationEnablingSent = false;
    void Start()
    {
        openWebsocket();
    }

    void Update()
    {
        CameraTransform.transform.position = cameraPosition;

        if (cameraOrientationEnabled == 1)
        {
            if (!orientationEnablingSent)
            {
                Debug.Log("VR Tracker : asking for orientation");
                orientationEnablingSent = true;

                webSocket.SendAsync("cmd=mac&uid=" + userID, OnSendComplete);
                assignTag(tagID);
                TagOrientation(tagID, true);
            }
            CameraTransform.transform.rotation = Quaternion.Euler(cameraOrientation);
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
        assignTag(tagID);
    }

    private void OnMessageHandler(object sender, MessageEventArgs e)
    {
        if (e.Data.Contains("cmd=position"))
        {
            string[] datas = e.Data.Split('&');
            foreach (string data in datas)
            {
                string[] datasplit = data.Split('=');

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
        }
        else if (e.Data.Contains("cmd=specialcmd"))
        {
            Debug.Log("VR Tracker : " + e.Data);
            string[] datas = e.Data.Split('&');
            string uid = null;
            string command = null;
            foreach (string data in datas)
            {
                string[] datasplit = data.Split('=');

                if (datasplit[0] == "uid")
                {
                    uid = datasplit[1];
                }

                else if (datasplit[0] == "data")
                {
                    command = datasplit[1];
                }
            }
            if (uid != null && command != null)
                receiveSpecialCommand(uid, command);

        }
        else if (e.Data.Contains("cmd=taginfos"))
        {
            string[] datas = e.Data.Split('&');

            string uid = null;
            string status = null;
            int battery = 0;

            foreach (string data in datas)
            {
                string[] datasplit = data.Split('=');

                if (datasplit[0] == "uid")
                {
                    uid = datasplit[1];
                }
                else if (datasplit[0] == "status")
                {
                    status = datasplit[1];
                }
                else if (datasplit[0] == "battery")
                {
                    battery = int.Parse(datasplit[1]);
                }
            }
            if (uid != null && status != null)
                receiveTagInformations(uid, status, battery);

        }
        else if (e.Data.Contains("cmd=error"))
        {
            webSocket.SendAsync("cmd=mac&uid=" + userID, OnSendComplete);
            assignTag(tagID);
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
        this.webSocket.Close();
    }

    private void sendMyUID(string uid)
    {
        webSocket.SendAsync(uid, OnSendComplete);
    }

    public void assignTag(string TagID)
    {
        webSocket.SendAsync("cmd=tagassign&uid=" + TagID, OnSendComplete);
    }

    public void assignATag()
    {
        webSocket.SendAsync("cmd=assignatag", OnSendComplete);
    }

    public void unAssignTag(string TagID)
    {
        webSocket.SendAsync("cmd=tagunassign&uid=" + TagID, OnSendComplete);
    }

    public void unAssignAllTags()
    {
        webSocket.SendAsync("cmd=tagunassignall", OnSendComplete);
    }

    public void getTagInformations(string TagID)
    {
        webSocket.SendAsync("cmd=taginfos&uid=" + TagID, OnSendComplete);
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

    public void setTagColor(string TagID, int red, int green, int blue)
    {
        webSocket.SendAsync("cmd= color&r=" + red + "&g=" + green + "&b=" + blue + "&uid=" + TagID, OnSendComplete);
    }

    public void sendTagCommand(string TagID, string command)
    {
        Debug.Log("VR Tracker : " + command);
        webSocket.SendAsync("cmd=specialcmd&uid=" + TagID + "&data=" + command, OnSendComplete);
    }

    public void sendUserBattery(int battery)
    {
        webSocket.SendAsync("cmd=usrbattery&battery=" + battery, OnSendComplete);
    }

    public void receiveSpecialCommand(string TagID, string data)
    {
    }

    public void receiveTagInformations(string TagID, string status, int battery)
    {
    }

    void OnApplicationQuit()
    {
        TagOrientation(tagID, false);
        closeWebsocket();
    }
}