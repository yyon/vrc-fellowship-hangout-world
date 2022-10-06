
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace ArchiTech
{
    public class PenAutoEnableToggle : UdonSharpBehaviour
    {
        public PenPallete pallete;
        public PoolManager pool;
        private Toggle autoEnable;
        private Image indicator;
        private PenController[] pens;
        private bool skipLog = false;

        private void log(string value)
        {
            if (!skipLog) Debug.Log("[<color=#1199cc>PenAutoEnableToggle</color>] " + value);
        }

        void Start()
        {
            log("Starting");
            if (pallete == null) pallete = GetComponent<PenPallete>();
            autoEnable = GetComponent<Toggle>();
            indicator = GetComponent<Image>();
            if (autoEnable.isOn)
            {
                indicator.color = pallete.UIActiveWithAlpha(indicator.color.a);
            }
            else
            {
                indicator.color = pallete.UIInactiveWithAlpha(indicator.color.a);
            }
        }

        private void getPens()
        {
            pens = new PenController[pool.available.Length];
            for (int i = 0; i < pens.Length; i++)
            {
                pens[i] = (PenController)pool.available[i].eventTarget;
            }
        }

        public void UpdateAutoEnable()
        {
            if (pens == null) getPens();
            foreach (PenController pen in pens)
            {
                pen.autoEnable = autoEnable.isOn;
            }
            if (autoEnable.isOn)
            {
                indicator.color = pallete.UIActiveWithAlpha(indicator.color.a);
            }
            else
            {
                indicator.color = pallete.UIInactiveWithAlpha(indicator.color.a);
            }
        }

        // new void OnPlayerJoined(VRCPlayerApi player) {
        //     if (player.isLocal && pool.available.Length > 0) UpdateAutoEnable();
        // }

        public void LateUpdate()
        {
            // once pens are available, run the autoenable check
            if (pens == null && pool.available != null && pool.available.Length > 0) UpdateAutoEnable();
        }
    }
}
