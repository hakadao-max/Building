using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class TemporaryColorHint : MonoBehaviour
{
    private const string BaseColorProperty = "_BaseColor";
    private const string ColorProperty = "_Color";
    private const float RestoreDuration = 0.8f;

    private readonly List<RendererColorState> rendererStates = new List<RendererColorState>();
    private readonly List<GraphicColorState> graphicStates = new List<GraphicColorState>();
    private Coroutine hintCoroutine;

    public static void Show(Transform target, float duration, Color color)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            return;
        }

        TemporaryColorHint colorHint = target.GetComponent<TemporaryColorHint>();
        if (colorHint == null)
        {
            colorHint = target.gameObject.AddComponent<TemporaryColorHint>();
        }

        colorHint.Show(duration, color);
    }

    private void OnDestroy()
    {
        RestoreColors();
    }

    private void Show(float duration, Color color)
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            RestoreColors();
        }

        CacheColorStates();
        if (rendererStates.Count == 0 && graphicStates.Count == 0)
        {
            return;
        }

        hintCoroutine = StartCoroutine(HintRoutine(Mathf.Max(0.1f, duration), color));
    }

    private IEnumerator HintRoutine(float duration, Color color)
    {
        ApplyColor(color);
        yield return new WaitForSeconds(duration);

        float elapsedTime = 0f;
        while (elapsedTime < RestoreDuration)
        {
            float t = elapsedTime / RestoreDuration;
            ApplyInterpolatedColor(color, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        RestoreColors();
        hintCoroutine = null;
    }

    private void CacheColorStates()
    {
        rendererStates.Clear();
        graphicStates.Clear();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.enabled && meshRenderer.gameObject.activeInHierarchy)
        {
            Material[] materials = meshRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (TryGetMaterialColor(material, out Color originalColor))
                {
                    rendererStates.Add(new RendererColorState(material, originalColor));
                }
            }
        }

        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphicStates.Add(new GraphicColorState(graphic, graphic.color));
        }
    }

    private void ApplyColor(Color color)
    {
        foreach (RendererColorState state in rendererStates)
        {
            SetMaterialColor(state.Material, color);
        }

        foreach (GraphicColorState state in graphicStates)
        {
            if (state.Graphic != null)
            {
                state.Graphic.color = color;
            }
        }
    }

    private void ApplyInterpolatedColor(Color fromColor, float t)
    {
        foreach (RendererColorState state in rendererStates)
        {
            SetMaterialColor(state.Material, Color.Lerp(fromColor, state.OriginalColor, t));
        }

        foreach (GraphicColorState state in graphicStates)
        {
            if (state.Graphic != null)
            {
                state.Graphic.color = Color.Lerp(fromColor, state.OriginalColor, t);
            }
        }
    }

    private void RestoreColors()
    {
        foreach (RendererColorState state in rendererStates)
        {
            SetMaterialColor(state.Material, state.OriginalColor);
        }

        foreach (GraphicColorState state in graphicStates)
        {
            if (state.Graphic != null)
            {
                state.Graphic.color = state.OriginalColor;
            }
        }
    }

    private static bool TryGetMaterialColor(Material material, out Color color)
    {
        color = Color.white;
        if (material == null)
        {
            return false;
        }

        if (material.HasProperty(BaseColorProperty))
        {
            color = material.GetColor(BaseColorProperty);
            return true;
        }

        if (material.HasProperty(ColorProperty))
        {
            color = material.GetColor(ColorProperty);
            return true;
        }

        return false;
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty(BaseColorProperty))
        {
            material.SetColor(BaseColorProperty, color);
        }

        if (material.HasProperty(ColorProperty))
        {
            material.SetColor(ColorProperty, color);
        }
    }

    private readonly struct RendererColorState
    {
        public RendererColorState(Material material, Color originalColor)
        {
            Material = material;
            OriginalColor = originalColor;
        }

        public Material Material { get; }
        public Color OriginalColor { get; }
    }

    private readonly struct GraphicColorState
    {
        public GraphicColorState(Graphic graphic, Color originalColor)
        {
            Graphic = graphic;
            OriginalColor = originalColor;
        }

        public Graphic Graphic { get; }
        public Color OriginalColor { get; }
    }
}
