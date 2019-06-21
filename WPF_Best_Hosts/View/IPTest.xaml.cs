using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using Amib.Threading;
using Newtonsoft.Json.Linq;
using WPF_Best_Hosts.Lib;
using WPF_Best_Hosts.Model;
using Wpf_github_hosts;

namespace WPF_Best_Hosts.View
{
    /// <summary>
    /// IPTest.xaml 的交互逻辑
    /// </summary>
    public partial class IPTest : UserControl
    {
        private static ObservableCollection<PingData> PingDataList = new ObservableCollection<PingData>();
        private SmartThreadPool threadPool = new SmartThreadPool();
        public IPTest()
        {
            InitializeComponent();
        }

        private async void PingButton_OnClick(object sender, RoutedEventArgs e)
        {
            await Task.Run(PingButton_OnClick_invoke);
        }

        private void test(bool t)
        {
            PingButton.IsEnabled = t;
        }

        private void PingButton_OnClick_invoke()
        {
//            PingButton.IsEnabled = false;
            this.Dispatcher.Invoke(() => PingButton.IsEnabled = false);
            threadPool.Cancel();
//            PingDataList.Clear();
            this.Dispatcher.Invoke(() => PingDataList.Clear());
            var select_str =this.Dispatcher.Invoke(()=>ComboDomainBox.Text);
            var htmldata = HttpHelper.GetHtmlTask($"http://ping.chinaz.com/", select_str, new { host = select_str, linetype = "电信,多线,联通,移动,海外" });
            if (String.IsNullOrWhiteSpace(htmldata.Encode))
            {
                this.Dispatcher.Invoke(() => PingButton.IsEnabled = true);
                return;
            }
            var templist = new ObservableCollection<PingData>();
            foreach (var guidLocalKey in htmldata.GuidLocal.Keys) templist.Add(new PingData(guidLocalKey, htmldata.GuidLocal[guidLocalKey], select_str));

            this.Dispatcher.Invoke(() => {
                PingDataList = templist;
                PingDataGrid.ItemsSource = PingDataList;
            });
            

            List<IWorkItemResult> threadResults = new List<IWorkItemResult>();
            foreach (var guidLocalKey in htmldata.GuidLocal.Keys)
                threadResults.Add(threadPool.QueueWorkItem(new WorkItemCallback(UpdateData), new UpdateDataParams(guidLocalKey, select_str, htmldata.Encode)));
            SmartThreadPool.WaitAll(threadResults.ToArray());
            Task.Run(() => LocalTest(select_str));
            this.Dispatcher.Invoke(() => PingButton.IsEnabled = true);
            //            PingButton.IsEnabled = true;
        }

        private object UpdateData(object inObject)
        {
            var updateDataParams = inObject as UpdateDataParams;
            var changedata = PingDataList.First(u => u.LocalGuid == updateDataParams.GuidLocalKey);

            changedata.Ip = "loading...";
            changedata.IpLocal = "loading...";
            changedata.AnswerTime = "loading...";
            changedata.AnswerTtl = "loading...";
            try
            {
                var response = HttpHelper.GetPingDataTask("http://ping.chinaz.com/", "iframe.ashx", new { guid = updateDataParams.GuidLocalKey, host = updateDataParams.Domain, ishost = 0, encode = updateDataParams.Encode, checktype = 0 });
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
            changedata.LocalAnswerTime = "超时";
            changedata.AnswerTtl = "超时";
            changedata.TcpIpResult = "超时";
            return false;
        }

        private void LocalTest(string host)
        {
            threadPool.WaitForIdle();
            var pingList = PingDataList.Select(u => u.Ip).Distinct();
            using (var ping = new Ping())
            {
                foreach (var pingIp in pingList.Where(u => u != "超时"))
                {
                    foreach (var pingData in PingDataList.Where(u=>u.Ip==pingIp))
                    {
                        pingData.LocalAnswerTime = "loading...";
                        pingData.TcpIpResult = "loading...";
                    }
                    var pingStatus = ping.Send(pingIp);
                    HttpHelper.TcpIpTest(pingIp, host, out var timeConsume, out var dSize);
                    PingDataList.Where(u => u.Ip == pingIp).ToList().ForEach(u =>
                    {
                        u.LocalAnswerTime = pingStatus.Status == IPStatus.Success ? pingStatus.RoundtripTime.ToString() : "超时";
                        u.TimeConsume = timeConsume;
                        u.DSize = dSize;

                    });
                }
            }
        }

        private void CopyIpDomainClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = PingDataGrid.SelectedItem as PingData;
            var cliText = ((string.IsNullOrEmpty(selectedItem.Ip) || selectedItem.Ip.Contains("超时")) ? "无效ip" : selectedItem.Ip) + $" {selectedItem.Domain}";
            Clipboard.SetText(cliText);
        }

        private void CopyIpClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = PingDataGrid.SelectedItem as PingData;
            var cliText = (string.IsNullOrEmpty(selectedItem.Ip) || selectedItem.Ip.Contains("超时")) ? "无效ip" : selectedItem.Ip;
            Clipboard.SetText(cliText);
        }

        private void UpdateHostsClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = PingDataGrid.SelectedItem as PingData;
            var hostsData = ((string.IsNullOrEmpty(selectedItem.Ip) || selectedItem.Ip.Contains("超时")) ? "无效ip" : selectedItem.Ip) + $" {selectedItem.Domain}";
            Hosts.updateHosts(hostsData);
        }
    }
}
