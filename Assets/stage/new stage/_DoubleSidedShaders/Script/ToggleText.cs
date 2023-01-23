// © 2015 Mario Lelas
using UnityEngine;
using System.Collections;

namespace MLSpace
{
    public class ToggleText : MonoBehaviour
    {
        private UnityEngine.UI.Text m_textUI;       // text component
        private bool m_showInstr = false;           // show instructions flag
        private bool m_initialized = false;         // is class initialized ?
        

        /// <summary>
        /// initialize component
        /// </summary>
        public void Initialize()
        {
            if (m_initialized) return;
            m_textUI = GetComponent<UnityEngine.UI.Text>();
            if (!m_textUI) { Debug.LogError("cannot find component 'Text'"); return; }
            m_initialized = true;
        }

        // Unity start method
        void Start()
        {
            Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_initialized)
            {
                Debug.LogError("component not initialized.");
                return;
            }

            if (Input.GetKeyDown(KeyCode.F1))
                m_showInstr = !m_showInstr;


            if (m_showInstr)
                ShowInstr();
            else ShowF1();
        }

        // show instructions string
        private void ShowInstr()
        {
            if (!m_textUI)
            {
                Debug.LogError("object cannot be null.");
                return;
            }
            
            m_textUI.text = "Hold Mouse 0 To move" +
                "\nW - forward camera relative" +
                "\nS - backwards camera relative" +
                "\nA - left camera relative" +
                "\nD - right camera relative";
        }

        // show press key string
        private void ShowF1()
        {
            if (!m_textUI)
            {
                Debug.LogError("object cannot be null.");
                return;
            }

            m_textUI.text = "Press F1 to instructions.";
        }
    } 
}
