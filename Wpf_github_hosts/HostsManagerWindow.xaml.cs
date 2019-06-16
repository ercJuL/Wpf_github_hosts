using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Wpf_github_hosts.Annotations;

namespace Wpf_github_hosts
{
    /// <summary>
    /// HostsManagerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HostsManagerWindow : MetroWindow
    {

        private static ObservableCollection<HostsData> PingDataList = new ObservableCollection<HostsData>();
        public HostsManagerWindow()
        {
            InitializeComponent();
            InitHostsData();
            PingDataGrid.ItemsSource = PingDataList;
        }

        public async void InitHostsData()
        {
            var HostsLines = Hosts.ReadHosts();
            foreach (var line in HostsLines)
            {
                 var lineTrim = line.Trim();
                if (Regex.IsMatch(lineTrim, "\\d+\\.\\d+\\.\\d+\\.\\d+[ \\t]{1,}[\\.\\w]+$"))
                {
                    PingDataList.Add(new HostsData()
                    {
                        Ip = Regex.Match(lineTrim, "\\d+\\.\\d+\\.\\d+\\.\\d+").Value,
                        Domain = lineTrim.Split(' ').Last(),
                        State = line.StartsWith("#")?"失效":"激活"
                    });
                }
            }
        }

        public class HostsData:INotifyPropertyChanged
        {
            private string _ip;
            private string _domain;
            private string _state;

            public string Ip
            {
                get => _ip;
                set
                {
                    _ip = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Ip"));
                }
            }

            public string Domain
            {
                get => _domain;
                set
                {
                    _domain = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Domain"));
                }
            }

            public string State
            {
                get => _state;
                set
                {
                    _state = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void PingDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            var cell = datagrid.CurrentCell;
            var item = cell.Item as HostsData;

            if (item !=null && cell.Column.DisplayIndex == 2)
            {
                item.State = item.State == "激活" ? "失效" : "激活";
                var hostsTxt = (item.State == "激活" ? "" : "#") + item.Ip + " " + item.Domain;
                Hosts.updateHosts(hostsTxt);
            }
        }

        private void PingDataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;
            var item = e.Row.Item as HostsData;
            if (Regex.IsMatch(newValue, "\\d+\\.\\d+\\.\\d+\\.\\d+"))
            {
                var hostsTxt = (item.State == "激活" ? "" : "#") + newValue + " " + item.Domain;
                Hosts.updateHosts(hostsTxt);
            }
            else if(Regex.IsMatch(newValue, "[\\.\\w]+"))
            {
                var hostsTxt = (item.State == "激活" ? "" : "#") + item.Ip + " " + newValue;
                Hosts.updateHosts(hostsTxt);
            }
        }
    }
}
