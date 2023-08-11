using UnityEngine;

namespace GUI.GameWindow {
    public class Square : MonoBehaviour {
        public static Color lightCol { get; } = new Color32(231, 205, 193, 255);
        public static Color darkCol { get; } = new Color32(181, 116, 97, 255);
        private SpriteRenderer _sr;

        public Color color {
            set => _sr.color = value;
        }

        private void Awake() {
            _sr = gameObject.GetComponent<SpriteRenderer>();
        }
    }
}