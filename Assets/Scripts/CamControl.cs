using UnityEngine;
using UnityEngine.VR;

public class CamControl : MonoBehaviour
{

    private Camera mycam;
    private float sensivity = 3.0f;
    // Use this for initialization
    void Start()
    {
        mycam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!UnityEngine.XR.XRSettings.isDeviceActive)
        {

            float mouseLeftRight = Input.GetAxis("Mouse X") * sensivity;
            transform.Rotate(0, mouseLeftRight, 0);


            float mouseUpDown = Input.GetAxis("Mouse Y") * sensivity;
            transform.Rotate(mouseUpDown, 0, 0);


            ////float sensitivity = 0.05f;
            //Vector3 vp = mycam.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mycam.nearClipPlane));
            ////vp.x -= 0.5f;
            ////vp.y -= 0.5f;
            ////vp.x *= sensitivity;
            ////vp.y *= sensitivity;
            ////vp.x += 0.5f;
            ////vp.y += 0.5f;
            //Vector3 sp = mycam.ViewportToScreenPoint(vp);

            //Vector3 v = mycam.ScreenToWorldPoint(sp);
            //transform.LookAt(v, Vector3.up);
        }
    }
}
