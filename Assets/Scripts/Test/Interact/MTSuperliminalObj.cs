using UnityEngine;

namespace Test.Interact
{
    public class MTSuperliminalObj : MTBaseInteractObj
    {
        public float maxDistance = 10;

        private float originDistance;
        private float originScale;
        private Vector3 targetScale;

        protected override void OnBeginInteract(Vector3 oriPos, Collider targetCollider)
        {
            base.OnBeginInteract(oriPos, targetCollider);
            originDistance = Vector3.Distance(oriPos, transform.position);
            originScale = transform.localScale.x;
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
            return;
            var resolvedPosition = targetRigidbody.position;
            var originPos = resolvedPosition;
            var oriScale = transform.localScale;
            transform.localScale = Vector3.one * 0.1f;
            targetRigidbody.position = oriPos;
            Physics.SyncTransforms();
            if (targetRigidbody.SweepTest(
                    lookRotation,
                    out RaycastHit hit,
                    maxDistance,
                    QueryTriggerInteraction.Ignore))
            {
                float allowedDistance = Mathf.Max(0f, hit.distance - skinWidth);
                Vector3 allowedMovement = lookRotation * allowedDistance;
                resolvedPosition = oriPos + allowedMovement;
            }
            else
            {
                resolvedPosition = oriPos + lookRotation * maxDistance;
            }

            //targetRigidbody.position = originPos;
            // var resolvedPosition = GetResolvedPosition(targetPos);
            float curDistance = Vector3.Distance(oriPos, resolvedPosition);
            float s = curDistance / originDistance;
            targetScale = Vector3.one * s;
            transform.localScale = targetScale;
            Physics.SyncTransforms();
            if (targetRigidbody.SweepTest(
                    lookRotation,
                    out hit,
                    maxDistance,
                    QueryTriggerInteraction.Ignore))
            {
                float allowedDistance = Mathf.Max(0f, hit.distance - skinWidth);
                Vector3 allowedMovement = lookRotation * allowedDistance;
                resolvedPosition = oriPos + allowedMovement;
            }
            else
            {
                resolvedPosition = oriPos + lookRotation * maxDistance;
            }


            curDistance = Vector3.Distance(oriPos, resolvedPosition);
            s = curDistance / originDistance;
            targetScale = Vector3.one * s;
            targetRigidbody.position = resolvedPosition;
            transform.position = resolvedPosition;
            transform.localScale = targetScale;
        }

        private void tte(Vector3 oriPos, Vector3 lookRotation)
        {
            Vector3 resolvedPosition;
            var lstScale = Vector3.one * 0.1f;
            transform.localScale = lstScale;
            targetRigidbody.position = oriPos;
            transform.position = oriPos;
            Physics.SyncTransforms();
            float lastAllowedDistance;
            if (targetRigidbody.SweepTest(
                    lookRotation,
                    out RaycastHit hit,
                    maxDistance,
                    QueryTriggerInteraction.Ignore))
            {
                lastAllowedDistance = Mathf.Max(0f, hit.distance - skinWidth);
                Vector3 allowedMovement = lookRotation * lastAllowedDistance;
                resolvedPosition = oriPos + allowedMovement;
            }
            else
            {
                lastAllowedDistance = maxDistance;
                resolvedPosition = oriPos + lookRotation * maxDistance;
            }

            float curDistance;
            float s;
            for (int i = 0; i < 4; i++)
            {
                float curAllowedDistance = maxDistance;
                curDistance = Vector3.Distance(oriPos, resolvedPosition);
                s = curDistance / originDistance;
                lstScale = (lstScale +  Vector3.one * s) * 0.5f ;
                transform.localScale = lstScale;
                Physics.SyncTransforms();
                if (targetRigidbody.SweepTest(
                        lookRotation,
                        out hit,
                        maxDistance,
                        QueryTriggerInteraction.Ignore))
                {
                    curAllowedDistance = Mathf.Max(0f, hit.distance - skinWidth);
                    Vector3 allowedMovement = lookRotation * curAllowedDistance;
                    resolvedPosition = oriPos + allowedMovement;
                }
                else
                {
                    resolvedPosition = oriPos + lookRotation * maxDistance;
                }

                if (Mathf.Abs(s - lstScale.x) < 0.1f)
                {
                    break;
                }
                lastAllowedDistance = curAllowedDistance;
            }


            curDistance = Vector3.Distance(oriPos, resolvedPosition);
            s = curDistance / originDistance;
            targetScale = Vector3.one * s;
            targetRigidbody.position = resolvedPosition;
            transform.position = resolvedPosition;
            transform.localScale = targetScale;
        }
    }
}