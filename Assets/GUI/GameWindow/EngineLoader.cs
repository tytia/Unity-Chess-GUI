using System.IO;
using SFB;
using UnityEngine;

namespace GUI.GameWindow {
    public class EngineLoader : MonoBehaviour {
        [SerializeField] private GameObject _invalidPathText;

        private void Awake() {
            _invalidPathText.SetActive(!File.Exists(GameManager.engine.path));
        }

        public void LoadEngine() {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select engine", "", "exe", false);
            if (paths.Length != 0) {
                PlayerPrefs.SetString("engine_path", paths[0]);
                GameManager.engine.path = paths[0];
                _invalidPathText.SetActive(false);
            }
        }
    }
}