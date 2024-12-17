using Microsoft.Maui.Controls;
using Reversi.Animations;
using Reversi.Game;

namespace Reversi
{
    [QueryProperty(nameof(Mode), "mode")]
    public class TwoPlayerGamePage : ContentPage, IGamePage
    {
        #region Fields
        private const int BOARD_SIZE = 8;
        private readonly GameBase gameLogic;
        private Button[,] boardButtons;
        private Label statusLabel;
        private Label timerLabel;
        private Grid boardGrid;
        private GameTimer gameTimer;
        private (int Row, int Col)? lastMove = null;
        private Dictionary<(int Row, int Col), PowerUp> powerUpCells = new();
        private bool hasDoubleMove = false;
        private PowerUpsMenu powerUpsMenu;
        private bool isInitialized = false;
        #endregion

        #region Properties
        private GameMode _mode;
        public GameMode Mode
        {
            get => _mode;
            set
            {
                if (_mode == value) return;
                _mode = value;
                System.Diagnostics.Debug.WriteLine($"Mode set to: {_mode}");
                if (!isInitialized)
                {
                    InitializeGameMode(_mode);
                    InitializeUI();
                    isInitialized = true;
                }
            }
        }

        public bool IsBlackTurn => gameLogic.IsBlackTurn;
        public int[,] GameBoard => gameLogic.GameBoard;
        public Button[,] BoardButtons => boardButtons;
        public Label TimerLabel => timerLabel;
        public Button CurrentPlayerButton => GetCurrentPlayerButton();
        #endregion

        public TwoPlayerGamePage()
        {
            gameLogic = new ReversiGame(this);
            boardButtons = new Button[BOARD_SIZE, BOARD_SIZE];
            BackgroundColor = Colors.Black;

            gameLogic.InitializeGame();
        }

        private void InitializeGameMode(GameMode mode)
        {
            if (!GameModeSettings.Settings.TryGetValue(mode, out var settings))
            {
                throw new ArgumentOutOfRangeException(nameof(mode), $"Invalid game mode: {mode}");
            }

            if (settings.TimePerMove > 0)
            {
                gameTimer = new GameTimer(timerLabel, settings.TimePerMove, OnTimeUp);
            }
        }

