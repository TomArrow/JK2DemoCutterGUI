using Microsoft.Win32;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using DemoCutterGUI.TableMappings;
using System.ComponentModel;

namespace DemoCutterGUI
{

    

    
    /// <summary>
    /// Interaction logic for DemoDatabaseExplorer.xaml
    /// </summary>
    public partial class DemoDatabaseExplorer : Window
    {

        SQLiteConnection dbConn = null;
        Mutex dbMutex = new Mutex();

        string currentStatus = "Idle";


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
            }
            SetStatusSafe(null);
            UpdateGUI();
        }

        bool connIsPrepared()
        {
            var r = dbConn.GetTableInfo("rets");
            var k = dbConn.GetTableInfo("kills");
            var ka = dbConn.GetTableInfo("killAngles");
            return r.Count > 0 && k.Count == 0 && ka.Count == 0;
        }

        void SetStatusSafe(string status = null)
        {
            this.currentStatus = status == null ? "Idle." : status;
            UpdateGUI(true);
        }


        private void updateGUIDispatcherReal(bool noDbLock)
        {
            statusTxt.Text = currentStatus;

            if (!noDbLock)
            {
                lock (dbMutex)
                {
                    bool dbConnExists = dbConn != null;
                    requiresOpenDbWrap.IsEnabled = dbConnExists;

                    if (dbConnExists)
                    {
                        if (connIsPrepared())
                        {
                            Object[] obj = new Object[] { };
                            List<Ret> res = dbConn.Query<Ret>("SELECT * FROM rets WHERE boostCountTotal>0 LIMIT 0,100") as List<Ret>;
                            Console.WriteLine(res);
                        }
                    }
                }
            }
        }
        void UpdateGUI(bool noDbLock = false)
        {
            Dispatcher.Invoke(() => {

                updateGUIDispatcherReal(noDbLock);
            }); 
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
                }
                UpdateGUI();
                SetStatusSafe(null);
            }
        }


        bool dbIsBeingPrepared = false;
        private void prepareBtn_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                if (dbIsBeingPrepared) return;
                lock (dbMutex)
                {
                    dbIsBeingPrepared = true;
                    var r = dbConn.GetTableInfo("rets");
                    var k = dbConn.GetTableInfo("kills");
                    var ka = dbConn.GetTableInfo("killAngles");
                    if (r.Count == 0 && k.Count > 0 && ka.Count > 0)
                    {
                        // We need to create a "rets" table
                        //dbConn.Execute("DELETE FROM killAngles WHERE isReturn=0 OR isSuicide=1");

                        // remove kills that are suicides
                        SetStatusSafe("Deleting suicides.");
                        dbConn.Execute("DELETE FROM killAngles WHERE isSuicide=1");

                        // Get rid of corresponding main kill entries
                        SetStatusSafe("Deleting kills with no kill angle.");
                        dbConn.Execute("DELETE FROM kills WHERE hash NOT IN (SELECT hash FROM killAngles)");

                        // Create rets table
                        SetStatusSafe("Creating merged kill table.");
                        dbConn.Execute("CREATE TABLE rets AS SELECT * FROM killAngles LEFT JOIN kills ON killAngles.hash=kills.hash");

                        // Remove kills and killAngles
                        SetStatusSafe("Deleting kills table.");
                        dbConn.Execute("DROP TABLE kills");
                        SetStatusSafe("Deleting killAngles table.");
                        dbConn.Execute("DROP TABLE killAngles");

                        // Compact the database
                        SetStatusSafe("Compacting database.");
                        dbConn.Execute("VACUUM");

                        string[] tablesToIndex = new string[] { "rets", "captures", "defragRuns", "killSprees", "laughs"/*,"playerDemoStats"*/ };
                        foreach (string table in tablesToIndex)
                        {
                            var fields = dbConn.GetTableInfo(table);
                            foreach (var field in fields)
                            {
                                SetStatusSafe($"Creating index for field {field.Name} in table {table}.");
                                dbConn.CreateIndex(table, field.Name, false);
                            }
                        }
                        SetStatusSafe(null);

                    }


                    Debug.WriteLine(r);
                    dbIsBeingPrepared = false;
                }
                UpdateGUI();
            });

            
        }
    }
}
