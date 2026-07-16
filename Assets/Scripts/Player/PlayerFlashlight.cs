using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerFlashlight : MonoBehaviour
{
    [LabelText("手电筒按键")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    [LabelText("手电筒灯光")]
    [SerializeField] private Light flashlightLight;

    [LabelText("默认开启")]
    [SerializeField] private bool startsOn;

    [LabelText("相机模式局部位置")]
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0.25f, -0.2f, 0.35f);

    [LabelText("相机模式局部角度")]
    [SerializeField] private Vector3 cameraLocalEulerAngles;

    [LabelText("第三人称局部位置")]
    [SerializeField] private Vector3 thirdPersonLocalPosition = new Vector3(0.25f, 1.35f, 0.85f);

    [LabelText("第三人称局部角度")]
    [SerializeField] private Vector3 thirdPersonLocalEulerAngles;

    [LabelText("手电筒强度")]
    [SerializeField] private float intensity = 2f;

    [LabelText("手电筒范围")]
    [SerializeField] private float range = 18f;

    [LabelText("手电筒角度")]
    [SerializeField] private float spotAngle = 55f;

    private SimplePlayerController controller;
    private bool initialized;

    private void OnValidate()
    {
        intensity = Mathf.Max(0f, intensity);
        range = Mathf.Max(0.1f, range);
        spotAngle = Mathf.Clamp(spotAngle, 1f, 179f);
    }

    public void Initialize(SimplePlayerController owner)
    {
        controller = owner;
        if (initialized)
        {
            return;
        }

        initialized = true;
        if (flashlightLight == null)
        {
            return;
        }

        flashlightLight.type = LightType.Spot;
        flashlightLight.intensity = intensity;
        flashlightLight.range = range;
        flashlightLight.spotAngle = spotAngle;
        flashlightLight.enabled = startsOn;
    }

    private void Update()
    {
        if (controller != null
            && GameController.PlayerControlEnabled
            && controller.AllowsManualAbilities)
        {
            TickInput();
        }
    }

    public void TickInput()
    {
        if (toggleKey != KeyCode.None && RuntimeInput.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void OnViewModeChanged(PlayerViewMode mode, Camera activeCamera)
    {
        if (flashlightLight == null)
        {
            return;
        }

        if (mode == PlayerViewMode.ThirdPerson)
        {
            flashlightLight.transform.SetParent(transform, false);
            flashlightLight.transform.localPosition = thirdPersonLocalPosition;
            flashlightLight.transform.localRotation = Quaternion.Euler(thirdPersonLocalEulerAngles);
            return;
        }

        if (activeCamera == null)
        {
            return;
        }

        flashlightLight.transform.SetParent(activeCamera.transform, false);
        flashlightLight.transform.localPosition = cameraLocalPosition;
        flashlightLight.transform.localRotation = Quaternion.Euler(cameraLocalEulerAngles);
    }

    public void Toggle()
    {
        if (flashlightLight == null)
        {
            return;
        }

        if (controller != null)
        {
            OnViewModeChanged(controller.CurrentViewMode, controller.ActiveCamera);
        }

        flashlightLight.enabled = !flashlightLight.enabled;
    }
}
