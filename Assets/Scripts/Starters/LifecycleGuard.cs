using System.Collections;
using UnityEngine;
using Managers;

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

        /// <summary>
        /// quita la pausa sin tocar el timeScale (lo usa el menú principal al empezar)
        /// </summary>
        public static void ClearPause()
        {
            if (!IsPaused) return;
            IsPaused = false;
            AudioListener.pause = false;
            PausedChanged?.Invoke(false);
        }

        public void SetPaused(bool value)
        {
            IsPaused = value;
            // con el menú principal / de muerte abierto el juego sigue parado
            Time.timeScale = (value || GameFlow.MenuOpen) ? 0f : 1f;
            AudioListener.pause = value;
            PausedChanged?.Invoke(value);
        }
    }
}