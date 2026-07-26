using System;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    public class MTTeleportPanel : MTPanelBase
    {

        public Button btnClose;
        public RawImage imgMap;

        public RenderTexture rt;

        private float left; 
        private float right; 
        private float top; 
        private float bottom; 

        private void Awake()
        {
            btnClose.onClick.AddListener(Hide);
            InitMiniMapRect();

        }

        private void InitMiniMapRect()
        {
            var cam = GameObject.Find("MiniMapCamera").GetComponent<Camera>(); 
            rt =cam.targetTexture;
            imgMap.texture = rt;
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            Vector3 camPos = cam.transform.position;

            left = camPos.x - halfWidth;
            right = camPos.x + halfWidth;
            top = camPos.z + halfHeight;
            bottom = camPos.z - halfHeight;

            // Vector2 topsLeft = new Vector2(left, top);
            // Vector2 bottomRight = new Vector2(right, bottom);
            //
            // new GameObject("1").transform.position = new Vector3(left, 0, top);
            // new GameObject("2").transform.position = new Vector3(left, 0, bottom);
            // new GameObject("3").transform.position = new Vector3(right, 0, bottom);
            // new GameObject("4").transform.position = new Vector3(right, 0, top);
        }

        public bool GetPos(out Vector3 pos)
        {
            pos = Vector3.zero;
            RectTransform rectTrans = imgMap.rectTransform;
            
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTrans,
                    RuntimeInput.GetPointerPosition(),
                    null,
                    out Vector2 localPoint))
            {
                return false;
            }
            var normalizedPosition = Vector2.zero;
            Rect rect = rectTrans.rect;
            normalizedPosition.x = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            normalizedPosition.y = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
            Debug.Log(normalizedPosition);
            pos = new Vector3(normalizedPosition.x * (left + right), normalizedPosition.y * (top + bottom));
            return normalizedPosition.x >= 0f && normalizedPosition.x <= 1f
                                              && normalizedPosition.y >= 0f && normalizedPosition.y <= 1f;
        }
    }
}