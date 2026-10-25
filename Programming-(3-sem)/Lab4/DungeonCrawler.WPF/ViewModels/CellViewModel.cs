using DungeonCrawler.Core.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DungeonCrawler.WPF.ViewModels
{
    public class CellViewModel : INotifyPropertyChanged
    {
        private readonly Cell _cell;
        private Brush _backgroundColor = Brushes.White;
        private string _content = "";
        private Brush _foregroundColor = Brushes.Black;

        private static readonly Brush DarkFloor = (Brush)new BrushConverter().ConvertFromString("#2D2D2D");
        private static readonly Brush DarkWall = (Brush)new BrushConverter().ConvertFromString("#3E3E3E");
        private static readonly Brush DarkExit = (Brush)new BrushConverter().ConvertFromString("#1A4D4D");

        public Cell Cell => _cell;
        public int X { get; }
        public int Y { get; }

        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public Brush ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                _foregroundColor = value;
                OnPropertyChanged();
            }
        }

        public CellViewModel(int x, int y, Cell cell)
        {
            X = x;
            Y = y;
            _cell = cell;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (_cell.Entity != null)
            {
                Content = _cell.Entity.Symbol;
                ForegroundColor = GetBrushFromColor(_cell.Entity.Color);
                BackgroundColor = _cell.Type == CellType.Wall ? DarkWall : DarkFloor;
            }
            else
            {
                Content = _cell.Type switch
                {
                    CellType.Wall => "#",
                    CellType.Exit => "E",
                    _ => "."
                };
                ForegroundColor = _cell.Type switch
                {
                    CellType.Wall => Brushes.Gray,
                    CellType.Exit => Brushes.Cyan,
                    _ => Brushes.White
                };
                BackgroundColor = _cell.Type switch
                {
                    CellType.Wall => DarkWall,
                    CellType.Exit => DarkExit,
                    _ => DarkFloor
                };
            }
        }

        private Brush GetBrushFromColor(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Yellow => Brushes.Gold,
                ConsoleColor.Red => Brushes.Red,
                ConsoleColor.Magenta => Brushes.Magenta,
                ConsoleColor.Green => Brushes.LimeGreen,
                ConsoleColor.Cyan => Brushes.Cyan,
                ConsoleColor.Blue => Brushes.DodgerBlue,
                _ => Brushes.White
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}