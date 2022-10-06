
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


namespace ArchiTech
{
    [RequireComponent(typeof(Button))]
    public class PenToggle : UdonSharpBehaviour
    {
        public PenPallete pallete;
        private int lastOwnerId = -1;
        private bool lastInkState;
        public PenController pen;
        private GameObject ink;
        private Image indicator;
        private PickupManager pickupManager;
        private Text btnText;
        private bool skipLog = false;
        private string initText;

        private void log(string value)
        {
            if (!skipLog) Debug.Log("[<color=#cc9911>PenToggle</color>] " + value);
        }
        void Start()
        {
            if (pallete == null) pallete = GetComponent<PenPallete>();
            var obj = pen.transform.Find("PenObject");
            pickupManager = obj.GetComponent<PickupManager>();
            ink = pen.transform.Find("InkPool").gameObject;
            btnText = transform.Find("Label").GetComponent<Text>();
            indicator = GetComponent<Image>();
            initText = btnText.text;
        }

        public void ToggleInk()
        {
            pen.OnPreventedPickup();
            UpdateLabel();
        }

        public void UpdateLabel()
        {
            var ownerId = pickupManager.GetOwner();
            if (ownerId == -1)
            {
                btnText.text = initText;
                indicator.color = pallete.UIInactiveWithAlpha(indicator.color.a);
                btnText.resizeTextForBestFit = false;
            }
            else if (ownerId == Networking.LocalPlayer.playerId)
            {
                btnText.text = "YOUR PEN";
                indicator.color = pallete.UIDisabledWithAlpha(indicator.color.a);
                btnText.resizeTextForBestFit = false;
            }
            else
            {
                var pname = VRCPlayerApi.GetPlayerById(ownerId).displayName;
                btnText.text = pname + "\n" + (ink.activeSelf ? "(Shown)" : "(Hidden)");
                indicator.color = ink.activeSelf ?
                    pallete.UIActiveWithAlpha(indicator.color.a)
                    : pallete.UIInactiveWithAlpha(indicator.color.a);
                btnText.resizeTextForBestFit = true;
            }
        }

        void Update()
        {
            var ownerId = pickupManager.GetOwner();
            if (lastOwnerId != ownerId)
            {
                lastOwnerId = ownerId;
                UpdateLabel();
            }
            if (lastInkState != ink.activeSelf)
            {
                lastInkState = ink.activeSelf;
                UpdateLabel();
            }
        }
    }
}