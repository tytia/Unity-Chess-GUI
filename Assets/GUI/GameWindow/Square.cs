using UnityEngine;

namespace GUI.GameWindow
{
    public class Square : MonoBehaviour
    {
        public static Color lightCol { get; } = new Color32(219, 183, 166, 255);
        public static Color darkCol { get; } = new Color32(128, 79, 67, 255);
        private SpriteRenderer _sr;

        public Color color { set => _sr.color = value; }

        private void Awake()
        {
            _sr = gameObject.GetComponent<SpriteRenderer>();
        }
    }
}