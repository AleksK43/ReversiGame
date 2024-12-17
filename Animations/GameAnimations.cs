using Microsoft.Maui.Controls;

namespace Reversi.Animations
{
    public static class GameAnimations
    {
        #region Button Animations
        public static Task BorderColorTo(this Button button, Color fromColor, Color toColor, uint length)
        {
            var tcs = new TaskCompletionSource<bool>();
            new Animation(v =>
            {
                button.BorderColor = new Color(
                    (float)(fromColor.Red + (toColor.Red - fromColor.Red) * v),
                    (float)(fromColor.Green + (toColor.Green - fromColor.Green) * v),
                    (float)(fromColor.Blue + (toColor.Blue - fromColor.Blue) * v),
                    (float)(fromColor.Alpha + (toColor.Alpha - fromColor.Alpha) * v)
                );
            }).Commit(button, "BorderColorAnimation", 16, length, Easing.Linear, (v, c) => tcs.SetResult(true));
            return tcs.Task;
        }

        public static Task BorderWidthTo(this Button button, double fromWidth, double toWidth, uint length)
        {
            var tcs = new TaskCompletionSource<bool>();
            new Animation(v =>
            {
                button.BorderWidth = fromWidth + (toWidth - fromWidth) * v;
            }).Commit(button, "BorderWidthAnimation", 16, length, Easing.Linear, (v, c) => tcs.SetResult(true));
            return tcs.Task;
        }

        public static async Task AnimatePiecePlacement(Button button, Color playerColor)
        {
            await button.ScaleTo(0.1, 150, Easing.CubicOut);
            button.BackgroundColor = playerColor;

            await Task.WhenAll(
                button.ScaleTo(1, 300, Easing.BounceOut),
                button.RotateTo(360, 300, Easing.CubicOut)
            );

            button.Rotation = 0;
        }

        public static async Task AnimateFlip(Button button, Color playerColor)
        {
            await Task.WhenAll(
                button.ScaleTo(0.1, 150, Easing.CubicOut),
                button.RotateYTo(90, 150)
            );

            button.BackgroundColor = playerColor;

            await Task.WhenAll(
                button.ScaleTo(1, 300, Easing.BounceOut),
                button.RotateYTo(360, 300)
            );

            button.RotationY = 0;
        }

        public static async Task AnimatePowerUpCollect(Button button, Color powerUpColor, Color finalColor)
        {
            await Task.WhenAll(
                button.ScaleTo(1.2, 200, Easing.SpringOut),
                button.BorderColorTo(powerUpColor, finalColor, 500)
            );

            for (int i = 0; i < 3; i++)
            {
                await Task.WhenAll(
                    button.BorderWidthTo(1, 3, 100),
                    button.BorderColorTo(finalColor, powerUpColor, 100)
                );
                await Task.WhenAll(
                    button.BorderWidthTo(3, 1, 100),
                    button.BorderColorTo(powerUpColor, finalColor, 100)
                );
            }

            await button.ScaleTo(1, 200, Easing.SpringIn);
        }
        #endregion

        #region Label Animations
        public static async Task AnimateTimerWarning(Label timerLabel)
        {
            timerLabel.TextColor = Colors.Red;
            for (int i = 0; i < 3; i++)
            {
                await timerLabel.ScaleTo(1.2, 200);
                await timerLabel.ScaleTo(1.0, 200);
            }
        }

        public static async Task AnimateStatusUpdate(Label statusLabel, Color color)
        {
            await statusLabel.FadeTo(0.5, 200);
            statusLabel.TextColor = color;
            await statusLabel.FadeTo(1, 200);
        }

        public static async Task AnimateNoMovesAvailable(Label statusLabel, Color originalColor)
        {
            Color warningColor = Colors.Orange;
            statusLabel.TextColor = warningColor;

            await Task.WhenAll(
                statusLabel.ScaleTo(1.1, 200),
                statusLabel.RotateTo(5, 100)
            );
            await statusLabel.RotateTo(-5, 100);
            await statusLabel.RotateTo(0, 100);

            await Task.Delay(500);

            await statusLabel.ScaleTo(1, 200);
            statusLabel.TextColor = originalColor;
        }
        #endregion

        #region Grid Animations
        public static async Task AnimateBoardShake(Grid board)
        {
            for (int i = 0; i < 5; i++)
            {
                await board.TranslateTo(Random.Shared.Next(-10, 11), 0, 50);
            }
            await board.TranslateTo(0, 0, 50);
        }

        public static async Task AnimateBoardPulse(Grid board)
        {
            await board.ScaleTo(1.02, 200);
            await board.ScaleTo(0.98, 200);
            await board.ScaleTo(1.0, 200);
        }

        public static async Task AnimateGameEnd(Grid board)
        {
            await Task.WhenAll(
                board.ScaleTo(0.95, 500, Easing.SpringOut),
                board.FadeTo(0.8, 500)
            );
        }
        #endregion

        #region Special Effects
        public static async Task AnimateDoubleMovePowerUp(Button currentButton)
        {
            for (int i = 0; i < 2; i++)
            {
                await Task.WhenAll(
                    currentButton.ScaleTo(1.1, 150),
                    currentButton.BorderColorTo(Colors.Gold, Colors.White, 150)
                );
                await Task.WhenAll(
                    currentButton.ScaleTo(1.0, 150),
                    currentButton.BorderColorTo(Colors.White, Colors.Gold, 150)
                );
            }
        }

        public static async Task AnimateSwapColors(IEnumerable<Button> buttons, Color color1, Color color2)
        {
            foreach (var button in buttons.Where(b => b.BackgroundColor != Colors.Black))
            {
                _ = Task.Run(async () =>
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Task.WhenAll(
                            button.RotateTo(180, 300),
                            button.ScaleTo(0.1, 300)
                        );

                        button.BackgroundColor = button.BackgroundColor == color1 ? color2 : color1;

                        await Task.WhenAll(
                            button.RotateTo(360, 300),
                            button.ScaleTo(1, 300)
                        );
                    });
                });
                await Task.Delay(50); // Stagger effect
            }
        }
        public static async Task DisplayAlert(string title, string message, string cancel)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public static async Task AnimateExtraTime(Label timerLabel)
        {
            var originalColor = timerLabel.TextColor;
            timerLabel.TextColor = Colors.Green;

            for (int i = 0; i < 3; i++)
            {
                await Task.WhenAll(
                    timerLabel.ScaleTo(1.2, 100),
                    timerLabel.RotateTo(5, 100)
                );
                await Task.WhenAll(
                    timerLabel.ScaleTo(1.0, 100),
                    timerLabel.RotateTo(-5, 100)
                );
            }

            await timerLabel.RotateTo(0, 100);
            timerLabel.TextColor = originalColor;
        }
        #endregion
    }
}