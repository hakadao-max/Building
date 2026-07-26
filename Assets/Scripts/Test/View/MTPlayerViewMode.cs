using UnityEngine;

namespace Test
{
    public enum MTViewMode
    {
        First,
        Third,
    }

    public abstract class MTPlayerViewMode : MonoBehaviour
    {
        protected MTPlayerManager playerManager;

        protected MTCameraController CameraController => playerManager.CameraController;
        public void Init(MTPlayerManager playerManager)
        {
            this.playerManager = playerManager;
            OnInit();
        }

        public void Enter()
        {
            OnEnter();
        }

        public void Exit()
        {
            OnExit();
        }

        protected virtual void OnExit()
        {
            
        }

        protected virtual void OnInit()
        {
        }

        public abstract void Tick();

        protected abstract void OnEnter();

    }
}