using UnityEngine;

namespace GUI.GameWindow
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private Sprite[] _sprites;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = gameObject.GetComponent<SpriteRenderer>();
        }
    }
}
