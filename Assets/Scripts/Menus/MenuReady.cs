using UnityEngine;

public class MenuReady : MonoBehaviour
{
    bool logged;   // makes sure we only log once

    void Update()
    {
        if (logged) return;
        logged = true;
        // Time since the app started, when the menu is first interactive
        Debug.Log($"[perf] interactive {Time.realtimeSinceStartup:F2}s");
    }
}