using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace IFCConverto.MVVM
{
    public class SimpleDelegateCommand : ICommand
    {
        private readonly Action execute;        
        private readonly Func<bool> canExecute;

        public SimpleDelegateCommand(Action execute) : this(execute, null)
        {
        }

        public SimpleDelegateCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
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

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute();
        }

        public void Execute(object parameter)
        {            
            if (this.CanExecute(parameter))
            {
                if (parameter == null)
                {
                    this.execute();
                }                
            }
        }       
    }
}