using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace IFCConverto.MVVM
{
    public class ParamDelegateCommand<T> : ICommand
    {
        private Action<object> execute;
        private Func<bool> canExecute;

        public ParamDelegateCommand(Action<object> execute) : this(execute, null)
        {

        }

        public ParamDelegateCommand(Action<object> execute, Func<bool> canExecute)
        {
            if(execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute();
        }

        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                if (parameter != null)
                {
                    this.execute(parameter);
                }
            }
        }
    }
}