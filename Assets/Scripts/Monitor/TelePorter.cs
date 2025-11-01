using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TVScreenTeleporter : MonoBehaviour
{
    [Header("Refs")]
    public TVScreenController tv;          // drag your TV here
    public Transform[] arrivalPoints;      // optional: per-camera arrival markers

    [Header("Defaults")]
    public float forwardOffset = 1.2f;     // if no arrival point, spawn in front of cam
    public bool matchCameraYawOnly = true; // keep player upright
    public LayerMask groundMask = ~0;      // snap-to-ground mask
    public float snapDownDistance = 3f;

    [Header("Gating")]
    public bool requirePowerOn = true;     // only works when TV is on

    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true; // must be trigger for touch
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Warp(other.transform);
    }

    // You can also call this from a button (see TVTeleportInteract below)
    public void Warp(Transform playerRoot)
    {
        if (!tv || tv.SourceCount == 0) return;
        if (requirePowerOn && !tv.IsOn) return;

        Camera cam = tv.sources[tv.ActiveIndex];
        if (!cam) return;

        // 1) Choose destination
        Vector3 dstPos; Quaternion dstRot;

        // Prefer authored arrival point for this feed
        if (arrivalPoints != null &&
            tv.ActiveIndex < arrivalPoints.Length &&
            arrivalPoints[tv.ActiveIndex] != null)
        {
            dstPos = arrivalPoints[tv.ActiveIndex].position;
            dstRot = arrivalPoints[tv.ActiveIndex].rotation;
        }
        else
        {
            // Fallback: in front of the camera, with (optional) yaw-only rotation
            var cT = cam.transform;
            dstPos = cT.position + cT.forward * forwardOffset;

            if (matchCameraYawOnly)
                dstRot = Quaternion.Euler(0f, cT.eulerAngles.y, 0f);
            else
                dstRot = cT.rotation;
        }

        // 2) Snap to ground (optional)
        if (Physics.Raycast(dstPos + Vector3.up, Vector3.down, out var hit, snapDownDistance + 1f, groundMask))
            dstPos = hit.point;

        // 3) Move the player safely (works with CharacterController)
        var cc = playerRoot.GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            playerRoot.SetPositionAndRotation(dstPos, dstRot);
            cc.enabled = true;
        }
        else
        {
            playerRoot.SetPositionAndRotation(dstPos, dstRot);
        }
    }
}
