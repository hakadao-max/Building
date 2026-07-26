using UnityEngine;

namespace Test
{
    public class MTThirdViewMode : MTPlayerViewMode
    {
        public float cameraDistance;
        public float cameraHeight;

        public GameObject model;

        [SerializeField] private float characterTurnSpeed = 720f;


        [Header("动画参数")] [LabelText("待机状态名")] [SerializeField]
        private string idleStateName = "idle1";

        [LabelText("行走状态名")] [SerializeField] private string walkStateName = "walk";

        [LabelText("奔跑状态名")] [SerializeField] private string runStateName = "run";

        private Animator animator;


        protected override void OnInit()
        {
            base.OnInit();
            animator = model.GetComponent<Animator>();
        }


        public override void Tick()
        {
            TickCamera();
            TickTransform();
            TickAnimation();
        }

        protected override void OnEnter()
        {
            playerManager.CameraController.ReSet();
            playerManager.CameraController.SetMoveOffset(new Vector3(0, cameraHeight, 0));
            playerManager.CameraController.SetLookAtOffset(cameraDistance);

            model.SetActive(true);
        }

        protected override void OnExit()
        {
            model.SetActive(false);
        }

        private void TickCamera()
        {
            playerManager.CameraController.TickYawPitch(true);
        }

        private void TickTransform()
        {
            var dir = RuntimeInput.GetMoveDir();
            // dir = playerManager.CameraController.GetTransformedDir(dir);
            var yawRotation = Quaternion.Euler(0, playerManager.CameraController.Yaw, 0);
            dir = yawRotation * dir;
            playerManager.MoveCom?.Tick(dir);

            if (playerManager.MoveCom.IsIdle)
            {
                return;
            }

            dir.y = 0;
            dir = Vector3.ClampMagnitude(new Vector3(dir.x, 0f, dir.z), 1f);

            var targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                characterTurnSpeed * Time.deltaTime
            );
        }


        private void TickAnimation()
        {
            if (playerManager.MoveCom.IsIdle)
            {
                animator.Play(idleStateName);
            }
            else if (playerManager.MoveCom.IsRunning)
            {
                animator.Play(runStateName);
            }
            else
            {
                animator.Play(walkStateName);
            }
        }
    }
}