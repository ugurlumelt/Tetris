using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ImageSource[] tileImages = new ImageSource[]
        {

            new BitmapImage(new Uri("TetrisAssets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/TileRed.png", UriKind.Relative)),
         
        };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {

            new BitmapImage(new Uri("TetrisAssets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("TetrisAssets/Block-Z.png", UriKind.Relative)),

        };

        private readonly Image[,] imageControls;
        private readonly int maxDelay = 1000;
        private readonly int minDelay = 75;
        private readonly int delayDecrease = 25;
        MediaPlayer backgroundPlayer;


        private GameState gameState = new GameState();  

        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
            backgroundPlayer = new MediaPlayer();
        }

        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    Image imageControl = new Image
                    {
                        Width= cellSize,
                        Height= cellSize,

                    };

                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);
                    Canvas.SetLeft(imageControl, c*cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }

            }
            return imageControls;
        }

        private void DrawGrid(GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    imageControls[r,c].Opacity= 1;  
                    imageControls[r, c].Source = tileImages[id];   

                }
            }

        }

        private void DrawBlock(Block block) 
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1;   
                imageControls[p.Row, p.Column].Source = tileImages[block.Id]; 
            }
        
        }

        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];

        }

        private void DrawHeldBlock(Block heldBlock)
        {
            if (heldBlock ==  null)
            {
                HoldImage.Source = blockImages[0];
            }
            else
            {
                HoldImage.Source= blockImages[heldBlock.Id];  
            }

        }

        private void Draw(GameState gameState) 
        {
            DrawGrid(gameState.GameGrid);
            DrawGhostBlock(gameState.CurrentBlock);
            DrawBlock(gameState.CurrentBlock);
            DrawNextBlock(gameState.BlockQueue);
            DrawHeldBlock(gameState.HeldBlock);
            HighScore.Text = $"High Score : {gameState.HighScore} ";
            ScoreText.Text = $"Score : {gameState.Score} ";
        }

        private void DrawGhostBlock(Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.20;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }

        private async Task GameLoop()
        {
            Draw(gameState);
            
            while (!gameState.GameOver)
            {
                int delay = Math.Max(minDelay, maxDelay - (gameState.Score * delayDecrease));
                await Task.Delay(delay);
                gameState.MoveBlockDown(); 
                Draw(gameState);
             
            }
            GameOverMenu.Visibility= Visibility.Visible;
            FinalScoreText.Text = $"Score : {gameState.Score}";
        }

        private void Window_Keydown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.Left:
                    gameState.MoveBlockLeft();
                    break;
                case Key.Right:
                    gameState.MoveBlockRight();
                    break;
                case Key.Down:
                    gameState.MoveBlockDown();
                    break;
                case Key.Up:
                    gameState.RotateBlockCW();
                    break;
                case Key.Z:
                    gameState.RotateBlockCCW();
                    break;
                case Key.C:
                    gameState.HoldBlock();
                    break;
                case Key.Space:
                    gameState.DropBlock();
                    break;
                default:
                    return;
            }

            Draw(gameState);  
        }       

        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            PlayBackgroundMusic();
            await GameLoop();
        }


        private void PlayBackgroundMusic()
        {
            backgroundPlayer.Open(new Uri(Directory.GetCurrentDirectory() + "\\background.mp3"));
            backgroundPlayer.MediaEnded += new EventHandler(BackgroundMusic_Ended);
            backgroundPlayer.Play();
        }

        private void BackgroundMusic_Ended(object sender, EventArgs e)
        {
            backgroundPlayer.Position = TimeSpan.Zero;
            backgroundPlayer.Play();
        }

        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            gameState = new GameState(gameState.HighScore);
            GameOverMenu.Visibility= Visibility.Hidden;
            await GameLoop();
        }

        private void BobSongButtonStop_Click(object sender, RoutedEventArgs e)
        {
            BobSongStopButton.Visibility = Visibility.Hidden;
            gameState.GameGrid.PlaySound();
            BobSongSoundButton.Visibility = Visibility.Visible;
        }

        private void BobSongSoundButton_Click(object sender, RoutedEventArgs e)
        {
            BobSongSoundButton.Visibility = Visibility.Hidden;
            gameState.GameGrid.StopSound();
            BobSongStopButton.Visibility = Visibility.Visible;
        }

        private void BackgroundSongStopButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundSongStopButton.Visibility = Visibility.Hidden;
            backgroundPlayer.Play();
            BackgroundSongPlayButton.Visibility = Visibility.Visible;
        }

        private void BackgroundSongPlayButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundSongPlayButton.Visibility = Visibility.Hidden;
            backgroundPlayer.Stop();
            BackgroundSongStopButton.Visibility = Visibility.Visible;
        }
    }
}
