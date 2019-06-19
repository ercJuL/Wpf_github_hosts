using System;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Documents;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using WPF_Best_Hosts.View;

namespace WPF_Best_Hosts.Domain
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            if (snackbarMessageQueue == null) throw new ArgumentNullException(nameof(snackbarMessageQueue));

            DemoItems = new []
            {
                new DemoItem("Home", new Home()), 
                new DemoItem("IPTest", new IPTest()), 
                new DemoItem("HostsManage", new HostsManage()), 
            };
        }

        public DemoItem[] DemoItems { get; }
    }
}