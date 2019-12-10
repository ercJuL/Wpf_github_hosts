using ErcTools.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ErcTools.ViewModel
{
    public class MainViewModel: ViewModelBase
    {
        private readonly DelegateCommand _closeWindowsCommand;
        public ICommand CloseWindowsCommand => _closeWindowsCommand;
        public MainViewModel()
        {
            _closeWindowsCommand = new DelegateCommand(OnCloseWindows, CanCloseWindows);
        }

        private void OnCloseWindows(object commandParameter)
        {
            Application.Current.Shutdown();
        }

        private bool CanCloseWindows(object commandParameter)
        {
            return true;
        }
    }
}
