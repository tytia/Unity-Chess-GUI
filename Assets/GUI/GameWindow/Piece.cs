using UnityEngine;

namespace GUI.GameWindow {
    public class Piece : MonoBehaviour {
        private SpriteRenderer _sr;
        private Transform _square;
        private Camera _cam;
        public Chess.Piece piece { get; set; }

        private void Awake() {
            _sr = gameObject.GetComponent<SpriteRenderer>();
            _cam = Camera.main;
        }
        
        private void OnMouseDown() {
            _sr.sortingOrder = 1;
            MoveToMouse();
        }
        
        private void OnMouseDrag() {
            MoveToMouse();
        }
        
        private void OnMouseUp() {
            MoveToNearestSquare();
        }
        
        private void MoveToMouse() {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0 - _cam.transform.position.z; // distance from camera
            transform.position = _cam.ScreenToWorldPoint(mousePos);
        }
        
        private void MoveToNearestSquare() {
            Vector2 mousePosWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D pointCollider = Physics2D.OverlapPoint(mousePosWorld, LayerMask.GetMask("Board"));

            if (pointCollider != null) {
                foreach (Piece p in pointCollider.GetComponentsInChildren<Piece>()) {
                    GameManager.CapturePiece(p);
                }

                transform.parent = pointCollider.transform;
            }

            transform.position = transform.parent.position;
            _sr.sortingOrder = 0;
        }

        public void SetSprite(Sprite sprite) {
            _sr.sprite = sprite;
        }
    }
}