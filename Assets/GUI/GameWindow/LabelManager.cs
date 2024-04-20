using System;
using Chess;
using TMPro;
using UnityEngine;
using Utility;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class LabelManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        // darker color needed for legibility
        private static readonly Color _labelDarkCol = new Color32(42, 17, 11, 255);
        private readonly TextMeshProUGUI[] _fileLabels = new TextMeshProUGUI[8];
        private readonly TextMeshProUGUI[] _rankLabels = new TextMeshProUGUI[8];
        

        private void Start() {
            DrawFileLabels();
            DrawRankLabels();
        }

        private void DrawFileLabels() {
            for (var i = 0; i <= 7; i++) {
                var point = i.ToSquarePosVector2(PieceColor.White);
                TextMeshProUGUI lb = Instantiate(_label, point, Quaternion.identity, transform);
                lb.text = ((SquarePos)i).ToString()[0].ToString();
                lb.alignment = TextAlignmentOptions.BottomRight;
                lb.color = (point.x + point.y) % 2 == 0 ? Square.lightColor : _labelDarkCol;
                lb.name = lb.text;
                _fileLabels[i] = lb;
            }
        }

        private void DrawRankLabels() {
            for (var i = 0; i <= 56; i += 8) {
                var point = i.ToSquarePosVector2(PieceColor.White);
                TextMeshProUGUI lb = Instantiate(_label, point, Quaternion.identity, transform);
                lb.text = ((SquarePos)i).ToString()[1].ToString();
                lb.alignment = TextAlignmentOptions.TopLeft;
                lb.color = (point.x + point.y) % 2 == 0 ? Square.lightColor : _labelDarkCol;
                lb.name = lb.text;
                _rankLabels[i / 8] = lb;
            }
        }

        public void Flip() {
            foreach (TextMeshProUGUI lb in _fileLabels) {
                lb.text = ((char)('a' + 'h' - lb.text.ToCharArray()[0])).ToString();
                lb.name = lb.text;
            }

            foreach (TextMeshProUGUI lb in _rankLabels) {
                lb.text = (9 - Int32.Parse(lb.text)).ToString();
                lb.name = lb.text;
            }
        }
    }
}
