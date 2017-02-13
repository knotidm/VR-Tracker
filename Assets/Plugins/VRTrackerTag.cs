using UnityEngine;

namespace Assets.Plugins
{
    public class VRTrackerTag : MonoBehaviour
    {
        public Vector3 positionOffset;
        public Vector3 orientationOffset;
        //public string status;
        //public int battery;

        private Vector3 position;
        private Vector3 orientation;

        public int orientationEnabled = 0;

        public string id;

        void Start()
        {
        }

        void Update()
        {
            transform.position = position;
            transform.rotation = Quaternion.Euler(orientation);
        }

        public void updatePosition(Vector3 position)
        {
            this.position = position + positionOffset;
        }

        public void updateOrientation(Vector3 orientation)
        {
            this.orientation = orientation + orientationOffset;
        }

        public void onSpecialCommand(string data)
        {
            Debug.Log("VR Tracker : special command - " + data);
        }
    }
}
