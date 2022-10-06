
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

using UnityEngine.UI;

namespace ArchiTech
{
    public class PenController : UdonSharpBehaviour
    {
        public bool showActiveColorOnModel;
        public MeshRenderer model;
        public byte materialSlotForColor;
        private Transform penContainer;
        private VRC_Pickup pickup;
        private BoxCollider box;
        private PickupManager pickupManager;
        private PenSyncData sync;
        [HideInInspector] public bool autoEnable = false;
        // public Material[] matPresets;
        private bool autoEnabled = false; // track state
        private GameObject tip;
        private GameObject joint;
        private LineRenderer previewLine;
        private LineRenderer activeLine;
        [HideInInspector] public GameObject inkPool;
        private Material colorIndicator;
        // [UdonSynced] private int currentMaterial;
        private Color initialColor = Color.white;
        private Color color = Color.white;
        private float brushLOD = 0.015f;
        private float brushWidth = 0.01f;
        private bool lineMode = false;
        private bool jointVisibility = false;
        private SphereCollider eraser;
        private MeshRenderer eraserIndicator;
        private LineRenderer[] lines;
        private LineRenderer[] linesToErase;
        // [UdonSynced]
        private byte drawMode = 0;
        private byte lastDrawMode = 0;
        private bool lastHasOwner = true;
        private Ray penPointer;

        [HideInInspector] public RaycastHit hit;
        // 0 = none, 1 = head, 2 = left hand, 3 = right hand
        [HideInInspector] public byte hitSource = 0;
        [HideInInspector] public bool hitFound;
        private Transform cursor;
        private LineRenderer cursorLine;
        private PenUnifiedMenu unifiedMenu;
        private bool skipMenu = false;
        private uint firstSyncRemaining = 0;
        private bool skipForcedInitialLine = false;
        private bool skipLog = false;
        private Vector3 pointerDir = new Vector3(0.75f, 0f, 1f);
        private const byte IDLE = 0;
        private const byte DRAW = 1;
        private const byte ERASE = 2;

        private void log(string value)
        {
            if (!skipLog) Debug.Log("[<color=#aaff55>PenController</color>] " + value);
        }

        void Start()
        {
            log("Starting " + gameObject.name);
            penContainer = transform.Find("PenObject");
            pickupManager = penContainer.GetComponent<PickupManager>();
            sync = transform.Find("PenSync").GetComponent<PenSyncData>();
            pickup = (VRC_Pickup)penContainer.GetComponent(typeof(VRC_Pickup));
            box = penContainer.GetComponent<BoxCollider>();
            tip = penContainer.Find("Tip").gameObject;
            joint = penContainer.Find("Joint").gameObject;
            inkPool = transform.Find("InkPool").gameObject;
            eraser = penContainer.Find("Eraser").GetComponent<SphereCollider>();
            eraserIndicator = eraser.transform.Find("Visual").GetComponent<MeshRenderer>();
            if (showActiveColorOnModel && model != null)
            {
                colorIndicator = model.materials[materialSlotForColor];
                // penContainer.Find("ActiveColor").gameObject.SetActive(false);
            }
            else
            {
                var c = penContainer.Find("ActiveColor");
                c.gameObject.SetActive(true);
                colorIndicator = c.GetComponent<MeshRenderer>().material;
            }
            sync.SetProgramVariable("colorIndicator", colorIndicator);

            unifiedMenu = transform.Find("PenMenu").GetComponent<PenUnifiedMenu>();
            // colorMenu = menuContainer.Find("ColorMenu").GetComponent<PenColorMenu>();
            // brushMenu = menuContainer.Find("BrushMenu").GetComponent<PenBrushMenu>();
            cursor = penContainer.Find("Cursor");
            cursorLine = cursor.GetComponent<LineRenderer>();
            cursor.gameObject.SetActive(false);
            cursorLine.enabled = Networking.LocalPlayer != null && Networking.LocalPlayer.IsUserInVR();

            // historyMenu = menuContainer.Find("HistoryMenu").GetComponent<PenHistoryMenu>();
            // settingsMenu = menuContainer.Find("SettingsMenu").GetComponent<PenSettingsMenu>();
            lines = new LineRenderer[100];
            linesToErase = new LineRenderer[100];
            eraserIndicator.enabled = false;
            initialColor = sync.initialColor = sync.color = color = generateColor();
            unifiedMenu._LoadColor(color);
            unifiedMenu.activeColor = colorIndicator.color = color;
            inkPool.SetActive(false);
            previewLine = tip.GetComponent<LineRenderer>();
            lineDefaults(previewLine);
            activeLine = newLine();
            unifiedMenu._Close();
            updatePenPointer();
        }

