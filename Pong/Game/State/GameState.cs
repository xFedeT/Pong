using System;

namespace Pong.Core
{
    /// <summary>
    /// Represents the current state of the game including ball position, paddle positions, and scores
    /// </summary>
    public class GameState
    {
        private const int DefaultBallStartX = 390;
        private const int DefaultBallStartY = 240;
        private const int DefaultPaddleY = 200;
        private const int InitialBallSpeed = 2;

        public int BallPositionX { get; set; } = DefaultBallStartX;
        public int BallPositionY { get; set; } = DefaultBallStartY;
        public int BallVelocityX { get; set; } = InitialBallSpeed;
        public int BallVelocityY { get; set; } = InitialBallSpeed;

        public int Player1PaddleY { get; set; } = DefaultPaddleY;
        public int Player2PaddleY { get; set; } = DefaultPaddleY;
        
        public int Player1Score { get; set; } = 0;
        public int Player2Score { get; set; } = 0;

        /// <summary>
        /// Resets the game state to initial values
        /// </summary>
        public void Reset()
        {
            BallPositionX = DefaultBallStartX;
            BallPositionY = DefaultBallStartY;
            BallVelocityX = InitialBallSpeed;
            BallVelocityY = InitialBallSpeed;
            Player1PaddleY = DefaultPaddleY;
            Player2PaddleY = DefaultPaddleY;
            Player1Score = 0;
            Player2Score = 0;
        }

        /// <summary>
        /// Resets ball position and adjusts velocity direction
        /// </summary>
        public void ResetBallPosition()
        {
            BallPositionX = DefaultBallStartX;
            BallPositionY = DefaultBallStartY;
            BallVelocityX = BallVelocityX > 0 ? -4 : 4;
            BallVelocityY = 4;
        }

        /// <summary>
        /// Creates a formatted string representation of the current game state
        /// </summary>
        public string SerializeState()
        {
            return $"{BallPositionX},{BallPositionY},{Player1PaddleY},{Player2PaddleY},{Player1Score},{Player2Score}";
        }
    }
}