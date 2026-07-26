using UnityEngine;

namespace Test.Trigger
{
    public class MTWorldTipTrigger : MTTrigger
    {
        public string title;
        public string content;

        public Vector3 offset;
        public Vector3 rot;
        
        public bool followTarget;
        protected override void OnEnter()
        {
            if (MTUIManager.Instance.TryGetPanel("worldTipPanel", out MTWorldTipPanel panel))
            {
                MTUIManager.Instance.Open("worldTipPanel");
                var target = followTarget ? MTPlayerManager.Instance.transform : null;
                panel.ShowWorldTip(GetHashCode(),title,content,transform.position + offset,Quaternion.Euler(rot),target);
            }
        }

        protected override void OnExit()
        {
            
            if (MTUIManager.Instance.TryGetPanel("worldTipPanel", out MTWorldTipPanel panel))
            {
                var target = followTarget ? MTPlayerManager.Instance.transform : null;
                panel.HideWorldItem(GetHashCode());
            }
        }
    }
}