using ErcTools.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ErcTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool mRestoreForDragMove;
        public MainWindow()
        {
            var mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
            InitializeComponent();

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    this.MaxWidth = SystemParameters.WorkArea.Width + 16;
                    this.MaxHeight = SystemParameters.WorkArea.Height + 16;
                    this.BorderThickness = new Thickness(5);
                    break;
                case WindowState.Normal:
                    this.BorderThickness = new Thickness(0);
                    break;
            }
        }
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ClickCount == 2)
            {
                if (ResizeMode != ResizeMode.CanResize && ResizeMode != ResizeMode.CanResizeWithGrip) return;
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                mRestoreForDragMove = WindowState == WindowState.Maximized;
                DragMove();
            }
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreForDragMove)
            {
                mRestoreForDragMove = false;
                WindowState = WindowState.Normal;
                //var point = e.MouseDevice.GetPosition(this);
                //                Left = point.X - ClientGrid.ActualWidth * point.X / SystemParameters.WorkArea.Width - WindowShadowBorder.Margin.Left;
                //                Top = point.Y - ClientGrid.ActualHeight * point.Y / SystemParameters.WorkArea.Height - WindowShadowBorder.Margin.Top;
                //DragMove();
            }
        }
    }
}
