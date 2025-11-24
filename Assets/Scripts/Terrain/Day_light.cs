using UnityEngine;
using UnityEngine.Events;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0, 24)] public float timeOfDay = 12f;

    [Tooltip("Base full day+night length in seconds before multipliers are applied")]
    public float totalDefaultCycleLength = 300f;   // Default = 5 minutes

    [Header("Length Multipliers (Change These In Inspector)")]
    [Tooltip("1 = normal, 2 = day lasts twice as long, 0.5 = day is shorter")]
    public float dayLengthMultiplier = 1f;

    [Tooltip("1 = normal, 2 = night lasts twice as long, 0.5 = night is shorter")]
    public float nightLengthMultiplier = 1f;

    [Header("References")]
    public Light directionalLight;
    public Material skyboxMaterial;

    [Header("Lighting Settings")]
    public Gradient lightColor;
    public AnimationCurve lightIntensity;
    public AnimationCurve skyboxExposure;

    [Header("Events")]
    public UnityEvent onSunrise;
    public UnityEvent onSunset;

    private bool sunriseTriggered = false;
    private bool sunsetTriggered = false;

    void Update()
    {
        // Split default 5-minute cycle into equal halves
        float baseDaySeconds = totalDefaultCycleLength / 2f;
        float baseNightSeconds = totalDefaultCycleLength / 2f;

        // Apply multipliers
        float actualDaySeconds = baseDaySeconds * dayLengthMultiplier;
        float actualNightSeconds = baseNightSeconds * nightLengthMultiplier;

        // 12 hours of day (6 → 18)
        float daySpeed = 12f / actualDaySeconds;

        // 12 hours of night (18 → 24 & 0 → 6)
        float nightSpeed = 12f / actualNightSeconds;

        // Check if it's day or night
        bool isDay = timeOfDay >= 6f && timeOfDay < 18f;

        // Pick correct speed
        float speed = isDay ? daySpeed : nightSpeed;

        // Advance time
        timeOfDay += Time.deltaTime * speed;

        if (timeOfDay >= 24f)
            timeOfDay = 0f;

        float t = timeOfDay / 24f;

        // Update lighting & skybox
        directionalLight.transform.localRotation = Quaternion.Euler((t * 360f) - 90f, 170f, 0);
        directionalLight.color = lightColor.Evaluate(t);
        directionalLight.intensity = lightIntensity.Evaluate(t);
        RenderSettings.skybox.SetFloat("_Exposure", skyboxExposure.Evaluate(t));

        HandleEvents();
    }

    void HandleEvents()
    {
        if (timeOfDay >= 6f && timeOfDay < 6.1f && !sunriseTriggered)
        {
            onSunrise.Invoke();
            sunriseTriggered = true;
            sunsetTriggered = false;
            Debug.Log("Sunrise");
        }

        if (timeOfDay >= 18f && timeOfDay < 18.1f && !sunsetTriggered)
        {
            onSunset.Invoke();
            sunsetTriggered = true;
            sunriseTriggered = false;
            Debug.Log("Sunset");
        }
    }
}
