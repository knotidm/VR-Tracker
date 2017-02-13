using UnityEngine;

namespace Assets.Plugins
{
    public class VRTrackerTag : MonoBehaviour
    {
        private Vector3 position;
        private Vector3 orientation;

        public Transform CameraTransform;
        public Vector3 positionOffset;
        public Vector3 orientationOffset;
        public int orientationEnabled;
        public string id;

        //public string status;
        //public int battery;

        void Start()
        {
        }

        void Update()
        {
            if (CameraTransform != null)
            {
                CameraTransform.transform.position = position;
                CameraTransform.transform.rotation = Quaternion.Euler(orientation);
            }
            else
            {
                transform.position = position;
                transform.rotation = Quaternion.Euler(orientation);
            }
        }

        public void UpdatePosition(Vector3 position)
        {
            this.position = position + positionOffset;
        }

        public void UpdateOrientation(Vector3 orientation)
        {
            this.orientation = orientation + orientationOffset;
        }

        public void OnSpecialCommand(string data)
        {
            Debug.Log("VR Tracker : special command - " + data);
        }
    }
}