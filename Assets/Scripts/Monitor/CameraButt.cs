using UnityEngine;

public class TVInputButton : MonoBehaviour, IInteractable
{
    public TVScreenController screen;
    public int sourceIndex = 0; 
    [TextArea] public string prompt = "Press E to switch input";
    public string Prompt => prompt;

    public void Interact(Transform interactor)
    {
        if (screen) screen.SelectSource(sourceIndex);
    }
}
