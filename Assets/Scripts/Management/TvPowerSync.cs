using UnityEngine;

public class TVPowerSync : MonoBehaviour, IPowerConsumer
{
    public TVScreenController tv;
    public bool forceOffOnPowerLoss = true;       // OFF when grid = false
    public bool autoOnWhenPowerReturns = false;   // auto-ON when grid = true

    void Awake()
    {
        if (!tv) tv = GetComponent<TVScreenController>();
    }

    void OnEnable()
    {
        PowerGridManager.Instance?.Register(this);
    }

    void OnDisable()
    {
        PowerGridManager.Instance?.Unregister(this);
    }

    public void OnPowerChanged(bool isOn)
    {
     //   Debug.Log($"[TVPowerSync] Power changed: {isOn}", this);

        if (!tv) return;

        if (!isOn && forceOffOnPowerLoss)
        {
            tv.SetPower(false);       // force screen off
        }
        else if (isOn && autoOnWhenPowerReturns)
        {
            tv.SetPower(true);        // optionally auto power-on when grid comes back
        }
        // else: leave it off, user must press TV power button manually
    }
}
