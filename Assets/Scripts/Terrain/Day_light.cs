using UnityEngine;
using UnityEngine.Events;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0, 24)] public float timeOfDay = 12f;
    public float totalDefaultCycleLength = 300f; // Default = 5 minutes

    [Header("Length Multipliers")]
    public float dayLengthMultiplier = 1f;
    public float nightLengthMultiplier = 1f;

    [Header("References")]
    public Light directionalLight;
    public Material skyboxMaterial;

    [Header("Lighting Settings")]
    public Gradient lightColor;
    public AnimationCurve lightIntensity;
    public AnimationCurve skyboxExposure;

    [Header("Fog Settings")]
    public Color dayFogColor = Color.gray;
    public Color nightFogColor = Color.black; // Pitch dark
    public float dayFogDensity = 0.01f;
    public float nightFogDensity = 0.8f; // denser for night

    [Header("Events")]
    public UnityEvent onSunrise;
    public UnityEvent onSunset;
    public UnityEvent onNewDay;

    private bool sunriseTriggered = false;
    private bool sunsetTriggered = false;
    private float currentExposure = 1f;
    private float currentAmbient = 1f;
    private float currentSunIntensity = 1f;
    private Color currentSunColor;

    public int currentDay = 1;
    public bool isNight = false;

    void Start()
    {
        currentSunColor = lightColor.Evaluate(timeOfDay / 24f);
    }

    private float lightingUpdateInterval = 0.6f; // 10 updates per second
    private float lightingTimer = 0f;

    void Update()
    {
        UpdateTime();

        lightingTimer += Time.deltaTime;
        if (lightingTimer >= lightingUpdateInterval)
        {
            UpdateLighting();
            lightingTimer = 0f;
        }

        HandleEvents();
    }


    void UpdateTime()
    {
        float baseDaySeconds = totalDefaultCycleLength / 2f;
        float baseNightSeconds = totalDefaultCycleLength / 2f;

        float actualDaySeconds = baseDaySeconds * dayLengthMultiplier;
        float actualNightSeconds = baseNightSeconds * nightLengthMultiplier;

        float daySpeed = 12f / actualDaySeconds;
        float nightSpeed = 12f / actualNightSeconds;

        bool isDay = timeOfDay >= 6f && timeOfDay < 18f;
        float speed = isDay ? daySpeed : nightSpeed;

        timeOfDay += Time.deltaTime * speed;
        if (timeOfDay >= 24f)
            timeOfDay = 0f;
    }

    void UpdateLighting()
    {
        float t = timeOfDay / 24f;

        // --- Sun rotation ---
        float sunAngle;
        if (timeOfDay >= 6f && timeOfDay < 18f) // Day
        {
            float dayT = (timeOfDay - 6f) / 12f;
            sunAngle = Mathf.Lerp(-90f, 90f, dayT);
        }
        else // Night
        {
            float nightT = timeOfDay >= 18f ? (timeOfDay - 18f) / 12f : (timeOfDay + 6f) / 12f;
            sunAngle = Mathf.Lerp(90f, 270f, nightT);
        }
        directionalLight.transform.localRotation = Quaternion.Euler(sunAngle, 170f, 0);

        // --- Sun color & intensity ---
        bool night = timeOfDay >= 18f || timeOfDay < 6f;
        Color targetSunColor = night ? Color.black : lightColor.Evaluate(t);
        currentSunColor = Color.Lerp(currentSunColor, targetSunColor, Time.deltaTime * 2f);
        directionalLight.color = currentSunColor;

        float targetSunIntensity = night ? 0f : lightIntensity.Evaluate(t);
        currentSunIntensity = Mathf.Lerp(currentSunIntensity, targetSunIntensity, Time.deltaTime * 2f);
        directionalLight.intensity = currentSunIntensity;

        // --- Skybox exposure ---
        float targetExposure = night ? 0f : 1f;
        currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f);
        if (skyboxMaterial != null)
            skyboxMaterial.SetFloat("_Exposure", currentExposure);

        // --- Ambient intensity & color (smooth lerp) ---
        Color targetAmbientColor = night ? Color.black : Color.white;
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmbientColor, Time.deltaTime * 2f);

        float targetAmbient = night ? 0f : 1f;
        currentAmbient = Mathf.Lerp(currentAmbient, targetAmbient, Time.deltaTime * 2f);
        RenderSettings.ambientIntensity = currentAmbient;

        // --- Reflection intensity (smooth lerp) ---
        float targetReflection = night ? 0f : 1f;
        RenderSettings.reflectionIntensity = Mathf.Lerp(RenderSettings.reflectionIntensity, targetReflection, Time.deltaTime * 2f);

        // --- Fog ---
        Color targetFogColor = night ? Color.black : dayFogColor;
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, Time.deltaTime * 2f);

        float targetFogDensity = night ? nightFogDensity : dayFogDensity;
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * 2f);
    }


    void HandleEvents()
    {
        // SUNRISE
        if (timeOfDay >= 6f && timeOfDay < 6.1f && !sunriseTriggered)
        {
            onSunrise.Invoke();
            onNewDay.Invoke();
            currentDay++;
            isNight = false;

            sunriseTriggered = true;
            sunsetTriggered = false;
        }

        // SUNSET
        if (timeOfDay >= 18f && timeOfDay < 18.1f && !sunsetTriggered)
        {
            onSunset.Invoke();
            isNight = true;

            sunsetTriggered = true;
            sunriseTriggered = false;
        }
    }
}
