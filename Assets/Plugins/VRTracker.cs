using UnityEngine;
using WebSocketSharp;

public class VRTracker : MonoBehaviour
{
    private WebSocket webSocket;
    private Vector3 position;
    private Vector3 orientation;
    public int orientationEnabled = 0;
    public Transform CameraTransform;
    public Vector3 positionOffset;
    public Vector3 orientationOffset;
    private int counter = 0;

    public string TagUID = "5c:cf:7f:c4:4b:7e";
    public string UserUID = "ABC123";

    private bool orientationEnablingSent = false;
    void Start()
    {
        openWebsocket();
    }

    void Update()
    {
        CameraTransform.transform.position = position;

        if (orientationEnabled == 1)
        {
            if (!orientationEnablingSent)
            {
                Debug.Log("VR Tracker : asking for orientation");
                orientationEnablingSent = true;

                webSocket.SendAsync("cmd=mac&uid=" + UserUID, OnSendComplete);
                assignTag(TagUID);
                TagOrientation(TagUID, true);
            }
            CameraTransform.transform.rotation = Quaternion.Euler(orientation);
        }
        else if (counter < 100)
        {
            counter++;
        }
        else if (counter == 100)
        {
            orientationEnabled = 1;
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
        webSocket.SendAsync("cmd=mac&uid=" + UserUID, OnSendComplete);
        assignTag(TagUID);
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
                        position.x = float.Parse(datasplit[1]) + positionOffset.x;
                        break;
                    case "z":
                        position.y = float.Parse(datasplit[1]) + positionOffset.y;
                        break;
                    case "y":
                        position.z = float.Parse(datasplit[1]) + positionOffset.z;
                        break;
                    case "ox":
                        orientation.y = -float.Parse(datasplit[1]) + orientationOffset.y;
                        break;
                    case "oy":
                        orientation.z = -float.Parse(datasplit[1]) + orientationOffset.z;
                        break;
                    case "oz":
                        orientation.x = -float.Parse(datasplit[1]) + orientationOffset.x;
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
            webSocket.SendAsync("cmd=mac&uid=" + UserUID, OnSendComplete);
            assignTag(TagUID);
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
            orientationEnabled = 1;
            en = "true";
        }
        else
        {
            orientationEnabled = 0;
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
        TagOrientation(TagUID, false);
        closeWebsocket();
    }
}