using Starters;
using UnityEngine;
using UnityEngine.UI;

namespace Starters
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] Toggle hapticsToggle;

        void OnEnable()
        {
            hapticsToggle.SetIsOnWithoutNotify(Haptics.Enabled);
        }

        public void OnHapticsToggleChanged(bool value)
        {
            Haptics.Enabled = value;
        }
        public void SetTextSmall() => TextScale.Factor = 0.85f;
        public void SetTextNormal() => TextScale.Factor = 1f;
        public void SetTextLarge() => TextScale.Factor = 1.25f;
    }
}