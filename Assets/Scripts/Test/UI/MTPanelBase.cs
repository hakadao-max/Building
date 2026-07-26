using UnityEngine;

namespace Test
{
    public class MTPanelBase : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }
        
        protected virtual void OnHide(){}
        
    }
}