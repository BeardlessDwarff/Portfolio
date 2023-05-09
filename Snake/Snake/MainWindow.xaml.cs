using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValueToImage = new Dictionary<GridValue, ImageSource>()
        {
            {GridValue.Empty, Images.SnakeEmpty },
            {GridValue.Snake, Images.SnakeBody },
            {GridValue.Food, Images.SnakeFood }
        };

        private readonly Dictionary<Direction, int> directionToRotation = new Dictionary<Direction, int>()
        {
            {Direction.up, 0 },
            {Direction.right, 90 },
            {Direction.Down, 180 },
            {Direction.left, 270 }
        };

        private readonly int rows = 15;
        private readonly int columns = 15;
        private bool gameRunning = false;

        private readonly Image[,] gridImages;
        private GameState gameState;


        public MainWindow()
        {
            InitializeComponent();

            gridImages = SetupGrid();
            newGame();
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, columns];

            GameGrid.Rows = rows;
            GameGrid.Columns = columns;

            double columnToRowRatio = (columns / (double)rows);

            GameGrid.Height = GameGrid.Height * columnToRowRatio;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.SnakeEmpty, //Set source
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image; //Add to array
                    GameGrid.Children.Add(image); //Add as child
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE = {gameState.Score}";

        }

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValueToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Column];
            image.Source = Images.SnakeHead;

            int rotation = directionToRotation[gameState.Dir];

            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            bool first = true;

            foreach (Position pos in gameState.SnakePositions())
            {
                if (first)
                {
                    first = false;
                    gridImages[pos.Row, pos.Column].Source = Images.SnakeDeadHead;
                } else
                {
                    gridImages[pos.Row, pos.Column].Source = Images.SnakeDeadBody;
                }

                await Task.Delay(50);
            }

        }



        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            newGame();
        }

        private void newGame()
        {
            gameState = new GameState(rows, columns);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                
                await RunGame();

                gameRunning = false;
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver) return;

            switch (e.Key)
            {
                case Key.W: case Key.Up:
                    gameState.ChangeDirection(Direction.up);
                    break;
                case Key.S: case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
                case Key.A: case Key.Left:
                    gameState.ChangeDirection(Direction.left);
                    break;
                case Key.Right: case Key.D:
                    gameState.ChangeDirection(Direction.right);
                    break;
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(100);

                gameState.Move();
                Draw();
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1;  i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(1000);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(100);

            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "PRESS ANY KEY TO RESTART";
        }


    }



    



    




}
