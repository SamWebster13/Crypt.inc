using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Renderer))]
public class DoorShield : MonoBehaviour
{
    [Header("Visual")]
    public Renderer shieldRenderer;  
    public Color greenColor = new Color(0f, 1f, 0f, 0.25f);
    public Color redColor = new Color(1f, 0f, 0f, 0.25f);
    public float emissionBoost = 1.5f;

    [Header("State")]
    public bool startsOpen = true;  
    public bool IsOpen { get; private set; }

    BoxCollider col;
    MaterialPropertyBlock mpb;
    float baseAlpha = 0.25f;

   
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int LegacyColorID = Shader.PropertyToID("_Color");
    static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        if (!shieldRenderer) shieldRenderer = GetComponent<Renderer>();

        if (shieldRenderer && shieldRenderer.sharedMaterial != null)
        {
            var c = shieldRenderer.sharedMaterial.HasProperty(BaseColorID)
                ? shieldRenderer.sharedMaterial.GetColor(BaseColorID)
                : shieldRenderer.sharedMaterial.color;
            baseAlpha = c.a;
        }

        if (mpb == null) mpb = new MaterialPropertyBlock();
        SetOpen(startsOpen);
    }

    void OnValidate()
    {
        if (!col) col = GetComponent<BoxCollider>();
        if (!shieldRenderer) shieldRenderer = GetComponent<Renderer>();
    }

    public void Toggle() => SetOpen(!IsOpen);

    public void SetOpen(bool open)
    {
        IsOpen = open;
        col.isTrigger = open;

        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        if (!shieldRenderer) return;

        var c = IsOpen ? greenColor : redColor;
        c.a = baseAlpha; 

        mpb.Clear();
        mpb.SetColor(BaseColorID, c);
        mpb.SetColor(LegacyColorID, c);
        mpb.SetColor(EmissionID, new Color(c.r, c.g, c.b) * emissionBoost);

        shieldRenderer.SetPropertyBlock(mpb);
    }
}
