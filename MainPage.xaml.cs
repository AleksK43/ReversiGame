using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Reversi
{
    public partial class MainPage : ContentPage
    {
        private readonly Random random = new();
        private readonly List<MatrixColumn> matrixColumns = new();
        private readonly List<Label> titleLetters = new();
        private bool isAnimationComplete = false;
        private const string TITLE = "REVERSI";
        private const int MATRIX_COLUMNS = 30;

        public MainPage()
        {
            InitializeComponent();
            InitializeMatrixGrid();
            InitializeTitle();
        }

        private void InitializeMatrixGrid()
        {
            // Configure matrix grid columns
            for (int i = 0; i < MATRIX_COLUMNS; i++)
            {
                MainContainerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            // Create matrix columns
            for (int i = 0; i < MATRIX_COLUMNS; i++)
            {
                var column = new MatrixColumn(random.Next(20, 35));
                matrixColumns.Add(column);
                MainContainerGrid.Children.Add(column.StackLayout);
                Grid.SetColumn(column.StackLayout, i);
            }
        }

        private void InitializeTitle()
        {
            var titleContainer = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            foreach (char letter in TITLE)
            {
                var letterLabel = new Label
                {
                    Text = letter.ToString(),
                    TextColor = Colors.LightGreen,
                    FontSize = 72,
                    FontAttributes = FontAttributes.Bold,
                    Opacity = 0,
                    Margin = new Thickness(0, 40, 0, 0)
                };
                titleLetters.Add(letterLabel);
                titleContainer.Children.Add(letterLabel);
            }

            MainContainerGrid.Children.Add(titleContainer);
            Grid.SetRow(titleContainer, 0);
            Grid.SetColumnSpan(titleContainer, MATRIX_COLUMNS);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!isAnimationComplete)
            {
                await StartAnimation();
            }
        }

        private async Task StartAnimation()
        {
            foreach (var column in matrixColumns)
            {
                _ = column.StartAnimation();
                await Task.Delay(50);
            }

            await Task.Delay(2000);

            foreach (var letter in titleLetters)
            {
                await Task.WhenAll(
                    letter.FadeTo(1, 500),
                    letter.ScaleTo(1.2, 250).ContinueWith((t) => letter.ScaleTo(1, 250))
                );
                await Task.Delay(100);
            }

            await Task.Delay(1000);

            MenuStackLayout.IsVisible = true;
            await MenuStackLayout.FadeTo(1, 1000);

            isAnimationComplete = true;
        }

        private async void OnPlayVsComputer(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//computerGame");
        }

        private async void OnPlayVsPlayer(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("gameModeSelection");
        }

        private void OnOptions(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("//OptionsPage");
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Current?.Quit();
        }

        
    }

    public class MatrixColumn
    {
        private readonly Random random = new();
        public StackLayout StackLayout { get; }
        private readonly List<Label> characters = new();
        private CancellationTokenSource? animationCancellation;

        public MatrixColumn(int length)
        {
            StackLayout = new StackLayout
            {
                Spacing = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            for (int i = 0; i < length; i++)
            {
                var label = new Label
                {
                    Text = GetRandomCharacter(),
                    TextColor = Colors.LightGreen,
                    Opacity = 0,
                    FontSize = 20,
                    HorizontalOptions = LayoutOptions.Center
                };
                characters.Add(label);
                StackLayout.Children.Add(label);
            }
        }

        

        private string GetRandomCharacter()
        {
            const string chars = "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン0123456789";
            return chars[random.Next(chars.Length)].ToString();
        }

        public async Task StartAnimation()
        {
            animationCancellation?.Cancel();
            animationCancellation = new CancellationTokenSource();
            var token = animationCancellation.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    for (int i = 0; i < characters.Count; i++)
                    {
                        if (token.IsCancellationRequested) break;

                        characters[i].Text = GetRandomCharacter();

                        _ = characters[i].FadeTo(1, 100)
                            .ContinueWith(async (t) =>
                            {
                                if (!token.IsCancellationRequested)
                                {
                                    await Task.Delay(random.Next(100, 500), token);
                                    if (!token.IsCancellationRequested)
                                    {
                                        await characters[i].FadeTo(0, 100);
                                    }
                                }
                            }, token);

                        await Task.Delay(50, token);
                    }

                    await Task.Delay(random.Next(500, 2000), token);
                }
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        public void StopAnimation()
        {
            animationCancellation?.Cancel();
        }
    }
}