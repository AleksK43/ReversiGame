namespace Reversi.Game
{
    public class GameTimer : IDisposable
    {
        private readonly Label timerLabel;
        private readonly int timePerMove;
        private IDispatcherTimer timer;
        private int remainingSeconds;
        private Action onTimeUp;

        public GameTimer(Label timerLabel, int timePerMove, Action onTimeUp)
        {
            this.timerLabel = timerLabel;
            this.timePerMove = timePerMove;
            this.onTimeUp = onTimeUp;

            if (timePerMove > 0)
            {
                timer = Application.Current.Dispatcher.CreateTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
            }
        }

        public void StartTurn()
        {
            if (timePerMove <= 0) return;

            remainingSeconds = timePerMove;
            UpdateTimerDisplay();
            timer.Start();
        }

        public void StopTurn()
        {
            if (timePerMove <= 0) return;
            timer.Stop();
        }

        public void AddTime(int seconds)
        {
            if (timePerMove <= 0) return;
            remainingSeconds += seconds;
            UpdateTimerDisplay();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            UpdateTimerDisplay();

            if (remainingSeconds <= 0)
            {
                timer.Stop();
                onTimeUp?.Invoke();
            }
        }

        private void UpdateTimerDisplay()
        {
            timerLabel.Text = $"Czas: {remainingSeconds}s";
            if (remainingSeconds <= 5)
            {
                timerLabel.TextColor = Colors.Red;
                // Dodaj animację pulsowania dla ostatnich 5 sekund
                var animation = new Animation(v => timerLabel.Scale = v, 1, 1.2);
                animation.Commit(timerLabel, "TimerPulse", 16, 500, Easing.SinInOut,
                    (v, c) => timerLabel.Scale = 1);
            }
            else
            {
                timerLabel.TextColor = Colors.White;
            }
        }

        public void Dispose()
        {
            timer?.Stop();
        }
    }
}