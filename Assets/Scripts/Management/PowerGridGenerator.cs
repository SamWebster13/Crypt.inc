// PowerGridManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PowerGridManager : MonoBehaviour
{
    public static PowerGridManager Instance { get; private set; }

    [Header("Warmup")]
    public float warmupSeconds = 1.0f;

    [Header("Debug")]
    public bool verbose = true;

    bool isOn;
    public bool IsOn => isOn;
    Coroutine warmupCo;

    [SerializeField] bool debugIsOn;

    readonly List<IPowerConsumer> consumers = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void SetPowerState(bool on)
    {
        isOn = on;
        debugIsOn = on;
        if (verbose) Debug.Log($"[PowerGrid] SetPowerState({on})");
        PowerIndicator.SnapAll(on);   // update lights instantly
        NotifyAll(on);                // notify registered consumers (TV, etc.)
    }

    public void TogglePower()
    {
        if (verbose) Debug.Log($"[PowerGrid] TogglePower pressed. Was {isOn}");
        if (isOn) TurnOff(); else TurnOn();
    }

    public void TurnOn()
    {
        if (isOn) return;
        if (warmupCo != null) StopCoroutine(warmupCo);
        warmupCo = StartCoroutine(CoWarmup());
    }

    public void TurnOff()
    {
        if (warmupCo != null) { StopCoroutine(warmupCo); warmupCo = null; }
        if (verbose) Debug.Log("[PowerGrid] TurnOff() called");
        SetPowerState(false);
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
        warmupCo = null;
        SetPowerState(true);
    }

    // Optional registry (TV sync etc.)
    public void Register(IPowerConsumer c)
    {
        if (c == null) return;
        if (!consumers.Contains(c))
        {
            consumers.Add(c);
            var mb = c as MonoBehaviour;
            if (verbose) Debug.Log($"[PowerGrid] + Registered {(mb ? mb.name : c.GetType().Name)} ({c.GetType().Name})");
            c.OnPowerChanged(isOn);
        }
    }

    public void Unregister(IPowerConsumer c)
    {
        if (c == null) return;
        if (consumers.Remove(c))
        {
            var mb = c as MonoBehaviour;
            if (verbose) Debug.Log($"[PowerGrid] - Unregistered {(mb ? mb.name : c.GetType().Name)}");
        }
    }

    void NotifyAll(bool on)
    {
        if (verbose) Debug.Log($"[PowerGrid] NotifyAll({on}) to {consumers.Count} consumers");
        for (int i = 0; i < consumers.Count; i++)
        {
            var c = consumers[i];
            if (c == null) { if (verbose) Debug.Log($"[PowerGrid]   consumer {i} is NULL"); continue; }
            var mb = c as MonoBehaviour;
            if (verbose) Debug.Log($"[PowerGrid]   -> notifying {(mb ? mb.name : c.GetType().Name)} ({c.GetType().Name}) with {on}");
            c.OnPowerChanged(on);
        }
    }
}
