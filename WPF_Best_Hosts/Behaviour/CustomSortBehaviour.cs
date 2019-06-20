using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPF_Best_Hosts.Lib
{
    // https://stackoverflow.com/questions/18122751/wpf-datagrid-customsort-for-each-column/18218963#18218963
    public static class DataGridSort
    {
        public static readonly DependencyProperty ComparerProperty = DependencyProperty.RegisterAttached(
            "Comparer",
            typeof(IComparer),
            typeof(DataGridSort),
            new PropertyMetadata(
                default(IComparer),
                OnComparerChanged));

        private static readonly DependencyProperty ColumnComparerProperty = DependencyProperty.RegisterAttached(
            "ColumnComparer",
            typeof(ColumnComparer),
            typeof(DataGridSort),
            new PropertyMetadata(default(ColumnComparer)));

        private static readonly DependencyProperty PreviousComparerProperty = DependencyProperty.RegisterAttached(
            "PreviousComparer",
            typeof(IComparer),
            typeof(DataGridSort),
            new PropertyMetadata(default(IComparer)));

        public static readonly DependencyProperty UseCustomSortProperty = DependencyProperty.RegisterAttached(
            "UseCustomSort",
            typeof(bool),
            typeof(DataGridSort),
            new PropertyMetadata(default(bool), OnUseCustomSortChanged));

        public static void SetComparer(DataGridColumn element, IComparer value)
        {
            element.SetValue(ComparerProperty, value);
        }

        public static IComparer GetComparer(DataGridColumn element)
        {
            return (IComparer)element.GetValue(ComparerProperty);
        }

        public static void SetUseCustomSort(DependencyObject element, bool value)
        {
            element.SetValue(UseCustomSortProperty, value);
        }

        public static bool GetUseCustomSort(DependencyObject element)
        {
            return (bool)element.GetValue(UseCustomSortProperty);
        }

        private static void OnComparerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (DataGridColumn)d;
            var columnComparer = new ColumnComparer((IComparer)e.NewValue, column);
            column.SetValue(ColumnComparerProperty, columnComparer);
        }

        private static void OnUseCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            if ((bool)e.NewValue)
            {
                WeakEventManager<DataGrid, DataGridSortingEventArgs>.AddHandler(dataGrid, nameof(dataGrid.Sorting), OnDataGridSorting);
            }
            else
            {
                WeakEventManager<DataGrid, DataGridSortingEventArgs>.RemoveHandler(dataGrid, nameof(dataGrid.Sorting), OnDataGridSorting);
            }
        }

        private static void OnDataGridSorting(object sender, DataGridSortingEventArgs e)
        {
            var column = e.Column;
            var columnComparer = (ColumnComparer)column.GetValue(ColumnComparerProperty);
            var dataGrid = (DataGrid)sender;
            var view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource) as ListCollectionView;
            if (view == null)
            {
                return;
            }
            if (columnComparer == null)
            {
                view.CustomSort = (IComparer)dataGrid.GetValue(PreviousComparerProperty);
            }
            else
            {
                if (!(view.CustomSort is ColumnComparer))
                {
                    dataGrid.SetValue(PreviousComparerProperty, view.CustomSort);
                }

                switch (column.SortDirection)
                {
                    case ListSortDirection.Ascending:
                        column.SortDirection = ListSortDirection.Descending;
                        view.CustomSort = columnComparer.Descending;
                        break;
                    case null:
                    case ListSortDirection.Descending:
                        column.SortDirection = ListSortDirection.Ascending;
                        view.CustomSort = columnComparer.Ascending;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                e.Handled = true;
            }
        }

        private class ColumnComparer : IComparer
        {
            private readonly IComparer valueComparer;
            private readonly DataGridColumn column;
            private readonly InvertedComparer inverted;

            public ColumnComparer(IComparer valueComparer, DataGridColumn column)
            {
                this.valueComparer = valueComparer;
                this.column = column;
                inverted = new InvertedComparer(this);
            }

            public IComparer Ascending => this;

            public IComparer Descending => inverted;

            int IComparer.Compare(object x, object y)
            {
                if (x == y)
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                // this can perhaps be a bit slow
                // Not adding caching yet.
                var xProp = x.GetType().GetProperty(column.SortMemberPath);
                var xValue = xProp.GetValue(x);
                var yProp = x.GetType().GetProperty(column.SortMemberPath);
                var yValue = yProp.GetValue(y);
                return valueComparer.Compare(xValue, yValue);
            }

            private class InvertedComparer : IComparer
            {
                private readonly IComparer comparer;

                public InvertedComparer(IComparer comparer)
                {
                    this.comparer = comparer;
                }

                public int Compare(object x, object y)
                {
                    return comparer.Compare(y, x);
                }
            }
        }
    }
}
