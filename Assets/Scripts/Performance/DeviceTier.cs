using UnityEngine;
using UnityEngine.Rendering.Universal;

// Picks the quality level (Low / Mid / High) when the app starts.
// Quality levels must be named Low (0), Mid (1), High (2) in Project Settings > Quality.
// Put this on the persistent App object. Script Execution Order: -100
public class DeviceTier : MonoBehaviour
{
    // Other scripts (PerformanceMode, Telemetry) read these values
    public static string TierName { get; private set; } = "Mid";
    public static int BaseFrameRate { get; private set; } = 60;
    public static float BaseRenderScale { get; private set; } = 1f;

    void Awake()
    {
        int tier;

        if (PlayerPrefs.HasKey("tier_override"))
        {
            // A manual choice from the settings menu wins
            tier = PlayerPrefs.GetInt("tier_override");
        }
        else
        {
            // Simple automatic rule based on RAM (change the numbers for your phones)
            int ramMb = SystemInfo.systemMemorySize;
            if (ramMb < 4000) tier = 0;        // Low
            else if (ramMb < 8000) tier = 1;   // Mid
            else tier = 2;                     // High
        }

        tier = Mathf.Clamp(tier, 0, 2);

        // Apply the quality level (this also switches the URP asset)
        QualitySettings.SetQualityLevel(tier, true);
        TierName = QualitySettings.names[tier];

        // Low tier runs at 30 fps, the others at 60 fps
        BaseFrameRate = tier == 0 ? 30 : 60;
        Application.targetFrameRate = BaseFrameRate;

        // Remember the render scale of this tier so Balanced mode can go back to it
        var urp = UniversalRenderPipeline.asset;
        if (urp != null) BaseRenderScale = urp.renderScale;

        // Lower shader detail on Low tier (only matters if your shaders use LOD blocks)
        Shader.globalMaximumLOD = tier == 0 ? 300 : 600;

        Debug.Log($"tier={TierName}");
    }

    // Called by the settings Dropdown. 0 = Auto, 1 = Low, 2 = Mid, 3 = High
    // The new tier applies on the next launch.
    public void SetTierOverride(int option)
    {
        if (option == 0) PlayerPrefs.DeleteKey("tier_override");
        else PlayerPrefs.SetInt("tier_override", option - 1);
        PlayerPrefs.Save();
    }
}
