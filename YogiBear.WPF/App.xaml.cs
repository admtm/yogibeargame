using Microsoft.Win32;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using YogiBear.WPF.ViewModel;
using YogiBear.Model;
using YogiBear.Persistence;

namespace YogiBear.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private GameModel model = null!;
        private MainWindowModel viewModel = null!;
        private MainWindow view = null!;

        public App()
        {
            Startup += OnStartup;
        }

        private void OnStartup(object? sender, StartupEventArgs e)
        {
            IYogiGameDataAccess dataAccess = new YogiGameFileDataAccess();
            IBasicTimer rangerTimer = new BasicTimerAggregation();
            IBasicTimer gameTimer = new BasicTimerAggregation();
            model = new GameModel(dataAccess, rangerTimer, gameTimer);
            model.GameOver += new EventHandler<YogiGameEventArgs>(OnGameOver);
            viewModel = new MainWindowModel(model);

            viewModel.LoadGame += new EventHandler(OnLoadGame);
            viewModel.SaveGame += new EventHandler(OnSaveGame);
            viewModel.ExitGame += new EventHandler(OnExitGame);

            viewModel.EasyMode += async (sender, e) => await LoadLevel("easy.txt");
            viewModel.MediumMode += async (sender, e) => await LoadLevel("medium.txt");
            viewModel.HardMode += async (sender, e) => await LoadLevel("hard.txt");

            view = new MainWindow();
            view.DataContext = viewModel;
            view.Closing += new CancelEventHandler(OnClosing);
            view.Show();
        }
        private void OnClosing(object? sender, CancelEventArgs e)
        {
            model.StopTimers();

            if (MessageBox.Show("Are you sure you want to exit?", "Yogi Bear Game", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
                if (!model.IsGameOver)
                {
                    model.StartTimers();
                }
            }
        }

        private async void OnLoadGame(object? sender, EventArgs e)
        {
            model.StopTimers();
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                openFileDialog.Title = "Select a Game File";

                if (openFileDialog.ShowDialog() == true)
                {
                    await Reset(openFileDialog.FileName);
                }
            }
            catch (YogiBoardDataException)
            {
                MessageBox.Show($"Error loading file", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (!model.IsGameOver)
            {
                model.StartTimers();
            }
        }
        private async void OnSaveGame(object? sender, EventArgs e)
        {
            model.StopTimers();
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save a Game File";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    try
                    {
                        await model.SaveGameAsync(filePath);
                        MessageBox.Show("Game saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving game: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Error saving file", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (!model.IsGameOver)
            {
                model.StartTimers();
            }
        }
        private void OnExitGame(object? sender, EventArgs e)
        {
            view.Close();
        }
        private void OnGameOver(object? sender, YogiGameEventArgs e)
        {
            MessageBox.Show(e.IsWon ? "Congratulations, You won!" + Environment.NewLine + $"Time spent: {TimeSpan.FromSeconds(e.GameTimeElapsed)}" : "You lost!");
        }

        private async Task LoadLevel(string levelFileName)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string projectRoot = Directory.GetParent(currentDir)!.Parent!.Parent!.FullName;
            string filePath = Path.Combine(projectRoot, "levels", levelFileName);

            try
            {
                await Reset(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading level: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task Reset(string fileName)
        {
            model.ResetGame();
            await model.LoadGameAsync(fileName);
            viewModel.MakeFields();
            viewModel.ElapsedTime = "00:00";
            viewModel.CollectedBaskets = $"0/{model.BasketCount}";
            model.StartTimers();
            viewModel.IsGameLoaded = true;
            viewModel.IsGamePaused = false;
        }

        public void Dispose()
        {
            viewModel.Dispose();
            model.Dispose();
        }
    }
}
