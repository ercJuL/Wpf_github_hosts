using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using Wpf_github_hosts.Annotations;

namespace Wpf_github_hosts
{
    public partial class MainWindow
    {
        public class PingData : INotifyPropertyChanged
        {
            private string _localName;
            private string _ip;
            private string _ipLocal;
            private string _answerTime;
            private string _localAnswerTime;
            private string _answerTtl;
            private string _localAnswerTtl;
            public event PropertyChangedEventHandler PropertyChanged;

            public PingData()
            {
            }

            public PingData(string localGuid, string localName, string domain)
            {
                LocalName = localName;
                LocalGuid = localGuid;
                Domain = domain;
            }

            public string LocalName
            {
                get => _localName;
                set
                {
                    _localName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalName"));
                }
            }

            public string Ip
            {
                get => _ip;
                set
                {
                    _ip = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Ip"));
                }
            }

            public string IpLocal
            {
                get => _ipLocal;
                set
                {
                    _ipLocal = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IpLocal"));
                }
            }

            public string AnswerTime
            {
                get => _answerTime;
                set
                {
                    _answerTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AnswerTime"));
                }
            }

            public string LocalAnswerTime
            {
                get => _localAnswerTime;
                set
                {
                    _localAnswerTime = Regex.IsMatch(value,"^\\d+$")?value + " 毫秒" :value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalAnswerTime"));
                }
            }

            public string AnswerTtl
            {
                get => _answerTtl;
                set
                {
                    _answerTtl = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AnswerTtl"));
                }
            }

            public string LocalAnswerTtl
            {
                get => _localAnswerTtl;
                set
                {
                    _localAnswerTtl = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalAnswerTtl"));
                }
            }

            public string LocalGuid { get; set; }
            public string Domain { get; set; }
        }

        public class PingPercentClass : INotifyPropertyChanged
        {
            private double _pingPercentData;

            public PingPercentClass(Double pingPercentData)
            {
                PingPercentData = pingPercentData;
            }
            public double PingPercentData
            {
                get => _pingPercentData;
                set
                {
                    _pingPercentData = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PingPercentData"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private class UpdateDataParams
        {
            public UpdateDataParams(string guidLocalKey, string domain, string encode)
            {
                GuidLocalKey = guidLocalKey;
                Domain = domain;
                Encode = encode;
            }
            public string GuidLocalKey { get; set; }
            public string Domain { get; set; }
            public string Encode { get; set; }
        }
    }
}