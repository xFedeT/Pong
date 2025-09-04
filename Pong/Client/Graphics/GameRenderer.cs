using System.Drawing;
using System.Windows.Forms;
using Pong.Core;

namespace Pong.Client.Graphics
{
    /// <summary>
    /// Handles all game rendering operations
    /// </summary>
    public class GameRenderer
    {
        private readonly Brush ballBrush;
        private readonly Brush player1Brush;
        private readonly Brush player2Brush;
        private readonly Pen fieldBorderPen;

        public GameRenderer()
        {
            ballBrush = Brushes.White;
            player1Brush = Brushes.Blue;
            player2Brush = Brushes.Red;
            fieldBorderPen = new Pen(Color.White, 3);
        }

        /// <summary>
        /// Renders the complete game scene
        /// </summary>
        public void RenderGame(PaintEventArgs e, GameState gameState)
        {
            var graphics = e.Graphics;
            
            // Enable anti-aliasing for smoother rendering
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            DrawGameField(graphics);
            DrawGameElements(graphics, gameState);
        }

        private void DrawGameField(System.Drawing.Graphics graphics)
        {
            // Draw the main playing field border
            graphics.DrawRectangle(fieldBorderPen, 
                GameConfiguration.FieldOffsetX, 
                GameConfiguration.FieldOffsetY,
                GameConfiguration.FieldWidth - 20, 
                GameConfiguration.GameAreaHeight);

            // Draw center line (optional visual enhancement)
            DrawCenterLine(graphics);
        }

        private void DrawCenterLine(System.Drawing.Graphics graphics)
        {
            using (var dashedPen = new Pen(Color.White, 2))
            {
                dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                
                int centerX = GameConfiguration.FieldWidth / 2;
                graphics.DrawLine(dashedPen, 
                    centerX, GameConfiguration.FieldOffsetY,
                    centerX, GameConfiguration.FieldOffsetY + GameConfiguration.GameAreaHeight);
            }
        }

        private void DrawGameElements(System.Drawing.Graphics graphics, GameState gameState)
        {
            DrawBall(graphics, gameState);
            DrawPaddles(graphics, gameState);
        }

        private void DrawBall(System.Drawing.Graphics graphics, GameState gameState)
        {
            graphics.FillEllipse(ballBrush, 
                gameState.BallPositionX, 
                gameState.BallPositionY, 
                GameConfiguration.BallSize, 
                GameConfiguration.BallSize);
        }

        private void DrawPaddles(System.Drawing.Graphics graphics, GameState gameState)
        {
            // Player 1 paddle (left side)
            graphics.FillRectangle(player1Brush,
                GameConfiguration.Player1PaddleX,
                gameState.Player1PaddleY + GameConfiguration.PaddleYOffset,
                GameConfiguration.PaddleWidth,
                GameConfiguration.PaddleHeight);

            // Player 2 paddle (right side)
            graphics.FillRectangle(player2Brush,
                GameConfiguration.Player2PaddleX,
                gameState.Player2PaddleY + GameConfiguration.PaddleYOffset,
                GameConfiguration.PaddleWidth,
                GameConfiguration.PaddleHeight);
        }

        /// <summary>
        /// Creates and configures the main game panel
        /// </summary>
        public Panel CreateGamePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };
        }

        /// <summary>
        /// Creates the score display label
        /// </summary>
        public Label CreateScoreLabel()
        {
            return new Label
            {
                Text = "0 - 0",
                Dock = DockStyle.Top,
                Font = new Font("Arial", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = GameConfiguration.ScoreLabelHeight,
                ForeColor = Color.White,
                BackColor = Color.Black
            };
        }

        /// <summary>
        /// Creates the countdown display label
        /// </summary>
        public Label CreateCountdownLabel()
        {
            return new Label
            {
                Text = "",
                Dock = DockStyle.Top,
                Font = new Font("Arial", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = GameConfiguration.CountdownLabelHeight,
                ForeColor = Color.Yellow,
                BackColor = Color.Black
            };
        }

        /// <summary>
        /// Updates the score display
        /// </summary>
        public void UpdateScoreDisplay(Label scoreLabel, int player1Score, int player2Score)
        {
            if (scoreLabel?.InvokeRequired == true)
            {
                scoreLabel.Invoke(new System.Action(() => 
                    scoreLabel.Text = $"{player1Score} - {player2Score}"));
            }
            else if (scoreLabel != null)
            {
                scoreLabel.Text = $"{player1Score} - {player2Score}";
            }
        }

        /// <summary>
        /// Updates the countdown display
        /// </summary>
        public void UpdateCountdownDisplay(Label countdownLabel, string text)
        {
            if (countdownLabel?.InvokeRequired == true)
            {
                countdownLabel.Invoke(new System.Action(() => countdownLabel.Text = text));
            }
            else if (countdownLabel != null)
            {
                countdownLabel.Text = text;
            }
        }
    }
}