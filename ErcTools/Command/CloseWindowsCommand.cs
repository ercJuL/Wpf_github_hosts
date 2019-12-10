using System.Windows;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using System;

namespace ErcTools.Command
{
    public class CloseWindowsCommand: ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void InvokeCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
