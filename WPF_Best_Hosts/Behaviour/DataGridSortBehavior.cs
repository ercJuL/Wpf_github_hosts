using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPF_Best_Hosts.Behaviour
{
    // https://stackoverflow.com/questions/18122751/wpf-datagrid-customsort-for-each-column/18218963#18218963
    public class DataGridSortBehavior
    {
        public static IComparer GetSorter(DataGridColumn column)
        {
            return (IComparer)column.GetValue(SorterProperty);
        }

        public static void SetSorter(DataGridColumn column, IComparer value)
        {
            column.SetValue(SorterProperty, value);
        }

        public static bool GetAllowCustomSort(DataGrid grid)
        {
            return (bool)grid.GetValue(AllowCustomSortProperty);
        }

        public static void SetAllowCustomSort(DataGrid grid, bool value)
        {
            grid.SetValue(AllowCustomSortProperty, value);
        }

        public static readonly DependencyProperty SorterProperty = DependencyProperty.RegisterAttached("Sorter", typeof(IComparer),
            typeof(DataGridSortBehavior));
        public static readonly DependencyProperty AllowCustomSortProperty = DependencyProperty.RegisterAttached("AllowCustomSort", typeof(bool),
            typeof(DataGridSortBehavior), new UIPropertyMetadata(false, OnAllowCustomSortChanged));

        private static void OnAllowCustomSortChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var grid = (DataGrid)obj;

            bool oldAllow = (bool)e.OldValue;
            bool newAllow = (bool)e.NewValue;

            if (!oldAllow && newAllow)
            {
                grid.Sorting += HandleCustomSorting;
            }
            else
            {
                grid.Sorting -= HandleCustomSorting;
            }
        }

        public static bool ApplySort(DataGrid grid, DataGridColumn column)
        {
            IComparer sorter = GetSorter(column);
            if (sorter == null)
            {
                return false;
            }

            var listCollectionView = CollectionViewSource.GetDefaultView(grid.ItemsSource) as ListCollectionView;
            if (listCollectionView == null)
            {
                throw new Exception("The ICollectionView associated with the DataGrid must be of type, ListCollectionView");
            }

            listCollectionView.CustomSort = new DataGridSortComparer(sorter, column.SortDirection ?? ListSortDirection.Ascending, column.SortMemberPath);
            return true;
        }

        private static void HandleCustomSorting(object sender, DataGridSortingEventArgs e)
        {
            IComparer sorter = GetSorter(e.Column);
            if (sorter == null)
            {
                return;
            }

            var grid = (DataGrid)sender;
            e.Column.SortDirection = e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (ApplySort(grid, e.Column))
            {
                e.Handled = true;
            }
        }

        private class DataGridSortComparer : IComparer
        {
            private IComparer comparer;
            private ListSortDirection sortDirection;
            private string propertyName;
            private PropertyInfo property;

            public DataGridSortComparer(IComparer comparer, ListSortDirection sortDirection, string propertyName)
            {
                this.comparer = comparer;
                this.sortDirection = sortDirection;
                this.propertyName = propertyName;
            }

            public int Compare(object x, object y)
            {
                try
                {
                    PropertyInfo property = this.property ?? (this.property = x.GetType().GetProperty(propertyName));
                    object value1 = property.GetValue(x);
                    object value2 = property.GetValue(y);

                    int result = comparer.Compare(value1, value2);
                    if (sortDirection == ListSortDirection.Descending)
                    {
                        result = -result;
                    }
                    return result;
                }
                catch (Exception e)
                {
                    throw e;
                }
                
            }
        }
    }
}
