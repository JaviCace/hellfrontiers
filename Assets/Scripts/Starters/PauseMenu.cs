using System.Collections;
using UnityEngine;

namespace Starters
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] LifecycleGuard guard;

        void OnEnable() { LifecycleGuard.PausedChanged += Show; Show(LifecycleGuard.IsPaused); }
        void OnDisable() { LifecycleGuard.PausedChanged -= Show; }
        void Show(bool paused) => panel.SetActive(paused);

        void Update()
        {
            
        }

        public void OnResumePressed() => guard.SetPaused(false);
    }
}