namespace Reversi.Game
{
    public enum GameMode
    {
        Classic,
        Blitz,
        PowerUp,
        Computer
    }

    public class GameSettings
    {
        public int TimePerMove { get; init; }
        public bool HasPowerUps { get; init; }
        public bool IsComputerOpponent { get; init; }
        public int ComputerDifficulty { get; init; }
    }

    public static class GameModeSettings
    {
        public static Dictionary<GameMode, GameSettings> Settings = new()
            {
                { GameMode.Classic, new GameSettings
                    {
                        TimePerMove = 0,
                        HasPowerUps = false,
                        IsComputerOpponent = false,
                        ComputerDifficulty = 0
                    }
                },
                { GameMode.Blitz, new GameSettings
                    {
                        TimePerMove = 30,
                        HasPowerUps = false,
                        IsComputerOpponent = false,
                        ComputerDifficulty = 0
                    }
                },
                { GameMode.PowerUp, new GameSettings
                    {
                        TimePerMove = 45,
                        HasPowerUps = true,
                        IsComputerOpponent = false,
                        ComputerDifficulty = 0
                    }
                },
                { GameMode.Computer, new GameSettings
                    {
                        TimePerMove = 0,
                        HasPowerUps = false,
                        IsComputerOpponent = true,
                        ComputerDifficulty = 1
                    }
                }
            };
    }
}