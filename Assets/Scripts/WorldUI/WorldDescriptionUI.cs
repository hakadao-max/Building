using TMPro;
using UnityEngine;

public sealed class WorldDescriptionUI : MonoBehaviour
{
    private const string TipUITemplateName = "TipUI";
    private const string TitleTextName = "Title";
    private const string DescriptionTextName = "Description";

    [Header("内容配置")]
    [LabelText("标题")]
    [SerializeField] private string title = "场景元素";

    [LabelText("说明内容")]
    [TextArea(2, 6)]
    [SerializeField] private string description = "在这里填写场景元素说明。";

    [Header("显示配置")]
    [LabelText("显示偏移")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    [LabelText("始终面向相机")]
    [SerializeField] private bool faceCamera = true;

    [LabelText("只绕Y轴旋转")]
    [SerializeField] private bool rotateOnlyYAxis;

    [LabelText("仅详情查看时显示")]
    [SerializeField] private bool onlyShowInDetailInspect = true;

    [LabelText("最大显示距离")]
    [SerializeField] private float maxVisibleDistance;

    [LabelText("距离检测目标")]
    [SerializeField] private Transform distanceTarget;

    [Header("提示配置")]
    [LabelText("提示目标")]
    [SerializeField] private Transform hintTarget;

    [LabelText("提示颜色")]
    [SerializeField] private Color hintColor = Color.green;

    [LabelText("提示显示距离")]
    [SerializeField] private float hintRevealDistance = 8f;

    [LabelText("提示持续时间")]
    [SerializeField] private float hintDuration = 3f;

    private Transform cachedPlayerTransform;
    private GameObject worldUIInstance;
    private RectTransform tipUI;
    private TMP_Text titleText;
    private TMP_Text descriptionText;
    private bool isDetailInspectHighlighted;

    public bool RequiresDetailInspect => onlyShowInDetailInspect;
    public bool ShouldShowHint => worldUIInstance == null;

    private void LateUpdate()
    {
        Camera targetCamera = Camera.main;
        if (targetCamera == null || !ShouldDisplay(targetCamera))
        {
            RemoveWorldUI();
            return;
        }

        if (!EnsureWorldUI())
        {
            return;
        }

        ApplyParameters(targetCamera.transform);
    }

    private void OnDisable()
    {
        RemoveWorldUI();
    }

    private void OnValidate()
    {
        maxVisibleDistance = Mathf.Max(0f, maxVisibleDistance);
        hintRevealDistance = Mathf.Max(0f, hintRevealDistance);
        hintDuration = Mathf.Max(0.1f, hintDuration);
        ApplyTextParameters();
    }

    public void SetDescription(string newTitle, string newDescription)
    {
        title = newTitle;
        description = newDescription;
        ApplyTextParameters();
    }

    public void TryShowHint(Vector3 observerPosition)
    {
        Transform target = ResolveHintTarget();
        float sqrRevealDistance = hintRevealDistance * hintRevealDistance;
        if ((target.position - observerPosition).sqrMagnitude <= sqrRevealDistance)
        {
            TemporaryColorHint.Show(target, hintDuration, hintColor);
        }
    }

    public void SetDetailInspectHighlighted(bool highlighted)
    {
        isDetailInspectHighlighted = highlighted;
        if (!highlighted)
        {
            RemoveWorldUI();
        }
    }

    private bool EnsureWorldUI()
    {
        if (worldUIInstance != null)
        {
            return true;
        }

        worldUIInstance = UIManager.AddWorldUI(TipUITemplateName);
        if (worldUIInstance == null)
        {
            return false;
        }

        tipUI = worldUIInstance.GetComponent<RectTransform>();
        if (tipUI == null)
        {
            RemoveWorldUI();
            return false;
        }

        titleText = FindChildText(TitleTextName);
        descriptionText = FindChildText(DescriptionTextName);
        ApplyTextParameters();
        return true;
    }

    private void RemoveWorldUI()
    {
        if (worldUIInstance != null)
        {
            UIManager.RemoveWorldUI(worldUIInstance);
        }

        worldUIInstance = null;
        tipUI = null;
        titleText = null;
        descriptionText = null;
    }

    private void ApplyParameters(Transform cameraTransform)
    {
        tipUI.position = ResolveTipPosition();
        ApplyTextParameters();

        if (faceCamera)
        {
            RotateTowardsCamera(tipUI, cameraTransform);
        }
    }

    private void ApplyTextParameters()
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
    }

    private TMP_Text FindChildText(string childName)
    {
        if (worldUIInstance == null)
        {
            return null;
        }

        foreach (TMP_Text candidate in worldUIInstance.GetComponentsInChildren<TMP_Text>(true))
        {
            if (candidate.name == childName)
            {
                return candidate;
            }
        }

        return null;
    }

    private bool ShouldDisplay(Camera targetCamera)
    {
        if (onlyShowInDetailInspect)
        {
            return isDetailInspectHighlighted;
        }

        if (maxVisibleDistance <= 0f)
        {
            return true;
        }

        Transform targetTransform = ResolveDistanceTarget(targetCamera);
        float distance = Vector3.Distance(targetTransform.position, ResolveTipPosition());
        return distance <= maxVisibleDistance;
    }

    private Vector3 ResolveTipPosition()
    {
        return transform.TransformPoint(worldOffset);
    }

    private void RotateTowardsCamera(Transform target, Transform cameraTransform)
    {
        Vector3 lookDirection = target.position - cameraTransform.position;
        if (rotateOnlyYAxis)
        {
            lookDirection.y = 0f;
        }

        if (lookDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        if (rotateOnlyYAxis)
        {
            Vector3 eulerAngles = targetRotation.eulerAngles;
            target.rotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
            return;
        }

        target.rotation = targetRotation;
    }

    private Transform ResolveDistanceTarget(Camera targetCamera)
    {
        if (distanceTarget != null)
        {
            return distanceTarget;
        }

        if (cachedPlayerTransform == null)
        {
            SimplePlayerController playerController = GameController.PlayerController;
            if (playerController != null)
            {
                cachedPlayerTransform = playerController.transform;
            }
        }

        return cachedPlayerTransform != null ? cachedPlayerTransform : targetCamera.transform;
    }

    private Transform ResolveHintTarget()
    {
        if (hintTarget != null)
        {
            return hintTarget;
        }

        if (HasRenderableMesh(transform))
        {
            return transform;
        }

        return transform.parent != null ? transform.parent : transform;
    }

    private static bool HasRenderableMesh(Transform target)
    {
        MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        return meshRenderer != null && meshRenderer.enabled && meshFilter != null && meshFilter.sharedMesh != null;
    }
}
