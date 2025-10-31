using UnityEngine;

public class PowerGatedInteractable : MonoBehaviour, IInteractable, IPowerConsumer
{
    public MonoBehaviour target; 
    IInteractable _i;
    bool powered;

    void Awake() { _i = target as IInteractable; }

    void OnEnable()
    {
        var mgr = PowerGridManager.Instance;
        if (mgr != null)
        {
            mgr.Register(this);      
            powered = mgr.IsOn;     
        }
    }

    void OnDisable() => PowerGridManager.Instance?.Unregister(this);

    public string Prompt => (_i != null) ? _i.Prompt : "";

    public void Interact(Transform interactor)
    {
        if (!powered) return;        
        _i?.Interact(interactor);
    }

    public void OnPowerChanged(bool isOn) { powered = isOn; }
}
