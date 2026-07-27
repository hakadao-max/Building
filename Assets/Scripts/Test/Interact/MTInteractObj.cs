using UnityEngine;
using UnityEngine.Serialization;

namespace Test.Interact
{
    public class MTInteractObj : MTBaseInteractObj
    {
        protected override void OnTick(Vector3 oriPos, Vector3 lookRotation)
        {
            var resolvedPosition = GetResolvedPosition(oriPos + lookRotation * StartDistance);
            transform.position = resolvedPosition;
        }

    }
}
