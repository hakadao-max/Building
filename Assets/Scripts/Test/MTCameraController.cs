using System;
using UnityEngine;

namespace Test
{
    public class MTCameraController : MonoBehaviour
    {
        [LabelText("鼠标灵敏度")]
        [SerializeField] private float mouseSensitivity = 2.2f;

        [LabelText("俯仰最小角度")]
        [SerializeField] private float minPitch = -55f;

        [LabelText("俯仰最大角度")]
        [SerializeField] private float maxPitch = 70f;

        [SerializeField]private Transform pivotTarget;

        private Camera camera;
        private float yaw;
        private float pitch;

        public float Yaw => yaw;
        public float Pitch => pitch;

        [SerializeField] private bool test = false;

        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 offset;
        
        private void Awake()
        {
            if (pivotTarget == null)
            {
                pivotTarget = new GameObject("pivotTarget").transform;
                pivotTarget.position = transform.position;
                pivotTarget.rotation = transform.rotation;
                
                this.transform.SetParent(pivotTarget);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }

        public void TickYawPitch(bool lockCursorOnClick)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (lockCursorOnClick && RuntimeInput.GetMouseButtonDown(0))
                {
                    GameController.SetCursorLocked(true);
                }

                return;
            }

            Vector2 mouseDelta = RuntimeInput.GetMouseDelta();
            yaw += mouseDelta.x * mouseSensitivity;
            pitch -= mouseDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        }

        void Update()
        {
            if (!test)
                return;
            TickYawPitch(true);
        }

        private void LateUpdate()
        {
            
            if (!test)
                return;
            TickLook();
        }

        public void TickLook()
        {
            pivotTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
            if (followTarget != null)
            {
                pivotTarget.position = followTarget.position + offset;
            }
        }

        public Vector3 GetTransformedDir(Vector3 dir)
        {
            return pivotTarget.TransformDirection(dir);
        }

        public void ReSet()
        {
            pivotTarget.rotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
        }
        
        public void SetMoveOffset(Vector3 vector3)
        {
            offset = vector3;
        }
        
        public void SetLookAtOffset(float distance)
        {
            transform.localPosition = Vector3.back * distance;
        }
    }
}