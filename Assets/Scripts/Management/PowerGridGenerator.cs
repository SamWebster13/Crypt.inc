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

    bool isOn;
    public bool IsOn => isOn;
    Coroutine warmupCo;

    [SerializeField] bool debugIsOn;   // just mirrors state in Inspector

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
        Debug.Log($"[PowerGrid] SetPowerState({on})");
        NotifyAll(on);
    }

    public void TogglePower()
    {
        Debug.Log($"[PowerGrid] TogglePower pressed. Was {isOn}");
        if (isOn) TurnOff();
        else TurnOn();
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
        Debug.Log("[PowerGrid] TurnOff() called");
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

    public void Register(IPowerConsumer c)
    {
        if (c == null) return;
        if (!consumers.Contains(c))
        {
            consumers.Add(c);
            Debug.Log($"[PowerGrid] Register {((MonoBehaviour)c).name}. Pushing current state {isOn}");
            c.OnPowerChanged(isOn);
        }
    }

    public void Unregister(IPowerConsumer c)
    {
        if (c == null) return;
        if (consumers.Remove(c))
            Debug.Log($"[PowerGrid] Unregister {((MonoBehaviour)c).name}");
    }

    void NotifyAll(bool on)
    {
        Debug.Log($"[PowerGrid] NotifyAll({on}) to {consumers.Count} consumers");

        for (int i = 0; i < consumers.Count; i++)
        {
            var c = consumers[i];
            if (c == null)
            {
                Debug.Log($"[PowerGrid]   consumer index {i} is NULL");
                continue;
            }

            var mb = c as MonoBehaviour;
            string name = mb ? mb.name : c.GetType().Name;

            Debug.Log($"[PowerGrid]   -> notifying {name} ({c.GetType().Name}) with {on}");
            c.OnPowerChanged(on);
        }
    }

}
