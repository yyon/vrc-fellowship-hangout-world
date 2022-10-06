
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace ArchiTech
{
    public class PenSyncData : UdonSharpBehaviour
    {
        // [UdonSynced] private string syncData = "";
        [HideInInspector] [UdonSynced] public Color color = Color.white;
        [HideInInspector] [UdonSynced] public float brushLOD = 0.015f;
        [HideInInspector] [UdonSynced] public float brushWidth = 0.01f;
        [HideInInspector] [UdonSynced] public bool lineMode = false;
        [UdonSynced] private uint revision = 0;
        [HideInInspector] public Color initialColor = Color.white;
        private uint lastRevision = 0;
        private byte syncCount = 0;
        private Transform penContainer;
        private LineRenderer previewLine;
        private Material colorIndicator;
        private bool skipLog = false;

        private void log(string value)
        {
            if (!skipLog) Debug.Log("[<color=#aaff55>PenSyncData</color>] " + value);
        }

        void Start()
        {
            log("Starting");
            penContainer = transform.parent.Find("PenObject");
            previewLine = penContainer.Find("Tip").GetComponent<LineRenderer>();
            if (colorIndicator == null)
                colorIndicator = penContainer.Find("ActiveColor").GetComponent<MeshRenderer>().material;
            if (Networking.IsMaster) gameObject.SetActive(false);
        }

        new void OnPreSerialization()
        {
            revision++;
            log("Sending Data " + revision);
        }

        new void OnDeserialization()
        {
            log("Data received " + revision);
            if (lastRevision != revision)
            {
                // enforce color sync
                if (colorIndicator.color != color)
                    colorIndicator.color = color;

                if (previewLine.startColor != color)
                    previewLine.startColor = previewLine.endColor = color;

                // enforce brush size sync
                if (previewLine.startWidth != brushWidth)
                    previewLine.startWidth = previewLine.endWidth = brushWidth;

                if (syncCount >= 2 && revision - lastRevision < 10)
                {
                    // sync buffer has been cleared, accounting for up to 2 seconds of packet loss
                    syncCount = 0;
                    gameObject.SetActive(false);
                    log("Halting Sync Data");
                }
                syncCount++;
                lastRevision = revision;
            }
        }

        public void PropagateData()
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                log("Accepting Sync Data");
                gameObject.SetActive(true);
                syncCount = 0;
            }
        }

        public void _Reset() {
            brushLOD = 0.015f;
            brushWidth = 0.01f;
            color = initialColor;
            lineMode = false;
        }
    }
}