using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using DemoCutterGUI.TableMappings;
using Salaros.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DemoCutterGUI
{
    public class VisibilityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class PropertyPathOnObjectToValueConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }
            if (value.Length == 2 && (value[0] == null || value[0] == DependencyProperty.UnsetValue || value[1] == null || value[1] == DependencyProperty.UnsetValue))
            {
                return null;
            }
            if(value.Length == 3 && (value[0] == null || value[0] == DependencyProperty.UnsetValue || ((value[2] == null || value[2] == DependencyProperty.UnsetValue) && (value[1] == null || value[1] == DependencyProperty.UnsetValue))))
            {
                return null;
            }
            var property = value[0].GetType().GetProperty((value.Length == 3 && value[2] != null && value[2] != DependencyProperty.UnsetValue) ? (string)value[2] : (string)value[1]);
            var retVal= property.GetValue(value[0], null);
            if(targetType == typeof(bool?))
            {

                if (retVal is string)
                {
                    return null;
                } else if (retVal is bool)
                {
                    return (bool?)(bool)retVal;
                } else if (retVal is Int64 || retVal is int )
                {
                    bool? retBool = (bool?)((Int64)retVal != 0.0);
                    return retBool;
                } else if (retVal is float || retVal is double)
                {
                    bool? retBool = (bool?)((double)retVal != 0);
                    return retBool;
                }
            }
            retVal = retVal == null ? null : System.Convert.ChangeType(retVal, targetType);
            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class DemoDatabaseExplorer
    {


        bool layoutIsInitialized = false;
        DatabaseFieldInfo[] fieldInfoForSearch = DatabaseFieldInfo.GetDatabaseFieldInfos();

        static string gridfieldsConfigPath = "configs/gridFields.ini";
        static object gridFieldsConfigLock = new object();



        partial void Constructor()
        {
            // Register fields for monitoring whether they are active.
            foreach(DatabaseFieldInfo fieldInfo in fieldInfoForSearch)
            {
                fieldMan.RegisterFieldInfo(fieldInfo);
            }
            fieldMan.fieldInfoChanged += FieldMan_fieldInfoChanged;
        }

        private void FieldMan_fieldInfoChanged(object sender, DatabaseFieldInfo e)
        {
            //UpdateLayout();
            retsItemSource?.Reset();
        }

        IDataVirtualizingCollection<Ret> retsItemSource = null;
        private void InitializeLayoutDispatcher()
        {
            lock (dbMutex)
            {
                if (!layoutIsInitialized)
                {
                    List<string> visibleGridColumnsConfig = new List<string>();
                    HashSet<string> visibleGridColumns = new HashSet<string>();
                    bool defaultVisibleGridColumnsFound = false;
                    lock (gridFieldsConfigLock)
                    {
                        if (File.Exists(gridfieldsConfigPath))
                        {

                            ConfigParser cfg = new ConfigParser(gridfieldsConfigPath);
                            foreach(ConfigSection section in cfg.Sections)
                            {
                                visibleGridColumnsConfig.Add(section.SectionName);
                            }
                            string fields = cfg.GetValue("default","visibleGridFieldsRet");
                            if(fields != null)
                            {
                                defaultVisibleGridColumnsFound = true;
                                string[] fieldsArr = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                foreach (string field in fieldsArr)
                                {
                                    visibleGridColumns.Add(field);
                                }
                            }
                        }
                    }

                    visibleRetsColumnsComboBox.ItemsSource = visibleGridColumnsConfig.ToArray();
                    visibleRetsColumnsComboBox.SelectedValue = "default";

                    //fieldInfoForSearch = DatabaseFieldInfo.GetDatabaseFieldInfos();


                    Dictionary<string, Dictionary<string, GroupBox>> groupBoxes = new Dictionary<string, Dictionary<string, GroupBox>>()
                    {
                        { "Rets", new Dictionary<string, GroupBox>(){ 
                            { "Names", new GroupBox(){ Header="Meta",Content=new StackPanel() } },
                            { "Kill", new GroupBox(){ Header="Kill",Content=new StackPanel() } },
                            { "Position", new GroupBox(){ Header="Movement",Content=new StackPanel() } } 
                        } },
                    };

                    // Kills first.
                    var retColumns = dbConn.GetTableInfo("rets");
                    Dictionary<Tuple<string,string>, List<DatabaseFieldInfo>> categorizedFieldInfos = new Dictionary<Tuple<string, string>, List<DatabaseFieldInfo>>()
                    {
                        { new Tuple<string,string>("Rets","_all_"),new List<DatabaseFieldInfo>() },
                        { new Tuple<string,string>("Rets","Names"),new List<DatabaseFieldInfo>() },
                        { new Tuple<string,string>("Rets","Kill"),new List<DatabaseFieldInfo>() },
                        { new Tuple<string,string>("Rets","Position"),new List<DatabaseFieldInfo>() },
                        { new Tuple<string,string>("Rets","Rest"),new List<DatabaseFieldInfo>() },
                    };
                    //Dictionary<string, List<DataGridTextColumn>> dataGridCategorizedFieldInfos = new Dictionary<string, List<DataGridTextColumn>>()
                    //{
                    //    {"Rets",new List<DataGridTextColumn>() }
                    //};


                    //displayKill.Children.Clear();
                    retsGrid.Columns.Clear();
                    foreach(var retColumn in retColumns)
                    {

                        // Find corresponding search field
                        foreach (DatabaseFieldInfo fieldInfo in fieldInfoForSearch)
                        {
                            if (retColumn.Name.Equals(fieldInfo.FieldName, StringComparison.OrdinalIgnoreCase) && (fieldInfo.Category == "Kills" || fieldInfo.Category == "KillAngles"))
                            {
                                switch (fieldInfo.SubCategory)
                                {
                                    case "Names":
                                    case "Kill":
                                    case "Position":
                                        categorizedFieldInfos[new Tuple<string, string>("Rets", fieldInfo.SubCategory)].Add(fieldInfo);
                                        break;
                                    default:
                                        categorizedFieldInfos[new Tuple<string, string>("Rets", "Rest")].Add(fieldInfo);
                                        break;
                                }
                                categorizedFieldInfos[new Tuple<string, string>("Rets", "_all_")].Add(fieldInfo);
                                retsGrid.Columns.Add(new DataGridTextColumn() { Header = fieldInfo.FieldName, Binding = new Binding(fieldInfo.FieldName), Visibility= (!defaultVisibleGridColumnsFound || visibleGridColumns.Contains( fieldInfo.FieldName ))? Visibility.Visible : Visibility.Collapsed });
                                /*
                                if (!groupBoxes["Rets"].ContainsKey(fieldInfo.SubCategory))
                                {
                                    groupBoxes["Rets"][fieldInfo.SubCategory] = new GroupBox() { Header=fieldInfo.SubCategory, Content = new StackPanel() };
                                }
                                
                                var newTextBox = new TextBox();
                                newTextBox.SetBinding(TextBox.TextProperty,new Binding(fieldInfo.FieldName));
                                (groupBoxes["Rets"][fieldInfo.SubCategory].Content as StackPanel).Children.Add(newTextBox);*/
                                break;
                            }

                        }
                    }

                    /*foreach(var box in groupBoxes["Rets"])
                    {
                        displayKill.Children.Add(box.Value);
                    }*/

                    // Apply search fields
                    listKillsNames.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Names")].ToArray();
                    listKillsKill.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Kill")].ToArray();
                    listKillsPosition.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Position")].ToArray();
                    listKillsRest.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Rest")].ToArray();

                    retsSidePanel.Fields = categorizedFieldInfos[new Tuple<string, string>("Rets", "_all_")].ToArray();

                    //listKillsNamesDataView.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Names")].ToArray();
                    //listKillsKillDataView.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Kill")].ToArray();
                    //listKillsMovementDataView.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Position")].ToArray();
                    //listKillsRest.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Rest")].ToArray();
                }
                layoutIsInitialized = true;
            }
        }

        private void UpdateLayout()
        {
            bool doUpdateLayout = false;
            lock (dbMutex)
            {
                bool dbConnExists = dbConn != null;
                requiresOpenDbWrap.IsEnabled = dbConnExists;

                if (dbConnExists)
                {
                    if (connIsPrepared())
                    {
                        doUpdateLayout = true;
                    }
                }
            }
            if (doUpdateLayout)
            {
                Dispatcher.Invoke(UpdateLayoutDispatcher);
            }
        }

        private string sortField = null;
        private bool sortDescending = false;

        private string MakeSelectQuery(bool countQuery = false)
        {
            StringBuilder sb = new StringBuilder();

            if (countQuery)
            {
                sb.Append("SELECT COUNT(*) FROM rets");
            }
            else
            {
                sb.Append("SELECT * FROM rets");
            }

            DatabaseFieldInfo[] activeFields = fieldMan.getActiveFields();

            bool oneWhereDone = false;
            foreach (DatabaseFieldInfo field in activeFields)
            {
                if (field.Active)
                {
                    sb.Append(oneWhereDone ? " AND " : " WHERE ");
                    oneWhereDone = true;
                    if (field.IsNull)
                    {
                        sb.Append($"{field.FieldName} ");
                        sb.Append("IS NULL");
                    }
                    else if (field.Bool)
                    {
                        sb.Append(field.BoolContent ? "" : "NOT ");
                        sb.Append($"{field.FieldName} ");
                    }
                    else if (string.IsNullOrEmpty(field.Content))
                    {
                        sb.Append($"{field.FieldName} ");
                        sb.Append("IS NOT NULL");
                    }
                    else
                    {
                        sb.Append($"{field.FieldName} ");
                        //sb.Append($"='");
                        sb.Append($"LIKE '%");
                        sb.Append(field.Content.Replace("'","''"));
                        sb.Append($"%'");
                    }
                }
            }
            string sortFieldLocal = sortField;
            if(sortFieldLocal != null)
            {
                sb.Append(" ORDER BY ");
                sb.Append(sortFieldLocal);
                if (sortDescending)
                {
                    sb.Append(" DESC");
                } else
                {
                    sb.Append(" ASC");
                }
            }

            return sb.ToString();
        }

        private void UpdateLayoutDispatcher()
        {
            lock (dbMutex)
            {
                InitializeLayoutDispatcher();

                retsItemSource =
                    DataVirtualizingCollectionBuilder
                        .Build<Ret>(
                            pageSize: 30,
                            notificationScheduler: new SynchronizationContextScheduler(new DispatcherSynchronizationContext(Application.Current.Dispatcher))
                            )
                        .NonPreloading()
                        .LeastRecentlyUsed(10, 5)
                        .NonTaskBasedFetchers(
                            pageFetcher: (offset, pageSize, ct) =>
                            {
                                //List<Ret> res = dbConn.Query<Ret>($"SELECT * FROM rets LIMIT {offset},{pageSize}") as List<Ret>;
                                List<Ret> res = dbConn.Query<Ret>($"{MakeSelectQuery(false)} LIMIT {offset},{pageSize}") as List<Ret>;
                                return res.ToArray();
                            },
                            countFetcher: (ct) =>
                            {
                                //int res = dbConn.ExecuteScalar<int>($"SELECT COUNT(*) FROM rets");
                                int res = dbConn.ExecuteScalar<int>($"{MakeSelectQuery(true)}");
                                return res;
                            })
                        .ThrottledLifoPageRequests().AsyncIndexAccess((a,b)=> {
                            return new Ret() { hash="Loading, please wait."};
                        });
                        //.SyncIndexAccess();
                
                retsGrid.ItemsSource = retsItemSource;

            }
        }




        private void retsGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            sortField = e.Column.SortMemberPath; 
            e.Column.SortDirection = e.Column.SortDirection == System.ComponentModel.ListSortDirection.Ascending ? System.ComponentModel.ListSortDirection.Descending : System.ComponentModel.ListSortDirection.Ascending;
            sortDescending = e.Column.SortDirection == System.ComponentModel.ListSortDirection.Ascending;
            retsItemSource.Reset();
            e.Handled = true;
        }


        private void visibleRetsColumnsLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> visibleGridColumnsConfig = new List<string>();
            HashSet<string> visibleGridColumns = new HashSet<string>();
            bool visibleGridColumnsFound = false;
            lock (gridFieldsConfigLock)
            {
                if (File.Exists(gridfieldsConfigPath))
                {

                    ConfigParser cfg = new ConfigParser(gridfieldsConfigPath);
                    foreach (ConfigSection section in cfg.Sections)
                    {
                        visibleGridColumnsConfig.Add(section.SectionName);
                    }
                    string fields = cfg.GetValue(visibleRetsColumnsComboBox.Text, "visibleGridFieldsRet");
                    if (fields != null)
                    {
                        visibleGridColumnsFound = true;
                        string[] fieldsArr = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        foreach (string field in fieldsArr)
                        {
                            visibleGridColumns.Add(field);
                        }
                    }
                }
            }

            if (visibleGridColumnsFound)
            {
                foreach(var col in retsGrid.Columns)
                {
                    col.Visibility = visibleGridColumns.Contains(col.Header) ? Visibility.Visible : Visibility.Collapsed;
                }
            } else
            {
                MessageBox.Show("Specified preset not found.");
            }

            visibleRetsColumnsComboBox.ItemsSource = visibleGridColumnsConfig.ToArray();
            visibleRetsColumnsComboBox.SelectedValue = "default";
        }

        private void visibleRetsColumnsSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            HashSet<string> visibleGridColumns = new HashSet<string>();

            bool visibleGridColumnsFound = false;

            ConfigParser cfg = null;

            string targetConfigName = visibleRetsColumnsComboBox.Text;


            lock (gridFieldsConfigLock)
            {
                if (File.Exists(gridfieldsConfigPath))
                {
                    cfg = new ConfigParser(gridfieldsConfigPath);
                }
            }

            if(cfg == null)
            {
                cfg = new ConfigParser();
            }

            // Load existing cfg by this name if it exists, so we will only change visibility of columns that exist in the current db but not affect ones that might be relevant only for other dbs
            string fields = cfg.GetValue(targetConfigName, "visibleGridFieldsRet");
            if (fields != null)
            {
                visibleGridColumnsFound = true;
                string[] fieldsArr = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string field in fieldsArr)
                {
                    visibleGridColumns.Add(field);
                }
            }

            foreach (var col in retsGrid.Columns)
            {
                if (col.Visibility == Visibility.Visible)
                {
                    visibleGridColumns.Add(col.Header.ToString());
                } else
                {
                    visibleGridColumns.Remove(col.Header.ToString());
                }
            }

            string columnsStringToSaveBack = string.Join(',', visibleGridColumns);
            cfg.SetValue(targetConfigName, "visibleGridFieldsRet",columnsStringToSaveBack);

            lock (gridFieldsConfigLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(gridfieldsConfigPath));
                cfg.Save(gridfieldsConfigPath);
            }

            List<string> visibleGridColumnsConfig = new List<string>();
            foreach (ConfigSection section in cfg.Sections)
            {
                visibleGridColumnsConfig.Add(section.SectionName);
            }

            visibleRetsColumnsComboBox.ItemsSource = visibleGridColumnsConfig.ToArray();
            visibleRetsColumnsComboBox.SelectedValue = "default";
        }
    }

}
