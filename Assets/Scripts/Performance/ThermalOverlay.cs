using TMPro;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

// Shows frame time and thermal info on screen. Used for the thermal soak screenshots.
// Drag your ThermalLabel (TextMeshPro) into the "label" field.
public class ThermalOverlay : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    float timer;   // time passed since the last text update
    int frames;    // frames counted since the last text update

    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        // Only update the text twice per second
        if (timer < 0.5f) return;

        float frameMs = timer / frames * 1000f;

        var ap = Holder.Instance;
        if (ap != null && ap.Active)
        {
            var m = ap.ThermalStatus.ThermalMetrics;
            label.text = $"{frameMs:0.0} ms  {m.WarningLevel}\n" +
                         $"temp {m.TemperatureLevel:0.00}  trend {m.TemperatureTrend:0.00}";
        }
        else
        {
            label.text = $"{frameMs:0.0} ms  (AP inactive)";
        }

        timer = 0f;
        frames = 0;
    }
}
