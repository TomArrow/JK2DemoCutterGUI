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
using DemoCutterGUI.DatabaseExplorerElements;
using System.Globalization;
using System.Text.Json;

namespace DemoCutterGUI
{

    

    
    /// <summary>
    /// Interaction logic for DemoDatabaseExplorer.xaml
    /// </summary>
    public partial class DemoDatabaseExplorer : Window
    {
        partial void Constructor();
        partial void Constructor2(); // Yes it's extremely cringe lol. I misunderstood the concept at first and now I'm too lazy to find a more sensible approach
        partial void Constructor3();
        partial void Constructor4();
        partial void Constructor5();
        partial void Constructor6();
        partial void Constructor7();
        partial void Constructor8();
        partial void Destructor();
        partial void Destructor2();
        partial void Destructor3();
        partial void Destructor4();
        partial void Destructor5();
        partial void Destructor6();
        partial void Destructor7();
        partial void Destructor8();

        SQLiteConnection dbConn = null;
        Mutex dbMutex = new Mutex();

        string currentStatus = "Idle";

        DataBaseFieldInfoManager fieldMan = new DataBaseFieldInfoManager();


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
                layoutIsInitialized = false;
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
                UpdateLayout();
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
            /*CopyFieldCommand = new DoSomethingCommand(() => {
                this.CopyField();
            });*/
            this.DataContext = this;
            InitializeComponent();
            Constructor();
            Constructor2();
            Constructor3();
            Constructor4();
            Constructor5();
            Constructor6();
            Constructor7();
            Constructor8();
            this.Closed += DemoDatabaseExplorer_Closed;
        }

        private void DemoDatabaseExplorer_Closed(object sender, EventArgs e)
        {
            CloseDown();
        }

        ~DemoDatabaseExplorer()
        {
            CloseDown();
        }

        object closeDownLock = new object();
        bool closedDown = false;

        private void CloseDown()
        {
            lock (closeDownLock)
            {
                if (!closedDown)
                {
                    this.Closed -= DemoDatabaseExplorer_Closed;
                    Destructor();
                    Destructor2();
                    Destructor3();
                    Destructor4();
                    Destructor5();
                    Destructor6();
                    Destructor7();
                    Destructor8();
                }
                closedDown = true;
            }
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

        //private void killTextCopyBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    CopyField();
        //}

        public void CopyField()
        {
            TabItem tab = (TabItem)sidePanelTabs.SelectedItem;
            if (tab == null) return;
            SidePanel currentSidePanel = (SidePanel)tab.Content;
            if (currentSidePanel == null) return;
            currentSidePanel.CopyField();
        }

        private ICommand copyFieldCommand = null;
        public ICommand CopyFieldCommand { get {
                return copyFieldCommand ?? (copyFieldCommand = new DoSomethingCommand(()=> { this.CopyField(); }));
            } 
        }

        private void bspToMinimapBtn_Click(object sender, RoutedEventArgs e)
        {
            float resMultiplier = 1.0f;
            string multText = bspToMinimapResolutionMultiplierText.Text;
            if (!string.IsNullOrWhiteSpace(multText) && !float.TryParse(multText.Replace(',','.'),NumberStyles.Any, CultureInfo.InvariantCulture, out resMultiplier))
            {
                resMultiplier = 1.0f;
            }
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "BSP maps (*.bsp)|*.bsp";
            if(ofd.ShowDialog() == true)
            {
                StringBuilder errors = new StringBuilder();
                foreach (string filename in ofd.FileNames)
                {
                    try
                    {
                        Tools.BSPToMiniMap.MakeMiniMap(filename, 0.1f * resMultiplier, (int)(4000.0f * resMultiplier), (int)(4000.0f * resMultiplier));
                    }
                    catch (Exception ex)
                    {
                        errors.Append($"Error making minimap for {filename}: {ex.ToString()}\n");
                    }
                }
                if(errors.Length > 0)
                {
                    MessageBox.Show(errors.ToString());
                }
            }
        }

        private void demoMetaShowBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "json";
            ofd.Filter = "Supported demo files (*.dm_14;*.dm_15;*.dm_16;*.dm_25;*.dm_26;*.dm_66;*.dm_67;*.dm_68)|*.dm_14;*.dm_15;*.dm_16;*.dm_25;*.dm_26;*.dm_66;*.dm_67;*.dm_68|All files (*.*)|*.*";
            if (ofd.ShowDialog() != true)
            {
                return;
            }
            string jsonMetaData = HiddenMetaStuff.getMetaDataFromDemoFile(ofd.FileName);

            if (jsonMetaData == null)
            {
                MessageBox.Show("No metadata found.");
                return;
            }
            JsonSerializerOptions opts = new JsonSerializerOptions();
            opts.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals | System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
            try
            {

                DemoJSONMeta deSerialized = JsonSerializer.Deserialize<DemoJSONMeta>(jsonMetaData, opts);
                //selectedDemos[0].loadDataFromMeta(deSerialized);
                //selectedDemos[0].name = Path.GetFileName(ofd.FileName);
                MessageBox.Show(jsonMetaData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing JSON metadata of demo: {ex.ToString()}");
            }
        }
    }

    public class DoSomethingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action stuffToDo = null;

        public DoSomethingCommand(Action stuffToDoA)
        {
            if(stuffToDoA == null)
            {
                throw new InvalidOperationException("DoSomethingCommand must be passed a non-null Action");
            }
            stuffToDo = stuffToDoA;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            stuffToDo();
        }
    }
}
