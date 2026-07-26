using Suburb;
using UnityEngine;

namespace Test.Trigger
{
    public class MTOpenCloseTrigger : MTTipTrigger
    {
        public SimpleOpenClose openCloseCom;
        protected override void OnClick()
        {
            base.OnClick();
            openCloseCom.ObjectClicked();
        }
    }
}