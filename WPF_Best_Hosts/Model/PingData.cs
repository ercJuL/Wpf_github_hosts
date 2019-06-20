using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WPF_Best_Hosts.Lib;

namespace WPF_Best_Hosts.Model
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
        private int _timeConsume;
        private int _dSize;
        private string _tcpIpResult;
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
                _localAnswerTime = Regex.IsMatch(value, "^\\d+$") ? value + " 毫秒" : value;
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

        public string TcpIpResult
        {
            get => $"{Utils.HumanReadableByteCount(DSize / TimeConsume * 1000, false)}/s";
            set => _tcpIpResult = value;
        }

        public string TcpIpResultTip => $"下载{Utils.HumanReadableByteCount(DSize, false)}\n共耗时{TimeConsume}毫秒";
            public int TimeConsume
        {
            get => _timeConsume;
            set
            {
                _timeConsume = value;
                if (_dSize > 0)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TcpIpResult"));
                }
            }
        }

        public int DSize
        {
            get => _dSize;
            set
            {
                _dSize = value;
                if (_timeConsume > 0)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TcpIpResult"));
                }
            }
        }

        public string LocalGuid { get; set; }
        public string Domain { get; set; }
    }
}