        private LineRenderer newLine()
        {
            GameObject obj = VRCInstantiate(tip);
            obj.transform.SetParent(inkPool.transform);
            obj.name = "InkLine";
            LineRenderer line = obj.GetComponent<LineRenderer>();
            lineDefaults(line);
            newJoint(line, 0);
            newJoint(line, 1);
            lines = appendLine(lines, line);
            log("New Line Created");
            return line;
        }

        private void newJoint(LineRenderer line, int position)
        {
            if (line == previewLine) return; // do not make a joint on the preview line
            GameObject obj = VRCInstantiate(joint);
            obj.name = "InkJoint";
            var t = obj.transform;
            t.SetParent(line.transform);
            t.position = line.GetPosition(position);
            var s = brushWidth * 0.75f;
            t.localScale = new Vector3(s, s, s);
            var mesh = t.Find("Visual").GetComponent<MeshRenderer>();
            mesh.material.color = flipColor(color, false);
            mesh.enabled = jointVisibility;
            obj.SetActive(true);
        }

        private void lineDefaults(LineRenderer line)
        {
            line.enabled = false;
            line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = brushWidth;
            line.positionCount = 2;
            line.SetPosition(0, tip.transform.position);
            line.SetPosition(1, tip.transform.position);
        }

        private void updatePenPointer()
        {
            var local = Networking.LocalPlayer;
            if (Utilities.IsValid(local))
            {
                if (local.IsUserInVR())
                {
                    VRCPlayerApi.TrackingData hand;
                    Vector3 posOffset = Vector3.zero;
                    // better aiming, requires tracking scale data not yet exposed
                    // Vector3 posOffset = new Vector3(0.03f, 0f, 0.085f);
                    // pointerDir = new Vector3(0.7f, 0f, 0.85f);
                    if (pickup.currentHand == VRC_Pickup.PickupHand.Left)
                    {
                        hand = local.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                        hitSource = 2;
                    }
                    else
                    {
                        hand = local.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                        hitSource = 3;
                    }
                    penPointer = new Ray(hand.position + hand.rotation * posOffset, hand.rotation * pointerDir);
                }
                else
                {
                    var head = local.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                    penPointer = new Ray(head.position, head.rotation * Vector3.forward);
                    hitSource = 1;
                }
            }
        }

