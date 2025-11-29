using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TVScreenTeleporter : MonoBehaviour
{
    [Header("Refs")]
<<<<<<< Updated upstream
    public TVScreenController tv;          // drag your TV here
    public Transform[] arrivalPoints;      // optional: per-camera arrival markers
=======
    public TVScreenController tv;     // Has cameras inside it
    public Transform[] arrivalPoints; // Teleport locations for each camera index
    public Spawner spawner;           // Will use camera index to enable spawn point
>>>>>>> Stashed changes

    [Header("Defaults")]
    public float forwardOffset = 1.2f;     // if no arrival point, spawn in front of cam
    public bool matchCameraYawOnly = true; // keep player upright
    public LayerMask groundMask = ~0;      // snap-to-ground mask
    public float snapDownDistance = 3f;

    [Header("Gating")]
<<<<<<< Updated upstream
    public bool requirePowerOn = true;     // only works when TV is on
=======
    public bool requirePowerOn = true;
>>>>>>> Stashed changes

    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
<<<<<<< Updated upstream
        col.isTrigger = true; // must be trigger for touch
=======
        col.isTrigger = true;

        // Sync spawner immediately with current TV index
        if (spawner != null && tv != null)
            spawner.SetActiveDroneSpawnIndex(tv.ActiveIndex);

        // Subscribe to TV index change if controller exposes an event
        if (tv != null)
            tv.OnActiveIndexChanged += HandleCameraIndexChanged;
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (tv != null)
            tv.OnActiveIndexChanged -= HandleCameraIndexChanged;
>>>>>>> Stashed changes
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
<<<<<<< Updated upstream
        Warp(other.transform);
    }

    // You can also call this from a button (see TVTeleportInteract below)
=======

        Debug.Log($"[Teleporter] Player entered. TV ActiveIndex = {tv.ActiveIndex}");

        // Teleport player
        Warp(other.transform);

        // Enable the spawn point that matches current camera index
        if (spawner != null)
            spawner.SetActiveDroneSpawnIndex(tv.ActiveIndex);
        else
            Debug.LogWarning("[Teleporter] Spawner reference is missing!");
    }

    // -----------------
    // Teleport player
    // -----------------
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
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
=======
            var ct = cam.transform;
            dstPos = ct.position + ct.forward * forwardOffset;
            dstRot = matchCameraYawOnly
                ? Quaternion.Euler(0f, ct.eulerAngles.y, 0f)
                : ct.rotation;
        }

        if (Physics.Raycast(dstPos + Vector3.up, Vector3.down, out var hit, snapDownDistance, groundMask))
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
}
=======

    // -----------------
    // React to camera index change
    // -----------------
    private void HandleCameraIndexChanged(int newIndex)
    {
        Debug.Log($"[Teleporter] Camera index changed to {newIndex}");
        if (spawner != null)
            spawner.SetActiveDroneSpawnIndex(newIndex);
    }
}
>>>>>>> Stashed changes
