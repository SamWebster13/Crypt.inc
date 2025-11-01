using UnityEngine;

public class TVPowerButton : MonoBehaviour, IInteractable
{
    public TVScreenController screen;
    [TextArea] public string prompt = "Press E to power TV";
    public string Prompt => prompt;

    public void Interact(Transform interactor)
    {
        if (screen) screen.TogglePower();
    }
}
