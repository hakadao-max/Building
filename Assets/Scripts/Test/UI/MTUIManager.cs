using System;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class MTUIManager : MonoBehaviour
    {
        private static MTUIManager ins;
        public static MTUIManager Instance => ins;

        [Serializable]
        public class UIPanelConfig
        {
            public string name;
            public MTPanelBase uiPanel;
        }

        public List<UIPanelConfig> uiPanels;

        void Awake()
        {
            ins = this;

            foreach (var uiPanelConfig in uiPanels)
            {
                uiPanelConfig.uiPanel.Hide();
            }
        }

        public void Open(string name)
        {
            foreach (var uiPanelConfig in uiPanels)
            {
                if (uiPanelConfig.name == name)
                {
                    uiPanelConfig.uiPanel.Show();
                }
            }
        }

        public void Close(string name)
        {
            foreach (var uiPanelConfig in uiPanels)
            {
                if (uiPanelConfig.name == name)
                {
                    uiPanelConfig.uiPanel.Hide();
                }
            }
        }

        public bool TryGetPanel<T>(string name, out T panel) where T : MTPanelBase
        {
            panel = null;
            foreach (var uiPanelConfig in uiPanels)
            {
                if (uiPanelConfig.name == name)
                {
                    panel = uiPanelConfig.uiPanel as T;
                    return true;
                }
            }

            return false;
        }
    }
}