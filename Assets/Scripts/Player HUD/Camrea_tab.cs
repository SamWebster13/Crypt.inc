using UnityEngine;

public class TabletToggle : MonoBehaviour
{
    [Header("Assign the tab object (child)")]
    public GameObject flashlight;

    [Header("Key to toggle")]
    public KeyCode toggleKey = KeyCode.F;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && flashlight != null)

        {
            Debug.Log("Screenager lol");
            flashlight.SetActive(!flashlight.activeSelf);
        }
    }
}