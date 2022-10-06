
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


namespace ArchiTech
{
    public class PenUnifiedMenu : UdonSharpBehaviour
    {
        // GENERAL
        private const float TRIGGER_THRESHOLD = 0.1f;
        // multiply by 0.0111111111111111f ~= 1/360 * 4, used to find the quadrant based on a calculated angle
        private const float QUADRANT_ANGLE_MULTIPLIER = 0.0111111111111111f;
        private const byte MENU_NONE = 0;
        private const byte MENU_MAIN = 1;
        private const byte MENU_COLOR = 2;
        private const byte MENU_BRUSH = 3;
        private const byte MENU_HISTORY = 4;
        private const byte MENU_MISC = 5;
        private PenController pen;
        private Transform penObject;
        private Text debugTxt;
        private byte activeMenu = MENU_NONE;

        // MAIN MENU
        private const float animationSpeed = 7.5f;
        private Collider mainMenu;
        private Transform menuOptionColor;
        private Transform menuOptionBrush;
        private Transform menuOptionMisc;
        // private Transform mainOptionHistory;
        private Transform cursor;
        private LineRenderer cursorRay;
        private Vector3 hoverScale = new Vector3(1.15f, 1.15f, 1f);
        private Vector3 idleScale = Vector3.one;
        private bool openMenu = false;
        private byte focusedQuadrant = MENU_NONE;

        // COLOR MENU
        [HideInInspector] public Color activeColor = Color.white;
        private Collider colorMenu;
        private Image[] pallete;
        private byte palleteIndex = 0;
        private Transform palleteCursorT;
        private RawImage[] hueTargets;
        private Slider hue;
        private RectTransform hueT;
        private Slider saturation;
        private RectTransform saturationT;
        private Slider value;
        private RectTransform valueT;
        private RectTransform fadePreviewT;
        private bool previewUpdate = false;
        private RectTransform previewCursorT;
        private Image previewColor;
        private bool clearPallete = false;

        // BRUSH MENU
        private const float speed = 2f;
        private const float xScale = 65f;
        private const float yScale = 20f;
        private Collider brushMenu;
        private Slider widthModifier;
        private Slider lodModifier;
        [HideInInspector] public float lod = 0.015f;
        [HideInInspector] public float width = 0.01f;
        private TrailRenderer brushPreview;
        private RectTransform brushPreviewT;
        private bool forceClear;
        private Vector3 brushPreviewStartPos = new Vector3(0, -36f, 0);

        // MISC MENU
        private Collider miscMenu;
        [HideInInspector] public bool lineMode = false;
        private Toggle lineModeModifier;
        [System.NonSerialized] public bool jointVisibility = false;
        private Toggle jointVisibilityModifier;

        // INTERNAL
        private bool init = false;
        private bool skipLog = false;

        private void log(string value)
        {
            if (!skipLog) Debug.Log("[<color=#cc77bb>PenUnifiedMenu</color>] " + value);
        }

        void Start()
        {
            log("Starting");
            if (!init) initialize();
        }

        private void initialize()
        {
            initMainMenu();
            initColorMenu();
            initBrushMenu();
            initMiscMenu();
            init = true;
        }

        #region General

