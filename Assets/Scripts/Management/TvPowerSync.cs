using UnityEngine;

public class TVPowerSync : MonoBehaviour, IPowerConsumer
{
    public TVScreenController tv;                 
    public bool forceOffOnPowerLoss = true;      
    public bool autoOnWhenPowerReturns = false;   

    void Awake() { if (!tv) tv = GetComponent<TVScreenController>(); }
    void OnEnable() => PowerGridManager.Instance?.Register(this);
    void OnDisable() => PowerGridManager.Instance?.Unregister(this);

    public void OnPowerChanged(bool isOn)
    {
        if (!tv) return;
        if (!isOn && forceOffOnPowerLoss) tv.SetPower(false);
        else if (isOn && autoOnWhenPowerReturns) tv.SetPower(true);
    }
}
