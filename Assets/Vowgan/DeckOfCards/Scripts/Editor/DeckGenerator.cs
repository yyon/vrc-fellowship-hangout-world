using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

namespace Vowgan.DeckOfCards
{
    public class DeckGenerator : EditorWindow
    {

        public Transform DeckParent;
        public DeckManager Manager;


        [MenuItem("Tools/Vowgan/Deck Generator")]
        public static void ShowWindow()
        {
            EditorWindow win = GetWindow<DeckGenerator>("Deck Generator");
            win.minSize = new Vector2(50, 50);
            win.Show();
        }

        private void OnGUI()
        {
            DeckParent = (Transform)EditorGUILayout.ObjectField("Deck Parent", DeckParent, typeof(Transform), true);
            Manager = (DeckManager)EditorGUILayout.ObjectField("Manager", Manager, typeof(DeckManager), true);

            if (GUILayout.Button("Generate Deck"))
            {
                foreach (Transform cardTransform in DeckParent)
                {
                    GameObject card = cardTransform.gameObject;
                    Rigidbody rb = card.GetOrAddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                    card.GetOrAddComponent<VRCPickup>();

                    VRCObjectSync objectSync = card.GetOrAddComponent<VRCObjectSync>();
                    objectSync.AllowCollisionOwnershipTransfer = false;

                    CardLogic cardLogic = card.GetOrAddComponent<CardLogic>();
                    cardLogic.DeckManager = Manager;

                    BoxCollider boxCollider = card.GetOrAddComponent<BoxCollider>();
                    boxCollider.center = new Vector3(0, 0.001f, 0);
                    boxCollider.size = new Vector3(0.14f, 0.002f, 0.2f);
                }
            }
        }
    }
}