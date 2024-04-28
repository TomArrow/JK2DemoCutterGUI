using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        CombineCutter combineCutter;
        DemoDatabaseExplorer demoDatabaseExplorer;
        public MainWindow()
        {
            InitializeComponent();
            OpenExplorer();
            //OpenCutter();
            /*Thread ccThread = new Thread(() => {

                CombineCutter combineCutter = new CombineCutter();
                combineCutter.Show();
                combineCutter.Closed += SubWindow_Closed;
            });
            Thread ddeThread = new Thread(() => {

                DemoDatabaseExplorer demoDatabaseExplorer = new DemoDatabaseExplorer();
                demoDatabaseExplorer.Show();
                demoDatabaseExplorer.Closed += SubWindow_Closed;
            });

            ccThread.SetApartmentState(ApartmentState.STA);
            ddeThread.SetApartmentState(ApartmentState.STA);
            ccThread.IsBackground = true;
            ddeThread.IsBackground = true;

            ccThread.Start();
            ddeThread.Start();*/

            //this.WindowState = WindowState.Minimized;
            //this.Close();
        }

        private void SubWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void openExplorerBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenExplorer();
        }

        private void openCutterBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenCutter();
        }

        private void OpenExplorer()
        {

            demoDatabaseExplorer = new DemoDatabaseExplorer();
            demoDatabaseExplorer.Show();
            demoDatabaseExplorer.Closed += SubWindow_Closed;
            this.WindowState = WindowState.Minimized;
            this.Close();
        } 
        private void OpenCutter()
        {

            combineCutter = new CombineCutter();
            combineCutter.Show();
            combineCutter.Closed += SubWindow_Closed;
            this.WindowState = WindowState.Minimized;
            this.Close();
        }
    }
}
