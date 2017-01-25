using UnityEngine;
using WebSocketSharp;

public class VRTracker : MonoBehaviour {
	
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
	// Use this for initialization
	void Start () {
		openWebsocket();
	}
	
	// Update is called once per frame
	void Update () {
		CameraTransform.transform.position = position;
		
		if (orientationEnabled == 1) {
			if (!orientationEnablingSent) {
				Debug.Log ("VR Tracker : asking for orientation");
				orientationEnablingSent = true;
				
				webSocket.SendAsync ("cmd=mac&uid=" + UserUID, OnSendComplete);
				assignTag(TagUID);
				TagOrientation (TagUID, true);
			}
			CameraTransform.transform.rotation = Quaternion.Euler (orientation);
		} else if (counter < 100) {
			counter++;
		} else if (counter == 100) {
			orientationEnabled = 1;
			counter++;
		}
	}
	

	/*
	* Opens the websocket connection with the Gateway
	*/
	private void openWebsocket(){
		//Debug.Log("VR Tracker : opening websocket connection");
		webSocket = new WebSocket("ws://192.168.42.1:7777/user/");
		webSocket.OnOpen += OnOpenHandler;
		webSocket.OnMessage += OnMessageHandler;
		webSocket.OnClose += OnCloseHandler;
		webSocket.ConnectAsync();	
	}

	private void OnOpenHandler(object sender, System.EventArgs e) {
		Debug.Log("VR Tracker : connection established");
		//new WaitForSeconds(3);
		webSocket.SendAsync ("cmd=mac&uid="+UserUID, OnSendComplete);
		assignTag(TagUID);
		//assignATag ();
	}
	
	private void OnMessageHandler(object sender, MessageEventArgs e) {
		//Debug.Log ("VR Tracker : " + e.Data);
		if (e.Data.Contains ("cmd=position")) {
			string[] datas = e.Data.Split ('&');
			foreach (string data in datas){
				string[] datasplit = data.Split ('=');
				// Position
				if(datasplit[0] == "x"){
					position.x = float.Parse(datasplit[1]) + positionOffset.x;
				}
				else if(datasplit[0] == "z"){
					position.y = float.Parse(datasplit[1]) + positionOffset.y;
				}
				else if(datasplit[0] == "y"){
					position.z = float.Parse(datasplit[1]) + positionOffset.z;
				}
				
				// Orientation
				else if(datasplit[0] == "ox"){
					orientation.y = -float.Parse(datasplit[1]) + orientationOffset.y;
				}
				else if(datasplit[0] == "oy"){
					orientation.z = -float.Parse(datasplit[1]) + orientationOffset.z;
				}
				else if(datasplit[0] == "oz"){
					orientation.x = -float.Parse(datasplit[1]) + orientationOffset.x;
				}
			}
			
		} else if (e.Data.Contains ("cmd=specialcmd")) {
			Debug.Log ("VR Tracker : " + e.Data);
			string[] datas = e.Data.Split ('&');
			string uid = null;
			string command = null;
			foreach (string data in datas){
				string[] datasplit = data.Split ('=');
				
				// Tag UID sending the special command
				if(datasplit[0] == "uid"){
					uid = datasplit[1];
				}
				
				// Special Command
				else if(datasplit[0] == "data"){
					command = datasplit[1];
				}
			}
			if(uid != null && command != null)
				receiveSpecialCommand(uid, command);
			
		} else if (e.Data.Contains ("cmd=taginfos")) {
			string[] datas = e.Data.Split ('&');
			
			string uid = null;
			string status = null;
			int battery = 0;
			
			foreach (string data in datas){
				string[] datasplit = data.Split ('=');
				
				// Tag UID sending its informations
				if(datasplit[0] == "uid"){
					uid = datasplit[1];
				}
				// Tag status (“lost”, “tracking”, “unassigned”)
				else if(datasplit[0] == "status"){
					status = datasplit[1];
				}
				// Tag battery
				else if(datasplit[0] == "battery"){
					battery = int.Parse(datasplit[1]);
				}
			}
			if(uid != null && status != null)
				receiveTagInformations(uid, status, battery);
			
		}
		else if (e.Data.Contains ("cmd=error")) {
			// TODO Parse differnt kinds of errors
			webSocket.SendAsync ("cmd=mac&uid="+UserUID, OnSendComplete);
			assignTag(TagUID);
		} 
		else {
			Debug.Log ("VR Tracker : Unknown data received : " + e.Data);
		}
	}
	
