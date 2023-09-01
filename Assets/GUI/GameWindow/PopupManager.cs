using Chess;
using UnityEngine;

namespace GUI.GameWindow {
    public interface IPopup {
        public void Show(bool value);
    }

    public static class PopupManager {
        // popup scripts are assigned to static variables in their Start() methods
        public static PawnPromotionPopup pawnPromotion { private get; set; }

        public static void ShowPawnPromotionPopup(Piece pawn) {
            pawnPromotion.Assign(pawn);
            pawnPromotion.Show(true);
        }
    }
}