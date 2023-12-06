using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SidePanel.xaml
    /// </summary>
    public partial class SidePanel : UserControl
    {
        public static readonly DependencyProperty ReferenceMidPanelProperty = DependencyProperty.Register("ReferenceMidPanel",typeof(MidPanel),typeof(SidePanel),new PropertyMetadata(null));

        public MidPanel ReferenceMidPanel
        {
            get { return (MidPanel)GetValue(ReferenceMidPanelProperty);}
            set { SetValue(ReferenceMidPanelProperty, value); }
        }

        public static readonly DependencyProperty FieldsProperty = DependencyProperty.Register("Fields",typeof(DatabaseFieldInfo[]),typeof(SidePanel),new PropertyMetadata(null));

        public DatabaseFieldInfo[] Fields
        {
            get { return (DatabaseFieldInfo[])GetValue(FieldsProperty);}
            set { SetValue(FieldsProperty, value); }
        }

        public SidePanel()
        {
            InitializeComponent();
            this.Loaded += SidePanel_Loaded;
        }

        private void SidePanel_Loaded(object sender, RoutedEventArgs e)
        {

            DependencyPropertyDescriptor itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(ListView.ItemsSourceProperty,typeof(ListView));
            if (itemsSourceDescriptor != null)
            {
                itemsSourceDescriptor.AddValueChanged(listKillsNamesDataView, ItemsSourceChanged);
            }
        }


        private void ItemsSourceChanged(object sender, EventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listKillsNamesDataView.ItemsSource);
            if(view != null)
            {
                PropertyGroupDescription pgd = new PropertyGroupDescription("SubCategory");
                view.GroupDescriptions.Add(pgd);
            }
        }

        ~SidePanel()
        {
            CloseDown();
        }

        private void CloseDown()
        {
            DependencyPropertyDescriptor itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(ListView.ItemsSourceProperty, typeof(ListView));
            if (itemsSourceDescriptor != null)
            {
                itemsSourceDescriptor.RemoveValueChanged(listKillsNamesDataView, ItemsSourceChanged);
            }
        }

        private void killTextCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            CopyField();
        }

        public void CopyField()
        {
            if (killFieldText == null) return;
            string text = killFieldText.Text;
            if (text == null) return;
            Clipboard.SetText(text);
        }
    }
}