        private Color generateColor() => Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0.25f, 1f), Random.Range(0.25f, 1f));

        public void OnPreventedPickup()
        {
            // do not yet allow the pen owner to hide their own drawings.
            if (pickupManager.IsOwner(Networking.LocalPlayer)) return;
            inkPool.SetActive(!inkPool.activeSelf);
            if (!inkPool.activeSelf)
            {
                // ClearPen();
            }
            else
            {
                previewLine.SetPosition(0, tip.transform.position);
                previewLine.SetPosition(1, tip.transform.position);
            }
            UpdateLabel();
        }

        public void OnPreventedInteract()
        {
            OnPreventedPickup();
        }

        public void OnOwnerAssigned()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ALL_OwnerAssigned));
            Networking.SetOwner(Networking.LocalPlayer, sync.gameObject);
            log("You have taken ownership of " + gameObject.name);
        }

        public void ALL_OwnerAssigned()
        {
            EnablePickup();
            EnableSync();
            UpdateLabel();
            inkPool.SetActive(true);
        }

        public void OnOwnerRevoked()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ALL_OwnerRevoked));
            log("Your ownership of " + gameObject.name + " has been removed");
        }

        public void ALL_OwnerRevoked()
        {
            DisableSync();
            UpdateLabel();
        }

        public void OnOwnerPickup()
        {
            if (unifiedMenu._IsOpen()) DisablePickup();
            EnableSync();
            sync.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(sync.PropagateData));
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdateLabel));
        }

        public void OnOwnerDrop()
        {
            OnPickupUseUpImmediate();
            EnablePickup();
            DisableSync();
        }

        public void OnRespawn()
        {
            unifiedMenu._Close();
            EnablePickup();
        }

        public void ALL_DrawStart()
        {
            drawMode = DRAW;
        }

        public void ALL_EraseStart()
        {
            drawMode = ERASE;
        }

        public void ALL_DrawEnd()
        {
            drawMode = IDLE;
        }

        public void OnPickupUseUpImmediate()
        {
            // Skip if a menu is open
            if (unifiedMenu._IsOpen()) unifiedMenu.OnPickupUseImmediate();
            skipForcedInitialLine = false;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ALL_DrawEnd));
        }

        public void OnPickupUseDownImmediate()
        {
            if (unifiedMenu._IsOpen()) return;
            skipMenu = false;
            if (isEraserMode())
            { // engage erasing
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ALL_EraseStart));
            }
            else
            { // engage drawing
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ALL_DrawStart));
            }
        }

        public void OnPickupUse1()
        {
            // if menu is open skip normal operation and forward the event onto the menu.
            // if (unifiedMenu._IsOpen()) unifiedMenu.OnPickupUseImmediate();
        }

        public void OnPickupUse1Hold()
        {
            if (!lineMode && !skipForcedInitialLine && drawMode == DRAW)
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ALL_ForceInitialLine));
        }

        public void ALL_ForceInitialLine() => makeSegment();

        public void OnPickupUse2()
        {
            var menuIsOpen = unifiedMenu._IsOpen();
            if (isEraserMode()) // force erase entire drawing history
            {
                if (menuIsOpen) return; // disable when UI is open
                log("Clearing pen");
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ClearPen));
            }
            else // drawing mode swap between dots and lines
            {
                // if (skipMenu) return;
                if (menuIsOpen)
                {
                    unifiedMenu._Close();
                    EnablePickup();
                }
                else
                {
                    OnPickupUseUpImmediate();
                    DisablePickup();
                    unifiedMenu._Open(this);
                }
            }
        }

        public void _OnMenuClose()
        {
            log("Updating Options");
            sync.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(sync.PropagateData));
            EnablePickup();

            color = sync.color = unifiedMenu.activeColor;
            brushLOD = sync.brushLOD = unifiedMenu.lod;
            brushWidth = sync.brushWidth = unifiedMenu.width;
            lineMode = sync.lineMode = unifiedMenu.lineMode;
            jointVisibility = unifiedMenu.jointVisibility; // do not sync, is only locally needed

            colorIndicator.color = previewLine.startColor = previewLine.endColor = color;
            previewLine.startWidth = previewLine.endWidth = brushWidth;
        }

        public void ClearPen()
        {
            var root = inkPool.transform;
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                child.GetComponent<LineRenderer>().enabled = false; // hide
                Destroy(child.gameObject); // remove
            }
            lines = new LineRenderer[100];
            activeLine = previewLine; // active line must never be null, but we don't want to create an entirely new line just yet
        }

        public void LocalReset()
        {
            unifiedMenu._Reset();
            colorIndicator.color = unifiedMenu.activeColor = initialColor;
            unifiedMenu._LoadColor(initialColor);
            ClearPen();
            EnablePickup();
            autoEnabled = false;
        }

        public void OwnerReset()
        {
            sync._Reset();
            previewLine.startWidth = previewLine.endWidth = 0.01f;
            previewLine.startColor = previewLine.endColor = initialColor;
            colorIndicator.color = initialColor;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ALL_DrawEnd));
        }

        public void EnablePickup()
        {
            box.enabled = true;
        }
        public void DisablePickup()
        {
            box.enabled = false;
        }

        public void EnableSync()
        {
            log("Enabling Sync Data");
            sync.gameObject.SetActive(true);
        }

        public void DisableSync()
        {
            log("Disabling Sync Data");
            sync.gameObject.SetActive(false);
        }

        void Update()
        {
            var owner = pickupManager.GetOwner();
            var hasOwner = owner > -1;
            var isOwner = Networking.LocalPlayer != null && Networking.LocalPlayer.playerId == owner;
            var menuIsOpen = unifiedMenu._IsOpen();
            if (inkPool.activeSelf)
            {
                if (drawMode == IDLE && lastDrawMode == DRAW)
                {
                    // track draw mode change for OnPickupUseUp trigger
                    var pStart = previewLine.GetPosition(0);
                    var pEnd = previewLine.GetPosition(1);
                    // linemode skips segment creation normally, finalize line with a single segment.
                    if (lineMode && Vector3.Distance(pStart, pEnd) >= brushLOD) makeSegment();
                    previewLine.enabled = false;
                    activeLine.SetPosition(activeLine.positionCount - 1, pEnd);
                    // update the position of the last joint to align properly
                    activeLine.transform.GetChild(activeLine.positionCount - 1).position = pEnd;
                }
                else if (drawMode == DRAW && lastDrawMode == IDLE)
                {
                    // drawing has just been activated, setup position 0 to the tip's position
                    // otherwise init new line at this point
                    activeLine = newLine();
                    previewLine.enabled = true;
                    previewLine.SetPosition(0, tip.transform.position);
                }
                else if (drawMode == ERASE && lastDrawMode == IDLE)
                {
                    // erasing has been activated
                    eraserIndicator.enabled = true;
                    eraserIndicator.material.color = flipColor(color, false);
                }
                else if (drawMode == IDLE && lastDrawMode == ERASE)
                {
                    // eraser has been released, clear lines that were intersected
                    eraserIndicator.enabled = false;
                    eraseLines();
                    // positions = new Vector3[0];
                }
                if (drawMode == DRAW)
                { // drawing mode
                  // keep the endpoint of the current line updated
                    previewLine.SetPosition(1, tip.transform.position);
                }

                lastDrawMode = drawMode;
            }
            if (autoEnable && !autoEnabled && hasOwner)
            { // change for autoEnable state
                autoEnabled = true;
                inkPool.SetActive(true);
                UpdateLabel();
            }

            if (inkPool.activeSelf && !hasOwner)
            { // check for missing owner and enforce ink disable
                inkPool.SetActive(false);
            }
            else if (!inkPool.activeSelf && isOwner)
            { // check for player ownership and enforce ink enable
                inkPool.SetActive(true);
            }

            if (pickupManager.isHeld && 
            menuIsOpen && Physics.Raycast(penPointer, out hit, 3f) && hit.collider != null)
            {
                if (!cursor.gameObject.activeSelf)
                    cursor.gameObject.SetActive(true);
                hitFound = true;
                cursor.position = hit.point;
                cursor.rotation = hit.collider.transform.rotation;
                if (!cursorLine.enabled && Networking.LocalPlayer.IsUserInVR()) cursorLine.enabled = true;
                if (cursorLine.enabled)
                {
                    cursorLine.SetPosition(0, penPointer.origin);
                    cursorLine.SetPosition(1, hit.point);
                }

                if (Vector3.Distance(Networking.LocalPlayer.GetPosition(), unifiedMenu.transform.position) > 2f)
                {
                    unifiedMenu._Close();
                    return;
                }
                if (hit.collider == unifiedMenu._GetActiveMenu())
                {
                    if (!cursor.gameObject.activeSelf)
                        cursor.gameObject.SetActive(true);
                    unifiedMenu._PointerFocus();
                }
                else
                {
                    if (cursor.gameObject.activeSelf)
                        cursor.gameObject.SetActive(false);
                    unifiedMenu._PointerBlur();
                }

            }
            else
            {
                hitFound = false;
                if (cursor.gameObject.activeSelf)
                    cursor.gameObject.SetActive(false);
                if (menuIsOpen)
                    unifiedMenu._PointerBlur();
            }


            // enforce label sync
            // optimized form of: (hasOwner && pickup.InteractionText == "Claim") || (!hasOwner && pickup.InteractionText != "Claim")
            if (hasOwner != lastHasOwner)
            {
                lastHasOwner = hasOwner;
                UpdateLabel();
            }
        }

        void LateUpdate()
        {
            updateSyncedData();
            updatePenPointer();
            var rootOwner = Networking.GetOwner(gameObject);
            if (rootOwner != Networking.GetOwner(sync.gameObject))
            {
                // ensure the owner of the pen, owns the sync data as well.
                Networking.SetOwner(rootOwner, sync.gameObject);
            }
            if (inkPool.activeSelf)
            {
                if (drawMode == DRAW)
                {
                    var pStart = previewLine.GetPosition(0);
                    var pEnd = previewLine.GetPosition(1);
                    if (!lineMode && Vector3.Distance(pStart, pEnd) >= brushLOD)
                    {
                        skipForcedInitialLine = true;
                        skipMenu = true;
                        makeSegment();
                    }
                }
                // if (drawMode == ERASE)
                // { // eraser mode

                    // RaycastHit[] hits = Physics.SphereCastAll(
                    //     eraser.transform.position,
                    //     eraser.transform.lossyScale.z * eraser.radius,
                    //     Vector3.one, 0f, joint.layer
                    // );

                    // for (int i = 0; i < hits.Length; i++)
                    // {
                    //     Transform t = hits[i].transform.parent;
                    //     if (Utilities.IsValid(t) && t.name == "InkLine")
                    //     {
                    //         log("Line Transform is valid, checking cache");
                    //         var index = t.GetSiblingIndex();
                    //         if (Utilities.IsValid(lines[index]))
                    //         {
                    //             log("Line Is Valid, Marking for Erase: Line " + index);
                    //             LineRenderer line = t.GetComponent<LineRenderer>();
                    //             line.startColor = line.endColor = flipColor(line.startColor, false);
                    //             linesToErase = appendLine(linesToErase, line);
                    //             lines[index] = null;
                    //             for (int j = 0; j < t.childCount; j++)
                    //             {
                    //                 t.GetChild(j).gameObject.SetActive(false);
                    //             }
                    //         }
                    //     }
                    // }

                    // TODO: try switching this to using OverlapSphere against a SHITLOAD of zero dimension colliders on each individual line point
                    // var ce = eraser.transform.TransformPoint(eraser.center);
                    // var r = eraser.transform.lossyScale.z * eraser.radius;
                    // for (var i = 0; i < lines.Length; i++)
                    // {
                    //     if (lines[i] == null) continue;
                    //     Vector3[] positions = new Vector3[lines[i].positionCount];
                    //     lines[i].GetPositions(positions);
                    //     for (var j = 0; j < positions.Length; j++)
                    //     {
                    //         if (Vector3.Distance(positions[j], ce) <= r)
                    //         {
                    //             lines[i].startColor = lines[i].endColor = flipColor(lines[i].startColor, false);
                    //             linesToErase = appendLine(linesToErase, lines[i]);
                    //             lines[i] = null;
                    //             break;
                    //         }
                    //     }
                    // }
                // }
            }
        }

        public void OnEraserCollision(Collider other)
        {
            if (drawMode != ERASE) return;
            log("Draw mode is ERASE");
            Transform t = other.transform.parent;
            if (Utilities.IsValid(t) && t.name == "InkLine")
            {
                log("Line Transform is valid, checking cache");
                var index = t.GetSiblingIndex();
                if (Utilities.IsValid(lines[index]))
                {
                    log("Line Is Valid, Marking for Erase: Line " + index);
                    LineRenderer line = t.GetComponent<LineRenderer>();
                    line.startColor = line.endColor = flipColor(line.startColor, false);
                    linesToErase = appendLine(linesToErase, line);
                    lines[index] = null;
                    for (int i = 0; i < t.childCount; i++)
                    {
                        t.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
        }

        private void updateSyncedData()
        {
            brushLOD = sync.brushLOD;
            brushWidth = sync.brushWidth;
            color = sync.color;
            lineMode = sync.lineMode;
        }

        private void makeSegment()
        {
            var pStart = previewLine.GetPosition(0);
            var pEnd = previewLine.GetPosition(1);
            var aStart = activeLine.GetPosition(activeLine.positionCount - 1);
            var aEnd = activeLine.GetPosition(activeLine.positionCount - 2);
            var aRay = aStart - aEnd;
            var pRay = pStart - pEnd;
            if (Vector3.Angle(aRay, pRay) < 120f)
            { // line angle bend is too much, close current line and start new one
                activeLine = newLine(); // make new line instance
                                        // set first segment (0,1) to where the preview is currently
                activeLine.SetPosition(0, pStart);
                activeLine.SetPosition(1, pEnd);
            }
            else
            { // line angle bend is NOT too much, update existing line with new segment
                if (activeLine.enabled) activeLine.positionCount += 1; // add new position if line has already been enabled
                activeLine.SetPosition(activeLine.positionCount - 1, pEnd); // set latest pos to the preview's end point
                newJoint(activeLine, activeLine.positionCount - 1);
            }
            activeLine.enabled = true;
            // Update the preview position for next segment
            previewLine.SetPosition(0, pEnd);
        }

        private float calculateAvatarHeight(VRCPlayerApi player)
        {
            var head = player.GetBonePosition(HumanBodyBones.Head);
            var neck = player.GetBonePosition(HumanBodyBones.Neck);
            var spine = player.GetBonePosition(HumanBodyBones.Spine);
            var hip = player.GetBonePosition(HumanBodyBones.Hips);
            var knee = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
            var foot = player.GetBonePosition(HumanBodyBones.RightFoot);
            var height =
                  Vector3.Distance(head, neck)
                + Vector3.Distance(neck, spine)
                + Vector3.Distance(spine, hip)
                + Vector3.Distance(hip, knee)
                + Vector3.Distance(knee, foot);
            return height;
        }

        // calculate whether tip or eraser is forward.
        private bool isEraserMode()
        {
            // use the head direction for desktop, the currently held hand for VR
            Vector3 looking = penContainer.forward;
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                looking = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward;
            }
            else if (pickup.currentHand == VRC_Pickup.PickupHand.Right)
            {
                looking = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation * new Vector3(0.75f, -1f, 1f);
            }
            else if (pickup.currentHand == VRC_Pickup.PickupHand.Left)
            {
                looking = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation * new Vector3(0.75f, 1f, 1f);
            }
            return Vector3.Angle(penContainer.forward, looking) > 90;
        }


        public void UpdateLabel()
        {
            var ownerId = pickupManager.GetOwner();
            var owner = VRCPlayerApi.GetPlayerById(ownerId);
            if (ownerId == -1 || owner == null)
            {
                pickup.InteractionText = "Claim";
            }
            else if (ownerId == Networking.LocalPlayer.playerId)
            {
                pickup.InteractionText = "Pickup Your Pen";
            }
            else
            {
                var prefix = Networking.LocalPlayer.IsUserInVR() ? "Grab" : "Click";
                pickup.InteractionText = prefix + " to toggle: "
                    + (inkPool.activeSelf ? "(Shown) " : "(Hidden) ")
                    + owner.displayName;
            }
        }

        private void eraseLines()
        {

            if (linesToErase.Length > 0)
            {
                var count = 0;
                for (var i = 0; i < linesToErase.Length; i++)
                {
                    if (linesToErase[i] == null) continue;
                    var line = linesToErase[i];
                    Destroy(line.gameObject);
                    linesToErase[i] = null;
                    count++;
                }
                log("Erased " + count + " lines");
                lines = expandLines(lines, 0); // cleans up the array so all nulls are at the end.
                activeLine = previewLine; // active line must never be null, but we don't want to create an entirely new line just yet
            }
        }

        private LineRenderer[] expandLines(LineRenderer[] arr, int add)
        {
            var newArray = new LineRenderer[arr.Length + add];
            var index = 0;
            for (var i = 0; i < arr.Length; i++)
            {
                if (arr[i] == null) continue;
                newArray[index] = arr[i];
                index++;
            }
            return newArray;
        }

        private LineRenderer[] appendLine(LineRenderer[] lineGroup, LineRenderer line)
        {
            var i = 0;
            for (; i < lineGroup.Length; i++)
            {
                if (lineGroup[i] == null)
                {
                    lineGroup[i] = line;
                    return lineGroup;
                }
            }
            lineGroup = expandLines(lineGroup, 100);
            lineGroup[i] = line;
            return lineGroup;
        }

        private int getLineIndex(LineRenderer[] lineGroup, LineRenderer line)
        {
            for (var i = 0; i < lineGroup.Length; i++)
            {
                if (lineGroup[i].Equals(line)) return i;
            }
            return -1;
        }

        private Color flipColor(Color src, bool withAlphaAdj)
        {
            float h, s, v;
            Color.RGBToHSV(src, out h, out s, out v);
            h = (h + 0.5f) % 1f;
            var o = Color.HSVToRGB(h, 1f, 1f);
            if (withAlphaAdj) o = new Color(o.r, o.g, o.b, 0.99f);
            return o;
        }
    }
}
