namespace Reversi.Game
{
    public class ComputerPlayer
    {
        private readonly GameBase gameLogic;
        private readonly Random random = new Random();

        public ComputerPlayer(GameBase gameLogic)
        {
            this.gameLogic = gameLogic;
        }

        public (int Row, int Col)? GetNextMove()
        {
            var validMoves = new List<(int Row, int Col)>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (gameLogic.IsValidMove(row, col))
                    {
                        validMoves.Add((row, col));
                    }
                }
            }

            if (!validMoves.Any())
                return null;

            var cornerMoves = validMoves.Where(move => IsCornerMove(move.Row, move.Col));
            if (cornerMoves.Any())
                return cornerMoves.ElementAt(random.Next(cornerMoves.Count()));

            var edgeMoves = validMoves.Where(move => IsEdgeMove(move.Row, move.Col));
            if (edgeMoves.Any())
                return edgeMoves.ElementAt(random.Next(edgeMoves.Count()));

            return validMoves[random.Next(validMoves.Count)];
        }

        private bool IsCornerMove(int row, int col)
        {
            return (row == 0 || row == 7) && (col == 0 || col == 7);
        }

        private bool IsEdgeMove(int row, int col)
        {
            return row == 0 || row == 7 || col == 0 || col == 7;
        }
    }
}