using System.IO;
using UnityEditor;
using UnityEngine;

public class Screenshot : EditorWindow
{

    int resWidth = Screen.width * 4;
    int resHeight = Screen.height * 4;

    public Camera myCamera;
    int scale = 1;
    RenderTexture renderTexture;
    bool isTransparent = true;
    ScreenShotUI UIScript;
    bool useUIRect = false;   
    private readonly string SETTINGS_KEY = "lovatto.screenshot.settings";
    private Texture2D Preview;
    private bool refresh = false;
    public Settings settings;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        UIScript = FindObjectOfType<ScreenShotUI>();
        if (EditorPrefs.HasKey(SETTINGS_KEY))
        {
            settings = JsonUtility.FromJson<Settings>(EditorPrefs.GetString(SETTINGS_KEY));
        }
        else
        {
            settings = new Settings();
            if (UIScript != null)
            {
                if (UIScript.TempCamera != null) { settings.tempFov = UIScript.TempCamera.fieldOfView; }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        EditorPrefs.SetString(SETTINGS_KEY, JsonUtility.ToJson(settings));
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawSettings();
        GUILayout.Space(20);
        if (!useUIRect)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Default Options", EditorStyles.boldLabel);
            if (GUILayout.Button("Set To Screen Size"))
            {
                resHeight = (int)Handles.GetMainGameViewSize().y;
                resWidth = (int)Handles.GetMainGameViewSize().x;
            }
            if (GUILayout.Button("Default Size"))
            {
                resHeight = 1440;
                resWidth = 2560;
                scale = 1;
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            DrawPreview();
        }

        EditorGUILayout.Space();
        if(Preview != null)
        EditorGUILayout.LabelField("Screenshot will be taken at " + Preview.width + " x " + Preview.height + " px", EditorStyles.boldLabel);
        settings.saveAsSprite = EditorGUILayout.ToggleLeft("Save As Sprite", settings.saveAsSprite, EditorStyles.toolbarButton);
        if (GUILayout.Button("RENDER", GUILayout.MinHeight(60)))
        {
            if (settings.path == "")
            {
                settings.path = EditorUtility.SaveFolderPanel("Path to Save Images", settings.path, Application.dataPath);
            }
            Save();
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Open Last Screenshot", EditorStyles.toolbarButton, GUILayout.MaxWidth(160)))
        {
            if (lastScreenshot != "")
            {
                Application.OpenURL("file://" + lastScreenshot);
                Debug.Log("Opening File " + lastScreenshot);
            }
        }
        if (GUILayout.Button("Open Folder", EditorStyles.toolbarButton, GUILayout.MaxWidth(100)))
        {
            Application.OpenURL("file://" + settings.path);
        }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck() || refresh)
        {
            BuildPreview();
        }
    }

    private int GetNearesMultipler4(int value)
    {
        return ((value + 4 / 2) / 4) * 4;
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawPreview()
    {
        if (UIScript != null && Preview != null)
        {
            Rect r = EditorGUILayout.BeginHorizontal("box");
            EditorGUI.DrawRect(r, Color.black);
            GUILayout.FlexibleSpace();
            r = GUILayoutUtility.GetRect(Preview.width, Preview.height);
            GUI.Box(r, GUIContent.none);
            GUI.DrawTexture(r, Preview);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            r.x = r.x + r.width;
            r.width = 20;
            r.height = 20;
            if (GUI.Button(r, "R", EditorStyles.toolbarButton))
            {
                refresh = true;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawSettings()
    {
        if (UIScript != null)
        {
            useUIRect = EditorGUILayout.ToggleLeft("Use UI Resolution", useUIRect, EditorStyles.toolbarButton);
        }

        EditorGUILayout.LabelField("Resolution: " + string.Format("{0} x {1}", UIScript.ScreenShotRect.width, UIScript.ScreenShotRect.height), EditorStyles.boldLabel);

        EditorGUILayout.Space();

        isTransparent = EditorGUILayout.ToggleLeft("Transparent Background", isTransparent, EditorStyles.toolbarButton);
        settings.useMonoColor = EditorGUILayout.ToggleLeft("Use Mono Color", settings.useMonoColor, EditorStyles.toolbarButton);
        if (settings.useMonoColor)
        {
            settings.grayScale = EditorGUILayout.ToggleLeft("Grey Scale", settings.grayScale, EditorStyles.toolbarButton);
            settings.fixedAlpha = EditorGUILayout.ToggleLeft("Sharp Edges", settings.fixedAlpha, EditorStyles.toolbarButton);
        }
        settings.cropEdges = EditorGUILayout.ToggleLeft("Crop Edges", settings.cropEdges, EditorStyles.toolbarButton);
        settings.forceMultipleOfFour = EditorGUILayout.ToggleLeft("Force Multiple Of Four", settings.forceMultipleOfFour, EditorStyles.toolbarButton);
        GUILayout.Space(10);
        scale = EditorGUILayout.IntSlider("Scale", scale, 1, 15);
        settings.tempFov = EditorGUILayout.Slider("Temp Fov", settings.tempFov, 1, 120);
        if (UIScript != null && UIScript.TempCamera != null)
        {
            UIScript.TempCamera.fieldOfView = settings.tempFov;
        }
        settings.extraBlack = EditorGUILayout.Slider("Black Intensity", settings.extraBlack, -1, 1);
        if (!settings.useMonoColor)
        {
            settings.colorIntensity = EditorGUILayout.Slider("Contrast", settings.colorIntensity, 0, 3);
            settings.brigness = EditorGUILayout.Slider("Brightness", settings.brigness, -1, 1);
            settings.whiteIntensity = EditorGUILayout.Slider("White Intensity", settings.whiteIntensity, -1, 1);
        }

        if (settings.grayScale && settings.useMonoColor)
        {
            settings.extraGrayScale = EditorGUILayout.Slider("Extra Grey Scale", settings.extraGrayScale, 0, 1);
        }
        if (settings.useMonoColor)
        {
            settings.MonoColor = EditorGUILayout.ColorField("MonoColor", settings.MonoColor);
        }

        EditorGUILayout.Space();

        GUILayout.Label("Save Path", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(settings.path, GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
        {
            settings.path = EditorUtility.SaveFolderPanel("Path to Save Images", settings.path, Application.dataPath);
        }
        settings.customName = EditorGUILayout.TextField(settings.customName);
        EditorGUILayout.EndHorizontal();

        myCamera = EditorGUILayout.ObjectField("Render Camera", myCamera, typeof(Camera), true, null) as Camera;
        if (myCamera == null)
        {
            myCamera = Camera.main;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void BuildPreview()
    {
        if (useUIRect)
        {
            resWidth = (int)UIScript.ScreenShotRect.width;
            resHeight = (int)UIScript.ScreenShotRect.height;
        }

        int resWidthN = resWidth * scale;
        int resHeightN = resHeight * scale;
        if (settings.forceMultipleOfFour)
        {
            resWidthN = GetNearesMultipler4(resWidthN);
            resHeightN = GetNearesMultipler4(resHeightN);
        }
        Vector2 screenSize = new Vector2(resWidthN, resHeightN);
        RenderTexture rt = new RenderTexture((int)screenSize.x, (int)screenSize.y, 24);
        rt.antiAliasing = 8;
        myCamera.targetTexture = rt;

        TextureFormat tFormat;
        if (isTransparent)
            tFormat = TextureFormat.ARGB32;
        else
            tFormat = TextureFormat.RGB24;

        Preview = null;
        Preview = new Texture2D(resWidthN, resHeightN, tFormat, false);
        float fov = myCamera.fieldOfView;
        myCamera.fieldOfView = settings.tempFov;
        myCamera.Render();
        RenderTexture.active = rt;
        Preview.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);

        if (settings.useMonoColor)
        {
            Color[] defaultColors = Preview.GetPixels();
            for (int i = 0; i < defaultColors.Length; i++)
            {
                if (defaultColors[i].a <= 0) continue;
                Color c = settings.MonoColor;
                float a = settings.fixedAlpha ? 1 : defaultColors[i].a;
                if (settings.grayScale)
                {
                    float eb = defaultColors[i].grayscale < 0.5f ? settings.extraBlack : 0;
                    float gs = defaultColors[i].grayscale + settings.extraGrayScale - eb;
                    c = new Color(gs, gs, gs, a);
                }
                else
                {
                    c.a = a;
                }
                defaultColors[i] = c;
            }
            Preview.SetPixels(defaultColors);
        }
        else
        {
            if (settings.colorIntensity != 1)
            {
                Color[] defaultColors = Preview.GetPixels();
                float st = settings.brigness + 1;
                float wi = 1 - settings.whiteIntensity;
                for (int i = 0; i < defaultColors.Length; i++)
                {
                    if (defaultColors[i].a <= 0) continue;

                    defaultColors[i].r *= st;
                    defaultColors[i].g *= st;
                    defaultColors[i].b *= st;

                    defaultColors[i].r = Mathf.Pow(defaultColors[i].r, settings.colorIntensity);
                    defaultColors[i].g = Mathf.Pow(defaultColors[i].g, settings.colorIntensity);
                    defaultColors[i].b = Mathf.Pow(defaultColors[i].b, settings.colorIntensity);

                    if (defaultColors[i].grayscale > 0.5f)
                    {
                        defaultColors[i].r = Mathf.Pow(defaultColors[i].r, wi);
                        defaultColors[i].g = Mathf.Pow(defaultColors[i].g, wi);
                        defaultColors[i].b = Mathf.Pow(defaultColors[i].b, wi);
                    }
                }
                Preview.SetPixels(defaultColors);
            }
        }
        Preview.Apply();

        if (settings.cropEdges)
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            for (int x = 0; x < Preview.width; x++)
            {
                for (int y = 0; y < Preview.height; y++)
                {
                    Color color = Preview.GetPixel(x, y);
                    if (color.a != 0)
                    {
                        // This sets the edges to the current coordinate
                        // if this pixel lies outside the current boundaries
                        minX = Mathf.Min(x, minX);
                        maxX = Mathf.Max(x, maxX);
                        minY = Mathf.Min(y, minY);
                        maxY = Mathf.Max(y, maxY);
                    }
                }
            }

            Texture2D result = new Texture2D(maxX - minX, maxY - minY, TextureFormat.ARGB32, false);
            for (int x = 0; x < maxX - minX; x++)
            {
                for (int y = 0; y < maxY - minY; y++)
                {
                    result.SetPixel(x, y, Preview.GetPixel(x + minX, y + minY));
                }
            }
            result.Apply();
            Preview = result;
            Preview.Apply();
        }

        myCamera.targetTexture = null;
        myCamera.fieldOfView = fov;
        RenderTexture.active = null;
        refresh = false;
    }

    void Save()
    {
        int resWidthN = resWidth * scale;
        int resHeightN = resHeight * scale;
        string finalPath = ScreenShotName(resWidthN, resHeightN);
        if (File.Exists(finalPath))
        {
            finalPath = EditorUtility.SaveFilePanelInProject("Save Path", settings.path, settings.customName, ".png");
        }
        if (string.IsNullOrEmpty(finalPath)) return;

        byte[] bytes = Preview.EncodeToPNG();

        System.IO.File.WriteAllBytes(finalPath, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", finalPath));
        Application.OpenURL(finalPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (settings.saveAsSprite)
        {
            if (finalPath.StartsWith(Application.dataPath))
                finalPath = "Assets" + finalPath.Substring(Application.dataPath.Length);

            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(finalPath);
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.alphaIsTransparency = true;
                ti.mipmapEnabled = false;//performance
                ti.isReadable = false;//performance
                ti.crunchedCompression = true;
                ti.sRGBTexture = settings.useMonoColor ? false : true;
                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();
            }
            else
            {
                Debug.Log("Couldn't find it: " + finalPath);
            }
        }
    }

    public string lastScreenshot = "";


    public string ScreenShotName(int width, int height)
    {

        string strPath = "";
        string nameImage = settings.customName == string.Empty ? "screen_{1}x{2}_{3}.png" : string.Format("{0}.png", settings.customName);
        strPath = string.Format("{0}/" + nameImage,
                             settings.path,
                             width, height,
                                       System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        lastScreenshot = strPath;

        return strPath;
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Lovatto/Screen shot")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(Screenshot));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Screenshoot");
    }

    [System.Serializable]
    public class Settings
    {
        public float tempFov = 30;
        public float colorIntensity = 1;
        public float whiteIntensity = 1;
        public float brigness = 0;
        public float extraBlack = 0;
        public bool useMonoColor = false;
        public bool grayScale = false;
        public bool fixedAlpha = true;
        public float extraGrayScale = 0;
        public Color MonoColor = Color.white;
        public string customName = string.Empty;
        public bool cropEdges = false;
        public string path = "";
        public bool forceMultipleOfFour = true;
        public bool saveAsSprite = true;
    }
}