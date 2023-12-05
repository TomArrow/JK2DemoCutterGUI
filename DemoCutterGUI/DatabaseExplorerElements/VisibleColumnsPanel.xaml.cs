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
    /// Interaction logic for VisibleColumnsPanel.xaml
    /// </summary>
    public partial class VisibleColumnsPanel : UserControl
    {

        public static readonly DependencyProperty ReferenceGridProperty = DependencyProperty.Register("ReferenceGrid", typeof(DataGrid), typeof(VisibleColumnsPanel), new PropertyMetadata(null));

        public DataGrid ReferenceGrid
        {
            get { return (DataGrid)GetValue(ReferenceGridProperty); }
            set { SetValue(ReferenceGridProperty, value); }
        }
        public VisibleColumnsPanel()
        {
            InitializeComponent();
        }
    }
}
