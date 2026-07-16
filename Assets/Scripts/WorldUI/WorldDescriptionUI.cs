using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
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

    [Header("提示配置")]
    [LabelText("提示目标")]
    [SerializeField] private Transform hintTarget;

    [LabelText("提示颜色")]
    [SerializeField] private Color hintColor = Color.green;

    [LabelText("提示显示距离")]
    [SerializeField] private float hintRevealDistance = 8f;

    [LabelText("提示持续时间")]
    [SerializeField] private float hintDuration = 3f;

    private readonly Dictionary<PlayerInteractionHintInput, int> overlappingPlayers =
        new Dictionary<PlayerInteractionHintInput, int>();
    private GameObject worldUIInstance;
    private RectTransform tipUI;
    private TMP_Text titleText;
    private TMP_Text descriptionText;
    private bool isDetailInspectHighlighted;

    public bool RequiresDetailInspect => onlyShowInDetailInspect;
    public bool ShouldShowHint => worldUIInstance == null;

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        EnsureTriggerCollider();
    }

    private void LateUpdate()
    {
        Camera targetCamera = Camera.main;
        if (targetCamera == null || !ShouldDisplay())
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
        overlappingPlayers.Clear();
        RemoveWorldUI();
    }

    private void Update()
    {
        foreach (PlayerInteractionHintInput hintInput in overlappingPlayers.Keys)
        {
            if (hintInput == null
                || !ShouldShowHint
                || hintInput.RevealKey == KeyCode.None
                || !RuntimeInput.GetKeyDown(hintInput.RevealKey))
            {
                continue;
            }

            TryShowHint(hintInput.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteractionHintInput hintInput = other.GetComponentInParent<PlayerInteractionHintInput>();
        if (hintInput == null)
        {
            return;
        }

        overlappingPlayers.TryGetValue(hintInput, out int overlapCount);
        overlappingPlayers[hintInput] = overlapCount + 1;
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteractionHintInput hintInput = other.GetComponentInParent<PlayerInteractionHintInput>();
        if (hintInput == null || !overlappingPlayers.TryGetValue(hintInput, out int overlapCount))
        {
            return;
        }

        if (overlapCount > 1)
        {
            overlappingPlayers[hintInput] = overlapCount - 1;
            return;
        }

        overlappingPlayers.Remove(hintInput);
    }

    private void OnValidate()
    {
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

    private bool ShouldDisplay()
    {
        if (overlappingPlayers.Count == 0)
        {
            return false;
        }

        return !onlyShowInDetailInspect || isDetailInspectHighlighted;
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

    private void EnsureTriggerCollider()
    {
        SphereCollider triggerCollider = GetComponent<SphereCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.radius = 2f;
        }

        triggerCollider.isTrigger = true;
        if (Mathf.Approximately(triggerCollider.radius, 0f))
        {
            triggerCollider.radius = 2f;
        }
    }
}
