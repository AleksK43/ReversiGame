using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Reversi.Game
{
    public class PowerUp
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Action<TwoPlayerGamePage> Effect { get; set; }
    }

    public static class PowerUps
    {
        public static List<PowerUp> AvailablePowerUps = new()
        {
            new PowerUp
            {
                Name = "Double Move",
                Description = "Wykonaj dwa ruchy pod rząd",
                Effect = (game) => {
                    game.EnableDoubleMove();
                    if (game.CurrentPlayerButton is Button button)
                    {
                        Application.Current.Dispatcher.Dispatch(async () => {
                            await button.ScaleTo(1.2, 150);
                            await button.ScaleTo(1, 150);
                            button.BorderWidth = 3;
                            button.BorderColor = Colors.Gold;
                        });
                    }
                }
            },
            new PowerUp
           {
                Name = "Color Swap",
                Description = "Zamień kolory wszystkich pionków",
                Effect = async (game) => {
                var buttons = game.BoardButtons;
                foreach (var button in buttons)
                {
                    if (button.BackgroundColor != UIColors.EmptyCell &&
                    button.BackgroundColor != UIColors.ValidMove)
                 
                {
                await button.RotateYTo(90, 300);
                button.BackgroundColor = button.BackgroundColor == UIColors.Player1Color ?
                UIColors.Player2Color : UIColors.Player1Color;
                await button.RotateYTo(0, 300);
                }
                }
                 game.SwapColors();
                }
            },
            new PowerUp
            {
                Name = "Extra Time",
                Description = "+30 sekund do czasu",
                Effect = (game) => {
                    const int extraSeconds = 30;
                    game.AddExtraTime(extraSeconds);

                    Application.Current.Dispatcher.Dispatch(async () => {
                        if (game.TimerLabel != null)
                        {
                            game.TimerLabel.TextColor = Colors.Green;
                            game.TimerLabel.Scale = 1;

                            for (int i = 0; i < 3; i++)
                            {
                                await game.TimerLabel.ScaleTo(1.2, 100);
                                await game.TimerLabel.ScaleTo(1, 100);
                            }

                            game.TimerLabel.TextColor = Colors.White;
                        }
                    });
                }
            },
            new PowerUp
            {
                Name = "Area Control",
                Description = "Przejmij kontrolę nad losowym rogiem planszy",
                Effect = async (game) => {
                    var corners = new List<(int row, int col)>
                    {
                        (0, 0), (0, 7),
                        (7, 0), (7, 7)
                    };

                    var random = new Random();
                    var availableCorners = corners.Where(c =>
                        game.GameBoard[c.row, c.col] == 0).ToList();

                    if (availableCorners.Any())
                    {
                        var corner = availableCorners[random.Next(availableCorners.Count)];
                        await Application.Current.Dispatcher.DispatchAsync(async () => {
                            var button = game.BoardButtons[corner.row, corner.col];
                            await button.ScaleTo(1.5, 200);
                            await button.ScaleTo(0.5, 200);

                            button.BackgroundColor = game.IsBlackTurn
                                ? UIColors.Player1Color
                                : UIColors.Player2Color;

                            await button.ScaleTo(1, 200);
                        });
                    }
                }
            }
        };
    }
}