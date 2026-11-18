using UnityEngine;
using UnityEngine.AdaptivePerformance;

// Listens to thermal and bottleneck events from the phone and writes them to the log.
// Put this on the persistent App object in the Boot scene.
public class AdaptiveBootstrap : MonoBehaviour
{
    IAdaptivePerformance ap;   // the Adaptive Performance system

    void Start()
    {
        ap = Holder.Instance;

        // If the provider is not available (old Android, wrong settings), stop here
        if (ap == null || !ap.Active)
        {
            Debug.Log("[AP] provider inactive on this device");
            return;
        }

        // Subscribe to the two events
        ap.ThermalStatus.ThermalEvent += OnThermal;
        ap.PerformanceStatus.PerformanceBottleneckChangeEvent += OnBottleneck;

        // Print the starting thermal state once
        var m = ap.ThermalStatus.ThermalMetrics;
        Debug.Log($"[AP] active warning={m.WarningLevel} level={m.TemperatureLevel:0.00}");
    }

    void OnDestroy()
    {
        // Always unsubscribe so we do not keep dead references
        if (ap == null) return;
        ap.ThermalStatus.ThermalEvent -= OnThermal;
        ap.PerformanceStatus.PerformanceBottleneckChangeEvent -= OnBottleneck;
    }

    // Called only when the thermal level changes
    void OnThermal(ThermalMetrics m)
    {
        Debug.Log($"[AP] thermal warning={m.WarningLevel} " +
                  $"level={m.TemperatureLevel:0.00} trend={m.TemperatureTrend:0.00}");
    }

    // Called when the main bottleneck changes (CPU, GPU, TargetFrameRate...)
    void OnBottleneck(PerformanceBottleneckChangeEventArgs e)
    {
        Debug.Log($"[AP] bottleneck={e.PerformanceBottleneck}");
    }
}
