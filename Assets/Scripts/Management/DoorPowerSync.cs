using UnityEngine;

public class DoorPowerSync : MonoBehaviour, IPowerConsumer
{
    public DoorShield door;
    public bool openWhenNoPower = true;
    public bool closeOnPowerReturn = false;

    void Awake()
    {
        if (!door) door = GetComponent<DoorShield>();
    }

    void OnEnable()
    {
        var mgr = PowerGridManager.Instance;
        Debug.Log($"[DoorPowerSync:{name}] OnEnable. mgr={mgr}", this);

        if (mgr != null)
        {
            Debug.Log($"[DoorPowerSync:{name}] Registering with PowerGridManager", this);
            mgr.Register(this);  // pushes current state and stores us in the list
        }
        else
        {
            Debug.LogWarning($"[DoorPowerSync:{name}] No PowerGridManager found in OnEnable.", this);
        }
    }

    void OnDisable()
    {
        Debug.Log($"[DoorPowerSync:{name}] OnDisable – unregistering", this);
        PowerGridManager.Instance?.Unregister(this);
    }

    public void OnPowerChanged(bool isOn)
    {
        Debug.Log($"[DoorPowerSync:{name}] OnPowerChanged -> {isOn}", this);

        if (!door) return;

        if (!isOn && openWhenNoPower)
        {
            door.SetOpen(true);
        }
        else if (isOn && closeOnPowerReturn)
        {
            door.SetOpen(false);
        }
    }
}
