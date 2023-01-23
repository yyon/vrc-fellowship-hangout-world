// © 2015 Mario Lelas
using UnityEngine;
using System.Collections;

namespace MLSpace
{

    /// <summary>
    /// Camera controller script
    /// </summary>
    public class CameraScript : MonoBehaviour
    {
        /// <summary>
        /// Speed of camera movement
        /// </summary>
        [Tooltip("Speed of camera movement.")]
        public float speed = 2.0f;            
  
        /// <summary>
        /// Speed of camera rotation
        /// </summary>
        [Tooltip ("Speed of camera rotation.")]
        public float angularSpeed = 100.0f;

        private float m_totalXAngleDeg = 0;     // accumulated camera rotation on x axis
        private float m_totalYAngleDeg = 0;     // accumulated camera rotation on y axis

#if UNITY_IOS || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8 || UNITY_WP8_1
        private enum Move { None, Forward, Backward, Left, Right };
        Move m_move = Move.None;

        public void OnBtnForwDown()     { m_move = Move.Forward; }
        public void OnBtnBackDown()     { m_move = Move.Backward; }
        public void OnBtnRightDown()    { m_move = Move.Right; }
        public void OnBtnLeftDown()     { m_move = Move.Left; }
        public void BtnStop()           { m_move = Move.None; }
#endif

        // Use this for initialization
        void Start()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            m_totalXAngleDeg = euler.x;
            m_totalYAngleDeg = euler.y;
            transform.rotation = Quaternion.Euler(euler.x, euler.y, 0); // straighten camera

        }



        // update camera transform
        void FixedUpdate()
        {
#if UNITY_IOS || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8 || UNITY_WP8_1

            switch (m_move)
            {
                case Move.Forward :
                    {
                        float f = speed * Time.deltaTime;
                        transform.position += transform.forward * f;
                    }
                    break;
                case Move.Backward :
                    {
                        float f = speed * Time.deltaTime;
                        transform.position -= transform.forward * f;
                    }
                    break;
                case Move.Right :
                    {
                        float currentAngleY = Time.deltaTime * angularSpeed;
                        m_totalYAngleDeg += currentAngleY;

                        Quaternion rotation =
                            Quaternion.Euler
                            (
                                m_totalXAngleDeg,
                                m_totalYAngleDeg,
                                0
                            );
                        transform.rotation = rotation;
                    }
                    break;
                case Move.Left:
                    {
                        float currentAngleY = Time.deltaTime * angularSpeed;
                        m_totalYAngleDeg -= currentAngleY;

                        Quaternion rotation =
                            Quaternion.Euler
                            (
                                m_totalXAngleDeg,
                                m_totalYAngleDeg,
                                0
                            );
                        transform.rotation = rotation;
                    }
                    break;
            }
#else
            if (!Input.GetMouseButton(0)) 
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return; 
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;


            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            float angleAroundY = Input.GetAxisRaw("Mouse X");
            float angleAroundX = -Input.GetAxisRaw("Mouse Y");

            float f = v * speed * Time.deltaTime;
            float s = h * speed * Time.deltaTime;

            transform.position += transform.forward * f;
            transform.position += transform.right * s;



            float currentAngleY = angleAroundY * Time.deltaTime * angularSpeed;
            float currentAngleX = angleAroundX * Time.deltaTime * angularSpeed;


            m_totalXAngleDeg += currentAngleX;
            m_totalYAngleDeg += currentAngleY;

            Quaternion rotation =
                Quaternion.Euler
                (
                    m_totalXAngleDeg,
                    m_totalYAngleDeg,
                    0
                );
            transform.rotation = rotation;
#endif
        }
    } 
}
