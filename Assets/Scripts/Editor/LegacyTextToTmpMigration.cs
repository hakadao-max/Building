using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class LegacyTextToTmpMigration
{
    private const string MigrationKey = "Building.LegacyTextToTmpMigration.V2";

    static LegacyTextToTmpMigration()
    {
        EditorApplication.delayCall += RunOnce;
    }

    [MenuItem("工具/UI/迁移所有旧 Text 到 TMP")]
    public static void MigrateAllUiPrefabs()
    {
        PrefabSwapPanelPrefabCreator.CreatePanelPrefab();
        EditorPrefs.SetBool(MigrationKey, true);
        Debug.Log("UI TextMeshPro 迁移完成：预制体交换面板已重新生成。");
    }

    private static void RunOnce()
    {
        if (EditorPrefs.GetBool(MigrationKey, false) || !HasLegacyText())
        {
            return;
        }

        MigrateAllUiPrefabs();
    }

    private static bool HasLegacyText()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string prefabGuid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.GetComponentInChildren<Text>(true) != null)
            {
                return true;
            }
        }

        return false;
    }

}
