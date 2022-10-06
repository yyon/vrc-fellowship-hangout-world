
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArchiTech
{
    public class HandTest : UdonSharpBehaviour
    {
        public LineRenderer leftLine;
        public LineRenderer rightLine;
        public Vector3 leftDir;
        public Vector3 rightDir;
        void Update()
        {
            var left = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            leftLine.SetPosition(0, left.position);
            leftLine.SetPosition(1, left.position + left.rotation * leftDir);
            var right = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            rightLine.SetPosition(0, right.position);
            rightLine.SetPosition(1, right.position + right.rotation * rightDir);
        }
    }
}