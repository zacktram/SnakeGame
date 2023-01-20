using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SnakeParts;


namespace PlayingAround
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();

        const int snakeSquareSize = 20;
        const int snakeStartLength = 3;
        const int snakeStartSpeed = 400;
        const int snakeSpeedThreshold = 100;

        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private List<SnakePart> snakeParts = new List<SnakePart>();

        public enum SnakeDirection { Left, Right, Down, Up };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength;
        private Random rnd = new Random();

        private UIElement snakeFood = null;
        private SolidColorBrush foodBrush = Brushes.Red;

        private int currentScore = 0;

        public MainWindow()
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            moveSnake();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            drawGameArea();

        }



        private void drawGameArea()
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = snakeSquareSize,
                    Height = snakeSquareSize,
                    Fill = nextIsOdd ? Brushes.White : Brushes.Black

                };

                gameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextIsOdd = !nextIsOdd;
                nextX += snakeSquareSize;

                if (nextX >= gameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += snakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY >= gameArea.ActualHeight)
                {
                    doneDrawingBackground = true;
                }

            }
        }

        private void drawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.uIElement == null)
                {
                    snakePart.uIElement = new Rectangle()
                    {
                        Width = snakeSquareSize,
                        Height = snakeSquareSize,
                        Fill = (snakePart.isHead ? snakeHeadBrush : snakeBodyBrush)
                    };

                    gameArea.Children.Add(snakePart.uIElement);
                    Canvas.SetTop(snakePart.uIElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.uIElement, snakePart.Position.X);
                }
            }
        }

        private void moveSnake()
        {
            // Deletes the last part of the snake, while there are more items in snakeParts than there is in snakeLength
            while (snakeParts.Count >= snakeLength)
            {
                gameArea.Children.Remove(snakeParts[0].uIElement);
                snakeParts.RemoveAt(0);
            }

            // Creates body parts For each snakePart in snakeParts treat each as a rectangle and fill with the snakeBodyBrush, mark that there is no head 
            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.uIElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.isHead = false;
            }

            // Determines the direction to expand the snake based on the current direction of the snake
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;

            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= snakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += snakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= snakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += snakeSquareSize;
                    break;
            }

            // Adds snakeHead to snakeParts list
            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                isHead = true
            });

            drawSnake();

            DoCollisionCheck();
        }

        private void StartNewGame()
        {
            // Removes existing snakeBodyParts and snakeFood from the previous game
            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.uIElement != null)
                    gameArea.Children.Remove(snakeBodyPart.uIElement);
            }
            snakeParts.Clear();
            if (snakeFood != null)
                gameArea.Children.Remove(snakeFood);

            // Resets score, snakeLegth to snakeStartLength, snakeDirection, gameTickTimer
            currentScore = 0;
            snakeLength = snakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(snakeSquareSize * 5, snakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(snakeStartSpeed);

            // Redraws snake and snakeFood
            drawSnake();
            DrawSnakeFood();

            UpdateGameStatus();

            gameTickTimer.IsEnabled = true;
        }

        private Point GetNextFoodPosition()
        {
            int maxX = (int)(gameArea.ActualWidth / snakeSquareSize);
            int maxY = (int)(gameArea.ActualHeight / snakeSquareSize);
            int foodX = rnd.Next(0, maxX) * snakeSquareSize;
            int foodY = rnd.Next(0, maxY) * snakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                {
                    return GetNextFoodPosition();
                }
            }

            return new Point(foodX, foodY);
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();
            snakeFood = new Ellipse()
            {
                Width = snakeSquareSize,
                Height = snakeSquareSize,
                Fill = foodBrush
            };

            gameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;

                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;

                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;

                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;

                case Key.Space:
                    StartNewGame();
                    break;
            }

            if (snakeDirection != originalSnakeDirection)
                moveSnake();
        }

        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
                return;
            }

            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= gameArea.ActualHeight) || (snakeHead.Position.X < 0) || (snakeHead.Position.X >= gameArea.ActualWidth))
            {
                EndGame();
            }

            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                {
                    EndGame();
                }
            }
        }

        // Method for when snake eats food
        private void EatSnakeFood()
        {
            // Adds one to snakeLength and currentScore when snake collides with snakeFood
            snakeLength++;
            currentScore++;

            // Updates timeInterval by choosing the max value between snakeSpeedThreshold (100ms) and gameTickTimer 
            // gameTickTimer updated by multiplying the current score by 2 then subtracting from the gameTickTimer (Initally set at 400ms) -> allows for the game to speed up and get more difficult on each piece of food eaten
            int timerInterval = Math.Max(snakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);

            // Removes existing snakeFood once snake collides with it
            gameArea.Children.Remove(snakeFood);

            //Creates new snakeFood
            DrawSnakeFood();
            UpdateGameStatus();

        }

        // Updates the Title property of the Window to show real time currentScore and game speed
        private void UpdateGameStatus()
        {
            this.Title = "Snake Game - Score: " + currentScore + " - Game Speed: " + gameTickTimer.Interval.TotalMilliseconds;
        }

        private void EndGame()
        {
            gameTickTimer.IsEnabled = false;
            MessageBox.Show("Game Over!" + "\nTo start a new game press the SpaceBar");
        }
    }
}
