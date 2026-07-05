using System;
using UnityEngine;

[Serializable]
public sealed class PrefabSwapOption
{
    [LabelText("配置名称")]
    [SerializeField] private string displayName;

    [LabelText("预制体")]
    [SerializeField] private GameObject prefab;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName)
        ? (prefab != null ? prefab.name : string.Empty)
        : displayName.Trim();

    public GameObject Prefab => prefab;
}
