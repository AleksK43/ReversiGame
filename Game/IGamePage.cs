using Microsoft.Maui.Controls;
using Reversi.Game;

namespace Reversi
{
    public interface IGamePage
    {
        Button[,] BoardButtons { get; }
        Task HandleMove(int row, int col);
        Task EndGame();
        bool IsBlackTurn { get; }
        int[,] GameBoard { get; }
        void EnableDoubleMove();
        void SwapColors();
        void AddExtraTime(int seconds);
    }
}