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

namespace DemoCutterGUI.DatabaseExplorerElements
{

    /// <summary>
    /// Interaction logic for MidPanel.xaml
    /// </summary>
    public partial class MidPanel : UserControl
    {
        public static readonly DependencyProperty Items1Property = DependencyProperty.Register("Items1", typeof(System.Collections.IEnumerable), typeof(MidPanel), new PropertyMetadata(null));
        public static readonly DependencyProperty Items2Property = DependencyProperty.Register("Items2", typeof(System.Collections.IEnumerable), typeof(MidPanel), new PropertyMetadata(null));
        public static readonly DependencyProperty Items3Property = DependencyProperty.Register("Items3", typeof(System.Collections.IEnumerable), typeof(MidPanel), new PropertyMetadata(null));
        public static readonly DependencyProperty Items4Property = DependencyProperty.Register("Items4", typeof(System.Collections.IEnumerable), typeof(MidPanel), new PropertyMetadata(null));

        public System.Collections.IEnumerable Items1
        {
            get { return (System.Collections.IEnumerable)GetValue(Items1Property); }
            set { SetValue(Items1Property, value); }
        }
        public System.Collections.IEnumerable Items2
        {
            get { return (System.Collections.IEnumerable)GetValue(Items2Property); }
            set { SetValue(Items2Property, value); }
        }
        public System.Collections.IEnumerable Items3
        {
            get { return (System.Collections.IEnumerable)GetValue(Items3Property); }
            set { SetValue(Items3Property, value); }
        }
        public System.Collections.IEnumerable Items4
        {
            get { return (System.Collections.IEnumerable)GetValue(Items4Property); }
            set { SetValue(Items4Property, value); }
        }

        public static readonly DependencyProperty Items1NameProperty = DependencyProperty.Register("Items1Name", typeof(string), typeof(MidPanel), new PropertyMetadata("Items1"));
        public static readonly DependencyProperty Items2NameProperty = DependencyProperty.Register("Items2Name", typeof(string), typeof(MidPanel), new PropertyMetadata("Items2"));
        public static readonly DependencyProperty Items3NameProperty = DependencyProperty.Register("Items3Name", typeof(string), typeof(MidPanel), new PropertyMetadata("Items3"));
        public static readonly DependencyProperty Items4NameProperty = DependencyProperty.Register("Items4Name", typeof(string), typeof(MidPanel), new PropertyMetadata("Various"));

        public string Items1Name
        {
            get { return (string)GetValue(Items1NameProperty); }
            set { SetValue(Items1NameProperty, value); }
        }
        public string Items2Name
        {
            get { return (string)GetValue(Items2NameProperty); }
            set { SetValue(Items2NameProperty, value); }
        }
        public string Items3Name
        {
            get { return (string)GetValue(Items3NameProperty); }
            set { SetValue(Items3NameProperty, value); }
        }
        public string Items4Name
        {
            get { return (string)GetValue(Items4NameProperty); }
            set { SetValue(Items4NameProperty, value); }
        }




        public event EventHandler<SortingInfo> sortingChanged;

        private void OnSortingChanged(SortingInfo sortInfo)
        {
            sortingChanged?.Invoke(this, sortInfo);
        }

        public MidPanel()
        {
            InitializeComponent();
        }

        public DataGrid TheGrid
        {
            get
            {
                return retsGrid;
            }
        }

        string sortField;
        bool sortDescending;

        private void retsGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            sortField = e.Column.SortMemberPath;
            e.Column.SortDirection = e.Column.SortDirection == System.ComponentModel.ListSortDirection.Ascending ? System.ComponentModel.ListSortDirection.Descending : System.ComponentModel.ListSortDirection.Ascending;
            sortDescending = e.Column.SortDirection == System.ComponentModel.ListSortDirection.Ascending;
            OnSortingChanged(new SortingInfo() { descending = sortDescending, fieldName = sortField });
            //retsItemSource.Reset();
            e.Handled = true;
        }
    }
}
