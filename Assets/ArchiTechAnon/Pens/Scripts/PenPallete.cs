
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArchiTech
{
    public class PenPallete : UdonSharpBehaviour
    {
        public Color uiActive = Color.cyan;
        public Color uiInactive = Color.white;
        public Color uiError = Color.red;
        public Color uiDisabled = Color.gray;
        [Header("OR")]
        [Tooltip("Overrides the default values above. Useful for definining a pallete in a prefab, but allowing the containing scene to override it.")]
        public PenPallete inheritFrom;

        void Start()
        {
            if (inheritFrom != null)
            {
                uiActive = inheritFrom.uiActive;
                uiInactive = inheritFrom.uiInactive;
                uiDisabled = inheritFrom.uiDisabled;
                uiError = inheritFrom.uiError;
            }
        }

        public Color UIActiveWithAlpha(float alpha) => new Color(uiActive.r, uiActive.g, uiActive.b, alpha);
        public Color UIInactiveWithAlpha(float alpha) => new Color(uiInactive.r, uiInactive.g, uiInactive.b, alpha);
        public Color UIErrorWithAlpha(float alpha) => new Color(uiError.r, uiError.g, uiError.b, alpha);
        public Color UIDisabledWithAlpha(float alpha) => new Color(uiDisabled.r, uiDisabled.g, uiDisabled.b, alpha);

    }
}
