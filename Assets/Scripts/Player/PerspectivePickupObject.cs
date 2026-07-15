using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public sealed class PerspectivePickupObject : MonoBehaviour
{
    [Header("拾取配置")]
    [LabelText("可拾取距离")]
    [SerializeField] private float interactionDistance = 2.3f;

    [LabelText("最小缩放倍率")]
    [SerializeField] private float minimumScaleMultiplier = 0.15f;

    [LabelText("最大缩放倍率")]
    [SerializeField] private float maximumScaleMultiplier = 8f;

    [LabelText("最小跟随距离")]
    [SerializeField] private float minimumHoldDistance = 0.5f;

    [LabelText("最大跟随距离")]
    [SerializeField] private float maximumHoldDistance = 30f;

    [LabelText("障碍检测层")]
    [SerializeField] private LayerMask obstructionLayerMask = ~0;

    [LabelText("障碍表面间距")]
    [SerializeField] private float obstructionPadding = 0.03f;

    [LabelText("Box检测次数")]
    [SerializeField] private int boxCheckCount = 32;

    [LabelText("Box前移步长")]
    [SerializeField] private float boxAdvanceStep = 0.25f;

    [LabelText("Box尺寸倍率")]
    [SerializeField] private float boxSizeMultiplier = 0.95f;

    [LabelText("位置跟随速度")]
    [SerializeField] private float positionFollowSpeed = 24f;

    [LabelText("最大下落速度")]
    [SerializeField] private float maximumFallSpeed = 18f;

    private Rigidbody targetRigidbody;
    private Collider[] ownColliders;
    private Camera holdingCamera;
    private Vector3 pickupScale;
    private float pickupDistance;
    private float desiredDistance;
    private Vector3 pickupBoundsCenterOffset;
    private Vector3 pickupBoundsHalfExtents;
    private bool isHeld;
    private Collider[] holderColliders;
    private readonly Collider[] overlapHits = new Collider[64];

    public bool IsHeld => isHeld;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnValidate()
    {
        interactionDistance = Mathf.Max(0.1f, interactionDistance);
        minimumScaleMultiplier = Mathf.Max(0.01f, minimumScaleMultiplier);
        maximumScaleMultiplier = Mathf.Max(minimumScaleMultiplier, maximumScaleMultiplier);
        minimumHoldDistance = Mathf.Max(0.05f, minimumHoldDistance);
        maximumHoldDistance = Mathf.Max(minimumHoldDistance, maximumHoldDistance);
        obstructionPadding = Mathf.Max(0f, obstructionPadding);
        boxCheckCount = Mathf.Clamp(boxCheckCount, 1, 128);
        boxAdvanceStep = Mathf.Max(0.01f, boxAdvanceStep);
        boxSizeMultiplier = Mathf.Clamp(boxSizeMultiplier, 0.1f, 1.5f);
        positionFollowSpeed = Mathf.Max(0f, positionFollowSpeed);
        maximumFallSpeed = Mathf.Max(0.1f, maximumFallSpeed);
    }

    private void FixedUpdate()
    {
        if (isHeld || targetRigidbody == null || targetRigidbody.isKinematic)
        {
            return;
        }

        Vector3 velocity = targetRigidbody.linearVelocity;
        if (velocity.y < -maximumFallSpeed)
        {
            velocity.y = -maximumFallSpeed;
            targetRigidbody.linearVelocity = velocity;
        }
    }

    private void OnDisable()
    {
        if (isHeld)
        {
            Release();
        }
    }

    public bool TryPickup(Camera playerCamera, Collider[] playerColliders)
    {
        if (isHeld || playerCamera == null)
        {
            return false;
        }

        CacheComponents();
        holdingCamera = playerCamera;
        pickupScale = transform.localScale;
        pickupDistance = Mathf.Max(Vector3.Distance(playerCamera.transform.position, transform.position), minimumHoldDistance);
        desiredDistance = maximumHoldDistance;
        CachePickupBounds();

        targetRigidbody.linearVelocity = Vector3.zero;
        targetRigidbody.angularVelocity = Vector3.zero;
        targetRigidbody.useGravity = false;
        targetRigidbody.isKinematic = true;
        holderColliders = playerColliders;
        SetHolderCollisionsIgnored(true);
        isHeld = true;
        return true;
    }

    public bool IsWithinPickupDistance(float hitDistance)
    {
        return hitDistance <= interactionDistance;
    }

    public void TickHeldObject()
    {
        if (!isHeld || holdingCamera == null)
        {
            return;
        }

        Transform cameraTransform = holdingCamera.transform;
        float availableDistance = ResolveAvailableDistance(cameraTransform);
        float currentDistance = ResolveBoxAdjustedDistance(cameraTransform, availableDistance);
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * currentDistance;
        float positionT = positionFollowSpeed <= 0f ? 1f : 1f - Mathf.Exp(-positionFollowSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionT);

        float actualDistance = Mathf.Max(
            Vector3.Distance(cameraTransform.position, transform.position),
            minimumHoldDistance);
        float actualScaleMultiplier = Mathf.Clamp(
            actualDistance / pickupDistance,
            minimumScaleMultiplier,
            maximumScaleMultiplier);
        transform.localScale = pickupScale * actualScaleMultiplier;
    }

    public void Release()
    {
        if (!isHeld)
        {
            return;
        }

        isHeld = false;
        holdingCamera = null;
        SetHolderCollisionsIgnored(false);
        holderColliders = null;
        targetRigidbody.isKinematic = false;
        targetRigidbody.useGravity = true;
        targetRigidbody.WakeUp();
    }

    private float ResolveAvailableDistance(Transform cameraTransform)
    {
        RaycastHit[] hits = Physics.RaycastAll(
            cameraTransform.position,
            cameraTransform.forward,
            desiredDistance,
            obstructionLayerMask,
            QueryTriggerInteraction.Ignore);

        float nearestDistance = desiredDistance;
        foreach (RaycastHit hit in hits)
        {
            if (IsIgnoredCollider(hit.collider))
            {
                continue;
            }

            nearestDistance = Mathf.Min(nearestDistance, hit.distance - obstructionPadding);
        }

        return nearestDistance;
    }

    private float ResolveBoxAdjustedDistance(Transform cameraTransform, float rayDistance)
    {
        float candidateDistance = Mathf.Clamp(rayDistance, minimumHoldDistance, maximumHoldDistance);
        for (int checkIndex = 0; checkIndex < boxCheckCount; checkIndex++)
        {
            float scaleMultiplier = Mathf.Clamp(
                candidateDistance / pickupDistance,
                minimumScaleMultiplier,
                maximumScaleMultiplier);
            Vector3 candidatePosition = cameraTransform.position + cameraTransform.forward * candidateDistance;
            Vector3 candidateCenter = candidatePosition + pickupBoundsCenterOffset * scaleMultiplier;
            Vector3 candidateHalfExtents = pickupBoundsHalfExtents * scaleMultiplier * boxSizeMultiplier
                + Vector3.one * obstructionPadding;

            if (!HasBlockingOverlap(candidateCenter, candidateHalfExtents))
            {
                break;
            }

            candidateDistance = Mathf.Max(minimumHoldDistance, candidateDistance - boxAdvanceStep);
            if (candidateDistance <= minimumHoldDistance)
            {
                break;
            }
        }

        return candidateDistance;
    }

    private bool HasBlockingOverlap(Vector3 center, Vector3 halfExtents)
    {
        int hitCount = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            overlapHits,
            Quaternion.identity,
            obstructionLayerMask,
            QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hitCount; i++)
        {
            if (!IsIgnoredCollider(overlapHits[i]))
            {
                return true;
            }
        }

        return false;
    }

    private void CachePickupBounds()
    {
        bool hasBounds = false;
        Bounds combinedBounds = default;
        foreach (Collider ownCollider in ownColliders)
        {
            if (ownCollider == null || !ownCollider.enabled || ownCollider.isTrigger)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = ownCollider.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(ownCollider.bounds);
            }
        }

        if (!hasBounds)
        {
            pickupBoundsCenterOffset = Vector3.zero;
            pickupBoundsHalfExtents = Vector3.one * 0.5f;
            return;
        }

        pickupBoundsCenterOffset = combinedBounds.center - transform.position;
        pickupBoundsHalfExtents = combinedBounds.extents;
    }

    private bool IsIgnoredCollider(Collider candidate)
    {
        if (candidate == null || IsOwnCollider(candidate))
        {
            return true;
        }

        if (holderColliders == null)
        {
            return false;
        }

        foreach (Collider holderCollider in holderColliders)
        {
            if (candidate == holderCollider)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsOwnCollider(Collider candidate)
    {
        foreach (Collider ownCollider in ownColliders)
        {
            if (candidate == ownCollider)
            {
                return true;
            }
        }

        return false;
    }

    private void SetHolderCollisionsIgnored(bool ignored)
    {
        if (holderColliders == null)
        {
            return;
        }

        foreach (Collider ownCollider in ownColliders)
        {
            if (ownCollider == null)
            {
                continue;
            }

            foreach (Collider holderCollider in holderColliders)
            {
                if (holderCollider != null && holderCollider != ownCollider)
                {
                    Physics.IgnoreCollision(ownCollider, holderCollider, ignored);
                }
            }
        }
    }

    private void CacheComponents()
    {
        if (targetRigidbody == null)
        {
            targetRigidbody = GetComponent<Rigidbody>();
        }

        if (ownColliders == null || ownColliders.Length == 0)
        {
            ownColliders = GetComponentsInChildren<Collider>(true);
        }
    }
}
