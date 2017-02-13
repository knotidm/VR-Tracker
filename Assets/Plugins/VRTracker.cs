using Assets.Plugins;
using UnityEngine;
using WebSocketSharp;

public class VRTracker : MonoBehaviour
{
    private WebSocket webSocket;
    private Vector3 position;
    private Vector3 orientation;
    private int counter = 0;
    private VRTrackerTag[] tags;
    private bool orientationEnablingSent = false;
    public string userID;

    void Start()
    {
        openWebsocket();
        findTags();
    }

    void Update()
    {

        foreach (VRTrackerTag tag in tags)
        {
            if (tag.orientationEnabled == 1)
            {
                if (!orientationEnablingSent)
                {
                    Debug.Log("Tag " + tag.id + ": asking for orientation");
                    orientationEnablingSent = true;
                    webSocket.SendAsync("cmd=mac&uid=" + userID, OnSendComplete);
                    assignTag(tag.id);
                    TagOrientation(tag.id, true);
                }
            }
            else if (counter < 100)
            {
                counter++;
            }
            else if (counter == 100)
            {
                tag.orientationEnabled = 1;
                counter++;
            }
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

    void findTags()
    {
        tags = FindObjectsOfType(typeof(VRTrackerTag)) as VRTrackerTag[];
        foreach (VRTrackerTag tag in tags)
        {
            Debug.Log("Found VR Tracker Tag: " + tag.id);
        }
    }

    private void OnOpenHandler(object sender, System.EventArgs e)
    {
        Debug.Log("VR Tracker : connection established");
        webSocket.SendAsync("cmd=mac&uid=" + userID, OnSendComplete);
        foreach (VRTrackerTag tag in tags)
        {
            assignTag(tag.id);
        }
    }

    private void OnMessageHandler(object sender, MessageEventArgs e)
    {
        if (e.Data.Contains("cmd=position"))
        {
            string[] dataByTag = e.Data.Split(new string[] { "&uid=" }, System.StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < dataByTag.Length; i++)
            {
                bool positionUpdated = false;
                bool orientationUpdated = false;
                string[] datas = dataByTag[i].Split('&');
                string tagID = datas[0];
                foreach (string data in datas)
                {
                    string[] datasplit = data.Split('=');

                    switch (datasplit[0])
                    {
                        case "x":
                            positionUpdated = true;
                            position.x = float.Parse(datasplit[1]);
                            break;
                        case "z":
                            position.y = float.Parse(datasplit[1]);
                            break;
                        case "y":
                            position.z = float.Parse(datasplit[1]);
                            break;
                        case "ox":
                            orientationUpdated = true;
                            orientation.y = -float.Parse(datasplit[1]);
                            break;
                        case "oy":
                            orientation.z = -float.Parse(datasplit[1]);
                            break;
                        case "oz":
                            orientation.x = float.Parse(datasplit[1]);
                            break;
                    }
                }

                foreach (VRTrackerTag tag in tags)
                {
                    if (tag.id == tagID)
                    {
                        if (tag.orientationEnabled == 1 && orientationUpdated)
                            tag.updateOrientation(orientation);
                        if (positionUpdated)
                        {
                            tag.updatePosition(position);
                        }
                    }
                }
            }
        }

        else if (e.Data.Contains("cmd=error"))
        {
            Debug.LogWarning("VR Tracker : " + e.Data);
            webSocket.SendAsync("cmd=mac&uid=" + userID, OnSendComplete);
            foreach (VRTrackerTag tag in tags)
            {
                assignTag(tag.id);
            }
        }
        else
        {
            Debug.Log("VR Tracker data received : " + e.Data);
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

    public void assignTag(string tagID)
    {
        webSocket.SendAsync("cmd=tagassign&uid=" + tagID, OnSendComplete);
    }

    public void TagOrientation(string tagID, bool enable)
    {
        string en = "";
        if (enable)
        {
            en = "true";
        }
        else
        {
            en = "false";
        }

        webSocket.SendAsync("cmd=orientation&orientation=" + en + "&uid=" + tagID, OnSendComplete);
    }

    void OnApplicationQuit()
    {
        foreach (VRTrackerTag tag in tags)
        {
            TagOrientation(tag.id, false);
        }
        closeWebsocket();
    }

    private void closeWebsocket()
    {
        Debug.Log("VR Tracker : closing websocket connection");
        webSocket.Close();
    }
}