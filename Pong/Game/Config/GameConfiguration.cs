namespace Pong.Core
{
    /// <summary>
    /// Contains all game configuration constants and settings
    /// </summary>
    public static class GameConfiguration
    {
        // Game field dimensions
        public const int FieldWidth = 800;
        public const int FieldHeight = 520;
        public const int GameAreaHeight = 500;
        
        // Paddle configuration
        public const int PaddleWidth = 10;
        public const int PaddleHeight = 100;
        public const int PaddleSpeed = 10;
        public const int PaddleMaxY = 400;
        public const int Player1PaddleX = 20;
        public const int Player2PaddleX = 760;
        
        // Ball configuration
        public const int BallSize = 20;
        public const int BallInitialSpeed = 2;
        public const int MaxBallSpeed = 12;
        public const int BallSpeedIncrement = 1;
        
        // Network configuration
        public const int ServerPort = 12346;
        public const string DefaultServerIP = "127.0.0.1";
        
        // UI configuration
        public const int WindowExtraWidth = 40;
        public const int WindowExtraHeight = 120;
        public const int ScoreLabelHeight = 40;
        public const int CountdownLabelHeight = 50;
        public const int GameTimerInterval = 30;
        public const int GameLoopDelay = 2;
        
        // Game timing
        public const int CountdownDuration = 1000; // milliseconds
        public const int ReconnectionDelay = 500; // milliseconds
        
        // Display offsets
        public const int FieldOffsetX = 10;
        public const int FieldOffsetY = 10;
        public const int PaddleYOffset = 10;
    }
}