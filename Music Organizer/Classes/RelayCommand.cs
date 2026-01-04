using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Music_Organizer
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter is T tParam)
                return _canExecute(tParam);

            return _canExecute(default);
        }

        public void Execute(object parameter)
        {
            if (parameter is T tParam)
            {
                _execute(tParam);
                return;
            }

            _execute(default);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

}
