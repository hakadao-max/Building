using UnityEngine;

namespace Test.Interact
{
    public class MTSuperliminalObj : MTBaseInteractObj
    {
        [LabelText("最大检测距离")]
        [Min(0.01f)]
        public float maxDistance = 10f;

        [LabelText("首次检测缩放")]
        [Min(0.001f)]
        [SerializeField] private float initialDetectionScale = 0.1f;

        [LabelText("缩放迭代次数")]
        [Min(1)]
        [SerializeField] private int scaleIterationCount = 6;

        [LabelText("缩放收敛误差")]
        [Min(0f)]
        [SerializeField] private float scaleTolerance = 0.01f;

        private float originDistance;
        private float originScale;
        private Vector3 originLocalScale;
        private Vector3 targetScale;

        protected override void OnBeginInteract(Vector3 oriPos, Collider targetCollider)
        {
            base.OnBeginInteract(oriPos, targetCollider);
            originDistance = Mathf.Max(
                Vector3.Distance(oriPos, transform.position),
                0.0001f
            );
            originScale = transform.localScale.x;
            originLocalScale = transform.localScale;
            targetScale = transform.localScale;
            targetRigidbody.detectCollisions = false;
        }

        protected override void OnEndInteract()
        {
            base.OnEndInteract();
            targetRigidbody.detectCollisions = true;
        }

        protected override void OnTick(Vector3 oriPos, Vector3 lookRotation)
        {
            tte(oriPos, lookRotation);
        }

        private void tte(Vector3 oriPos, Vector3 lookRotation)
        {
            Vector3 direction = lookRotation.normalized;
            float probeScale = Mathf.Max(initialDetectionScale, 0.001f);
            float confirmedMinScale = 0.001f;
            float resolvedDistance = SweepAtScale(
                oriPos,
                direction,
                probeScale
            );
            float calculatedScale = CalculateScale(resolvedDistance);

            for (int i = 1; i < scaleIterationCount; i++)
            {
                if (Mathf.Abs(calculatedScale - probeScale) <= scaleTolerance)
                {
                    break;
                }

                if (calculatedScale > probeScale)
                {
                    // 当前尺寸能够完成扫描，它就是一个已经确认可用的下界。
                    confirmedMinScale = Mathf.Max(
                        confirmedMinScale,
                        probeScale
                    );

                    // 结果变大时，在已确认下界与计算值之间继续向上逼近。
                    probeScale =
                        (confirmedMinScale + calculatedScale) * 0.5f;
                }
                else
                {
                    // 当前尺寸过大时，不直接跳到计算值，而是在已确认下界
                    // 与当前探测尺寸之间折半，避免结果突然缩得过小。
                    probeScale =
                        (confirmedMinScale + probeScale) * 0.5f;
                }

                probeScale = Mathf.Max(probeScale, 0.001f);
                resolvedDistance = SweepAtScale(
                    oriPos,
                    direction,
                    probeScale
                );
                calculatedScale = CalculateScale(resolvedDistance);
            }

            resolvedDistance =
                confirmedMinScale * originDistance /
                Mathf.Max(Mathf.Abs(originScale), 0.0001f);

            Vector3 resolvedPosition =
                oriPos + direction * resolvedDistance;

            targetScale = GetLocalScale(confirmedMinScale);
            targetRigidbody.position = resolvedPosition;
            transform.position = resolvedPosition;
            transform.localScale = targetScale;
        }

        private float SweepAtScale(
            Vector3 origin,
            Vector3 direction,
            float probeScale)
        {
            transform.position = origin;
            transform.localScale = GetLocalScale(probeScale);
            Physics.SyncTransforms();

            if (targetRigidbody.SweepTest(
                    direction,
                    out RaycastHit hit,
                    maxDistance,
                    QueryTriggerInteraction.Ignore))
            {
                return Mathf.Max(0f, hit.distance - skinWidth);
            }

            return maxDistance;
        }

        private float CalculateScale(float distance)
        {
            return Mathf.Max(
                originScale * distance / originDistance,
                0.001f
            );
        }

        private Vector3 GetLocalScale(float xScale)
        {
            if (Mathf.Abs(originScale) < 0.0001f)
            {
                return Vector3.one * xScale;
            }

            return originLocalScale * (xScale / originScale);
        }
    }
}
