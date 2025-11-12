using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PowerIndicator : MonoBehaviour, IPowerConsumer
{
    // ---------- Global visual events ----------
    public static System.Action<float> OnWarmupPulse;  // called with t in [0,1]
    public static System.Action<bool> OnVisualSnap;   // instant on/off

    [Header("Colors")]
    public Color onColor = new Color(1f, 0.85f, 0.2f, 1f);
    public Color offColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    public float emission = 2.0f;

    Renderer rend;
    MaterialPropertyBlock mpb;
    bool powered;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int LegacyColorID = Shader.PropertyToID("_Color");
    static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    void OnEnable()
    {
        if (!rend) rend = GetComponent<Renderer>();
        mpb ??= new MaterialPropertyBlock();

        // register with the grid (so we get current state)
        PowerGridManager.Instance?.Register(this);

        // subscribe to global events
        OnWarmupPulse += HandleWarmupPulse;
        OnVisualSnap += HandleSnap;
    }

    void OnDisable()
    {
        PowerGridManager.Instance?.Unregister(this);

        OnWarmupPulse -= HandleWarmupPulse;
        OnVisualSnap -= HandleSnap;
    }

    // -------- IPowerConsumer --------
    public void OnPowerChanged(bool isOn)
    {
        powered = isOn;
        Apply(isOn ? onColor : offColor);
    }

    // called DURING warmup while power is still logically off
    void HandleWarmupPulse(float t)
    {
        // if we’re already marked as powered, ignore warmup
        if (powered) return;

        t = Mathf.Clamp01(t);
        t = Mathf.SmoothStep(0f, 1f, t);
        Color c = Color.Lerp(offColor, onColor, t * 0.9f);  // stop just short of full on
        Apply(c);
    }

    // called whenever the grid snaps ON/OFF
    void HandleSnap(bool isOn)
    {
        Apply(isOn ? onColor : offColor);
    }

    void Apply(Color c)
    {
        if (!rend) return;

        mpb.Clear();
        mpb.SetColor(BaseColorID, c);
        mpb.SetColor(LegacyColorID, c);
        mpb.SetColor(EmissionID, c * emission);
        rend.SetPropertyBlock(mpb);
    }

    // ---------- static helpers for PowerGrid ----------
    public static void BroadcastWarmup(float t) => OnWarmupPulse?.Invoke(t);
    public static void SnapAll(bool isOn) => OnVisualSnap?.Invoke(isOn);
}
