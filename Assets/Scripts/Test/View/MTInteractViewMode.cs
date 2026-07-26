using Test.Interact;
using UnityEngine;

namespace Test
{
    public class MTInteractViewMode : MTFirstViewMode
    {
        [LabelText("交互按键")]
        public KeyCode Key = KeyCode.Q;

        [LabelText("检测距离")]
        public float checkDistance = 3f;

        private Vector3 oriPos => playerManager.transform.position + offset;

        [LabelText("可交互物体层")]
        public LayerMask checkObjLayer;
        private Quaternion LookRotation => Quaternion.Euler(
            CameraController.Pitch,
            CameraController.Yaw,
            0f
        );
        
        private Vector3 targetPos => oriPos + LookRotation * Vector3.forward * currentObj.floatDistance;
        
        private MTBaseInteractObj currentObj;
        public override void Tick()
        {
            base.Tick();
            CheckInteractableObj();
            UpdateInteractableObj();
        }

        private void UpdateInteractableObj()
        {
            if (currentObj == null)
            {
                return;
            }

            currentObj.Tick(oriPos,LookRotation * Vector3.forward);
        }

        private void CheckInteractableObj()
        {
            if (RuntimeInput.GetKeyDown(Key))
            {
                if (currentObj != null)
                {
                    currentObj.EndInteract();
                    currentObj = null;
                    return;
                }

                if (Physics.Raycast(oriPos,  playerManager.CameraController.transform.forward, out var hit, checkDistance, checkObjLayer))
                {
                    currentObj = hit.transform.GetComponent<MTBaseInteractObj>();
                    if (currentObj != null)
                    {
                        currentObj.BeginInteract(playerManager.Collider,oriPos, targetPos);
                    }
                }
            }
        }
    }
}
