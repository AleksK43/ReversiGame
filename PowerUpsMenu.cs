using Microsoft.Maui.Controls;
using Reversi.Game;

namespace Reversi
{
    public class PowerUpsMenu : VerticalStackLayout
    {
        private readonly Dictionary<bool, List<PowerUp>> playerPowerUps = new();
        private readonly TwoPlayerGamePage gamePage;

        public PowerUpsMenu(TwoPlayerGamePage gamePage)
        {
            this.gamePage = gamePage;

            BackgroundColor = Colors.Transparent;
            Padding = new Thickness(10);
            Spacing = 10;
            MinimumWidthRequest = 200;

            // Inicjalizuj listy powerupów dla obu graczy
            playerPowerUps[true] = new List<PowerUp>(PowerUps.AvailablePowerUps);  // Dla gracza 1
            playerPowerUps[false] = new List<PowerUp>(PowerUps.AvailablePowerUps); // Dla gracza 2

            CreateButtons();
        }

        private void CreateButtons()
        {
            Children.Clear();

            var currentPlayerPowerUps = playerPowerUps[gamePage.IsBlackTurn];

            if (currentPlayerPowerUps.Count == 0)
            {
                Children.Add(new Label
                {
                    Text = "Wykorzystano wszystkie ulepszenia",
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 10)
                });
                return;
            }

            foreach (var powerUp in currentPlayerPowerUps)
            {
                var frame = new Frame
                {
                    BackgroundColor = Colors.Transparent,
                    BorderColor = Colors.Green,
                    Padding = new Thickness(10),
                    Content = new VerticalStackLayout
                    {
                        Children =
                        {
                            new Button
                            {
                                Text = powerUp.Name,
                                BackgroundColor = Colors.DarkGreen,
                                TextColor = Colors.White,
                                HeightRequest = 40
                            },
                            new Label
                            {
                                Text = powerUp.Description,
                                TextColor = Colors.Gray,
                                HorizontalOptions = LayoutOptions.Center,
                                Margin = new Thickness(0, 5, 0, 0)
                            }
                        }
                    }
                };

                ((Button)((VerticalStackLayout)frame.Content).Children[0]).Clicked += (s, e) => UsePowerUp(powerUp);
                Children.Add(frame);
            }
        }

        private void UsePowerUp(PowerUp powerUp)
        {
            powerUp.Effect(gamePage);
            playerPowerUps[gamePage.IsBlackTurn].Remove(powerUp);
            CreateButtons();
        }

        public void UpdateColors(bool isBlackTurn)
        {
            var color = isBlackTurn ? Colors.Green : Colors.Magenta;
            CreateButtons(); // Odśwież przyciski aby pokazać powerupy aktualnego gracza

            foreach (var child in Children)
            {
                if (child is Frame frame)
                {
                    frame.BorderColor = color;
                    if (frame.Content is VerticalStackLayout stack)
                    {
                        foreach (var stackChild in stack.Children)
                        {
                            if (stackChild is Button button)
                            {
                                button.BackgroundColor = color;
                            }
                        }
                    }
                }
            }
        }
    }
}