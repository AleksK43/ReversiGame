namespace Reversi
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("gameModeSelection", typeof(GameModeSelectionPage));
            Routing.RegisterRoute("twoPlayerGame", typeof(TwoPlayerGamePage));
            Routing.RegisterRoute("computerGame", typeof(ComputerGamePage));

        }
    }
}
