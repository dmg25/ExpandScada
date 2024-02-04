using ExpandScada.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExpandScada
{
    public class MultiActionCommand : ICommand
    {
        private List<ButtonAction> executeList;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public MultiActionCommand(List<ButtonAction> executeList, Func<object, bool> canExecute = null)
        {
            this.executeList = executeList;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            foreach (var action in this.executeList)
            {
                action.Execute();
            }
        }
    }
}
