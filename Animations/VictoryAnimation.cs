using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;

namespace Reversi.Animations
{
    public class VictoryAnimation
    {
        private readonly Color player1Color;
        private readonly Color player2Color;
        private Microsoft.Maui.Controls.Grid victoryContainer;
        private Label matrixEffect;
        private Label fatalityText;
        private Label winnerText;
        private CancellationTokenSource matrixCancellation;

        public VictoryAnimation(Color player1Color, Color player2Color)
        {
            this.player1Color = player1Color;
            this.player2Color = player2Color;
            matrixCancellation = new CancellationTokenSource();
            victoryContainer = new Microsoft.Maui.Controls.Grid();
            matrixEffect = new Label();
            fatalityText = new Label();
            winnerText = new Label();
        }

        public async Task ShowVictoryAnimation(Microsoft.Maui.Controls.Grid parentGrid, Microsoft.Maui.Controls.Grid gameBoard, bool isPlayer1Winner, int player1Score, int player2Score)
        {
            matrixCancellation = new CancellationTokenSource();

            // Przyciemnienie planszy
            await gameBoard.FadeTo(0.3, 500);

            CreateVictoryElements(isPlayer1Winner);

            // Dodaj kontener do głównego grida
            parentGrid.Children.Add(victoryContainer);
            Microsoft.Maui.Controls.Grid.SetRowSpan(victoryContainer, 3);

            // Uruchom animację matrix w tle
            StartMatrixAnimation();

            // Sekwencja animacji
            await PlayVictorySequence(gameBoard);

            // Wyświetl wynik
            string playerName = isPlayer1Winner ? "Zielony" : "Magenta";
            await ShowFinalScore(playerName, player1Score, player2Score);

            // Zakończ animację matrix
            matrixCancellation.Cancel();

            // Animacja wyjścia
            await Task.WhenAll(
                victoryContainer.FadeTo(0, 500),
                gameBoard.FadeTo(0, 500)
            );

            // Usuń kontener animacji
            parentGrid.Children.Remove(victoryContainer);
        }

        private void CreateVictoryElements(bool isPlayer1Winner)
        {
            Color winnerColor = isPlayer1Winner ? player1Color : player2Color;

            victoryContainer = new Microsoft.Maui.Controls.Grid
            {
                BackgroundColor = Colors.Transparent,
                ZIndex = 999,
                InputTransparent = true
            };

            matrixEffect = new Label
            {
                Text = GenerateMatrixText(),
                TextColor = winnerColor,
                Opacity = 0.3f,
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            fatalityText = new Label
            {
                Text = "FATALITY",
                TextColor = winnerColor,
                FontSize = 72,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Scale = 0,
                Rotation = -180
            };

            winnerText = new Label
            {
                Text = isPlayer1Winner ? "ZIELONY WYGRYWA!" : "MAGENTA WYGRYWA!",
                TextColor = winnerColor,
                FontSize = 48,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TranslationY = 100,
                Opacity = 0
            };

            victoryContainer.Children.Add(matrixEffect);
            victoryContainer.Children.Add(fatalityText);
            victoryContainer.Children.Add(winnerText);
        }

        private void StartMatrixAnimation()
        {
            Task.Run(async () =>
            {
                while (!matrixCancellation.Token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        matrixEffect.Text = GenerateMatrixText();
                    });
                    await Task.Delay(100);
                }
            }, matrixCancellation.Token);
        }

        private async Task PlayVictorySequence(Microsoft.Maui.Controls.Grid gameBoard)
        {
            // Fatality animation
            await Task.WhenAll(
                fatalityText.ScaleTo(2, 400, Easing.CubicOut),
                fatalityText.RotateTo(0, 400, Easing.CubicOut)
            );

            // Screen shake
            for (int i = 0; i < 5; i++)
            {
                gameBoard.TranslationX = Random.Shared.Next(-10, 11);
                await Task.Delay(50);
            }
            gameBoard.TranslationX = 0;

            // Pulsowanie napisu
            _ = Task.WhenAll(
                fatalityText.ScaleTo(1.8, 500, Easing.SinInOut),
                fatalityText.FadeTo(0.8, 500, Easing.SinInOut)
            );

            // Pokaż tekst zwycięzcy
            winnerText.TranslationY = 100;
            await Task.WhenAll(
                winnerText.FadeTo(1, 500),
                AnimateTranslationY(winnerText, 100, 0, 500)
            );

            // Efekt błyskawic
            for (int i = 0; i < 3; i++)
            {
                await FlashBackground();
                await Task.Delay(100);
            }
        }

        private async Task AnimateTranslationY(View view, double from, double to, uint duration)
        {
            view.TranslationY = from;
            await view.TranslateTo(view.TranslationX, to, duration, Easing.SpringOut);
        }

        private string GenerateMatrixText()
        {
            return string.Join("\n", Enumerable.Range(0, 20).Select(_ =>
                string.Join("", Enumerable.Range(0, 40).Select(_ =>
                    "アイウエオカキクケコサシスセソタチツテト"[Random.Shared.Next(20)]))));
        }

        private async Task ShowFinalScore(string winner, int score1, int score2)
        {
            await Task.Delay(2000);
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Koniec gry",
                    $"Wynik końcowy:\nZielony: {score1}\nMagenta: {score2}",
                    "OK"
                );
            }
        }

        private async Task FlashBackground()
        {
            victoryContainer.BackgroundColor = Color.FromRgba(0.0f, 1.0f, 0.0f, 0.3f);
            await Task.Delay(100);
            victoryContainer.BackgroundColor = Colors.Transparent;
            await Task.Delay(100);
        }
    }
}