// © 2015 Mario Lelas
using UnityEngine;

namespace MLSpace
{

    /// <summary>
    /// Script for rotating daylight and turning on / off other lights
    /// </summary>
    public class RotateLight : MonoBehaviour
    {
        /// <summary>
        /// Lights that will turn on / off on night / day.
        /// </summary>
        [Tooltip("Lights that will turn on / off on night / day.")]
        public GameObject[] m_NightLights;      // lights to be turn on on night

        /// <summary>
        /// Day light rotation speed.
        /// </summary>
        [Tooltip ("Day light rotation speed.")]
        public float rotateSpeed = 1.0f;

        [Tooltip ("Hour in which the light/s will turn on")]
        [Range (0,24)]
        public int timeToLightsOn = 22;

        [Tooltip("Hour in which the light/s will turn off")]
        [Range (0,24)]
        public float timeToLightsOff = 6;

        private Light m_SunLight;                   // main sun light
        private Vector3 m_rotation;                 // light rotation
        private bool m_NightLightsEnabled = false;  // night light enabled flag
        private bool m_afternoon = false;           // flags second half of day after 12:00 hours
        private bool m_initialized = false;         // is component initialized ?



        /// <summary>
        /// initialize component method
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            if (m_initialized) return;

            m_SunLight = GetComponentInChildren<Light>();
            if (!m_SunLight) { Debug.LogError("object cannot be null"); return; }

            m_rotation = transform.rotation.eulerAngles;
            for (int i = 0; i < m_NightLights.Length; i++)
            {
                if(m_NightLights [i]!=null)
                    m_NightLights[i].SetActive(false);
            }

            m_initialized = true;
        }

        // unity start method
        void Start()
        {
            Initialize();
        }

        // unity update method
        void Update()
        {
            if (!m_initialized)
            {
                Debug.LogError("component not initialized.");
                return;
            }


            m_rotation.x += rotateSpeed * Time.deltaTime;
            m_rotation.x = wrapAngle360(m_rotation.x);

            transform.rotation = Quaternion.Euler(m_rotation);
            float dot = Vector3.Dot(Vector3.down, transform.forward);

            if (dot < -0.99f)
                m_afternoon = false;
            if (dot > 0.99f)
                m_afternoon = true;

            float time = (dot + 1.0f) * 0.5f;
            time = time * 12.0f;
            if (m_afternoon) time = 24f - time;

            float att = Mathf.Clamp01(dot);
            m_SunLight.intensity = att;

            if (timeToLightsOff == timeToLightsOn) return;

            int round = (int)time;
            if (round == timeToLightsOn)
                EnableNightLights();
            if (round == timeToLightsOff)
                DisableNightLights();
        }
        

        // enable night lights
        private void EnableNightLights()
        {
            if (m_NightLightsEnabled) return;

            for (int i = 0; i < m_NightLights.Length; i++)
            {
                if (m_NightLights[i].transform.parent)
                {
                    MeshRenderer meshrend = m_NightLights[i].transform.parent.GetComponent<MeshRenderer>();
                    if (meshrend)
                    {
                        bool has = meshrend.sharedMaterial.HasProperty("_Emission");
                        if(has)meshrend.sharedMaterial.SetColor("_Emission", Color.white);
                    }
                }
                m_NightLights[i].SetActive(true);
            }
            m_NightLightsEnabled = true;
        }

        // disable night lights
        private void DisableNightLights()
        {
            if (!m_NightLightsEnabled) return;

            for (int i = 0; i < m_NightLights.Length; i++)
            {
                if(m_NightLights [i].transform .parent )
                {
                    MeshRenderer meshrend = m_NightLights[i].transform.parent.GetComponent<MeshRenderer>();
                    if(meshrend )
                    {
                        bool has = meshrend.sharedMaterial.HasProperty("_Emission");
                        if (has) meshrend.sharedMaterial.SetColor("_Emission", Color.black);
                    }
                }
                m_NightLights[i].SetActive(false);
            }
            m_NightLightsEnabled = false;
        }

        // wrap angle between 0 to 360 degrees
        private float wrapAngle360(float angle)
        {
            float newAngle = angle;
            if (angle > 360)
                newAngle -= 360;
            if (angle < 0)
                newAngle += 360;
            return newAngle;
        }
    }
    
}