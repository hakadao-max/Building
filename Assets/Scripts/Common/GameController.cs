using UnityEngine;

/// <summary>
/// Runtime-wide game state and access points.
/// </summary>
public static class GameController
{
    private static SimplePlayerController playerController;
    private static bool playerControlEnabled = true;

    /// <summary>
    /// Gets the active player controller. The scene is searched only when the cache is empty.
    /// </summary>
    public static SimplePlayerController PlayerController
    {
        get
        {
            if (playerController == null)
            {
                playerController = Object.FindFirstObjectByType<SimplePlayerController>(FindObjectsInactive.Include);
            }

            return playerController;
        }
    }

    public static bool PlayerControlEnabled => playerControlEnabled;

    public static Vector2Int CurrentResolution => new Vector2Int(Screen.width, Screen.height);

    public static FullScreenMode CurrentFullScreenMode => Screen.fullScreenMode;

    public static bool IsFullscreen => CurrentFullScreenMode != FullScreenMode.Windowed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        playerController = null;
        playerControlEnabled = true;
    }

    public static void SetPlayerControlEnabled(bool enabled)
    {
        if (playerControlEnabled == enabled)
        {
            return;
        }

        playerControlEnabled = enabled;
        SimplePlayerController controller = PlayerController;
        if (controller != null)
        {
            controller.ApplyControlPermission(enabled);
        }
    }

    public static void SetResolution(Vector2Int resolution, bool fullscreen)
    {
        SetResolution(
            resolution,
            fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
    }

    public static void SetResolution(Vector2Int resolution, FullScreenMode fullScreenMode)
    {
        Screen.SetResolution(
            Mathf.Max(1, resolution.x),
            Mathf.Max(1, resolution.y),
            fullScreenMode);
    }

    public static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
