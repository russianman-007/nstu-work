// Главная ViewModel, управляющая состоянием игры и командами интерфейса
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DungeonCrawler.WPF.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly GameService _gameService;
        private ObservableCollection<CellViewModel> _cells = new ObservableCollection<CellViewModel>();
        private string _statusText = "";
        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();

        public ObservableCollection<CellViewModel> Cells
        {
            get => _cells;
            set
            {
                _cells = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> LogMessages
        {
            get => _logMessages;
            set
            {
                _logMessages = value;
                OnPropertyChanged();
            }
        }

        public int DungeonWidth { get; private set; }
        public int DungeonHeight { get; private set; }

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand MoveLeftCommand { get; }
        public ICommand MoveRightCommand { get; }
        public ICommand AttackCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand LoadGameCommand { get; }

        // Инициализация сервиса игры и привязка команд
        public GameViewModel()
        {
            _gameService = new GameService();
            DungeonWidth = _gameService.CurrentGame.Dungeon.Width;
            DungeonHeight = _gameService.CurrentGame.Dungeon.Height;

            InitializeGrid();
            UpdateDisplay();

            MoveUpCommand = new RelayCommand(_ => Move(0, -1));
            MoveDownCommand = new RelayCommand(_ => Move(0, 1));
            MoveLeftCommand = new RelayCommand(_ => Move(-1, 0));
            MoveRightCommand = new RelayCommand(_ => Move(1, 0));
            AttackCommand = new RelayCommand(_ => Attack());
            NewGameCommand = new RelayCommand(_ => NewGame());
            SaveGameCommand = new RelayCommand(_ => SaveGame());
            LoadGameCommand = new RelayCommand(_ => LoadGame());
        }

        // Создание ViewModel для всех клеток игрового поля
        private void InitializeGrid()
        {
            Cells.Clear();
            for (int y = 0; y < DungeonHeight; y++)
            {
                for (int x = 0; x < DungeonWidth; x++)
                {
                    var cell = _gameService.CurrentGame.Dungeon.GetCell(new Position(x, y));
                    Cells.Add(new CellViewModel(x, y, cell!));
                }
            }
        }

        // Обновление отображения поля, статуса и журнала событий
        private void UpdateDisplay()
        {
            var game = _gameService.CurrentGame;

            foreach (var cellVm in Cells)
            {
                cellVm.UpdateVisuals();
            }

            var hero = game.Hero;

            // Информация о врагах в радиусе атаки
            if (hero != null && !game.IsGameOver && !game.IsGameWon)
            {
                var attackPositions = new Position[]
                {
                    new Position(hero.Position.X + 1, hero.Position.Y),
                    new Position(hero.Position.X - 1, hero.Position.Y),
                    new Position(hero.Position.X, hero.Position.Y + 1),
                    new Position(hero.Position.X, hero.Position.Y - 1)
                };

                bool enemiesNearby = false;
                foreach (var pos in attackPositions)
                {
                    var cell = game.Dungeon.GetCell(pos);
                    if (cell?.Entity is Enemy enemy && enemy.IsAlive)
                    {
                        game.AddLog($"{enemy.Name} на клетке ({pos.X},{pos.Y}) - Здоровье: {enemy.Health}");
                        enemiesNearby = true;
                    }
                }
                if (!enemiesNearby)
                {
                    game.AddLog("Врагов в радиусе атаки нет");
                }
            }

            if (game.IsGameWon)
            {
                StatusText = $"ПОБЕДА! Очки: {hero?.Score ?? 0}";
            }
            else if (game.IsGameOver)
            {
                StatusText = "ИГРА ОКОНЧЕНА";
            }
            else
            {
                StatusText = $"Здоровье: {hero?.Health ?? 0} | Очки: {hero?.Score ?? 0} | Ход: {game.Turn}";
            }

            LogMessages.Clear();
            foreach (var msg in game.Log)
            {
                LogMessages.Add(msg);
            }
        }

        // Обработка перемещения героя в заданном направлении
        private void Move(int dx, int dy)
        {
            if (_gameService.CurrentGame.IsGameOver || _gameService.CurrentGame.IsGameWon)
                return;

            var direction = (dx, dy) switch
            {
                (0, -1) => ConsoleKey.UpArrow,
                (0, 1) => ConsoleKey.DownArrow,
                (-1, 0) => ConsoleKey.LeftArrow,
                (1, 0) => ConsoleKey.RightArrow,
                _ => ConsoleKey.UpArrow
            };

            _gameService.ProcessInput(direction);
            UpdateDisplay();
        }

        // Обработка атаки героя
        private void Attack()
        {
            if (_gameService.CurrentGame.IsGameOver || _gameService.CurrentGame.IsGameWon)
                return;

            _gameService.ProcessInput(ConsoleKey.Spacebar);
            UpdateDisplay();
        }

        // Сброс и создание новой игры
        private void NewGame()
        {
            _gameService.NewGame();
            DungeonWidth = _gameService.CurrentGame.Dungeon.Width;
            DungeonHeight = _gameService.CurrentGame.Dungeon.Height;
            InitializeGrid();
            UpdateDisplay();
        }

        // Сохранение текущего состояния игры
        private void SaveGame()
        {
            _gameService.SaveGame();
            UpdateDisplay();
        }

        // Загрузка ранее сохраненного состояния игры
        private void LoadGame()
        {
            _gameService.LoadGame();
            DungeonWidth = _gameService.CurrentGame.Dungeon.Width;
            DungeonHeight = _gameService.CurrentGame.Dungeon.Height;
            InitializeGrid();
            UpdateDisplay();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Вызов события изменения свойства
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}