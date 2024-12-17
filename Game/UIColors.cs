// Game/UIColors.cs
namespace Reversi.Game
{
    public static class UIColors
    {
        public static Color EmptyCell = Colors.Black;
        public static Color Player1Color = Color.FromArgb("#00FF00"); // Matrix green
        public static Color Player2Color = Color.FromArgb("#FF00FF"); // Magenta
        public static Color ValidMove = Color.FromArgb("#003300");
        public static Color PowerUpColor = Color.FromArgb("#FFD700");
        public static Color BoardBackground = Color.FromArgb("#001100");
        public static Color CellBorder = Color.FromArgb("#003300");
        public static Color MenuButtonBackground = Color.FromArgb("#001100");
    }
}