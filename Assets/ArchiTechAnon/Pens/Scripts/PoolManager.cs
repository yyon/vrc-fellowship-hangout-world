
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace ArchiTech
{
    public class PoolManager : UdonSharpBehaviour
    {
        public GameObject pool;
        public bool allowOwningMultiple = false;
        public bool allowTheftWhileDropped = false;
        public bool allowTheftWhileHeld = false;
        [HideInInspector] public PickupManager[] available;
        private bool skipLog = false;

        private void log(string value)
        {
            if (!skipLog) Debug.Log("[<color=#aacc55>PoolManager</color>] " + value);
        }

        void Start()
        {
            log("Starting " + gameObject.name);
            available = pool.GetComponentsInChildren<PickupManager>();
            log($"Found {available.Length} pickups in the pool {name}");
            foreach (PickupManager obj in available)
            {
                obj.SetManager(this);
            }
            log("Available pool count " + available.Length);
        }

        public PickupManager GetFirstPlayerOwnedObject()
        {
            foreach (PickupManager obj in available)
            {
                if (obj.IsOwner(Networking.LocalPlayer)) return obj;
            }
            return null;
        }

        public PickupManager[] GetAllPlayerOwnedObjects()
        {
            int count = 0;
            foreach (PickupManager obj in available)
            {
                if (obj.IsOwner(Networking.LocalPlayer)) count++;
            }
            var pickups = new PickupManager[count];
            count = 0;
            for (int i = 0; i < available.Length; i++)
            {
                if (available[i].IsOwner(Networking.LocalPlayer))
                {
                    pickups[count] = available[i];
                    count++;
                }
            }
            return pickups;
        }

        new void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == Networking.LocalPlayer) return;
            foreach (PickupManager obj in available)
            {
                if (obj.IsOwner(player))
                {
                    obj.Release();
                }
            }
        }

        public void DisableOthers()
        {
            log("disabling others");
            foreach (PickupManager obj in available)
            {
                if (!obj.IsOwner(Networking.LocalPlayer))
                {
                    obj.Disable();
                }
            }
        }

        public void EnableOthers()
        {
            foreach (PickupManager obj in available)
            {
                if (!obj.IsOwner(Networking.LocalPlayer))
                {
                    obj.Enable();
                }
            }
        }

        public void RespawnPersonal()
        {
            var allOwned = GetAllPlayerOwnedObjects();
            foreach (PickupManager owned in allOwned)
            {
                if (owned != null) owned.Respawn();
            }
        }

        public void ReleasePersonal()
        {
            var allOwned = GetAllPlayerOwnedObjects();
            foreach (PickupManager owned in allOwned)
            {
                if (owned != null) owned.Release();
            }
            EnableOthers();
        }

        public void ReleaseAll()
        {
            if (!Networking.IsMaster) return;
            foreach (PickupManager obj in available)
            {
                obj.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(PickupManager.Release));
            }
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EnableOthers));
        }

        public void CallAll(string eventName)
        {
            foreach (PickupManager obj in available)
            {
                obj.SendCustomEvent(eventName);
            }
        }

        public void _SetProgramVariableAll(string name, object value)
        {
            foreach (PickupManager obj in available)
            {
                obj.SetProgramVariable(name, value);
            }
        }

    }
}
