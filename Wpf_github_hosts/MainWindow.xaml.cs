using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private LogHelper logHelper = new LogHelper();
        public MainWindow()
        {
            InitializeComponent();
            logHelper.LableComponent = DebugInfoTxt;
            PingList.ItemsSource = PingDataList;
            PingProgressBar.DataContext = progressBarHelper;
            var hostsManagerWindow = new HostsManagerWindow();
            hostsManagerWindow.Show();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            start_button.IsEnabled = false;
            threadPool.Cancel();
            PingDataList.Clear();
            var select_str = ComboDomainBox.Text;
            var result = await HttpHelper.GetHtmlTask($"http://ping.chinaz.com/", select_str, new {host=select_str,linetype="电信,多线,联通,移动,海外"});
            var htmldata = new HtmlSearch(result);
            
            foreach (var guidLocalKey in htmldata.GuidLocal.Keys) PingDataList.Add(new PingData(guidLocalKey, htmldata.GuidLocal[guidLocalKey], select_str));
            progressBarHelper.ValueMax = PingDataList.Count;
            List<IWorkItemResult> threadResults = new List<IWorkItemResult>();
            
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
                    PingDataList.Where(u => u.Ip == pingIp).ToList().ForEach(u => u.LocalAnswerTime = pingStatus.RoundtripTime.ToString());
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

        private async void DebugInfo_OnClick(object sender, RoutedEventArgs e)
        {
            logHelper.UpdateLog("click", LogHelper.InfoLevel.Info);
        }
    }
}