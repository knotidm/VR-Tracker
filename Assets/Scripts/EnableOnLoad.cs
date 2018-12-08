using UnityEngine.SceneManagement;
using UnityEngine;

public class EnableOnLoad : MonoBehaviour {

    public bool enableOnLoad = false;

	// Use this for initialization
	void Start () {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (this != null)
        {
            if (enableOnLoad)
            {
                transform.eulerAngles.Set(transform.eulerAngles.x, 0, transform.eulerAngles.z);

                Camera[] cams = GetComponentsInChildren<Camera>();
                foreach (Camera cam in cams)
                {
                    cam.enabled = true;
                    cam.transform.eulerAngles.Set(cam.transform.eulerAngles.x, 0, cam.transform.eulerAngles.z);
                }
                if (this.GetComponentInChildren<AudioListener>() != null)
                {
                    this.GetComponentInChildren<AudioListener>().enabled = true;
                }
            }
            else
            {
                Camera[] cams = GetComponentsInChildren<Camera>();
                foreach(Camera cam in cams)
                {
                    cam.enabled = false;
                }
				if(this.GetComponentInChildren<AudioListener>() != null)
                	this.GetComponentInChildren<AudioListener>().enabled = false;
            }
        }
    }
}
