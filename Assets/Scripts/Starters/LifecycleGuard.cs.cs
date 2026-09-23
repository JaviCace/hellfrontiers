using System.Collections;
using UnityEngine;

namespace Starters
{
    public class LifecycleGuard : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }
        public static event System.Action<bool> PausedChanged;

        void OnApplicationPause(bool paused)
        {
            if (!paused) return;
            PlayerPrefs.Save();
            SetPaused(true);
        }

        void OnApplicationFocus(bool f) { if (!f) SetPaused(true); }

        public void SetPaused(bool value)
        {
            IsPaused = value;
            Time.timeScale = value ? 0f : 1f;
            AudioListener.pause = value;
            PausedChanged?.Invoke(value);
        }
    }
}