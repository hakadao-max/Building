using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Test
{
    public class MTInteractTipPanel : MTPanelBase
    {
        public TextMeshProUGUI text;
        public KeyCode key = KeyCode.E;


        public void SetText(string text)
        {
            this.text.text = text;
        }
    }
}