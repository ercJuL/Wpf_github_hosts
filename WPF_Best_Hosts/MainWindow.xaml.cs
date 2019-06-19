using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using WPF_Best_Hosts.Domain;

namespace WPF_Best_Hosts
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Snackbar Snackbar;
        private bool mRestoreForDragMove;
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonUp += (s, e) => { mRestoreForDragMove = false; };
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2500);
            }).ContinueWith(t =>
            {
                MainSnackbar.MessageQueue.Enqueue("欢迎使用");
            }, TaskScheduler.FromCurrentSynchronizationContext());

            DataContext = new MainWindowViewModel(MainSnackbar.MessageQueue);

            Snackbar = this.MainSnackbar;
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            MenuToggleButton.IsChecked = false;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    this.MaxWidth = SystemParameters.WorkArea.Width + 16;
                    this.MaxHeight = SystemParameters.WorkArea.Height + 16;
                    this.BorderThickness = new Thickness(5); //最大化后需要调整
//                    WindowShadowBorder.Margin = new Thickness(0);
                    break;
                case WindowState.Normal:
                    this.BorderThickness = new Thickness(0);
//                    WindowShadowBorder.Margin = new Thickness(10);
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
                var point = e.MouseDevice.GetPosition(this);
//                Left = point.X - ClientGrid.ActualWidth * point.X / SystemParameters.WorkArea.Width - WindowShadowBorder.Margin.Left;
//                Top = point.Y - ClientGrid.ActualHeight * point.Y / SystemParameters.WorkArea.Height - WindowShadowBorder.Margin.Top;
                DragMove();
            }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
