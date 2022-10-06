
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArchiTech
{
    public class PenEraser : UdonSharpBehaviour
    {
        public PenController pen;
        void OnTriggerStay(Collider other) {
            pen.OnEraserCollision(other);
        }
    }
}
