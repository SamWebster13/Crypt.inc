using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PowerIndicator : MonoBehaviour, IPowerConsumer
{
   
    public static System.Action<float> OnWarmupPulse;   
    public static System.Action<bool> OnVisualSnap;        

    public Color onColor = new Color(1f, 0.85f, 0.2f, 1f);  
    public Color offColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    public float emission = 2.0f;

    Renderer rend;
    MaterialPropertyBlock mpb;
    bool powered;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    void OnEnable()
    {
        if (!rend) rend = GetComponent<Renderer>();
        mpb ??= new MaterialPropertyBlock();

        PowerGridManager.Instance?.Register(this);

        OnWarmupPulse += HandleWarmupPulse;
        OnVisualSnap += HandleSnap;
    }

    void OnDisable()
    {
        PowerGridManager.Instance?.Unregister(this);

        OnWarmupPulse -= HandleWarmupPulse;
        OnVisualSnap -= HandleSnap;
    }

    public void OnPowerChanged(bool isOn)
    {
        powered = isOn;
        Apply(powered ? onColor : offColor);
    }

    void HandleWarmupPulse(float t)
    {
        if (powered) return; 
        t = Mathf.Clamp01(t);
        t = Mathf.SmoothStep(0f, 1f, t);
        Apply(Color.Lerp(offColor, onColor, t * 0.9f)); 
    }

    void HandleSnap(bool isOn) => Apply(isOn ? onColor : offColor);

    void Apply(Color c)
    {
        if (!rend) return;
        mpb.Clear();
        mpb.SetColor(BaseColorID, c);
        mpb.SetColor(EmissionID, c * emission);
        rend.SetPropertyBlock(mpb);
    }

    public static void BroadcastWarmup(float t) => OnWarmupPulse?.Invoke(t);
    public static void SnapAll(bool isOn) => OnVisualSnap?.Invoke(isOn);
}
