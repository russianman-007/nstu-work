// Обработчик событий главного окна, перенаправляющий нажатия клавиш в команды ViewModel
using System.Windows;
using System.Windows.Input;
using DungeonCrawler.WPF.ViewModels;

namespace DungeonCrawler.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Перехват нажатий клавиш для управления игрой с клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not GameViewModel vm) return;

            switch (e.Key)
            {
                case Key.N:
                    vm.NewGameCommand.Execute(null);
                    break;
                case Key.Up:
                case Key.W:
                    vm.MoveUpCommand.Execute(null);
                    break;
                case Key.Down:
                case Key.S:
                    vm.MoveDownCommand.Execute(null);
                    break;
                case Key.Left:
                case Key.A:
                    vm.MoveLeftCommand.Execute(null);
                    break;
                case Key.Right:
                case Key.D:
                    vm.MoveRightCommand.Execute(null);
                    break;
                case Key.Space:
                    vm.AttackCommand.Execute(null);
                    break;
                case Key.F5:
                    vm.SaveGameCommand.Execute(null);
                    break;
                case Key.F9:
                    vm.LoadGameCommand.Execute(null);
                    break;
            }
        }
    }
}