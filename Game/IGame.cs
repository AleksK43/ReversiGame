
namespace Reversi.Game
{
    public interface IGame
    {
        bool IsBlackTurn { get; }
        int[,] GameBoard { get; }
        void EnableDoubleMove();
        void SwapColors();
        void AddExtraTime(int seconds);
    }
}