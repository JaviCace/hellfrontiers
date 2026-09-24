using System.Collections;
using UnityEngine;
using TMPro;
namespace Starters
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextScale : MonoBehaviour
    {
        const string Key = "textScale";
        public static event System.Action Changed;

        public static float Factor
        {
            get => PlayerPrefs.GetFloat(Key, 1f);
            set { PlayerPrefs.SetFloat(Key, value); PlayerPrefs.Save(); Changed?.Invoke(); }
        }

        TMP_Text _text;
        float _baseSize;

        void Awake() { _text = GetComponent<TMP_Text>(); _baseSize = _text.fontSize; }
        void OnEnable() { Changed += Apply; Apply(); }
        void OnDisable() { Changed -= Apply; }
        void Apply() { _text.fontSize = _baseSize * Factor; }
    }
}