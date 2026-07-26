using System;
using UnityEngine;

namespace Test.Trigger
{
    public class MTTrigger : MonoBehaviour
    {
        public KeyCode key =  KeyCode.E;

        protected bool isStayInTrigger;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }
            isStayInTrigger = true;
            OnEnter();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }
            isStayInTrigger = true;
            OnExit();
        }


        protected virtual void Update()
        {
            if (isStayInTrigger)
            {
                if (key != KeyCode.None && RuntimeInput.GetKeyDown(key))
                {
                    OnClick();
                }
            }
        }

        protected virtual void OnEnter()
        {
            
        }
        protected virtual void OnExit()
        {
            
        }
        protected virtual void OnClick()
        {
            
        }

    }
}