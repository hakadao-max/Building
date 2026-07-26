using System;
using TMPro;
using UnityEngine;

namespace Test
{
    public class WorldTipItem : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI content;

        private Transform followTarget = null;

        public float yawRange;
        private Quaternion defaultRot;

        public void Hide()
        {
            title.text = null;
            content.text = null;
            followTarget = null;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (followTarget != null)
            {
                var targetDir = transform.position - followTarget.position;
                targetDir = Quaternion.Inverse(defaultRot) * targetDir;
                float yaw = Mathf.Atan2(targetDir.x,targetDir.z) * Mathf.Rad2Deg;
                
                yaw = Mathf.Clamp(yaw, -yawRange, yawRange);
                var targetRot = defaultRot * Quaternion.Euler(0, yaw, 0);
                transform.rotation = targetRot;
            }
            
        }

        public void Show(string s, string content1, Vector3 worldPos, Quaternion rot)
        {
            title.text = s;
            content.text = content1;
            
            transform.position = worldPos;
            transform.rotation = rot;
            defaultRot = rot;
            gameObject.SetActive(true);
        }

        public void Show(string s, string content1, Vector3 worldPos, Quaternion rot, Transform target)
        {
            Show(s, content1, worldPos, rot);
            followTarget = target;
        }
    }
}