using UnityEngine;

namespace GUI {
    public class LimitFPS : MonoBehaviour {
        private void Start() {
            Application.targetFrameRate = 60;
        }
    }
}