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

namespace DemoCutterGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CombineCutter combineCutter = new CombineCutter();
        DemoDatabaseExplorer demoDatabaseExplorer = new DemoDatabaseExplorer();
        public MainWindow()
        {
            InitializeComponent();
            //combineCutter.Show();
            demoDatabaseExplorer.Show();
            demoDatabaseExplorer.Closed += SubWindow_Closed;
            this.WindowState = WindowState.Minimized;
            this.Close();
        }

        private void SubWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
