using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Wpf_github_hosts.Annotations;

namespace Wpf_github_hosts
{
    public class ProgressBarHelper:INotifyPropertyChanged
    {
        private double _valueMax;
        private double _valueNow;

        public double ValueMax
        {
            get => _valueMax;
            set
            {
                _valueMax = value;
                _valueNow=0;
                UpdateUi("ValueMax");
            }
        }

        public double ValueNow
        {
            get => Math.Round(_valueNow * 100 / ValueMax, 2);
            set
            {
                _valueNow=value;
                UpdateUi("ValueNow");
            }
        }

        public void Step()
        {
            _valueNow++;
            UpdateUi("ValueNow");
        }

        public Visibility Visibility => Math.Abs(_valueMax - _valueNow) > 0 ? Visibility.Visible : Visibility.Hidden;
        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateUi(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PerceentString"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
        }
    }
}