using UnityEngine;

[DefaultExecutionOrder(-50)]
public class Portal : MonoBehaviour
{
    [Header("Link")]
    public Portal linkedPortal;              // assign the OTHER portal

    [Header("Pieces")]
    public Renderer screenQuad;              // assign the Quad's Renderer
    public Camera portalCam;                 // assign child "PortalCam"
    public Transform screenPlane;            // assign the Quad transform (for plane/orientation)

    [Header("RT")]
    public int rtWidth = 1024;
    public int rtHeight = 1024;
    RenderTexture _rt;

    Camera _playerCam;

    void Awake()
    {
        if (!portalCam) portalCam = GetComponentInChildren<Camera>(true);
        if (!screenPlane && screenQuad) screenPlane = screenQuad.transform;

        // Allocate RT and hook up
        _rt = new RenderTexture(rtWidth, rtHeight, 16, RenderTextureFormat.ARGB32);
        _rt.name = name + "_RT";
        _rt.Create();

        portalCam.targetTexture = _rt;

        // Show RT on the screen quad
        if (screenQuad)
        {
            // Unlit/Texture expected; fallback to material property name "_MainTex"
            var mat = screenQuad.material;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", _rt);
            else mat.mainTexture = _rt;
        }

        portalCam.enabled = true; // let it render each frame
    }

    void Start()
    {
        if (!_playerCam) _playerCam = Camera.main;
    }

    void LateUpdate()
    {
        if (!_playerCam || !linkedPortal) return;

        // Mirror player camera through this portal to the linked portal
        // 1) Player in this portal's local space
        Vector3 localPos = transform.InverseTransformPoint(_playerCam.transform.position);
        Quaternion localRot = Quaternion.Inverse(transform.rotation) * _playerCam.transform.rotation;

        // 2) Mirror through the portal plane (flip X and Z, keep Y)
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        // For rotation, flip around the up axis and forward (equivalent to 180° yaw)
        localRot = new Quaternion(-localRot.x, localRot.y, -localRot.z, -localRot.w);

        // 3) Move into linked portal's space
        Vector3 camPos = linkedPortal.transform.TransformPoint(localPos);
        Quaternion camRot = linkedPortal.transform.rotation * localRot;

        portalCam.transform.SetPositionAndRotation(camPos, camRot);

        // Optional: clip plane to avoid seeing geometry behind the screen surface
        SetObliqueNearClip(portalCam, linkedPortal.screenPlane);
    }

    // Oblique near clip so the camera clips to the portal surface (prevents seeing past the doorway frame)
    static void SetObliqueNearClip(Camera cam, Transform clipPlane)
    {
        if (!cam || !clipPlane) return;

        // Plane in world space: portal forward is "outward"
        Vector3 n = clipPlane.forward;           // plane normal
        Vector3 p = clipPlane.position;          // a point on the plane
        // Slight bias in front of the plane to avoid artifacts
        p += n * 0.02f;

        // Convert to camera space
        Matrix4x4 w2c = cam.worldToCameraMatrix;
        Vector3 cPos = w2c.MultiplyPoint(p);
        Vector3 cNormal = w2c.MultiplyVector(n).normalized;

        // Camera-space plane: Ax + By + Cz + D = 0, as a 4-vector (A,B,C,D)
        Vector4 planeCS = new Vector4(cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot(cPos, cNormal));

        cam.projectionMatrix = cam.CalculateObliqueMatrix(planeCS);
    }

    // --- Teleport helpers ---
    // Returns signed distance: >0 in front, <0 behind (uses portal forward)
    public float SignedSide(Vector3 worldPoint)
    {
        Vector3 toPoint = worldPoint - transform.position;
        return Vector3.Dot(transform.forward, toPoint);
    }

    public void Teleport(Transform target, CharacterController cc = null)
    {
        if (!linkedPortal) return;

        // Player pose relative to THIS portal
        Vector3 localPos = transform.InverseTransformPoint(target.position);
        Quaternion localRot = Quaternion.Inverse(transform.rotation) * target.rotation;

        // Mirror through
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        localRot = new Quaternion(-localRot.x, localRot.y, -localRot.z, -localRot.w);

        // Move into LINKED portal's space
        Vector3 newWorldPos = linkedPortal.transform.TransformPoint(localPos);
        Quaternion newWorldRot = linkedPortal.transform.rotation * localRot;

        if (cc)
        {
            cc.enabled = false;
            target.SetPositionAndRotation(newWorldPos, newWorldRot);
            cc.enabled = true;
        }
        else
        {
            target.SetPositionAndRotation(newWorldPos, newWorldRot);
        }
    }
}
