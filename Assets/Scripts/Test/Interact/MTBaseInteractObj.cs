using UnityEngine;

namespace Test.Interact
{
    [RequireComponent(typeof(Rigidbody))]
    public class MTBaseInteractObj : MonoBehaviour
    {
        private float startDistance = 2f;

        [LabelText("碰撞间隙")]
        [Min(0.001f)]
        [SerializeField] protected float skinWidth = 0.02f;
        
        [LabelText("刚体")]
        [SerializeField] protected Rigidbody targetRigidbody;

        [LabelText("自身碰撞体")]
        protected Collider[] ownedColliders;

        protected Collider holderCollider;

        public float StartDistance => startDistance;
        
        private void Awake()
        {
            if (targetRigidbody == null)
            {
                targetRigidbody = GetComponent<Rigidbody>();
            }

            ownedColliders = GetComponentsInChildren<Collider>();
        }


        public void BeginInteract(Collider targetCollider, Vector3 oriPos)
        {
            startDistance = Vector3.Distance(transform.position, oriPos);
            holderCollider = targetCollider;
            targetRigidbody.isKinematic = true;
            SetColliderIgnored(true);

            OnBeginInteract(oriPos, targetCollider);
        }

        public void EndInteract()
        {
            SetColliderIgnored(false);
            holderCollider = null;
            targetRigidbody.isKinematic = false;
            
            OnEndInteract();
        }

        protected virtual void OnBeginInteract(Vector3 oriPos, Collider targetCollider)
        {
            
        }
        protected virtual void OnEndInteract()
        {
            
        }

        private void SetColliderIgnored(bool ignored)
        {
            if (holderCollider == null)
            {
                return;
            }

            foreach (Collider ownedCollider in ownedColliders)
            {
                if (ownedCollider == null)
                {
                    continue;
                }

                Physics.IgnoreCollision(holderCollider, ownedCollider, ignored);
            }
        }

        protected Vector3 GetResolvedPosition(Vector3 targetPosition)
        {
            if (holderCollider == null)
            {
                return targetPosition;
            }

            Vector3 currentPosition = targetRigidbody.position;
            Vector3 movement = targetPosition - currentPosition;

            float distance = movement.magnitude;
            if (distance < 0.0001f)
            {
                transform.position = currentPosition;
                return currentPosition;
            }

            Vector3 resolvedPosition;
            Vector3 direction = movement / distance;
            if (targetRigidbody.SweepTest(
                    direction,
                    out RaycastHit hit,
                    distance + skinWidth,
                    QueryTriggerInteraction.Ignore))
            {
                float allowedDistance = Mathf.Max(0f, hit.distance - skinWidth);
                Vector3 allowedMovement = direction * allowedDistance;
                Vector3 remainingMovement = movement - allowedMovement;
                Vector3 slideMovement = Vector3.ProjectOnPlane(
                    remainingMovement,
                    hit.normal
                );

                resolvedPosition =
                    currentPosition + allowedMovement + slideMovement;
            }
            else
            {
                resolvedPosition = targetPosition;
            }

            return resolvedPosition;
        }

        public void Tick(Vector3 oriPos, Vector3 lookRotation)
        {
            OnTick(oriPos, lookRotation);
        }

        protected virtual void OnTick(Vector3 oriPos, Vector3 lookRotation)
        {
            transform.position = oriPos + lookRotation * startDistance;
        }
    }
}