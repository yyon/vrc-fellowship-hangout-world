
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MochiCameraCtrl : UdonSharpBehaviour
{
    [SerializeField] GameObject m_Cameras;
    [SerializeField] Camera m_SubCamera;
    [SerializeField] Renderer m_MochiMat;

    void Start()
    {
        Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
    }

    private void Update()
    {
        VRCPlayerApi.TrackingData td = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        m_Cameras.transform.SetPositionAndRotation(td.position, td.rotation);
        m_MochiMat.material.SetMatrix("_SubProjMat", m_SubCamera.projectionMatrix);
        m_MochiMat.material.SetMatrix("_SubViewMat", m_SubCamera.worldToCameraMatrix);
    }
}
