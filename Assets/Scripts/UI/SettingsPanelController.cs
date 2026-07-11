using UnityEngine;
using TMPro;
using UnityEngine.UI;

public sealed class SettingsPanelController : MonoBehaviour
{
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

    [LabelText("面板根节点")]
    [SerializeField] private GameObject panelRoot;

    [LabelText("回到出生点按钮")]
    [SerializeField] private Button returnSpawnButton;

    [LabelText("退出按钮")]
    [SerializeField] private Button closeButton;

    [LabelText("退出游戏按钮")]
    [SerializeField] private Button closeGameButton;

    [LabelText("分辨率下拉框")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [LabelText("全屏开关")]
    [SerializeField] private Toggle fullscreenToggle;

    private float previousTimeScale = 1f;
    private bool isOpen;

    private void Awake()
    {
        EnsurePlayerController();
        BindButtons();
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

    private void OnDestroy()
    {
        UnbindButtons();
    }

    public void TogglePanel()
    {
        SetPanelVisible(!isOpen);
    }

    public void ClosePanel()
    {
        SetPanelVisible(false);
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReturnPlayerToSpawn()
    {
        EnsurePlayerController();
        if (playerController == null)
        {
            Debug.LogWarning("设置面板未找到玩家控制器。", this);
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

    public void ApplyResolution(int resolutionIndex)
    {
        Vector2Int resolution = GetResolution(resolutionIndex);
        bool isFullscreen = fullscreenToggle == null
            ? Screen.fullScreenMode != FullScreenMode.Windowed
            : fullscreenToggle.isOn;
        Screen.SetResolution(
            resolution.x,
            resolution.y,
            isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
    }

    public void ApplyFullscreen(bool isFullscreen)
    {
        int resolutionIndex = resolutionDropdown == null ? GetCurrentResolutionIndex() : resolutionDropdown.value;
        Vector2Int resolution = GetResolution(resolutionIndex);
        Screen.SetResolution(
            resolution.x,
            resolution.y,
            isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
    }

    public void SetPanelVisible(bool visible)
    {
        if (panelRoot == null)
        {
            isOpen = false;
            if (visible)
            {
                Debug.LogWarning("设置面板未绑定 Prefab 中的面板根节点。", this);
            }

            return;
        }

        bool wasOpen = isOpen;
        isOpen = visible;
        panelRoot.SetActive(isOpen);

        if (wasOpen == isOpen)
        {
            return;
        }

        EnsurePlayerController();
        if (playerController != null)
        {
            playerController.SetExternalInputLocked(isOpen);
        }

        if (isOpen)
        {
            RefreshDisplayControls();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!pauseWhenOpen)
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

    private void BindButtons()
    {
        if (returnSpawnButton != null)
        {
            returnSpawnButton.onClick.RemoveListener(ReturnPlayerToSpawn);
            returnSpawnButton.onClick.AddListener(ReturnPlayerToSpawn);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }

        BindButton(closeGameButton, CloseGame);

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(ApplyResolution);
            resolutionDropdown.onValueChanged.AddListener(ApplyResolution);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(ApplyFullscreen);
            fullscreenToggle.onValueChanged.AddListener(ApplyFullscreen);
        }
    }

    private void UnbindButtons()
    {
        if (returnSpawnButton != null)
        {
            returnSpawnButton.onClick.RemoveListener(ReturnPlayerToSpawn);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }

        UnbindButton(closeGameButton, CloseGame);

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(ApplyResolution);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(ApplyFullscreen);
        }
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static void UnbindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }

    private void RefreshDisplayControls()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.SetValueWithoutNotify(GetCurrentResolutionIndex());
            resolutionDropdown.RefreshShownValue();
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreenMode != FullScreenMode.Windowed);
        }
    }

    private void EnsurePlayerController()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<SimplePlayerController>();
        }
    }

    private static int GetCurrentResolutionIndex()
    {
        int bestIndex = 0;
        long bestDifference = long.MaxValue;
        for (int index = 0; index < 3; index++)
        {
            Vector2Int resolution = GetResolution(index);
            long widthDifference = resolution.x - Screen.width;
            long heightDifference = resolution.y - Screen.height;
            long difference = widthDifference * widthDifference + heightDifference * heightDifference;
            if (difference < bestDifference)
            {
                bestDifference = difference;
                bestIndex = index;
            }
        }

        return bestIndex;
    }

    private static Vector2Int GetResolution(int resolutionIndex)
    {
        return resolutionIndex switch
        {
            0 => new Vector2Int(1280, 720),
            1 => new Vector2Int(1920, 1080),
            _ => new Vector2Int(2560, 1440)
        };
    }
}
