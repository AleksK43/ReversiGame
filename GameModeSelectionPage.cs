using Microsoft.Maui.Controls;
using Reversi.Game;

namespace Reversi
{
    public class GameModeSelectionPage : ContentPage
    {
        private Grid mainContainer;
        private readonly Color matrixGreen = Color.FromArgb("#00FF00");

        public GameModeSelectionPage()
        {
            BackgroundColor = Colors.Black;

            mainContainer = new Grid
            {
                RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },  // Tytuł
                        new RowDefinition { Height = GridLength.Star },  // Menu
                        new RowDefinition { Height = GridLength.Auto }   // Powrót
                    },
                Padding = new Thickness(20)
            };

            var titleLabel = new Label
            {
                Text = "WYBIERZ TRYB GRY",
                TextColor = matrixGreen,
                FontSize = 32,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 40)
            };
            mainContainer.Add(titleLabel, 0, 0);

            var menuStack = new StackLayout
            {
                Spacing = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var classicButton = CreateModeButton("Gra Klasyczna", "Standardowe zasady gry Reversi", "classic");
            menuStack.Children.Add(classicButton);

            var blitzButton = CreateModeButton("Gra Błyskawiczna", "30 sekund na ruch!", "blitz");
            menuStack.Children.Add(blitzButton);

            var powerButton = CreateModeButton("Gra z Ulepszeniami", "Specjalne pola i power-upy", "power");
            menuStack.Children.Add(powerButton);

            mainContainer.Add(menuStack, 0, 1);

            var returnButton = new Button
            {
                Text = "Powrót",
                BackgroundColor = Color.FromArgb("#001100"),
                TextColor = matrixGreen,
                BorderColor = matrixGreen,
                BorderWidth = 1,
                WidthRequest = 150,
                HeightRequest = 40,
                Margin = new Thickness(0, 20, 0, 0),
                HorizontalOptions = LayoutOptions.Center
            };
            returnButton.Clicked += async (s, e) => await Navigation.PopAsync();
            mainContainer.Add(returnButton, 0, 2);

            Content = mainContainer;
            StartBackgroundAnimation();
        }

        private Frame CreateModeButton(string title, string description, string mode)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.FromRgba("#00110055"),
                BorderColor = matrixGreen,
                CornerRadius = 10,
                Padding = new Thickness(15),
                WidthRequest = 300
            };

            var stack = new StackLayout();

            var button = new Button
            {
                Text = title,
                TextColor = matrixGreen,
                BackgroundColor = Color.FromRgba("#00220055"),
                BorderColor = matrixGreen,
                BorderWidth = 1,
                HeightRequest = 50
            };
            button.Clicked += async (s, e) => await StartGame(mode);
            stack.Children.Add(button);

            var descLabel = new Label
            {
                Text = description,
                TextColor = matrixGreen,
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };
            stack.Children.Add(descLabel);

            frame.Content = stack;
            return frame;
        }

        private async Task StartGame(string mode)
        {
            GameMode gameMode = mode switch
            {
                "blitz" => GameMode.Blitz,
                "power" => GameMode.PowerUp,
                _ => GameMode.Classic
            };
            System.Diagnostics.Debug.WriteLine($"Navigating to game with mode: {gameMode}");


            var parameters = new Dictionary<string, object>
            {
                { "mode", gameMode }
            };
            await Shell.Current.GoToAsync("twoPlayerGame", parameters);
        }

        private void StartBackgroundAnimation()
        {
            // Animacja pulsowania tła
            var animation = new Animation(v => mainContainer.Scale = v, 1, 1.02);
            animation.Commit(this, "BackgroundAnimation", 16, 2000, Easing.SinInOut,
                (v, c) => mainContainer.Scale = 1, () => true);
        }
       
    }
  
}