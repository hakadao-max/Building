namespace Test.Trigger
{
    public class MTTipTrigger : MTTrigger
    {
        public string tipTxt = "按 E 交 互";

        protected override void OnEnter()
        {
            ShowText();
        }

        protected override void OnExit()
        {
            HideText();
        }

        private void ShowText()
        {
            if (string.IsNullOrEmpty(tipTxt))
            {
                return;
            }

            if (MTUIManager.Instance.TryGetPanel("tipPanel", out MTInteractTipPanel panel))
            {
                MTUIManager.Instance.Open("tipPanel");
                panel.SetText(tipTxt);
            }
        }

        private void HideText()
        {
            if (string.IsNullOrEmpty(tipTxt))
            {
                return;
            }
            
            MTUIManager.Instance.Close("tipPanel");
        }
    }
}