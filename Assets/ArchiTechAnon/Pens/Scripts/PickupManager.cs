
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace ArchiTech
{
    [RequireComponent(typeof(VRC_Pickup))]
    public class PickupManager : UdonSharpBehaviour
    {
        [Tooltip("Receives the following custom events: OnOwnerInteract, OnPreventedInteract, OnOwnerPickup, OnPreventedPickup, OnOwnerDrop, OnRespawn, OnOwnerAssigned, OnOwnerRevoked")]
        public UdonSharpBehaviour eventTarget;
        private int ownerId = -1;
        private Vector3 spawnPos;
        private Quaternion spawnRot;
        private Vector3 lastPos;
        private Quaternion lastRot;
        private PoolManager manager;
        private VRC_Pickup pickup;
        [Tooltip("If object is being managed by a PoolManager instance, this value implicitly uses the manager's value.")]
        public bool allowTheftWhileDropped = false;
        [Tooltip("If object is being managed by a PoolManager instance, this value implicitly uses the manager's value.")]
        public bool allowTheftWhileHeld = false;
        private bool init = false;
        private VRCPlayerApi local;
        [HideInInspector] public bool isHeld = false;
        private bool skipLog;

        private void log(string value)
        {
            if (!skipLog) Debug.Log("[<color=#ccff55>PickupManager</color>] " + value);
        }

        void Start()
        {
            log("Starting " + gameObject.name);
            local = Networking.LocalPlayer;
            pickup = (VRC_Pickup)gameObject.GetComponent(typeof(VRC_Pickup));
            lastPos = spawnPos = transform.position;
            lastRot = spawnRot = transform.rotation;
            init = true;
        }

        new void OnPickup()
        {
            log("Picking Up");
            var owner = Networking.GetOwner(eventTarget.gameObject);
            var isNotOwner = !owner.Equals(local);
            var hasManager = manager != null;
            var alreadyAnOwner = false;
            if (hasManager)
            {
                alreadyAnOwner = manager.GetFirstPlayerOwnedObject() != null;
                allowTheftWhileDropped = manager.allowTheftWhileDropped;
                allowTheftWhileHeld = manager.allowTheftWhileHeld;
            }
            var unowned = ownerId == -1;

            // if manager isn't assigned, pickup is independent and has no object pool management
            // if manager specifies multiple pickups can NOT be owned, check that the player doesn't already own another pickup that the manager controls.
            var canBePickedUp = unowned && (!hasManager || manager.allowOwningMultiple || !alreadyAnOwner);

            // pickup is already owned and is not owned by player and has a manager, and pickup is NOT held and manager allows theft while dropped, or pick IS held and manager allows theft while held
            var theftIsAllowed = isNotOwner && ((!isHeld && allowTheftWhileDropped) || (isHeld && allowTheftWhileHeld));

            if (canBePickedUp || theftIsAllowed)
            {
                if (hasManager)
                {
                    if (alreadyAnOwner && !manager.allowOwningMultiple)
                    {
                        // when one can steal, but not own multiple, the previously owned pickups (if there are any) must be released for others to use.
                        manager.ReleasePersonal();
                    }
                    manager.EnableOthers();
                }
                // no owner yet, assign, or if theft is allowed, switch owners
                AssignOwner(local);
                isNotOwner = false;
            }
            log("Player ID " + local.playerId + " is attempting to pickup object owned by Player ID " + owner.playerId);
            if (isNotOwner)
            {
                log("Preventing Pickup due to non-owner");
                pickup.Drop();
                transform.SetPositionAndRotation(lastPos, lastRot);
                // may or may not be needed, but to ensure network owner is not messed with unexpectedly, force re-assign based on the cached owner id.
                VRCPlayerApi revertOwner = null;
                if (!unowned) revertOwner = VRCPlayerApi.GetPlayerById(ownerId);
                Networking.SetOwner(revertOwner, eventTarget.gameObject);
                eventTarget.SendCustomEvent("OnPreventedPickup");
                log("Prevented pickup");
                return;
            }
            eventTarget.SendCustomEvent("OnOwnerPickup");
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PickedUp));
            log("Picked Up");
        }
        new void OnDrop()
        {
            log("Dropping");
            lastPos = transform.position;
            lastRot = transform.rotation;
            if (ownerId != local.playerId) return;
            eventTarget.SendCustomEvent("OnOwnerDrop");
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Dropped));
        }

        public void PickedUp() {
            isHeld = true;
        }

        public void Dropped() {
            isHeld = false;
        }

        new void OnPlayerJoined(VRCPlayerApi player)
        {
            if (IsOwner(Networking.LocalPlayer))
            {
                // refresh the owner id for each player that joins
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateOwner));
            }
        }

        new void OnPlayerLeft(VRCPlayerApi player)
        {
            if (local.isMaster && ownerId == player.playerId)
            {
                // master takes ownership in order to properly release the object
                log("Resetting Pen after owner has left");
                Networking.SetOwner(local, eventTarget.gameObject);
                ownerId = local.playerId;
                Release();
            }
        }

        public PoolManager GetManager()
        {
            return manager;
        }

        public void SetManager(PoolManager m)
        {
            manager = m;
        }

        public void Respawn()
        {
            transform.SetPositionAndRotation(spawnPos, spawnRot);
            lastPos = spawnPos;
            lastRot = spawnRot;
            eventTarget.SendCustomEvent("OnRespawn");
        }

        public int GetOwner()
        {
            return ownerId;
        }

        public bool IsOwner(VRCPlayerApi player)
        {
            return init && player != null && ownerId == player.playerId;
        }

        public bool IsOwnerId(int id)
        {
            return init && ownerId == id;
        }

        public bool HasOwner() => init && ownerId > -1;

        public void AssignOwner(VRCPlayerApi player)
        {
            Networking.SetOwner(player, eventTarget.gameObject);
            if (manager != null && !manager.allowOwningMultiple)
                manager.DisableOthers();
            eventTarget.SendCustomEvent("OnOwnerAssigned");
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateOwner));
        }

        public void UpdateOwner()
        {
            if (manager != null && !manager.allowOwningMultiple && ownerId == local.playerId)
            {
                // old owner was current player
                manager.EnableOthers();
            }
            ownerId = Networking.GetOwner(eventTarget.gameObject).playerId;
        }

        public void RemoveOwner()
        {
            ownerId = -1;
        }

        public void Disable()
        {
            if (ownerId > -1) return;
            eventTarget.SendCustomEvent("DisablePickup");
        }
        public void Enable()
        {
            if (ownerId > -1) return;
            eventTarget.SendCustomEvent("EnablePickup");
        }

        public void _RemoveOwner()
        {
            if (!IsOwner(local)) return;
            ownerId = -1;
            eventTarget.SendCustomEvent("OnOwnerRevoked");
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(RemoveOwner));
        }

        public void Release()
        {
            if (!IsOwner(local)) return; // only allow owner to release
            eventTarget.SendCustomEvent("OwnerReset");
            eventTarget.SendCustomNetworkEvent(NetworkEventTarget.All, "LocalReset");
            pickup.Drop();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Respawn));
            _RemoveOwner();
            Enable();
        }
    }
}