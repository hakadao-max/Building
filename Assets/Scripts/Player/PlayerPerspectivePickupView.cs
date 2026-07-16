using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerPerspectivePickupView : MonoBehaviour
{
    [LabelText("透视拾取模式按键")]
    [SerializeField] private KeyCode activationKey = KeyCode.Alpha7;

    [LabelText("拾取交互按键")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private PerspectivePickupObject heldObject;
    private Camera playerCamera;
    private Collider[] playerColliders;
    private bool isActive;
    private SimplePlayerController controller;

    public bool HasHeldObject => heldObject != null;
    public bool IsActivationRequested => activationKey != KeyCode.None && RuntimeInput.GetKeyDown(activationKey);

    private void Update()
    {
        if (controller == null || !GameController.PlayerControlEnabled)
        {
            return;
        }

        if (IsActivationRequested)
        {
            controller.ApplyViewMode(PlayerViewMode.PerspectivePickup);
        }

        if (isActive)
        {
            TryHandleInteraction(interactKey, playerCamera, playerColliders);
        }
    }

    private void LateUpdate()
    {
        if (isActive)
        {
            TickHeldObject();
        }
    }

    public void Enter(Camera camera, Collider[] colliders)
    {
        playerCamera = camera;
        playerColliders = colliders;
        isActive = true;
    }

    public void Bind(SimplePlayerController owner)
    {
        controller = owner;
    }

    public bool TryHandleInteraction(KeyCode interactKey, Camera playerCamera, Collider[] playerColliders)
    {
        if (interactKey == KeyCode.None || !RuntimeInput.GetKeyDown(interactKey))
        {
            return false;
        }

        if (heldObject != null)
        {
            ReleaseHeldObject();
            return true;
        }

        PerspectivePickupObject pickupObject = FindPickupObject(playerCamera);
        if (pickupObject == null || !pickupObject.TryPickup(playerCamera, playerColliders))
        {
            return false;
        }

        heldObject = pickupObject;
        return true;
    }

    public void TickHeldObject()
    {
        if (heldObject != null)
        {
            heldObject.TickHeldObject();
        }
    }

    public void Exit()
    {
        isActive = false;
        ReleaseHeldObject();
    }

    private static PerspectivePickupObject FindPickupObject(Camera playerCamera)
    {
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            Mathf.Infinity,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        PerspectivePickupObject nearestObject = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (RaycastHit hit in hits)
        {
            PerspectivePickupObject candidate = hit.collider.GetComponentInParent<PerspectivePickupObject>();
            if (candidate != null
                && !candidate.IsHeld
                && candidate.IsWithinPickupDistance(hit.distance)
                && hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestObject = candidate;
            }
        }

        return nearestObject;
    }

    private void ReleaseHeldObject()
    {
        if (heldObject == null)
        {
            return;
        }

        heldObject.Release();
        heldObject = null;
    }
}
