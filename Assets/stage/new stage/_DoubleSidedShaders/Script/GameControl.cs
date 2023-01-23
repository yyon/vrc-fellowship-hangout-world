// © 2015 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    public class GameControl : MonoBehaviour
    {
        /// <summary>
        /// buttons for mobiles
        /// </summary>
        public GameObject onMobileOnly;

        /// <summary>
        /// text ui
        /// </summary>
        public UnityEngine.UI.Text infoUI;

        // Use this for initialization
        void Start()
        {
            // disable buttons on mobiles
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_BLACKBERRY && !UNITY_WP8 && !UNITY_WP8_1
            if (onMobileOnly) onMobileOnly.SetActive(false);
#endif
        }


        // Update is called once per frame
        void Update()
        {
            // quit on escape
            if (Input.GetKey(KeyCode.Escape))
                Application.Quit();

#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_BLACKBERRY && !UNITY_WP8 && !UNITY_WP8_1
            info();
#endif
        }

        // control information
        private void info()
        {
            if (!infoUI) { return; }



            infoUI.text = "Hold left mouse button to enable camera control." +
                "\nw - forward\ns - back\na - left\nd - right";
        }
    } 
}
