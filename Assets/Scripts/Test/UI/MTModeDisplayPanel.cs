using TMPro;

namespace Test
{
    public class MTModeDisplayPanel : MTPanelBase
    {
        public TextMeshProUGUI text;

        public void SetText(string text)
        {
            this.text.text = text;
        }
    }
}