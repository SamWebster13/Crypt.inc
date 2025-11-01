using UnityEngine;

public class PowerGeneratorButton : MonoBehaviour, IInteractable
{
    [TextArea] public string prompt = "Power: Toggle Generator";
    public string Prompt => prompt;

    public void Interact(Transform interactor)
    {
        PowerGridManager.Instance?.TogglePower();
    }
}
