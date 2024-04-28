using System.IO;
using Chess;
using SFB;
using UnityEngine;

namespace GUI.GameWindow {
    public class EngineLoader : MonoBehaviour {
        [SerializeField] private GameObject _invalidPathText;

        private void Awake() {
            _invalidPathText.SetActive(!File.Exists(PlayerPrefs.GetString("engine_path")));
            Debug.Log(PlayerPrefs.GetString("engine_path"));
        }

        public void LoadEngine() {
            string directory = PlayerPrefs.HasKey("engine_path") ? Path.GetDirectoryName(PlayerPrefs.GetString("engine_path")) : "";
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select engine", directory, "exe", false);
            if (paths.Length != 0) {
                PlayerPrefs.SetString("engine_path", paths[0]);
                UCIThread.engine_path = paths[0];
                _invalidPathText.SetActive(false);
            }
        }
    }
}