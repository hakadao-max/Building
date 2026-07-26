using UnityEngine;

namespace Test
{
    public class MTFirstViewMode : MTPlayerViewMode
    {
        public Vector3 offset;
        private MTMoveCom moveCom;
        protected override void OnInit()
        {
            base.OnInit();
            moveCom = GetComponent<MTMoveCom>();
        }


        public override void Tick()
        {
            TickCamera();
            TickTransform();
            TickLight();
        }

        protected override void OnEnter()
        {
            playerManager.CameraController.ReSet();
            playerManager.CameraController.SetMoveOffset(offset);
        }

        private void TickCamera()
        {
            playerManager.CameraController.TickYawPitch(true);
        }

        private void TickTransform()
        {
            var dir = RuntimeInput.GetMoveDir();
            dir = CameraController.GetTransformedDir(dir);
            moveCom?.Tick(dir);
            
            transform.rotation = Quaternion.Euler(0f, CameraController.Yaw, 0f);
        }

        private void TickLight()
        {
            playerManager.LightController.TickLightPitch(CameraController.Pitch);
        }
        
    }
}