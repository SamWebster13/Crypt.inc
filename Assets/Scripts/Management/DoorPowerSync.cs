using UnityEngine;

public class DoorPowerSync : MonoBehaviour, IPowerConsumer
{
    public DoorShield door;             
    public bool openWhenNoPower = true;    
    public bool closeOnPowerReturn = false;

    void Awake() { if (!door) door = GetComponent<DoorShield>(); }
    void OnEnable() => PowerGridManager.Instance?.Register(this);
    void OnDisable() => PowerGridManager.Instance?.Unregister(this);

    public void OnPowerChanged(bool isOn)
    {
        if (!door) return;
        if (!isOn && openWhenNoPower) door.SetOpen(true);       
        else if (isOn && closeOnPowerReturn) door.SetOpen(false);
       
    }
}
