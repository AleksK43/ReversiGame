using Microsoft.Maui.Controls;

namespace Reversi.Game
{
    public class ReversiGame : GameBase
    {
        private readonly Reversi.IGamePage gamePage;
        private bool hasDoubleMove = false;

        public ReversiGame(Reversi.IGamePage gamePage)
        {
            this.gamePage = gamePage;
        }

        public override void InitializeGame()
        {
            SetInitialPosition();
            UpdateScore();
        }

        public override void EnableDoubleMove()
        {
            hasDoubleMove = true;
        }

        public override void SwapColors()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (GameBoard[row, col] != 0)
                    {
                        GameBoard[row, col] = GameBoard[row, col] == 1 ? 2 : 1;
                    }
                }
            }
            UpdateScore();
        }

        public override void AddExtraTime(int seconds)
        {
            // Implementacja dodawania czasu będzie obsługiwana przez stronę gry
            if (gamePage is TwoPlayerGamePage twoPlayerGame)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    twoPlayerGame.AddExtraTime(seconds);
                });
            }
        }

        protected override async Task EndGame()
        {
            UpdateScore();
            await gamePage.EndGame();
        }
    }
}