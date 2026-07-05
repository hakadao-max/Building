using UnityEngine;
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
    }

    private void EnsurePlayerController()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<SimplePlayerController>();
        }
    }
}
