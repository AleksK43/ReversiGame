using Microsoft.Maui.Controls;
using Reversi.Game;
using Reversi.Animations;

namespace Reversi
{
    public class ComputerGamePage : ContentPage, IGamePage
    {
        #region Fields
        private const int BOARD_SIZE = 8;
        private readonly GameBase gameLogic;
        private readonly ComputerPlayer computerPlayer;
        private Button[,] boardButtons;
        private Label statusLabel;
        private Grid boardGrid;
        private (int Row, int Col)? lastMove = null;
        private bool isInitialized = false;
        public Button[,] BoardButtons => boardButtons;
        #endregion

        public async Task HandleMove(int row, int col)
        {
            await OnBoardButtonClicked(row, col);
        }

        public ComputerGamePage()
        {
            gameLogic = new ReversiGame(this);
            computerPlayer = new ComputerPlayer(gameLogic);
            boardButtons = new Button[BOARD_SIZE, BOARD_SIZE];
            BackgroundColor = Colors.Black;
            gameLogic.InitializeGame();
            InitializeUI();
        }

        private void InitializeUI()
        {
            var mainGrid = new Grid
            {
                RowSpacing = 5,
                Padding = new Thickness(20)
            };

            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Board
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Return button

            InitializeStatusLabel(mainGrid);
            InitializeGameBoard(mainGrid);
            InitializeReturnButton(mainGrid);

            Content = mainGrid;
            RefreshGameDisplay();
        }

        private void InitializeStatusLabel(Grid mainGrid)
        {
            statusLabel = new Label
            {
                TextColor = UIColors.Player1Color,
                FontSize = 24,
                FontFamily = "Courier New",
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            mainGrid.Add(statusLabel, 0, 0);
            UpdateStatusLabel();
        }

        private void InitializeGameBoard(Grid mainGrid)
        {
            boardGrid = new Grid
            {
                RowSpacing = 4,
                ColumnSpacing = 4,
                BackgroundColor = UIColors.BoardBackground,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10)
            };

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                boardGrid.RowDefinitions.Add(new RowDefinition { Height = 70 });
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 70 });
            }

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    CreateBoardButton(row, col);
                }
            }

            mainGrid.Add(boardGrid, 0, 1);
        }

        private void CreateBoardButton(int row, int col)
        {
            var button = new Button
            {
                BackgroundColor = UIColors.EmptyCell,
                CornerRadius = 35,
                Margin = new Thickness(2),
                WidthRequest = 70,
                HeightRequest = 70,
                BorderColor = UIColors.CellBorder,
                BorderWidth = 2
            };

            button.Clicked += async (s, e) => await OnBoardButtonClicked(row, col);
            boardButtons[row, col] = button;
            boardGrid.Add(button, col, row);
        }

        private void InitializeReturnButton(Grid mainGrid)
        {
            var returnButton = new Button
            {
                Text = "Powrót do Menu",
                BackgroundColor = UIColors.MenuButtonBackground,
                TextColor = UIColors.Player1Color,
                BorderColor = UIColors.Player1Color,
                BorderWidth = 1,
                FontFamily = "Courier New",
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            returnButton.Clicked += OnReturnClicked;
            mainGrid.Add(returnButton, 0, 2);
        }

        private async Task OnBoardButtonClicked(int row, int col)
        {
            if (!gameLogic.IsBlackTurn) // Nie pozwól grać podczas tury komputera
                return;

            if (!gameLogic.IsValidMove(row, col))
                return;

            await ExecuteMove(row, col);

            // Po ruchu gracza, wykonaj ruch komputera
            await Task.Delay(500); // Małe opóźnienie przed ruchem komputera
            await MakeComputerMove();
        }

        private async Task ExecuteMove(int row, int col)
        {
            var flippedPieces = gameLogic.GetFlippedPieces(row, col);
            if (flippedPieces.Count == 0)
                return;

            lastMove = (row, col);
            var currentColor = gameLogic.IsBlackTurn ? UIColors.Player1Color : UIColors.Player2Color;

            gameLogic.PlacePiece(row, col);
            await GameAnimations.AnimatePiecePlacement(boardButtons[row, col], currentColor);

            foreach (var piece in flippedPieces)
            {
                gameLogic.FlipPiece(piece.Row, piece.Col);
                await GameAnimations.AnimateFlip(boardButtons[piece.Row, piece.Col], currentColor);
            }

            gameLogic.SwitchTurn();
            RefreshGameDisplay();

            if (!gameLogic.HasValidMoves())
            {
                await HandleNoValidMoves();
            }
        }

        private async Task MakeComputerMove()
        {
            var move = computerPlayer.GetNextMove();
            if (move.HasValue)
            {
                await ExecuteMove(move.Value.Row, move.Value.Col);
            }
        }

        private async Task HandleNoValidMoves()
        {
            gameLogic.SwitchTurn();
            if (!gameLogic.HasValidMoves())
            {
                await EndGame();
            }
            else
            {
                RefreshGameDisplay();
                if (!gameLogic.IsBlackTurn)
                {
                    await MakeComputerMove();
                }
            }
        }

        public async Task EndGame()
        {
            var victoryAnimation = new VictoryAnimation(UIColors.Player1Color, UIColors.Player2Color);
            await victoryAnimation.ShowVictoryAnimation(
                (Grid)boardGrid.Parent,
                boardGrid,
                gameLogic.BlackCount > gameLogic.WhiteCount,
                gameLogic.BlackCount,
                gameLogic.WhiteCount
            );

            await Shell.Current.GoToAsync("..");
        }

        private void RefreshGameDisplay()
        {
            gameLogic.UpdateScore();
            HighlightValidMoves();
            UpdateStatusLabel();
        }

        private void UpdateStatusLabel()
        {
            string currentPlayer = gameLogic.IsBlackTurn ? "Twój ruch" : "Ruch komputera";
            statusLabel.TextColor = gameLogic.IsBlackTurn ? UIColors.Player1Color : UIColors.Player2Color;
            statusLabel.Text = $"Ty: {gameLogic.BlackCount}  Komputer: {gameLogic.WhiteCount}\n{currentPlayer}";
        }

        private void HighlightValidMoves()
        {
            if (!gameLogic.IsBlackTurn) return; // Nie pokazuj podpowiedzi podczas tury komputera

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    var button = boardButtons[row, col];
                    if (gameLogic.GetPieceAt(row, col) == 0)
                    {
                        button.BackgroundColor = UIColors.EmptyCell;
                        button.BorderColor = UIColors.CellBorder;

                        if (gameLogic.IsValidMove(row, col))
                        {
                            button.BackgroundColor = UIColors.ValidMove;
                            button.BorderColor = UIColors.Player1Color;
                        }
                    }
                }
            }
        }

        private async void OnReturnClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Potwierdź",
                "Czy na pewno chcesz wrócić do menu?",
                "Tak",
                "Nie"
            );

            if (answer)
            {
                await Task.WhenAll(
                    boardGrid.ScaleTo(0.8, 300, Easing.CubicIn),
                    boardGrid.FadeTo(0, 300)
                );
                await Shell.Current.GoToAsync(".."); 
            }
        }

        #region IGame Implementation
        public void EnableDoubleMove() { }
        public void SwapColors() { }
        public void AddExtraTime(int seconds) { }
        public bool IsBlackTurn => gameLogic.IsBlackTurn;
        public int[,] GameBoard => gameLogic.GameBoard;
        #endregion
    }
}