using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SettingsPanelController : MonoBehaviour
{
    private const string DefaultPanelPrefabResourcePath = "UI/SettingsPanel";

    [LabelText("玩家控制器")]
    [SerializeField] private SimplePlayerController playerController;

    [LabelText("出生点")]
    [SerializeField] private Transform spawnPoint;

    [LabelText("唤出按键")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    [LabelText("启动时显示")]
    [SerializeField] private bool showOnStart;

    [LabelText("打开时暂停")]
    [SerializeField] private bool pauseWhenOpen;

    [LabelText("面板资源路径")]
    [SerializeField] private string panelPrefabResourcePath = DefaultPanelPrefabResourcePath;

    [LabelText("面板根节点")]
    [SerializeField] private GameObject panelRoot;

    [LabelText("回出生点按钮")]
    [SerializeField] private Button returnSpawnButton;

    private Canvas runtimeCanvas;
    private Button boundReturnSpawnButton;
    private float previousTimeScale = 1f;
    private bool isOpen;

    private void Awake()
    {
        EnsurePlayerController();
        EnsurePanel();
        SetPanelVisible(showOnStart);
    }

    private void Update()
    {
        if (toggleKey != KeyCode.None && RuntimeInput.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }
    }

    private void OnDisable()
    {
        if (isOpen)
        {
            SetPanelVisible(false);
        }
    }

    public void TogglePanel()
    {
        SetPanelVisible(!isOpen);
    }

    private void OnDestroy()
    {
        UnbindReturnButton();
    }

    public void ReturnPlayerToSpawn()
    {
        EnsurePlayerController();

        if (playerController == null)
        {
            return;
        }

        if (spawnPoint != null)
        {
            playerController.TeleportTo(spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            playerController.ReturnToSpawn();
        }

        SetPanelVisible(false);
    }

    public void SetPanelVisible(bool visible)
    {
        bool wasOpen = isOpen;
        if (visible && panelRoot == null)
        {
            EnsurePanel();
        }

        isOpen = visible && panelRoot != null;

        if (panelRoot != null)
        {
            panelRoot.SetActive(isOpen);
        }

        if (!wasOpen && !isOpen)
        {
            return;
        }

        foreach (SimplePlayerController controller in FindObjectsOfType<SimplePlayerController>())
        {
            controller.SetExternalInputLocked(isOpen);
        }

        if (isOpen)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        if (!pauseWhenOpen || wasOpen == isOpen)
        {
            return;
        }

        if (isOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = previousTimeScale;
        }
    }

    private void EnsurePlayerController()
    {
        if (playerController != null)
        {
            return;
        }

        playerController = FindObjectOfType<SimplePlayerController>();
        if (playerController == null)
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
            {
                playerController = taggedPlayer.GetComponent<SimplePlayerController>();
            }
        }
    }

    private void EnsurePanel()
    {
        if (panelRoot != null)
        {
            BindReturnButton();
            EnsureEventSystem();
            return;
        }

        GameObject panelPrefab = Resources.Load<GameObject>(GetPanelPrefabResourcePath());
        if (panelPrefab == null)
        {
            Debug.LogWarning($"未找到设置面板资源：Resources/{GetPanelPrefabResourcePath()}.prefab", this);
            return;
        }

        runtimeCanvas = CreateRuntimeCanvas();
        panelRoot = Instantiate(panelPrefab, runtimeCanvas.transform, false);
        panelRoot.name = panelPrefab.name;

        BindReturnButton();
        EnsureEventSystem();
    }

    private Canvas CreateRuntimeCanvas()
    {
        GameObject canvasObject = new GameObject("Runtime Settings Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private string GetPanelPrefabResourcePath()
    {
        if (string.IsNullOrWhiteSpace(panelPrefabResourcePath))
        {
            return DefaultPanelPrefabResourcePath;
        }

        return panelPrefabResourcePath.Trim();
    }

    private void BindReturnButton()
    {
        Button button = returnSpawnButton;
        if (button == null && panelRoot != null)
        {
            Transform buttonTransform = FindChildByName(panelRoot.transform, "Return Spawn Button");
            if (buttonTransform != null)
            {
                buttonTransform.TryGetComponent(out button);
            }
        }

        if (button == null && panelRoot != null)
        {
            button = panelRoot.GetComponentInChildren<Button>(true);
        }

        if (button == null)
        {
            Debug.LogWarning("设置面板 prefab 中没有找到回出生点按钮。", this);
            return;
        }

        if (boundReturnSpawnButton == button)
        {
            return;
        }

        UnbindReturnButton();
        button.onClick.RemoveListener(ReturnPlayerToSpawn);
        button.onClick.AddListener(ReturnPlayerToSpawn);

        returnSpawnButton = button;
        boundReturnSpawnButton = button;
    }

    private void UnbindReturnButton()
    {
        if (boundReturnSpawnButton != null)
        {
            boundReturnSpawnButton.onClick.RemoveListener(ReturnPlayerToSpawn);
            boundReturnSpawnButton = null;
        }
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null || FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }

    private static Transform FindChildByName(Transform parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }

        foreach (Transform child in parent)
        {
            Transform result = FindChildByName(child, childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
