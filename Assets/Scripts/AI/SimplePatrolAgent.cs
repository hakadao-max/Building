using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SimplePatrolAgent : MonoBehaviour
{
    [LabelText("巡逻路线")]
    [SerializeField] private SimplePatrolRoute route;

    [LabelText("启动时自动巡逻")]
    [SerializeField] private bool playOnStart = true;

    [LabelText("启动时移动到起点")]
    [SerializeField] private bool snapToStartWaypoint;

    [LabelText("起点索引")]
    [SerializeField] private int startWaypointIndex;

    [LabelText("循环方式")]
    [SerializeField] private SimplePatrolLoopMode loopMode = SimplePatrolLoopMode.Loop;

    [LabelText("行走速度")]
    [SerializeField] private float moveSpeed = 1.5f;

    [LabelText("到点距离")]
    [SerializeField] private float arriveDistance = 0.15f;

    [LabelText("到点停留时间")]
    [SerializeField] private float waitAtWaypoint = 1.5f;

    [LabelText("移动时转向")]
    [SerializeField] private bool rotateToMoveDirection = true;

    [LabelText("转向速度")]
    [SerializeField] private float rotationSpeed = 8f;

    [LabelText("模型朝向偏移")]
    [SerializeField] private float modelForwardYawOffset;

    [LabelText("使用重力")]
    [SerializeField] private bool useGravity = true;

    [LabelText("重力")]
    [SerializeField] private float gravity = -24f;

    [LabelText("跟随路线点高度")]
    [SerializeField] private bool followWaypointHeight;

    [LabelText("定时贴地")]
    [SerializeField] private bool snapToGround = true;

    [LabelText("贴地检测间隔")]
    [SerializeField] private float groundSnapInterval = 0.25f;

    [LabelText("贴地检测上方高度")]
    [SerializeField] private float groundProbeHeight = 1.5f;

    [LabelText("贴地检测距离")]
    [SerializeField] private float groundProbeDistance = 4f;

    [LabelText("贴地高度偏移")]
    [SerializeField] private float groundOffset;

    [LabelText("地面检测层")]
    [SerializeField] private LayerMask groundLayerMask = ~0;

    [LabelText("启用相遇交流")]
    [SerializeField] private bool enableEncounterCommunication = true;

    [LabelText("交流检测半径")]
    [SerializeField] private float encounterRadius = 1.2f;

    [LabelText("交流最大高度差")]
    [SerializeField] private float maxEncounterHeightDifference = 1.5f;

    [LabelText("交流时长")]
    [SerializeField] private float communicationDuration = 3f;

    [LabelText("交流冷却时间")]
    [SerializeField] private float encounterCooldown = 4f;

    [LabelText("交流后反向离开")]
    [SerializeField] private bool reverseAfterCommunication = true;

    [LabelText("再次交流分离距离")]
    [SerializeField] private float postCommunicationSeparationDistance = 2f;

    [LabelText("交流时互相看向")]
    [SerializeField] private bool facePartnerWhileCommunicating = true;

    [LabelText("动画组件")]
    [SerializeField] private Animator animator;

    [LabelText("自动播放动画")]
    [SerializeField] private bool animate = true;

    [LabelText("禁用动画Root Motion")]
    [SerializeField] private bool disableAnimatorRootMotion = true;

    [LabelText("待机状态名")]
    [SerializeField] private string idleStateName = "idle1";

    [LabelText("行走状态名")]
    [SerializeField] private string walkStateName = "walk";

    [LabelText("动画过渡时间")]
    [SerializeField] private float crossFadeDuration = 0.15f;

    private const float GroundedStickForce = -2f;
    private const int GroundHitBufferSize = 16;

    private static readonly List<SimplePatrolAgent> ActiveAgents = new List<SimplePatrolAgent>();

    private CharacterController characterController;
    private readonly RaycastHit[] groundHitBuffer = new RaycastHit[GroundHitBufferSize];
    private int currentWaypointIndex;
    private int targetWaypointIndex;
    private int moveDirectionSign = 1;
    private int idleStateHash;
    private int walkStateHash;
    private int currentAnimationHash;
    private float waitTimer;
    private float verticalVelocity;
    private float groundSnapTimer;
    private float communicationTimer;
    private float encounterCooldownTimer;
    private bool isPatrolling;
    private SimplePatrolAgent communicationPartner;
    private SimplePatrolAgent recentCommunicationPartner;

    private bool IsCommunicating => communicationTimer > 0f;

    private void Reset()
    {
        route = GetComponentInParent<SimplePatrolRoute>();
        animator = GetComponentInChildren<Animator>(true);
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        EnsureAnimator();
        RefreshAnimationHashes();
    }

    private void OnEnable()
    {
        if (!ActiveAgents.Contains(this))
        {
            ActiveAgents.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveAgents.Remove(this);
        BreakCommunication();
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartPatrol();
        }
        else
        {
            PlayAnimation(idleStateHash);
        }
    }

    private void Update()
    {
        UpdateEncounterCooldown();

        if (IsCommunicating)
        {
            UpdateCommunication();
            HandleTimedGroundSnap();
            return;
        }

        if (!isPatrolling || !HasUsableRoute())
        {
            PlayAnimation(idleStateHash);
            HandleTimedGroundSnap();
            TryStartEncounterCommunication();
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            PlayAnimation(idleStateHash);

            if (waitTimer <= 0f)
            {
                AdvanceTargetWaypoint();
            }

            HandleTimedGroundSnap();
            TryStartEncounterCommunication();
            return;
        }

        MoveToTargetWaypoint();
        HandleTimedGroundSnap();
        TryStartEncounterCommunication();
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        arriveDistance = Mathf.Max(0.01f, arriveDistance);
        waitAtWaypoint = Mathf.Max(0f, waitAtWaypoint);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        groundSnapInterval = Mathf.Max(0.05f, groundSnapInterval);
        groundProbeHeight = Mathf.Max(0.1f, groundProbeHeight);
        groundProbeDistance = Mathf.Max(0.1f, groundProbeDistance);
        encounterRadius = Mathf.Max(0.1f, encounterRadius);
        maxEncounterHeightDifference = Mathf.Max(0.1f, maxEncounterHeightDifference);
        communicationDuration = Mathf.Max(0.1f, communicationDuration);
        encounterCooldown = Mathf.Max(0f, encounterCooldown);
        postCommunicationSeparationDistance = Mathf.Max(encounterRadius, postCommunicationSeparationDistance);
        crossFadeDuration = Mathf.Max(0f, crossFadeDuration);
        gravity = Mathf.Approximately(gravity, 0f) ? -24f : -Mathf.Abs(gravity);
        RefreshAnimationHashes();
    }

    public void SetRoute(SimplePatrolRoute patrolRoute)
    {
        route = patrolRoute;
    }

    public void StartPatrol()
    {
        if (!HasUsableRoute())
        {
            isPatrolling = false;
            PlayAnimation(idleStateHash);
            return;
        }

        currentWaypointIndex = route.ClampWaypointIndex(startWaypointIndex);
        targetWaypointIndex = currentWaypointIndex;
        moveDirectionSign = 1;
        waitTimer = 0f;
        verticalVelocity = 0f;
        groundSnapTimer = 0f;
        communicationTimer = 0f;
        communicationPartner = null;
        recentCommunicationPartner = null;
        isPatrolling = true;

        if (snapToStartWaypoint && route.TryGetWaypointPosition(currentWaypointIndex, out Vector3 startPosition))
        {
            SetAgentPosition(startPosition);
        }

        SnapToGroundNow();
        ArriveAtTargetWaypoint();
    }

    public void StopPatrol()
    {
        isPatrolling = false;
        waitTimer = 0f;
        BreakCommunication();
        PlayAnimation(idleStateHash);
    }

    public void ResumePatrol()
    {
        if (HasUsableRoute())
        {
            isPatrolling = true;
        }
    }

    private void MoveToTargetWaypoint()
    {
        if (!route.TryGetWaypointPosition(targetWaypointIndex, out Vector3 targetPosition))
        {
            AdvanceTargetWaypoint();
            return;
        }

        if (moveSpeed <= 0f)
        {
            PlayAnimation(idleStateHash);
            return;
        }

        Vector3 toTarget = targetPosition - transform.position;
        if (!followWaypointHeight || (characterController != null && useGravity))
        {
            toTarget.y = 0f;
        }

        float distance = toTarget.magnitude;
        if (distance <= arriveDistance)
        {
            ArriveAtTargetWaypoint();
            return;
        }

        Vector3 moveDirection = toTarget / distance;
        float stepDistance = Mathf.Min(moveSpeed * Time.deltaTime, distance);
        Vector3 horizontalDisplacement = moveDirection * stepDistance;

        MoveAgent(horizontalDisplacement);

        if (rotateToMoveDirection)
        {
            RotateTowards(moveDirection);
        }

        PlayAnimation(walkStateHash);
    }

    private void MoveAgent(Vector3 displacement)
    {
        if (characterController != null && characterController.enabled)
        {
            Vector3 controllerDisplacement = displacement;

            if (useGravity)
            {
                if (characterController.isGrounded && verticalVelocity < 0f)
                {
                    verticalVelocity = GroundedStickForce;
                }

                verticalVelocity += gravity * Time.deltaTime;
                controllerDisplacement.y += verticalVelocity * Time.deltaTime;
            }

            characterController.Move(controllerDisplacement);
            return;
        }

        transform.position += displacement;
    }

    private void UpdateEncounterCooldown()
    {
        if (encounterCooldownTimer > 0f)
        {
            encounterCooldownTimer -= Time.deltaTime;
        }
    }

    private void TryStartEncounterCommunication()
    {
        if (!CanStartEncounter())
        {
            return;
        }

        float sqrEncounterRadius = encounterRadius * encounterRadius;
        for (int i = 0; i < ActiveAgents.Count; i++)
        {
            SimplePatrolAgent other = ActiveAgents[i];
            if (other == null || other == this || !other.CanStartEncounter() || IsRecentPartnerTooClose(other))
            {
                continue;
            }

            Vector3 offset = other.transform.position - transform.position;
            if (Mathf.Abs(offset.y) > maxEncounterHeightDifference)
            {
                continue;
            }

            offset.y = 0f;
            if (offset.sqrMagnitude > sqrEncounterRadius)
            {
                continue;
            }

            BeginCommunication(other);
            other.BeginCommunication(this);
            return;
        }
    }

    private bool CanStartEncounter()
    {
        return enableEncounterCommunication
            && isActiveAndEnabled
            && !IsCommunicating
            && encounterCooldownTimer <= 0f;
    }

    private void BeginCommunication(SimplePatrolAgent partner)
    {
        communicationPartner = partner;
        communicationTimer = communicationDuration;
        verticalVelocity = 0f;
        PlayAnimation(idleStateHash);
    }

    private void UpdateCommunication()
    {
        communicationTimer -= Time.deltaTime;
        PlayAnimation(idleStateHash);

        if (facePartnerWhileCommunicating && communicationPartner != null)
        {
            Vector3 lookDirection = communicationPartner.transform.position - transform.position;
            RotateTowards(lookDirection);
        }

        if (communicationTimer <= 0f)
        {
            FinishCommunication();
        }
    }

    private void FinishCommunication()
    {
        SimplePatrolAgent finishedPartner = communicationPartner;
        communicationTimer = 0f;
        communicationPartner = null;
        recentCommunicationPartner = finishedPartner;
        encounterCooldownTimer = encounterCooldown;

        if (reverseAfterCommunication)
        {
            ReversePatrolDirectionAfterCommunication();
        }
    }

    private void BreakCommunication()
    {
        SimplePatrolAgent partner = communicationPartner;
        communicationPartner = null;
        communicationTimer = 0f;
        encounterCooldownTimer = Mathf.Max(encounterCooldownTimer, encounterCooldown);
        recentCommunicationPartner = partner;

        if (partner != null && partner.communicationPartner == this)
        {
            partner.communicationPartner = null;
            partner.communicationTimer = 0f;
            partner.encounterCooldownTimer = Mathf.Max(partner.encounterCooldownTimer, partner.encounterCooldown);
            partner.recentCommunicationPartner = this;
        }
    }

    private void ReversePatrolDirectionAfterCommunication()
    {
        if (!HasUsableRoute() || route.WaypointCount <= 1)
        {
            return;
        }

        moveDirectionSign *= -1;
        waitTimer = 0f;

        if (targetWaypointIndex != currentWaypointIndex)
        {
            targetWaypointIndex = currentWaypointIndex;
            return;
        }

        if (TryGetNextWaypointIndex(currentWaypointIndex, out int nextWaypointIndex))
        {
            targetWaypointIndex = nextWaypointIndex;
            return;
        }

        isPatrolling = false;
        PlayAnimation(idleStateHash);
    }

    private bool IsRecentPartnerTooClose(SimplePatrolAgent other)
    {
        if (recentCommunicationPartner != other)
        {
            return false;
        }

        Vector3 offset = other.transform.position - transform.position;
        if (Mathf.Abs(offset.y) > maxEncounterHeightDifference)
        {
            recentCommunicationPartner = null;
            return false;
        }

        offset.y = 0f;
        float separationDistance = Mathf.Max(encounterRadius, postCommunicationSeparationDistance);
        if (offset.sqrMagnitude > separationDistance * separationDistance)
        {
            recentCommunicationPartner = null;
            return false;
        }

        return true;
    }

    private void HandleTimedGroundSnap()
    {
        if (!snapToGround)
        {
            return;
        }

        groundSnapTimer -= Time.deltaTime;
        if (groundSnapTimer > 0f)
        {
            return;
        }

        groundSnapTimer = groundSnapInterval;
        SnapToGroundNow();
    }

    private void SnapToGroundNow()
    {
        if (!TryGetGroundedPosition(out Vector3 groundedPosition))
        {
            return;
        }

        Vector3 currentPosition = transform.position;
        if (Mathf.Abs(currentPosition.y - groundedPosition.y) <= 0.001f)
        {
            return;
        }

        currentPosition.y = groundedPosition.y;
        SetAgentPosition(currentPosition);

        if (verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
        }
    }

    private bool TryGetGroundedPosition(out Vector3 groundedPosition)
    {
        Vector3 origin = transform.position + Vector3.up * groundProbeHeight;
        float maxDistance = groundProbeHeight + groundProbeDistance;
        int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, groundHitBuffer, maxDistance, groundLayerMask, QueryTriggerInteraction.Ignore);

        float nearestDistance = float.PositiveInfinity;
        Vector3 nearestPoint = Vector3.zero;
        bool hasGround = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = groundHitBuffer[i];
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestPoint = hit.point;
                hasGround = true;
            }
        }

        groundedPosition = nearestPoint + Vector3.up * groundOffset;
        return hasGround;
    }

    private void ArriveAtTargetWaypoint()
    {
        currentWaypointIndex = targetWaypointIndex;
        PlayAnimation(idleStateHash);

        if (waitAtWaypoint > 0f)
        {
            waitTimer = waitAtWaypoint;
            return;
        }

        AdvanceTargetWaypoint();
    }

    private void AdvanceTargetWaypoint()
    {
        if (!HasUsableRoute())
        {
            isPatrolling = false;
            return;
        }

        int waypointCount = route.WaypointCount;
        if (waypointCount <= 1)
        {
            isPatrolling = false;
            return;
        }

        if (TryGetNextWaypointIndex(currentWaypointIndex, out int nextIndex))
        {
            targetWaypointIndex = nextIndex;
            return;
        }

        isPatrolling = false;
        PlayAnimation(idleStateHash);
    }

    private bool TryGetNextWaypointIndex(int fromIndex, out int nextIndex)
    {
        nextIndex = fromIndex;

        if (!HasUsableRoute())
        {
            return false;
        }

        int waypointCount = route.WaypointCount;
        if (waypointCount <= 1)
        {
            return false;
        }

        int candidateIndex = fromIndex + moveDirectionSign;

        switch (loopMode)
        {
            case SimplePatrolLoopMode.Loop:
                if ((candidateIndex < 0 || candidateIndex >= waypointCount) && !route.ConnectLastToFirst)
                {
                    return false;
                }

                nextIndex = (candidateIndex + waypointCount) % waypointCount;
                return true;

            case SimplePatrolLoopMode.PingPong:
                if (candidateIndex < 0 || candidateIndex >= waypointCount)
                {
                    moveDirectionSign *= -1;
                    candidateIndex = fromIndex + moveDirectionSign;
                }

                nextIndex = route.ClampWaypointIndex(candidateIndex);
                return true;

            case SimplePatrolLoopMode.Once:
                if (candidateIndex < 0 || candidateIndex >= waypointCount)
                {
                    return false;
                }

                nextIndex = candidateIndex;
                return true;

            default:
                return false;
        }
    }

    private void RotateTowards(Vector3 moveDirection)
    {
        moveDirection.y = 0f;
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up)
            * Quaternion.Euler(0f, modelForwardYawOffset, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void SetAgentPosition(Vector3 position)
    {
        if (characterController != null)
        {
            bool wasEnabled = characterController.enabled;
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = wasEnabled;
            return;
        }

        transform.position = position;
    }

    private bool HasUsableRoute()
    {
        return route != null && route.WaypointCount > 0;
    }

    private void EnsureAnimator()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        if (animator != null && disableAnimatorRootMotion)
        {
            animator.applyRootMotion = false;
        }
    }

    private void RefreshAnimationHashes()
    {
        idleStateHash = string.IsNullOrEmpty(idleStateName) ? 0 : Animator.StringToHash(idleStateName);
        walkStateHash = string.IsNullOrEmpty(walkStateName) ? 0 : Animator.StringToHash(walkStateName);
    }

    private void PlayAnimation(int stateHash)
    {
        if (!animate)
        {
            return;
        }

        EnsureAnimator();
        if (animator == null || animator.runtimeAnimatorController == null || stateHash == 0)
        {
            return;
        }

        if (!animator.isActiveAndEnabled || !animator.HasState(0, stateHash) || currentAnimationHash == stateHash)
        {
            return;
        }

        if (crossFadeDuration <= 0f)
        {
            animator.Play(stateHash, 0);
        }
        else
        {
            animator.CrossFade(stateHash, crossFadeDuration, 0);
        }

        currentAnimationHash = stateHash;
    }
}