        void Update()
        {
            if (activeMenu == MENU_NONE) return;
            if (Utilities.IsValid(Networking.LocalPlayer))
            {
                var t = _GetActiveMenu().transform;
                t.LookAt(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
                t.Rotate(0, 180, 0);
            }
            if (activeMenu == MENU_MAIN) mainMenuUpdate();
            else if (activeMenu == MENU_COLOR) { }
            else if (activeMenu == MENU_BRUSH) brushMenuUpdate();
            else if (activeMenu == MENU_HISTORY) { }
            else if (activeMenu == MENU_MISC) { }
        }

        public void OnPickupUseImmediate()
        {
            if (activeMenu == MENU_MAIN)
                openMenu = true;
        }

        public void _PointerFocus()
        {
            if (pen != null)
            {
                RaycastHit hit = pen.hit;
                if (hit.collider == mainMenu)
                {
                    var localHit = mainMenu.transform.InverseTransformPoint(hit.point);
                    var angle = Vector3.Angle(localHit, Vector3.right * -1);
                    var cross = Vector3.Angle(localHit, Vector3.up);
                    if (cross > 90) angle += 180;
                    focusedQuadrant = (byte)(Mathf.FloorToInt(angle * QUADRANT_ANGLE_MULTIPLIER) + 2);
                    // results in quadrant values 2 through 5, 
                    // starting in upperleft going left-right-top-bottom flow, 
                    // so as to match the related menu identifier constants
                }
                else if (hit.collider == colorMenu)
                {
                    var r = fadePreviewT.rect;
                    var localPoint3 = fadePreviewT.InverseTransformPoint(pen.hit.point);
                    var localPoint = new Vector2(localPoint3.x, localPoint3.y);
                    var normal = Rect.PointToNormalized(r, localPoint);
                    if (r.Contains(localPoint)) previewUpdate = true;
                    if (previewUpdate)
                        switch (pen.hitSource)
                        {
                            case 1:
                                if (Input.GetButton("Fire1"))
                                {
                                    saturation.value = normal.x;
                                    value.value = normal.y;
                                }
                                else previewUpdate = false;
                                break;
                            case 2:
                                if (Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger") > TRIGGER_THRESHOLD)
                                {
                                    saturation.value = normal.x;
                                    value.value = normal.y;
                                }
                                else previewUpdate = false;
                                break;
                            case 3:
                                if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > TRIGGER_THRESHOLD)
                                {
                                    saturation.value = normal.x;
                                    value.value = normal.y;
                                }
                                else previewUpdate = false;
                                break;
                            default: break;
                        }
                }
            }
        }

        public void _PointerBlur()
        {
            if (activeMenu == MENU_MAIN)
            {
                focusedQuadrant = MENU_NONE;
                openMenu = false;
                doLerp(menuOptionColor.transform, idleScale, animationSpeed);
                doLerp(menuOptionBrush.transform, idleScale, animationSpeed);
                doLerp(menuOptionMisc.transform, idleScale, animationSpeed);
                // doLerp(menuOptionHistory.transform, idleScale, animationSpeed);
            }
        }

        public void _Open(PenController pen)
        {
            log("Opening Main Menu");
            this.pen = pen;
            if (!init) initialize();
            _OpenMainMenu();
            updateMenuPosition();
        }

        public void _Close()
        {
            if (activeMenu == MENU_NONE) return;
            _GetActiveMenu().gameObject.SetActive(false);
            openMenu = false;
            lod = lodModifier.value;
            width = widthModifier.value;
            lineMode = lineModeModifier.isOn;
            activeColor = previewColor.color;
            jointVisibility = jointVisibilityModifier.isOn;
            pen._OnMenuClose();
            this.pen = null;
            activeMenu = MENU_NONE;
            log("Menu Closed");
        }

        public void _Reset()
        {
            _Close();
            resetColor();
            resetBrush();
            resetMisc();
            // resetHistory();
        }

        public Collider _GetActiveMenu()
        {
            if (activeMenu == MENU_COLOR) return colorMenu;
            else if (activeMenu == MENU_BRUSH) return brushMenu;
            else if (activeMenu == MENU_MISC) return miscMenu;
            // if (activeMenu == MENU_HISTORY) return historyMenu;
            else return mainMenu;
        }

        public bool _IsOpen()
        {
            return activeMenu != MENU_NONE;
        }

        #endregion

        #region MainMenu

        private void initMainMenu()
        {
            var root = transform.Find("MainMenu");
            mainMenu = root.GetComponent<Collider>();
            menuOptionColor = root.Find("ColorItem");
            menuOptionBrush = root.Find("BrushItem");
            menuOptionMisc = root.Find("MiscItem");
            // mainOptionHistory = transform.Find("HistoryItem");
            penObject = transform.parent.Find("PenObject");
            root.gameObject.SetActive(false);
        }

        private void mainMenuUpdate()
        {
            if (pen.hitFound && pen.hit.collider != mainMenu)
            {
                focusedQuadrant = MENU_NONE;
                openMenu = false;
                doLerp(menuOptionColor.transform, idleScale, animationSpeed);
                doLerp(menuOptionBrush.transform, idleScale, animationSpeed);
                doLerp(menuOptionMisc.transform, idleScale, animationSpeed);
                // doLerp(menuOptionHistory.transform, idleScale, animationSpeed);
            }
            switch (focusedQuadrant)
            {
                case MENU_COLOR: // color
                    if (openMenu)
                    {
                        openMenu = false;
                        focusedQuadrant = MENU_NONE;
                        _ClickColor();
                        return;
                    }
                    doLerp(menuOptionColor.transform, hoverScale, animationSpeed);
                    doLerp(menuOptionBrush.transform, idleScale, animationSpeed);
                    doLerp(menuOptionMisc.transform, idleScale, animationSpeed);
                    // doLerp(menuOptionHistory.transform, idleScale, animationSpeed);
                    break;
                case MENU_BRUSH: // brush
                    if (openMenu)
                    {
                        openMenu = false;
                        focusedQuadrant = MENU_NONE;
                        _ClickBrush();
                        return;
                    }
                    doLerp(menuOptionColor.transform, idleScale, animationSpeed);
                    doLerp(menuOptionBrush.transform, hoverScale, animationSpeed);
                    doLerp(menuOptionMisc.transform, idleScale, animationSpeed);
                    // doLerp(menuOptionHistory.transform, idleScale, animationSpeed);
                    break;
                case MENU_HISTORY: // history
                    if (openMenu)
                    {
                        openMenu = false;
                        focusedQuadrant = MENU_NONE;
                        _ClickHistory();
                        return;
                    }
                    doLerp(menuOptionColor.transform, idleScale, animationSpeed);
                    doLerp(menuOptionBrush.transform, idleScale, animationSpeed);
                    doLerp(menuOptionMisc.transform, idleScale, animationSpeed);
                    // doLerp(menuOptionHistory.transform, hoverScale, animationSpeed);
                    break;
                case MENU_MISC: // misc
                    if (openMenu)
                    {
                        openMenu = false;
                        focusedQuadrant = MENU_NONE;
                        _ClickMisc();
                        return;
                    }
                    doLerp(menuOptionColor.transform, idleScale, animationSpeed);
                    doLerp(menuOptionBrush.transform, idleScale, animationSpeed);
                    doLerp(menuOptionMisc.transform, hoverScale, animationSpeed);
                    // doLerp(menuOptionHistory.transform, idleScale, animationSpeed);
                    break;
                default:
                    openMenu = false;
                    doLerp(menuOptionColor.transform, idleScale, animationSpeed);
                    doLerp(menuOptionBrush.transform, idleScale, animationSpeed);
                    doLerp(menuOptionMisc.transform, idleScale, animationSpeed);
                    // doLerp(menuOptionHistory.transform, idleScale, animationSpeed);
                    break;
            }
        }

        private void doLerp(Transform from, Vector3 to, float sneed)
        {
            from.localScale = Vector3.Lerp(from.localScale, to, Time.deltaTime * sneed);
        }

        public void _OpenMainMenu()
        {
            switch (activeMenu)
            {
                case MENU_COLOR: colorMenu.gameObject.SetActive(false); break;
                case MENU_BRUSH: brushMenu.gameObject.SetActive(false); break;
                case MENU_MISC: miscMenu.gameObject.SetActive(false); break;
                    // case MENU_HISTORY: historyMenu.gameObject.SetActive(false); break;
            }
            log("Opening Main Menu");
            mainMenu.gameObject.SetActive(true);
            activeMenu = MENU_MAIN;
        }

        // These _Click* methods are activated via custom menu open during the update method OR via UI event call.
        public void _ClickColor()
        {
            log("Opening Color Menu");
            activeMenu = MENU_COLOR;
            mainMenu.gameObject.SetActive(false);
            // _LoadColor(activeColor);
            colorMenu.gameObject.SetActive(true);
        }

        public void _ClickBrush()
        {
            log("Opening Brush Menu");
            activeMenu = MENU_BRUSH;
            mainMenu.gameObject.SetActive(false);
            // loadBrush(lod, width);
            brushMenu.gameObject.SetActive(true);
        }

        public void _ClickMisc()
        {
            log("Opening Misc Menu");
            activeMenu = MENU_MISC;
            mainMenu.gameObject.SetActive(false);
            // loadMisc(lineMode);
            miscMenu.gameObject.SetActive(true);
        }

        public void _ClickHistory()
        {
            log("Opening History Menu");
            // activeMenu = MENU_HISTORY;
            mainMenu.gameObject.SetActive(false);
            // loadHistory();
            // historyMenu.gameObject.SetActive(true);
            // updateMenuPosition();
            activeMenu = MENU_NONE;
        }

        private void updateMenuPosition()
        {
            transform.SetPositionAndRotation(penObject.position, Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation);
        }

        #endregion

        #region ColorMenu
        private void initColorMenu()
        {
            var root = transform.Find("ColorMenu");
            colorMenu = root.GetComponent<Collider>();
            hue = root.Find("Hue").GetComponent<Slider>();
            saturation = root.Find("Saturation").GetComponent<Slider>();
            value = root.Find("Value").GetComponent<Slider>();
            fadePreviewT = root.Find("FadePreview").GetComponent<RectTransform>();
            previewCursorT = fadePreviewT.Find("Cursor").GetComponent<RectTransform>();
            previewColor = root.Find("ColorPreview").GetComponent<Image>();
            hueTargets = new RawImage[]{
                saturation.transform.Find("BG").GetComponent<RawImage>(),
                value.transform.Find("BG").GetComponent<RawImage>(),
                fadePreviewT.Find("Render").Find("BG").GetComponent<RawImage>()
            };
            var palleteList = root.Find("Pallete").GetComponentsInChildren<Button>();
            pallete = new Image[palleteList.Length];
            for (int i = 0; i < palleteList.Length; i++)
            {
                pallete[i] = palleteList[i].GetComponent<Image>();
            }
            palleteCursorT = root.Find("Pallete").Find("Cursor");
            root.gameObject.SetActive(false);
        }

        private void resetColor()
        {
            colorMenu.gameObject.SetActive(false);
            foreach (Image item in pallete)
                item.color = Color.white;
            loadPallete(0);
        }

        public void _LoadColor(Color c)
        {
            float h, v, s;
            Color.RGBToHSV(c, out h, out s, out v);
            hue.value = h;
            saturation.value = s;
            value.value = v;
            _UpdateHue();
        }

        public void _UpdateHue()
        {
            for (int i = 0; i < hueTargets.Length; i++)
            {
                hueTargets[i].color = Color.HSVToRGB(hue.value, 1, 1);
            }
            _UpdatePreview();
        }

        public void _UpdatePreview()
        {
            var anchors = new Vector2(saturation.value, value.value);
            previewCursorT.anchorMax = anchors;
            previewCursorT.anchorMin = anchors;
            previewCursorT.anchoredPosition = previewCursorT.sizeDelta = new Vector2(0, 0);
            previewCursorT.ForceUpdateRectTransforms();
            previewColor.color = Color.HSVToRGB(hue.value, saturation.value, value.value);
            pallete[palleteIndex].color = previewColor.color;
        }

        private void loadPallete(byte index)
        {
            palleteIndex = index;
            _LoadColor(pallete[index].color);
            var t = pallete[index].transform;
            palleteCursorT.SetPositionAndRotation(t.position, t.rotation);
        }

        public void _LoadPallete0() => loadPallete(0);
        public void _LoadPallete1() => loadPallete(1);
        public void _LoadPallete2() => loadPallete(2);
        public void _LoadPallete3() => loadPallete(3);
        public void _LoadPallete4() => loadPallete(4);
        public void _LoadPallete5() => loadPallete(5);
        public void _LoadPallete6() => loadPallete(6);
        public void _LoadPallete7() => loadPallete(7);
        public void _LoadPallete8() => loadPallete(8);
        public void _LoadPallete9() => loadPallete(9);

        #endregion

        #region BrushMenu
        private void initBrushMenu()
        {
            var root = transform.Find("BrushMenu");
            brushMenu = root.GetComponent<Collider>();
            lodModifier = root.Find("LODModifier").GetComponent<Slider>();
            widthModifier = root.Find("WidthModifier").GetComponent<Slider>();
            brushPreview = root.Find("TrailPreview").GetComponent<TrailRenderer>();
            brushPreviewT = brushPreview.GetComponent<RectTransform>();
            root.gameObject.SetActive(false);
        }

        private void brushMenuUpdate()
        {
            brushPreviewT.anchoredPosition = brushPreviewStartPos + (
                Vector3.right * Mathf.Sin(Time.timeSinceLevelLoad * 0.5f * speed) * xScale
                - Vector3.up * Mathf.Sin(Time.timeSinceLevelLoad * speed) * yScale
            );
        }

        private void loadBrush(float lod, float width)
        {
            widthModifier.value = width;
            lodModifier.value = lod;
        }

        private void resetBrush()
        {
            brushMenu.gameObject.SetActive(false);
            widthModifier.value = 0.01f;
            lodModifier.value = 0.015f;
        }

        // These two methods are called via UI events
        public void _ModifyWidth()
        {
            brushPreview.startWidth = widthModifier.value;
            brushPreview.endWidth = widthModifier.value;
        }

        public void _ModifyLOD()
        {
            brushPreview.minVertexDistance = lodModifier.value;
        }

        #endregion

        #region MiscMenu
        private void initMiscMenu()
        {
            var root = transform.Find("MiscMenu");
            miscMenu = root.GetComponent<Collider>();
            lineModeModifier = root.Find("LineMode").GetComponent<Toggle>();
            jointVisibilityModifier = root.Find("LineJointVisibility").GetComponent<Toggle>();
            root.gameObject.SetActive(false);
        }

        private void loadMisc(bool lineMode)
        {
            lineModeModifier.isOn = lineMode;
        }

        private void resetMisc()
        {
            lineModeModifier.isOn = false;
        }

        public void _ModifyLineMode()
        {
            lineModeModifier.graphic.gameObject.SetActive(lineModeModifier.isOn);
        }

        public void _ToggleJointVisibility()
        {
            Transform ink = pen.transform.Find("InkPool");
            for (int i = 0; i < ink.childCount; i++) {
                var line = ink.GetChild(i);
                for (int j = 0; j < line.childCount; j++) {
                    var joint = line.GetChild(j);
                    joint.Find("Visual").GetComponent<MeshRenderer>().enabled = jointVisibilityModifier.isOn;
                }
            }
        }


        #endregion

    }
}
