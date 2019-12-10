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
        public ICommand CloseWindowsCommand { get; }
        public MainViewModel()
        {
            CloseWindowsCommand = new CloseWindowsCommand();
        }
    }
}
