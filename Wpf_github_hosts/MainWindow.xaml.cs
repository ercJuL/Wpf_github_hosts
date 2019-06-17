using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Amib.Threading;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;

namespace Wpf_github_hosts
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static ObservableCollection<PingData> PingDataList = new ObservableCollection<PingData>();
        private SmartThreadPool threadPool = new SmartThreadPool();
        private bool isUpdate = false;
        private ProgressBarHelper progressBarHelper = new ProgressBarHelper();
        PingPercentClass pingPercentClass = new PingPercentClass(0);
        private static ObservableCollection<HostsData> hostsDatas = new ObservableCollection<HostsData>();
        public MainWindow()
        {
            InitializeComponent();
            LogHelper.LableComponent = DebugInfoTxt;
            PingList.ItemsSource = PingDataList;
            PingProgressBar.DataContext = progressBarHelper;
            
            HostsDataGrid.ItemsSource = hostsDatas;
            LogHelper.UpdateLog("欢迎使用~");
            MessageBox.Show("如果要使用本软件修改hosts请使用管理员权限打开");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            start_button.IsEnabled = false;
            LogHelper.UpdateLog("等待结束前置任务");
            threadPool.Cancel();
            PingDataList.Clear();
            var select_str = ComboDomainBox.Text;
            LogHelper.UpdateLog("获取远程ip.....");
            var result = await HttpHelper.GetHtmlTask($"http://ping.chinaz.com/", select_str, new {host=select_str,linetype="电信,多线,联通,移动,海外"});
            var htmldata = new HtmlSearch(result);
            if (String.IsNullOrWhiteSpace(htmldata.Encode))
            {
                LogHelper.UpdateLog("获取远程ip出错，稍后再试");
                start_button.IsEnabled = true;
                return;
            }
            
            foreach (var guidLocalKey in htmldata.GuidLocal.Keys) PingDataList.Add(new PingData(guidLocalKey, htmldata.GuidLocal[guidLocalKey], select_str));
            progressBarHelper.ValueMax = PingDataList.Count;

            List<IWorkItemResult> threadResults = new List<IWorkItemResult>();
            LogHelper.UpdateLog("获取远程ping结果");
            foreach (var guidLocalKey in htmldata.GuidLocal.Keys)
                threadResults.Add(threadPool.QueueWorkItem(new WorkItemCallback(UpdateData), new UpdateDataParams(guidLocalKey, select_str, htmldata.Encode)));
            new Thread(LocalPing).Start();
            start_button.IsEnabled = true;
        }

        private object UpdateData(object inObject)
        {
            var updateDataParams = inObject as UpdateDataParams;
            var changedata = PingDataList.First(u => u.LocalGuid == updateDataParams.GuidLocalKey);

            changedata.Ip = "loding...";
            changedata.IpLocal = "loding...";
            changedata.AnswerTime = "loding...";
            changedata.AnswerTtl = "loding...";
            try
            {
                var response = HttpHelper.GetPingDataTask("http://ping.chinaz.com/", "iframe.ashx", new {guid = updateDataParams.GuidLocalKey, host = updateDataParams.Domain, ishost = 0, encode = updateDataParams.Encode, checktype = 0});
                if (response.Contains("state"))
                {
                    var jsonStr = Regex.Replace(response, "^\\(", "");
                    jsonStr = Regex.Replace(jsonStr, "\\)$", "");
                    var responseJson = JToken.Parse(jsonStr) as dynamic;


                    if (responseJson.state == 1)
                    {
                        changedata.Ip = responseJson.result.ip;
                        changedata.IpLocal = responseJson.result.ipaddress;
                        changedata.AnswerTime = responseJson.result.responsetime.Value.Contains("超时") ? "超时" : responseJson.result.responsetime;
                        changedata.AnswerTtl = responseJson.result.ttl.Value.Contains("超时") ? "超时" : responseJson.result.ttl;
                        progressBarHelper.Step();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                changedata.Ip = "";
            }
            changedata.Ip = "超时";
            changedata.IpLocal = "超时";
            changedata.AnswerTime = "超时";
            changedata.AnswerTtl = "超时";
            progressBarHelper.Step();
            return false;
        }

        private void LocalPing()
        {
            threadPool.WaitForIdle();
            var pingList = PingDataList.Select(u => u.Ip).Distinct();
            using (var ping = new Ping())
            {
                foreach (var pingIp in pingList.Where(u=>u!="超时"))
                {
                    var pingStatus = ping.Send(pingIp);
                    PingDataList.Where(u => u.Ip == pingIp).ToList().ForEach(u => u.LocalAnswerTime = pingStatus.Status == IPStatus.Success? pingStatus.RoundtripTime.ToString():"超时");
                }
            }
        }

        private void PingList_OnLoaded(object sender, RoutedEventArgs e)
        {
            //使listview根据内容自动调整宽度
            if (PingList.View is GridView gv)
            {
                gv.Columns[0].Width = (int) (0.16 * PingList.ActualWidth);
                gv.Columns[1].Width = (int) (0.2 * PingList.ActualWidth);
                gv.Columns[2].Width = (int) (0.2 * PingList.ActualWidth);
                gv.Columns[3].Width = (int) (0.16 * PingList.ActualWidth);
                gv.Columns[4].Width = (int) (0.16 * PingList.ActualWidth);
                gv.Columns[5].Width = (int) (0.12 * PingList.ActualWidth);
            }
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PingList_OnLoaded(null, null);
        }

        private void CopyIpDomainClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = PingList.SelectedItem as PingData;
            var cliText = ((string.IsNullOrEmpty(selectedItem.Ip) || selectedItem.Ip.Contains("超时")) ? "无效ip" : selectedItem.Ip) + $" {selectedItem.Domain}";
            Clipboard.SetText(cliText);
        }

        private void CopyIpClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = PingList.SelectedItem as PingData;
            var cliText = (string.IsNullOrEmpty(selectedItem.Ip) || selectedItem.Ip.Contains("超时")) ? "无效ip" : selectedItem.Ip;
            Clipboard.SetText(cliText);
        }

        private void UpdateHostsClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = PingList.SelectedItem as PingData;
            var hostsData = ((string.IsNullOrEmpty(selectedItem.Ip) || selectedItem.Ip.Contains("超时")) ? "无效ip" : selectedItem.Ip) + $" {selectedItem.Domain}";
            Hosts.updateHosts(hostsData);
        }

        private async void Sort_Click(object sender, RoutedEventArgs e)
        {
            if (PingDataList.Count < 1)
            {
                return;
            }
            var column = e.OriginalSource as GridViewColumnHeader;
            if (column == null || column.Column == null) return;
            var newPingDataList = new ObservableCollection<PingData>();
            var pingDataListSort = PingDataList.ToList();
            switch (column.Column.Header)
            {
                case "响应时间":
                    pingDataListSort.Sort((x, y) =>
                    {
                        var xMatch = string.IsNullOrEmpty(x.AnswerTime) ? Regex.Match("", "\\d+") : Regex.Match(x.AnswerTime, "\\d+");
                        var yMatch = string.IsNullOrEmpty(y.AnswerTime) ? Regex.Match("", "\\d+") : Regex.Match(y.AnswerTime, "\\d+");
                        if (xMatch.Success && yMatch.Success)
                            return (x.AnswerTime.StartsWith("<")?0: Convert.ToInt32(xMatch.Value))- (y.AnswerTime.StartsWith("<") ? 0 : Convert.ToInt32(yMatch.Value));
                        return Convert.ToInt32(yMatch.Success) - Convert.ToInt32(xMatch.Success);
                    });
                    PingDataList.Clear();
                    foreach (var data in pingDataListSort) PingDataList.Add(data);
                    PingList.ScrollIntoView(PingDataList[0]);
                    break;
                case "本地响应时间":
                    pingDataListSort.Sort((x, y) =>
                    {
                        var xMatch = string.IsNullOrEmpty(x.LocalAnswerTime) ? Regex.Match("", "\\d+") : Regex.Match(x.LocalAnswerTime, "\\d+");
                        var yMatch = string.IsNullOrEmpty(y.LocalAnswerTime) ? Regex.Match("", "\\d+") : Regex.Match(y.LocalAnswerTime, "\\d+");
                        if (xMatch.Success && yMatch.Success)
                            return Convert.ToInt32(xMatch.Value) - Convert.ToInt32(yMatch.Value);
                        return Convert.ToInt32(yMatch.Success) - Convert.ToInt32(xMatch.Success);
                    });
                    PingDataList.Clear();
                    foreach (var data in pingDataListSort) PingDataList.Add(data);
                    PingList.ScrollIntoView(PingDataList[0]);
                    break;
                case "TTL":
                    pingDataListSort.Sort((x, y) =>
                    {
                        var xMatch = string.IsNullOrEmpty(x.AnswerTtl) ? Regex.Match("", "\\d+") : Regex.Match(x.AnswerTtl, "\\d+");
                        var yMatch = string.IsNullOrEmpty(y.AnswerTtl) ? Regex.Match("", "\\d+") : Regex.Match(y.AnswerTtl, "\\d+");
                        if (xMatch.Success && yMatch.Success)
                            return Convert.ToInt32(xMatch.Value) - Convert.ToInt32(yMatch.Value);
                        return Convert.ToInt32(yMatch.Success) - Convert.ToInt32(xMatch.Success);
                    });
                    PingDataList.Clear();
                    foreach (var data in pingDataListSort) PingDataList.Add(data);
                    PingList.ScrollIntoView(PingDataList[0]);
                    break;
            }
        }

        private void HostsManagerButton_OnClick(object sender, RoutedEventArgs e)
        {
            InitHostsData();
            HostsManager.Visibility = Visibility.Visible;
        }
        public async void InitHostsData()
        {
            var HostsLines = Hosts.ReadHosts();
            foreach (var line in HostsLines)
            {
                var lineTrim = line.Trim();
                if (Regex.IsMatch(lineTrim, "\\d+\\.\\d+\\.\\d+\\.\\d+[ \\t]{1,}[\\.\\w]+$"))
                {
                    hostsDatas.Add(new HostsData()
                    {
                        Ip = Regex.Match(lineTrim, "\\d+\\.\\d+\\.\\d+\\.\\d+").Value,
                        Domain = lineTrim.Split(' ').Last(),
                        State = line.StartsWith("#") ? "失效" : "激活"
                    });
                }
            }
        }

        public class HostsData : INotifyPropertyChanged
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

        private void HostMangerClose_Click(object sender, RoutedEventArgs e)
        {
            HostsManager.Visibility = Visibility.Hidden;
        }
    }
}