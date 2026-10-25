// Реализация интерфейса ICommand для привязки команд в WPF
using System;
using System.Windows.Input;

namespace DungeonCrawler.WPF.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // Инициализация команды с действиями выполнения и проверки возможности выполнения
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Событие изменения возможности выполнения команды
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        // Проверка возможности выполнения команды
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        // Выполнение команды
        public void Execute(object? parameter) => _execute(parameter);
    }
}