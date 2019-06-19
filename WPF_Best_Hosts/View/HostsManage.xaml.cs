using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_Best_Hosts.Lib;
using WPF_Best_Hosts.Model;

namespace WPF_Best_Hosts.View
{
    /// <summary>
    /// HostsManage.xaml 的交互逻辑
    /// </summary>
    public partial class HostsManage : UserControl
    {
        public HostsManage()
        {
            InitializeComponent();
            Hosts.InitHostsData();
            HostsDataGrid.ItemsSource = Hosts.hostsDatas;
        }
        private void HostsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            var cell = datagrid.CurrentCell;
            var item = cell.Item as HostsData;

            if (item != null && cell.Column.DisplayIndex == 2)
            {
                item.State = item.State == "激活" ? "失效" : "激活";
                var hostsTxt = (item.State == "激活" ? "" : "#") + item.Ip + " " + item.Domain;
                Hosts.updateHosts(hostsTxt);
            }
        }

        private void HostsDataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;
            var item = e.Row.Item as HostsData;
            if (Regex.IsMatch(newValue, "\\d+\\.\\d+\\.\\d+\\.\\d+"))
            {
                var hostsTxt = (item.State == "激活" ? "" : "#") + newValue + " " + item.Domain;
                Hosts.updateHosts(hostsTxt);
            }
            else if (Regex.IsMatch(newValue, "[\\.\\w]+"))
            {
                var hostsTxt = (item.State == "激活" ? "" : "#") + item.Ip + " " + newValue;
                Hosts.updateHosts(hostsTxt);
            }
        }
    }
}
