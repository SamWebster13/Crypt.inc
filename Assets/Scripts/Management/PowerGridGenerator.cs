using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)] 
public class PowerGridManager : MonoBehaviour
{
    public static PowerGridManager Instance { get; private set; }

    [Header("Warmup")]
    public float warmupSeconds = 1.0f;

    bool isOn;
    public bool IsOn => isOn; 
    Coroutine warmupCo;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TogglePower() { if (isOn) TurnOff(); else TurnOn(); }

    public void TurnOn()
    {
        if (isOn) return;
        if (warmupCo != null) StopCoroutine(warmupCo);
        warmupCo = StartCoroutine(CoWarmup());
    }

    public void TurnOff()
    {
        if (warmupCo != null) { StopCoroutine(warmupCo); warmupCo = null; }
        isOn = false;
        PowerIndicator.SnapAll(false);
        NotifyAll(false);
    }

    IEnumerator CoWarmup()
    {
        float t = 0f;
        while (t < warmupSeconds)
        {
            t += Time.deltaTime;
            PowerIndicator.BroadcastWarmup(Mathf.Clamp01(t / warmupSeconds));
            yield return null;
        }
        isOn = true;
        warmupCo = null;
        PowerIndicator.SnapAll(true);
        NotifyAll(true);
    }

    // --- Registry of consumers ---
    readonly System.Collections.Generic.List<IPowerConsumer> consumers = new();

    public void Register(IPowerConsumer c)
    {
        if (c == null) return;
        if (!consumers.Contains(c)) consumers.Add(c);
        c.OnPowerChanged(isOn);             
    }

    public void Unregister(IPowerConsumer c) { consumers.Remove(c); }

    void NotifyAll(bool on)
    {
        for (int i = 0; i < consumers.Count; i++) consumers[i]?.OnPowerChanged(on);
    }
}
