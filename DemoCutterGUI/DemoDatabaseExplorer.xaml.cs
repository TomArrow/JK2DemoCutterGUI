using Microsoft.Win32;
using SQLite;
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
using System.Windows.Shapes;

namespace DemoCutterGUI
{
    /// <summary>
    /// Interaction logic for DemoDatabaseExplorer.xaml
    /// </summary>
    public partial class DemoDatabaseExplorer : Window
    {

        SQLiteConnection dbConn = null;
        Mutex dbMutex = new Mutex();

        // Anything needed if closing
        void CloseDatabase()
        {
            lock (dbMutex)
            {
                if (dbConn != null)
                {
                    dbConn.Close();
                    dbConn.Dispose();
                    dbConn = null;
                    dbNameTxt.Text = "[none]";
                }
                UpdateGUI();
            }
        }

        void UpdateGUI()
        {
            lock (dbMutex)
            {
                bool dbConnExists = dbConn != null;
                requiresOpenDbWrap.IsEnabled = dbConnExists;
            }
        }

        public DemoDatabaseExplorer()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SQLite database (*.db)|*.db|All files (*.*)|*.*";
            if(ofd.ShowDialog() == true)
            {
                lock (dbMutex)
                {
                    CloseDatabase();
                    dbConn = new SQLiteConnection(ofd.FileName, false);
                    dbNameTxt.Text = ofd.FileName;
                    UpdateGUI();
                }
            }
        }
    }
}
