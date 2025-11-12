using UnityEngine;

public class PoweredInteractable : MonoBehaviour, IInteractable, IPowerConsumer
{
    [Header("Interaction")]
    [Tooltip("Component that ALSO implements IInteractable (ShieldSwitch, TVPowerButton, etc)")]
    public MonoBehaviour target;

    IInteractable _i;

    bool hasPower = true;                  // gate flag

    [Header("Debug")]
    [SerializeField] bool debugHasPower;   // Inspector view

    [Header("Controls TV?")]
    public bool controlsTV;
    public TVScreenController tv;
    public bool forceOffOnPowerLoss = true;
    public bool autoOnWhenPowerReturn = false;

    void Awake()
    {
        _i = target as IInteractable;
        if (_i == null)
            Debug.LogWarning($"{name}: PoweredInteractable target does NOT implement IInteractable!", this);
    }

    void Start()
    {
        var mgr = PowerGridManager.Instance;
        if (mgr != null)
        {
            Debug.Log($"[PoweredInteractable:{name}] Registering with PowerGridManager", this);
            mgr.Register(this);              // will immediately call OnPowerChanged(isOn)
        }
        else
        {
            Debug.LogWarning($"{name}: No PowerGridManager found at Start.", this);
        }
    }

    void OnDisable()
    {
        PowerGridManager.Instance?.Unregister(this);
    }

    // -------- IPowerConsumer --------
    public void OnPowerChanged(bool isOn)
    {
        hasPower = isOn;
        debugHasPower = isOn;

        Debug.Log($"[PoweredInteractable:{name}] OnPowerChanged -> {isOn}", this);

        if (controlsTV && tv)
        {
            if (!isOn && forceOffOnPowerLoss)
                tv.SetPower(false);
            else if (isOn && autoOnWhenPowerReturn)
                tv.SetPower(true);
        }
    }

    // -------- IInteractable --------
    public string Prompt => _i != null ? _i.Prompt : "";

    public void Interact(Transform interactor)
    {
        var mgr = PowerGridManager.Instance;
        bool gridOn = mgr == null ? true : mgr.IsOn;

        Debug.Log($"[PoweredInteractable:{name}] Interact CALLED. hasPower={hasPower}, gridOn={gridOn}", this);

        if (!hasPower || !gridOn)
            return;          // grid off → ignore click

        _i?.Interact(interactor);
    }
}
