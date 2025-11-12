using UnityEngine;

public class PowerGatedInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Component that ALSO implements IInteractable (e.g. ShieldSwitch, TVPowerButton)")]
    public MonoBehaviour target;

    IInteractable _i;

    void Awake()
    {
        _i = target as IInteractable;
        if (_i == null)
        {
            //Debug.LogWarning($"{name}: PowerGatedInteractable target does NOT implement IInteractable!");
        }
    }

    public string Prompt => _i != null ? _i.Prompt : "";

    public void Interact(Transform interactor)
    {
        // Ask the global manager what the power state is RIGHT NOW
        var mgr = PowerGridManager.Instance;
        bool hasPower = (mgr == null) ? true : mgr.IsOn;

       // Debug.Log($"[PowerGate:{name}] Interact called. hasPower={hasPower}");

        if (!hasPower) return;     // grid off → ignore click completely

        _i?.Interact(interactor);  // forward to real button script (ShieldSwitch, TVPowerButton etc.)
    }
}
