using System.Text;

namespace Reversi.Game
{
    public abstract class GameBase : IGame
    {
        protected const int BOARD_SIZE = 8;
        protected int[,] gameBoard;
        protected bool isBlackTurn = true;
        protected int blackCount = 2;
        protected int whiteCount = 2;

        #region Properties
        public bool IsBlackTurn => isBlackTurn;
        public int[,] GameBoard => gameBoard;
        public int BlackCount => blackCount;
        public int WhiteCount => whiteCount;
        #endregion

        protected GameBase()
        {
            gameBoard = new int[BOARD_SIZE, BOARD_SIZE];
            InitializeGame();
        }

        #region Abstract Methods
        public abstract void InitializeGame();
        public abstract void EnableDoubleMove();
        public abstract void SwapColors();
        public abstract void AddExtraTime(int seconds);
        protected abstract Task EndGame();
        #endregion

        #region Game Logic Methods
        public void SwitchTurn() => isBlackTurn = !isBlackTurn;

        public bool IsValidMove(int row, int col)
        {
            if (!IsValidPosition(row, col) || gameBoard[row, col] != 0)
                return false;

            return GetFlippedPieces(row, col).Count > 0;
        }

        public List<(int Row, int Col)> GetFlippedPieces(int row, int col)
        {
            var flippedPieces = new List<(int Row, int Col)>();
            int currentPlayer = isBlackTurn ? 1 : 2;
            int opponent = isBlackTurn ? 2 : 1;

            int[] directions = { -1, 0, 1 };
            foreach (int dRow in directions)
            {
                foreach (int dCol in directions)
                {
                    if (dRow == 0 && dCol == 0)
                        continue;

                    CheckDirection(row, col, dRow, dCol, currentPlayer, opponent, flippedPieces);
                }
            }

            return flippedPieces;
        }

        private void CheckDirection(int row, int col, int dRow, int dCol,
            int currentPlayer, int opponent, List<(int Row, int Col)> flippedPieces)
        {
            var tempFlipped = new List<(int Row, int Col)>();
            int currentRow = row + dRow;
            int currentCol = col + dCol;

            while (IsValidPosition(currentRow, currentCol) &&
                   gameBoard[currentRow, currentCol] == opponent)
            {
                tempFlipped.Add((currentRow, currentCol));
                currentRow += dRow;
                currentCol += dCol;
            }

            if (IsValidPosition(currentRow, currentCol) &&
                gameBoard[currentRow, currentCol] == currentPlayer &&
                tempFlipped.Count > 0)
            {
                flippedPieces.AddRange(tempFlipped);
            }
        }

        public bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
        }

        public bool HasValidMoves()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (gameBoard[row, col] == 0 && IsValidMove(row, col))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Board Management Methods
        public void SwapPiece(int row, int col)
        {
            if (gameBoard[row, col] == 1)
                gameBoard[row, col] = 2;
            else if (gameBoard[row, col] == 2)
                gameBoard[row, col] = 1;

            UpdateScore();
        }

        public void FlipPiece(int row, int col)
        {
            if (IsValidPosition(row, col) && gameBoard[row, col] != 0)
            {
                gameBoard[row, col] = gameBoard[row, col] == 1 ? 2 : 1;
                UpdateScore();
            }
        }

        public void PlacePiece(int row, int col)
        {
            if (IsValidMove(row, col))
            {
                gameBoard[row, col] = isBlackTurn ? 1 : 2;
                var flippedPieces = GetFlippedPieces(row, col);
                foreach (var (flipRow, flipCol) in flippedPieces)
                {
                    gameBoard[flipRow, flipCol] = isBlackTurn ? 1 : 2;
                }
                UpdateScore();
            }
        }

        public int GetPieceAt(int row, int col)
        {
            return IsValidPosition(row, col) ? gameBoard[row, col] : 0;
        }

        public void SetInitialPosition()
        {
            ClearBoard();
            int center = BOARD_SIZE / 2;
            gameBoard[center - 1, center - 1] = 2; // Biały
            gameBoard[center - 1, center] = 1;     // Czarny
            gameBoard[center, center - 1] = 1;     // Czarny
            gameBoard[center, center] = 2;         // Biały
            UpdateScore();
        }

        public void ClearBoard()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
                for (int col = 0; col < BOARD_SIZE; col++)
                    gameBoard[row, col] = 0;

            UpdateScore();
        }
        #endregion

        #region Score and State Methods
        public void UpdateScore()
        {
            blackCount = 0;
            whiteCount = 0;

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (gameBoard[row, col] == 1)
                        blackCount++;
                    else if (gameBoard[row, col] == 2)
                        whiteCount++;
                }
            }
        }

        public bool IsBoardFull()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
                for (int col = 0; col < BOARD_SIZE; col++)
                    if (gameBoard[row, col] == 0)
                        return false;
            return true;
        }

        public bool CanPlayerMove(bool isBlackPlayer)
        {
            bool originalTurn = isBlackTurn;
            isBlackTurn = isBlackPlayer;
            bool canMove = HasValidMoves();
            isBlackTurn = originalTurn;
            return canMove;
        }
        #endregion

        #region Helper Methods
        protected bool IsCornerPosition(int row, int col)
        {
            return (row == 0 || row == BOARD_SIZE - 1) &&
                   (col == 0 || col == BOARD_SIZE - 1);
        }

        protected bool IsEdgePosition(int row, int col)
        {
            return row == 0 || row == BOARD_SIZE - 1 ||
                   col == 0 || col == BOARD_SIZE - 1;
        }

        public string GetBoardState()
        {
            var state = new StringBuilder();
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    state.Append(gameBoard[row, col]);
                    state.Append(' ');
                }
                state.AppendLine();
            }
            return state.ToString();
        }
        #endregion
    }
}
