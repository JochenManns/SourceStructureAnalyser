using System;
using System.Windows.Input;

namespace SourceStructureAnalyser
{
    public class Command<TParameterType> : ICommand
    {
        private readonly Func<TParameterType, bool> m_test;

        private readonly Action<TParameterType> m_execute;

        public event EventHandler CanExecuteChanged;

        public Command(Action<TParameterType> execute, Func<TParameterType, bool> test = null)
        {
            m_execute = execute;
            m_test = test;
        }

        public void FireChange() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object parameter) =>
            m_test?.Invoke((TParameterType)parameter) ?? true;

        public void Execute(object parameter) =>
            m_execute?.Invoke((TParameterType)parameter);
    }

    public class Command : Command<object>
    {
        public Command(Action execute, Func<bool> test = null)
            : base((execute == null) ? null : (Action<object>)(p => execute()), (test == null) ? null : (Func<object, bool>)(p => test()))
        {
        }
    }

}
