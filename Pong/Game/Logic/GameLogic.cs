using System;
using System.Drawing;

namespace Pong.Core
{
    /// <summary>
    /// Handles the core game logic including physics, collisions, and scoring
    /// </summary>
    public class GameLogic
    {
        private readonly GameState gameState;

        public GameLogic(GameState state)
        {
            gameState = state ?? throw new ArgumentNullException(nameof(state));
        }

        /// <summary>
        /// Updates the game state for one frame
        /// </summary>
        public bool UpdateGame()
        {
            MoveBall();
            return CheckCollisions();
        }

        /// <summary>
        /// Moves a player's paddle in the specified direction
        /// </summary>
        public void MovePaddle(int playerId, int direction)
        {
            if (playerId == 1)
            {
                gameState.Player1PaddleY = CalculateNewPaddlePosition(gameState.Player1PaddleY, direction);
            }
            else if (playerId == 2)
            {
                gameState.Player2PaddleY = CalculateNewPaddlePosition(gameState.Player2PaddleY, direction);
            }
        }

        private int CalculateNewPaddlePosition(int currentY, int direction)
        {
            int newPosition = currentY + direction * GameConfiguration.PaddleSpeed;
            return Math.Clamp(newPosition, 0, GameConfiguration.PaddleMaxY);
        }

        private void MoveBall()
        {
            gameState.BallPositionX += gameState.BallVelocityX;
            gameState.BallPositionY += gameState.BallVelocityY;
        }

        private bool CheckCollisions()
        {
            HandleWallCollisions();
            HandlePaddleCollisions();
            return HandleGoalDetection();
        }

        private void HandleWallCollisions()
        {
            if (gameState.BallPositionY <= 0 || gameState.BallPositionY >= GameConfiguration.GameAreaHeight - GameConfiguration.BallSize)
            {
                gameState.BallVelocityY *= -1;
            }
        }

        private void HandlePaddleCollisions()
        {
            Rectangle ballRect = CreateBallRectangle();
            Rectangle paddle1Rect = CreatePaddle1Rectangle();
            Rectangle paddle2Rect = CreatePaddle2Rectangle();

            if (ballRect.IntersectsWith(paddle1Rect))
            {
                HandleLeftPaddleCollision(paddle1Rect);
            }

            if (ballRect.IntersectsWith(paddle2Rect))
            {
                HandleRightPaddleCollision(paddle2Rect, ballRect);
            }
        }

        private Rectangle CreateBallRectangle()
        {
            return new Rectangle(gameState.BallPositionX, gameState.BallPositionY, 
                GameConfiguration.BallSize, GameConfiguration.BallSize);
        }

        private Rectangle CreatePaddle1Rectangle()
        {
            return new Rectangle(GameConfiguration.Player1PaddleX, gameState.Player1PaddleY, 
                GameConfiguration.PaddleWidth, GameConfiguration.PaddleHeight);
        }

        private Rectangle CreatePaddle2Rectangle()
        {
            return new Rectangle(GameConfiguration.Player2PaddleX + GameConfiguration.FieldOffsetX, gameState.Player2PaddleY, 
                GameConfiguration.PaddleWidth, GameConfiguration.PaddleHeight);
        }

        private void HandleLeftPaddleCollision(Rectangle paddle1Rect)
        {
            gameState.BallPositionX = paddle1Rect.Right;
            gameState.BallVelocityX = Math.Abs(gameState.BallVelocityX);
            IncreaseBallSpeed();
        }

        private void HandleRightPaddleCollision(Rectangle paddle2Rect, Rectangle ballRect)
        {
            gameState.BallPositionX = paddle2Rect.Left - ballRect.Width;
            gameState.BallVelocityX = -Math.Abs(gameState.BallVelocityX);
            IncreaseBallSpeed();
        }

        private void IncreaseBallSpeed()
        {
            if (Math.Abs(gameState.BallVelocityX) < GameConfiguration.MaxBallSpeed)
            {
                gameState.BallVelocityX += gameState.BallVelocityX > 0 ? 
                    GameConfiguration.BallSpeedIncrement : -GameConfiguration.BallSpeedIncrement;
                gameState.BallVelocityY += gameState.BallVelocityY > 0 ? 
                    GameConfiguration.BallSpeedIncrement : -GameConfiguration.BallSpeedIncrement;
            }
        }

        private bool HandleGoalDetection()
        {
            if (gameState.BallPositionX < 0)
            {
                gameState.Player2Score++;
                gameState.ResetBallPosition();
                return true;
            }
            
            if (gameState.BallPositionX > GameConfiguration.FieldWidth)
            {
                gameState.Player1Score++;
                gameState.ResetBallPosition();
                return true;
            }

            return false;
        }
    }
}