        private void InitializeUI()
        {
            var mainGrid = new Grid
            {
                RowSpacing = 5,
                Padding = new Thickness(20)
            };

            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            InitializeStatusLabel(mainGrid);
            InitializeTimerLabel(mainGrid);
            InitializeGameBoard(mainGrid);
            InitializePowerUpsControls(mainGrid);
            InitializeReturnButton(mainGrid);

            Content = mainGrid;

            if (Mode == GameMode.Blitz || Mode == GameMode.PowerUp)
            {
                var settings = GameModeSettings.Settings[Mode];
                gameTimer = new GameTimer(timerLabel, settings.TimePerMove, OnTimeUp);
            }

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

        private void InitializeTimerLabel(Grid mainGrid)
        {
            timerLabel = new Label
            {
                Text = "Czas: --",
                TextColor = Colors.White,
                FontSize = 20,
                FontFamily = "Courier New",
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            mainGrid.Add(timerLabel, 0, 1);
        }

        private void InitializePowerUpsControls(Grid mainGrid)
        {
            if (Mode != GameMode.PowerUp) return;

            var powerUpsContainer = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(10),
                MinimumWidthRequest = 200
            };

            powerUpsMenu = new PowerUpsMenu(this);
            powerUpsMenu.UpdateColors(IsBlackTurn);

            powerUpsContainer.Children.Add(new Label
            {
                Text = "Dostępne ulepszenia:",
                TextColor = Colors.Green,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });
            powerUpsContainer.Children.Add(powerUpsMenu);

            mainGrid.Add(powerUpsContainer, 1, 0);
            Grid.SetRowSpan(powerUpsContainer, 4);
        }

        private void InitializeGameBoard(Grid mainGrid)
        {
            boardGrid = new Grid
            {
                RowSpacing = 4,               // zwiększone z 2
                ColumnSpacing = 4,            // zwiększone z 2
                BackgroundColor = UIColors.BoardBackground,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10)    // zwiększone z 5
            };

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                boardGrid.RowDefinitions.Add(new RowDefinition { Height = 70 });     // zwiększone z 50
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 70 }); // zwiększone z 50
            }

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    CreateBoardButton(row, col);
                }
            }

            mainGrid.Add(boardGrid, 0, 2);
        }

        private Color GetInitialButtonColor(int piece)
        {
            return piece switch
            {
                1 => UIColors.Player1Color,
                2 => UIColors.Player2Color,
                _ => UIColors.EmptyCell
            };
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
            mainGrid.Add(returnButton, 0, 3);
        }

        #region Game Logic
        private async Task OnBoardButtonClicked(int row, int col)
        {
            if (!gameLogic.IsValidMove(row, col))
                return;

            var flippedPieces = gameLogic.GetFlippedPieces(row, col);
            if (flippedPieces.Count == 0)
                return;

            gameTimer?.StopTurn();
            lastMove = (row, col);

            await ExecuteMove(row, col, flippedPieces);

            if (!hasDoubleMove)
            {
                gameLogic.SwitchTurn();
            }
            else
            {
                hasDoubleMove = false;
                await GameAnimations.AnimateDoubleMovePowerUp(GetCurrentPlayerButton());
            }

            RefreshGameDisplay();

            if (!gameLogic.HasValidMoves())
            {
                await HandleNoValidMoves();
            }

            gameTimer?.StartTurn();
        }

        private async Task ExecuteMove(int row, int col, List<(int Row, int Col)> flippedPieces)
        {
            var currentColor = IsBlackTurn ? UIColors.Player1Color : UIColors.Player2Color;
            var button = boardButtons[row, col];

            gameLogic.PlacePiece(row, col);
            await GameAnimations.AnimatePiecePlacement(button, currentColor);

            foreach (var piece in flippedPieces)
            {
                gameLogic.FlipPiece(piece.Row, piece.Col);
                await GameAnimations.AnimateFlip(boardButtons[piece.Row, piece.Col], currentColor);
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
                await GameAnimations.AnimateNoMovesAvailable(statusLabel,
                    IsBlackTurn ? UIColors.Player1Color : UIColors.Player2Color);
                RefreshGameDisplay();
            }
        }

        public async Task EndGame()
        {
            gameTimer?.StopTurn();

            if (gameLogic.BlackCount != gameLogic.WhiteCount)
            {
                var victoryAnimation = new VictoryAnimation(UIColors.Player1Color, UIColors.Player2Color);
                await victoryAnimation.ShowVictoryAnimation(
                    (Grid)boardGrid.Parent,
                    boardGrid,
                    gameLogic.BlackCount > gameLogic.WhiteCount,
                    gameLogic.BlackCount,
                    gameLogic.WhiteCount
                );
            }
            else
            {
                await DisplayAlert("Remis!",
                    $"Gra zakończona remisem!\nZielony: {gameLogic.BlackCount}\nMagenta: {gameLogic.WhiteCount}",
                    "OK"
                );
                await boardGrid.FadeTo(0, 500);
            }

            await Shell.Current.GoToAsync("..");
        }
        #endregion

        #region UI Updates
        private void RefreshGameDisplay()
        {
            gameLogic.UpdateScore();
            HighlightValidMoves();
            UpdateStatusLabel();

            if (Mode == GameMode.PowerUp && powerUpsMenu != null)
            {
                powerUpsMenu.UpdateColors(IsBlackTurn);
            }
        }

        private void UpdateStatusLabel()
        {
            string currentPlayer = IsBlackTurn ? "Gracz 1 (Zielony)" : "Gracz 2 (Magenta)";
            statusLabel.TextColor = IsBlackTurn ? UIColors.Player1Color : UIColors.Player2Color;
            statusLabel.Text = $"Zielony: {gameLogic.BlackCount}  Magenta: {gameLogic.WhiteCount}\nRuch: {currentPlayer}";
        }

        private void HighlightValidMoves()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    UpdateCellHighlight(row, col);
                }
            }
        }

        private void UpdateCellHighlight(int row, int col)
        {
            var button = boardButtons[row, col];
            if (gameLogic.GetPieceAt(row, col) == 0)
            {
                button.BackgroundColor = UIColors.EmptyCell;
                button.BorderColor = powerUpCells.ContainsKey((row, col))
                    ? UIColors.PowerUpColor
                    : UIColors.CellBorder;

                if (gameLogic.IsValidMove(row, col))
                {
                    button.BackgroundColor = UIColors.ValidMove;
                    button.BorderColor = IsBlackTurn ? UIColors.Player1Color : UIColors.Player2Color;
                }
            }
        }
        #endregion

        #region Event Handlers
        private void OnTimeUp()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await GameAnimations.AnimateTimerWarning(timerLabel);
                await DisplayAlert("Koniec czasu!",
                    $"Czas na ruch minął!\n{(IsBlackTurn ? "Gracz 2 (Magenta)" : "Gracz 1 (Zielony)")} wygrywa!",
                    "OK");

               
                if (IsBlackTurn)
                {
                    for (int row = 0; row < BOARD_SIZE; row++)
                        for (int col = 0; col < BOARD_SIZE; col++)
                            if (gameLogic.GetPieceAt(row, col) == 0)
                                gameLogic.GameBoard[row, col] = 2;
                }
                else
                {
                    for (int row = 0; row < BOARD_SIZE; row++)
                        for (int col = 0; col < BOARD_SIZE; col++)
                            if (gameLogic.GetPieceAt(row, col) == 0)
                                gameLogic.GameBoard[row, col] = 1;
                }

                await EndGame();
            });
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
                gameTimer?.StopTurn();
                await Task.WhenAll(
                    boardGrid.ScaleTo(0.8, 300, Easing.CubicIn),
                    boardGrid.FadeTo(0, 300)
                );
                await Shell.Current.GoToAsync("..");
            }
        }
        #endregion

        #region Helper Methods
        private Button GetCurrentPlayerButton()
        {
            return lastMove.HasValue ? boardButtons[lastMove.Value.Row, lastMove.Value.Col] : null;
        }
        #endregion

        #region IGame Implementation
        public void EnableDoubleMove()
        {
            hasDoubleMove = true;
        }

        public void SwapColors()
        {
            gameLogic.SwapColors();
            RefreshGameDisplay();
        }

        public void AddExtraTime(int seconds)
        {
            gameTimer?.AddTime(seconds);
        }

        public async Task HandleMove(int row, int col)
        {
            await OnBoardButtonClicked(row, col);
        }
        #endregion

        #region Cleanup
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            gameTimer?.Dispose();
        }
        #endregion
    }
}