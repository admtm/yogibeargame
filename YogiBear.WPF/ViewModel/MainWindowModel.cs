using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using YogiBear.Model;
using YogiBear.Persistence;
using static YogiBear.Persistence.Character;

namespace YogiBear.WPF.ViewModel
{
    public class MainWindowModel : ViewModelBase, IDisposable
    {
        private GameModel model;
        public ObservableCollection<YogiBearGameField> GameGrid { get; private set; }
        private string elapsedTime = "00:00";
        private string collectedBaskets = $"0/0";
        private bool isMainMenuVisible = true;
        private bool isDifficultySelectionVisible = false;
        private bool isGamePanelVisible = false;
        private bool isGameLoaded = false;
        private bool isGamePaused = false;
        private bool isPausable = false;

        private int boardSize;
        public int BoardSize { get { return boardSize; } }
        public bool IsMainMenuVisible
        {
            get => isMainMenuVisible;
            set
            {
                isMainMenuVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsDifficultySelectionVisible
        {
            get => isDifficultySelectionVisible;
            set
            {
                isDifficultySelectionVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsGamePanelVisible
        {
            get => isGamePanelVisible;
            set
            {
                isGamePanelVisible = value;
                OnPropertyChanged();
            }
        }
        public bool IsGameLoaded 
        {
            get => isGameLoaded;
            set
            {
                isGameLoaded = value;
                OnPropertyChanged();
                MovePlayerCommand.RaiseCanExecuteChanged();
                if(isGameLoaded) IsPausable = true;
            }
        }

        public bool IsGamePaused
        {
            get => isGamePaused;
            set
            {
                isGamePaused = value;
                OnPropertyChanged();
                PauseCommand.RaiseCanExecuteChanged();
            }
        }
        public bool IsPausable
        {
            get => isPausable;
            set
            {
                isPausable = value;
                OnPropertyChanged();
            }
        }

        public string ElapsedTime
        {
            get
            {
                elapsedTime = TimeSpan.FromSeconds(model.GameTimeElapsed).ToString(@"mm\:ss");
                return elapsedTime;
            }
            set
            {
                elapsedTime = value;
                OnPropertyChanged();
            }
        }

        public string CollectedBaskets
        {
            get
            {
                collectedBaskets = $"{model.CollectedBasketCount}/{model.BasketCount}";
                return collectedBaskets;
            }
            set
            {
                collectedBaskets = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand ExitGameCommand { get; private set; }

        public DelegateCommand EasyModeCommand { get; private set; }
        public DelegateCommand MediumModeCommand { get; private set; }
        public DelegateCommand HardModeCommand { get; private set; }

        public DelegateCommand MovePlayerCommand { get; private set; }
        public DelegateCommand PauseCommand { get; private set; }

        public event EventHandler? LoadGame;
        public event EventHandler? SaveGame;
        public event EventHandler? ExitGame;

        public event EventHandler? EasyMode;
        public event EventHandler? MediumMode;
        public event EventHandler? HardMode;

        public MainWindowModel(GameModel model)
        {
            this.model = model;
            GameGrid = new ObservableCollection<YogiBearGameField>();
            boardSize = this.model.BoardSize;

            NewGameCommand = new DelegateCommand(x => OnNewGame());
            LoadGameCommand = new DelegateCommand(x => OnLoadGame());
            SaveGameCommand = new DelegateCommand(x => OnSaveGame());
            ExitGameCommand = new DelegateCommand(x => OnExitGame());

            EasyModeCommand = new DelegateCommand(x => OnEasyMode());
            MediumModeCommand = new DelegateCommand(x => OnMediumMode());
            HardModeCommand = new DelegateCommand(x => OnHardMode());

            MovePlayerCommand = new DelegateCommand(x => HandleDirection(x!), x => IsGameLoaded);
            PauseCommand = new DelegateCommand(x => HandlePause(), x => IsPausable);

            model.GameFieldChanged += new EventHandler<YogiGameFieldEventArgs>(OnGameFieldChanged);
            model.GameAdvanced += new EventHandler<YogiGameEventArgs>(OnGameAdvanced);
            model.GameOver += new EventHandler<YogiGameEventArgs>(OnGameOver);
            model.BasketCollected += new EventHandler(OnBasketCollected);
        }

        public void MakeFields()
        {
            GameGrid.Clear();
            for (int i = 0; i < model.BoardSize; i++)
            {
                for (int j = 0; j < model.BoardSize; j++)
                {
                    GameGrid.Add(new YogiBearGameField
                    {
                        X = i,
                        Y = j,
                        Content = model.GetCurrentPiece(i, j),
                        BackgroundColor = GetColor(model.GetCurrentPiece(i, j))
                    });
                }
            }
        }
        private void ShowMainMenu()
        {
            IsMainMenuVisible = true;
            IsDifficultySelectionVisible = false;
            IsGamePanelVisible = false;
        }

        private void ShowDifficultySelection()
        {
            IsMainMenuVisible = false;
            IsDifficultySelectionVisible = true;
            IsGamePanelVisible = false;
        }

        private void ShowGamePanel()
        {
            IsMainMenuVisible = false;
            IsDifficultySelectionVisible = false;
            IsGamePanelVisible = true;
        }
        private void HandleDirection(object dir)
        {
            if(dir is string stringDir)
            {
                if (!Enum.TryParse(stringDir, out Direction direction))
                    return;
                model.Step(direction);
            }
        }

        private void HandlePause()
        {
            if (isGamePaused)
            {
                model.StartTimers();
                IsGameLoaded = true;
                ShowGamePanel();
            }
            else
            {
                model.StopTimers();
                IsGameLoaded = false;
                ShowMainMenu();
            }
            IsGamePaused = !isGamePaused;
        }
        private Brush GetColor(Pieces piece)
        {
            switch (piece)
            {
                case Player:
                    return Brushes.SaddleBrown;
                case Ranger:
                    return Brushes.Goldenrod;
                case Item it:
                    BrushConverter b = new BrushConverter();
                    Brush c = (Brush)b.ConvertFrom(it.WhichColor())!;
                    return c;
                case null:
                    return Brushes.LawnGreen;
                default:
                    throw new ArgumentException(nameof(piece), $"Invalid item type: {piece}");
            }
        }


        private void OnGameFieldChanged(object? sender, YogiGameFieldEventArgs e)
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(() => { OnGameFieldChanged(sender, e); });
                return;
            }
            YogiBearGameField? field = GameGrid.FirstOrDefault(f => f.X == e.X && f.Y == e.Y);
            if (field != null)
            {
                Debug.WriteLine($"[OnGameFieldChanged] Updating field ({e.X}, {e.Y}) with new content: {field.Content}");
                field.Content = model.GetCurrentPiece(e.X, e.Y);
                field.BackgroundColor = GetColor(field.Content);
            }
        }


        private void OnBasketCollected(object? sender, EventArgs e)
        {
            if(!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(() => { OnBasketCollected(sender, e); });
                return;
            }
            OnPropertyChanged(nameof(CollectedBaskets));
        }

        private void OnGameAdvanced(object? sender, YogiGameEventArgs e)
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(() => { OnGameAdvanced(sender, e); });
                return;
            }
            OnPropertyChanged(nameof(ElapsedTime));
        }
        private void OnGameOver(object? sender , YogiGameEventArgs e)
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(() => { OnGameOver(sender, e); });
                return;
            }
            OnPropertyChanged(nameof(ElapsedTime));
        }
        private void OnNewGame()
        {
            ShowDifficultySelection();
            IsGameLoaded = false;
        }
        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
            ShowGamePanel();
        }
        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }
        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }


        private void OnEasyMode()
        {
            EasyMode?.Invoke(this, EventArgs.Empty);
            ShowGamePanel();
        }

        private void OnMediumMode()
        {
            MediumMode?.Invoke(this, EventArgs.Empty);
            ShowGamePanel();
        }

        private void OnHardMode()
        {
            HardMode?.Invoke(this, EventArgs.Empty);
            ShowGamePanel();
        }

        public void Dispose()
        {
            model.Dispose();
        }
    }
}