	private void OnCloseHandler(object sender, CloseEventArgs e) {
		Debug.Log("VR Tracker : connection closed for this reason: " + e.Reason);
	}
	
	private void OnSendComplete(bool success) {
		Debug.Log("VR Tracker : Send Complete");
	}
	

	
	/*
	* Close the ebsocket connection to the Gateway
	*/
	private void closeWebsocket(){
		Debug.Log("VR Tracker : closing websocket connection");
		this.webSocket.Close();
	}
	
	
	/* 
	* Send your Unique ID, it can be your MAC address for 
	* example but avoid the IP. It will be used by the Gateway
	* to identify you over the network. It is necessary on multi-gateway
	* configuration 
	*/
	private void sendMyUID(string uid){
		webSocket.SendAsync(uid, OnSendComplete);
	}
	
	/* 
	* Asks the gateway to assign a specific Tag to this device.  
	* Assigned Tags will then send their position to this device.
	*/
	public void assignTag(string TagID){
		webSocket.SendAsync("cmd=tagassign&uid=" + TagID, OnSendComplete);
	}
	
	/* 
	* Asks the gateway to assign a Tag to this device.  
	* Assigned Tags will then send their position to this device.
	*/
	public void assignATag(){
		webSocket.SendAsync("cmd=assignatag", OnSendComplete);
	}

	/* 
	* Asks the gateway to UNassign a specific Tag from this device.  
	* You will stop receiving updates from this Tag.
	*/
	public void unAssignTag(string TagID){
		webSocket.SendAsync("cmd=tagunassign&uid=" + TagID, OnSendComplete);
	}
	
	/* 
	* Asks the gateway to UNassign all Tags from this device.  
	* You will stop receiving updates from any Tag.
	*/
	public void unAssignAllTags(){
		webSocket.SendAsync("cmd=tagunassignall", OnSendComplete);
	}
	
	/* 
	* Ask for informations on a specific Tag
	*/
	public void getTagInformations(string TagID){
		webSocket.SendAsync("cmd=taginfos&uid=" + TagID, OnSendComplete);
	}
	
	/*
	* Enable or Disable orientation detection for a Tag
	*/
	public void TagOrientation(string TagID, bool enable){
		string en = "";
		if (enable) {
			orientationEnabled = 1;
			en = "true";
		} else {
			orientationEnabled = 0;
			en = "false";
		}
		
		webSocket.SendAsync("cmd=orientation&orientation=" + en + "&uid=" + TagID, OnSendComplete);
	}
	
	/*
	* Set a specific color on the Tag
	* R (0-255)
	* G (0-255)
	* B (0-255)
	*/
	public void setTagColor(string TagID, int red, int green, int blue){
		webSocket.SendAsync("cmd= color&r=" + red + "&g=" + green + "&b=" + blue + "&uid=" + TagID, OnSendComplete);
	}
	
	
	/* 
	* Send special command to a Tag
	*/
	public void sendTagCommand(string TagID, string command){
		Debug.Log("VR Tracker : " + command);
		webSocket.SendAsync("cmd=specialcmd&uid=" + TagID + "&data=" + command, OnSendComplete);
	}
	
	/* 
	* Send User device battery level to the Gateway
	* battery (0-100)
	*/
	public void sendUserBattery(int battery){
		webSocket.SendAsync("cmd=usrbattery&battery=" + battery, OnSendComplete);
	}
	
	/*
	* Executed on reception of a special command 
	*/
	public void receiveSpecialCommand(string TagID, string data){
		// TODO: You can do whatever you wants with the special command, have fun !
	}
	
	/*
	* Executed on reception of  tag informations
	*/
	public void receiveTagInformations(string TagID, string status, int battery){
		// TODO: You can do whatever you wants with the Tag informations
	}
	
	/* 
	* Ensure the Websocket is correctly closed on application quit
	*/
	void OnApplicationQuit() {
		TagOrientation (TagUID, false);
		closeWebsocket ();
	}
	
}