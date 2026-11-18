using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum PerfMode { Balanced, HighFps }

// Balanced / High FPS switch. Put this on the persistent App object in the Boot scene.
// Script Execution Order: DeviceTier = -100, PerformanceMode = -50
public class PerformanceMode : MonoBehaviour
{
    void Start()
    {
        // Load the saved mode (0 = Balanced, 1 = High FPS)
        int saved = PlayerPrefs.GetInt("perf_mode", 0);
        Apply(saved == 1 ? PerfMode.HighFps : PerfMode.Balanced);
    }

    public void Apply(PerfMode mode)
    {
        bool high = mode == PerfMode.HighFps;

        // High FPS = 120 fps at a lower render scale.
        // Balanced = the values chosen by DeviceTier for this phone.
        Application.targetFrameRate = high ? 120 : DeviceTier.BaseFrameRate;

        // Change the render scale of the active URP asset
        var urp = UniversalRenderPipeline.asset;
        if (urp != null)
        {
            urp.renderScale = high ? 0.8f : DeviceTier.BaseRenderScale;
        }

        // Remember the choice for the next launch
        PlayerPrefs.SetInt("perf_mode", high ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"perf_mode={mode} high={high}");
    }

    // Called by the UI Toggle (use the "Dynamic bool" entry in the OnValueChanged list)
    public void SetHighFps(bool on)
    {
        Apply(on ? PerfMode.HighFps : PerfMode.Balanced);
    }
}
