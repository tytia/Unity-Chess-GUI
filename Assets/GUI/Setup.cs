using GUI.GameWindow;
using UnityEngine;

namespace GUI {
    public class Setup : MonoBehaviour {
        [SerializeField] private RectTransform _canvas;

        private void Awake() {
            CenterCanvas();
            SystemMoveHandler.Initialise();
        }

        private void Start() {
            Application.targetFrameRate = 60;
        }
        
        private void CenterCanvas() {
            transform.position = new Vector3((float)8 / 2 - 0.5f, (float)8 / 2 - 0.5f, transform.position.z);
            _canvas.position = new Vector3(transform.position.x, transform.position.y, _canvas.position.z);
            
            // orthographic size is half of the viewport in world units
            // if the canvas' scale is 1, then the canvas' width and height have a 1:1 ratio with world units
            var cam = GetComponent<Camera>();
            _canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (cam.orthographicSize * 2 * cam.aspect) / _canvas.localScale.x);
            _canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (cam.orthographicSize * 2) / _canvas.localScale.x);
        }
    }
}