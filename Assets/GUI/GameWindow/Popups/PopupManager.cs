using Chess;

namespace GUI.GameWindow.Popups {
    public interface IPopup {
        public void Show(bool value);
    }

    public static class PopupManager {
        // popup scripts are assigned to static variables in their Start() methods
        public static PawnPromotionPopup pawnPromotionPopup { get; set; }
        public static NewGamePopup newGamePopup { get; set; }
        public static GameEndPopup gameEndPopup { get; set; }

        public static void ShowPawnPromotionPopup(Piece pawn) {
            pawnPromotionPopup.Assign(pawn);
            pawnPromotionPopup.Show(true);
        }
        
        public static void ShowNewGamePopup(bool value) {
            newGamePopup.Show(value);
        }
        
        public static void ShowGameEndPopup(bool value) {
            gameEndPopup.Show(value);
        }
    }
}