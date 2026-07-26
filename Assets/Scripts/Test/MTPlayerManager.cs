using System;
using System.Collections.Generic;
using Test;
using UnityEngine;

public class MTPlayerManager : MonoBehaviour
{
    [Serializable]
    public class ViewModeConfig
    {
        public KeyCode key = KeyCode.None;
        public string displayName = "XXX";
        public MTPlayerViewMode viewMode;
    }
    
    [ReadOnly]
    private ViewModeConfig currentViewMode = null;
    [SerializeField]
    private MTCameraController cameraController;

    public List<ViewModeConfig> viewModeList =new List<ViewModeConfig>();
    private MTMoveCom moveCom;
    public MTMoveCom MoveCom => moveCom;

    [SerializeField]private MTLightController lightController;
    public Collider Collider => GetComponentInChildren<Collider>(true);
    public MTLightController LightController => lightController;

    public MTCameraController CameraController => cameraController;
    
    private static MTPlayerManager instance;
    public static MTPlayerManager Instance => instance;
    void Awake()
    {
        instance = this;
        foreach (var viewModeConfig in viewModeList)
        {
            viewModeConfig.viewMode.Init(this);
        }
        
        moveCom = GetComponent<MTMoveCom>();
        
        cameraController = Camera.main.GetComponent<MTCameraController>();
    }

    private void Start()
    {
        SwitchMode(viewModeList[0]);
    }

    private void CheckViewModeSwitch()
    {
        foreach (var viewModeConfig in viewModeList)
        {
            if (viewModeConfig.key != KeyCode.None && RuntimeInput.GetKeyDown(viewModeConfig.key))
            {
                SwitchMode(viewModeConfig);
            }
        }
    }

    private void SwitchMode(ViewModeConfig targetViewModeConfig)
    {

        if (targetViewModeConfig == currentViewMode)
        {
            return;
        }
        if (currentViewMode != null)
        {
            currentViewMode.viewMode.Exit();
        }

        currentViewMode = targetViewModeConfig;
        currentViewMode.viewMode?.Enter();

        if (MTUIManager.Instance.TryGetPanel("displayPanel", out MTModeDisplayPanel displayPanel))
        {
            displayPanel.SetText(currentViewMode.displayName);
            MTUIManager.Instance.Open("displayPanel");
        }
    }
    
    private void Update()
    {
        CheckViewModeSwitch();
        currentViewMode.viewMode?.Tick();
    }

    public Vector3 GetTransformedDir(Vector3 dir)
    {
        return cameraController.GetTransformedDir(dir);
    }
}