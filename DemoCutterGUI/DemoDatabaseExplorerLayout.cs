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
using System.Reflection;
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

    static class VisibleFieldsManager
    {

        static string gridfieldsConfigPath = "configs/gridFields.ini";
        static object gridFieldsConfigLock = new object();

        public static (Dictionary<DatabaseFieldInfo.FieldCategory,HashSet<string>>, List<string>) GetVisibleGridColumnsAndPresets(string presetName = "default")
        {
            List<string> visibleGridColumnsConfig = new List<string>();
            Dictionary<DatabaseFieldInfo.FieldCategory, HashSet<string>> visibleGridColumns = new Dictionary<DatabaseFieldInfo.FieldCategory, HashSet<string>>();
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
                    DatabaseFieldInfo.FieldCategory[] categories = (DatabaseFieldInfo.FieldCategory[])Enum.GetValues(typeof(DatabaseFieldInfo.FieldCategory));
                    foreach(DatabaseFieldInfo.FieldCategory category in categories)
                    {
                        string fields = cfg.GetValue(presetName, $"visibleGridFields{category.ToString()}");
                        visibleGridColumns[category] = new HashSet<string>();
                        if (fields != null)
                        {
                            visibleGridColumnsFound = true;
                            string[] fieldsArr = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            foreach (string field in fieldsArr)
                            {
                                visibleGridColumns[category].Add(field);
                            }
                        }
                    }
                }
            }
            return (visibleGridColumnsFound ? visibleGridColumns : null, visibleGridColumnsConfig);
        }
        public static (Dictionary<DatabaseFieldInfo.FieldCategory,HashSet<string>>, List<string>) SetVisibleGridColumnsAndPresets(string presetName, Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<string, bool>> columnVisibility)
        {
            Dictionary<DatabaseFieldInfo.FieldCategory, HashSet<string>> visibleGridColumns = new Dictionary<DatabaseFieldInfo.FieldCategory, HashSet<string>>();

            bool visibleGridColumnsFound = false;

            ConfigParser cfg = null;

            string targetConfigName = presetName;


            lock (gridFieldsConfigLock)
            {
                if (File.Exists(gridfieldsConfigPath))
                {
                    cfg = new ConfigParser(gridfieldsConfigPath);
                }
            }

            if (cfg == null)
            {
                cfg = new ConfigParser();
            }

            // Load existing cfg by this name if it exists, so we will only change visibility of columns that exist in the current db but not affect ones that might be relevant only for other dbs
            DatabaseFieldInfo.FieldCategory[] categories = (DatabaseFieldInfo.FieldCategory[])Enum.GetValues(typeof(DatabaseFieldInfo.FieldCategory));
            foreach (DatabaseFieldInfo.FieldCategory category in categories)
            {
                visibleGridColumns[category] = new HashSet<string>();
                if (columnVisibility.ContainsKey(category))
                {
                    string fields = cfg.GetValue(presetName, $"visibleGridFields{category.ToString()}");
                    if (fields != null)
                    {
                        visibleGridColumnsFound = true;
                        string[] fieldsArr = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        foreach (string field in fieldsArr)
                        {
                            visibleGridColumns[category].Add(field);
                        }
                    }

                    foreach (KeyValuePair<string, bool> col in columnVisibility[category])
                    {
                        if (col.Value)
                        {
                            visibleGridColumns[category].Add(col.Key);
                        }
                        else
                        {
                            visibleGridColumns[category].Remove(col.Key);
                        }
                    }

                    string columnsStringToSaveBack = string.Join(',', visibleGridColumns[category]);
                    cfg.SetValue(targetConfigName, $"visibleGridFields{category.ToString()}", columnsStringToSaveBack);
                }
            }

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

            return (visibleGridColumnsFound ? visibleGridColumns : null, visibleGridColumnsConfig);
        }
    }


    public struct SortingInfo
    {
        public string fieldName;
        public bool descending;
    }


    public partial class DemoDatabaseExplorer
    {

        bool layoutIsInitialized = false;
        DatabaseFieldInfo[] fieldInfoForSearch = DatabaseFieldInfo.GetDatabaseFieldInfos();


        Dictionary<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> categoryPanels = null;

        partial void Constructor()
        {
            categoryPanels = new Dictionary<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection>()
            {
                { DatabaseFieldInfo.FieldCategory.Rets, new CategoryInfoCollection(){  midPanel=retsMidPanel, sidePanel=retsSidePanel, tableName="rets", dataType=typeof(Ret)} },
                { DatabaseFieldInfo.FieldCategory.Captures, new CategoryInfoCollection(){  midPanel=capsMidPanel, sidePanel=capsSidePanel, tableName="captures", dataType=typeof(Capture)} }
            };

            // Register fields for monitoring whether they are active.
            foreach (DatabaseFieldInfo fieldInfo in fieldInfoForSearch)
            {
                fieldMan.RegisterFieldInfo(fieldInfo);
            }
            fieldMan.fieldInfoChanged += FieldMan_fieldInfoChanged;

            foreach(KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> panelSet in categoryPanels)
            {
                panelSet.Value.midPanel.sortingChanged += MidPanel_sortingChanged;
            }
        }
        partial void Destructor()
        {
            fieldMan.fieldInfoChanged -= FieldMan_fieldInfoChanged;

            foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> panelSet in categoryPanels)
            {
                panelSet.Value.midPanel.sortingChanged -= MidPanel_sortingChanged;
            }
        }


        private void FieldMan_fieldInfoChanged(object sender, DatabaseFieldInfo e)
        {
            //UpdateLayout();
            if (sqlTableItemsSources.ContainsKey(e.Category))
            {
                sqlTableItemsSources[e.Category]?.Reset();
                //retsItemSource?.Reset();
            }
        }


        struct CategoryInfoCollection {
            public DatabaseExplorerElements.MidPanel midPanel;
            public DatabaseExplorerElements.SidePanel sidePanel;
            public string tableName;
            public Type dataType;
        }

        Dictionary<DatabaseFieldInfo.FieldCategory, IDataVirtualizingCollection> sqlTableItemsSources = new Dictionary<DatabaseFieldInfo.FieldCategory, IDataVirtualizingCollection>();

        private void InitializeLayoutDispatcher()
        {
            lock (dbMutex)
            {
                if (!layoutIsInitialized)
                {

                    (Dictionary<DatabaseFieldInfo.FieldCategory, HashSet<string>> visibleGridColumns, List<string> visibleGridColumnsConfig) = VisibleFieldsManager.GetVisibleGridColumnsAndPresets("default");

                    bool defaultVisibleGridColumnsFound = visibleGridColumns != null;

                    visibleColumnsPresetComboBox.ItemsSource = visibleGridColumnsConfig.ToArray();
                    visibleColumnsPresetComboBox.SelectedValue = "default";



                    // Kills first.
                    //Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLite.SQLiteConnection.ColumnInfo>> sqlColumns = new Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLite.SQLiteConnection.ColumnInfo>>();
                    Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLite.SQLiteConnection.ColumnInfo>> sqlColumns = new Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLite.SQLiteConnection.ColumnInfo>>();
                    
                    //var retColumns = dbConn.GetTableInfo("rets");

                    

                    Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<DatabaseFieldInfo.FieldSubCategory, List<DatabaseFieldInfo>>> categorizedFieldInfos = new Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<DatabaseFieldInfo.FieldSubCategory, List<DatabaseFieldInfo>>>();


                    foreach(KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> associatedPanel in categoryPanels)
                    {
                        associatedPanel.Value.midPanel.TheGrid.Columns.Clear();
                        sqlColumns[associatedPanel.Key] =  dbConn.GetTableInfo(associatedPanel.Value.tableName);


                        foreach (var sqlColumn in sqlColumns[associatedPanel.Key])
                        {

                            // Find corresponding search field
                            foreach (DatabaseFieldInfo fieldInfo in fieldInfoForSearch)
                            {

                                if (!categorizedFieldInfos.ContainsKey(fieldInfo.Category))
                                {
                                    categorizedFieldInfos[fieldInfo.Category] = new Dictionary<DatabaseFieldInfo.FieldSubCategory, List<DatabaseFieldInfo>>();
                                }
                                if (!categorizedFieldInfos[fieldInfo.Category].ContainsKey(fieldInfo.SubCategory))
                                {
                                    categorizedFieldInfos[fieldInfo.Category][fieldInfo.SubCategory] = new List<DatabaseFieldInfo>();
                                }
                                if (!categorizedFieldInfos[fieldInfo.Category].ContainsKey(DatabaseFieldInfo.FieldSubCategory.All))
                                {
                                    categorizedFieldInfos[fieldInfo.Category][DatabaseFieldInfo.FieldSubCategory.All] = new List<DatabaseFieldInfo>();
                                }

                                if (sqlColumn.Name.Equals(fieldInfo.FieldName, StringComparison.OrdinalIgnoreCase) && associatedPanel.Key == fieldInfo.Category /*&& (fieldInfo.Category == "Kills" || fieldInfo.Category == "KillAngles")*/)
                                {
                                    categorizedFieldInfos[fieldInfo.Category][fieldInfo.SubCategory].Add(fieldInfo);
                                    categorizedFieldInfos[fieldInfo.Category][DatabaseFieldInfo.FieldSubCategory.All].Add(fieldInfo);

                                    break;
                                }

                            }
                        }
                    }


                    foreach(KeyValuePair<DatabaseFieldInfo.FieldCategory, Dictionary<DatabaseFieldInfo.FieldSubCategory, List<DatabaseFieldInfo>>> kvp in categorizedFieldInfos)
                    {
                        if (categoryPanels.ContainsKey(kvp.Key))
                        {
                            foreach (KeyValuePair<DatabaseFieldInfo.FieldSubCategory, List<DatabaseFieldInfo>> subcategory in kvp.Value)
                            {
                                if (subcategory.Key == DatabaseFieldInfo.FieldSubCategory.All)
                                {
                                    continue;
                                }
                                foreach(DatabaseFieldInfo fieldInfo in subcategory.Value)
                                {
                                    categoryPanels[kvp.Key].midPanel.TheGrid.Columns.Add(new DataGridTextColumn() { Header = fieldInfo.FieldName, Binding = new Binding(fieldInfo.FieldName), Visibility = (!defaultVisibleGridColumnsFound || visibleGridColumns[kvp.Key].Contains(fieldInfo.FieldName)) ? Visibility.Visible : Visibility.Collapsed });
                                }
                            }

                            if (kvp.Value.ContainsKey(DatabaseFieldInfo.FieldSubCategory.Column1))
                            {
                                categoryPanels[kvp.Key].midPanel.Items1 = kvp.Value[DatabaseFieldInfo.FieldSubCategory.Column1];
                            }
                            if (kvp.Value.ContainsKey(DatabaseFieldInfo.FieldSubCategory.Column2))
                            {
                                categoryPanels[kvp.Key].midPanel.Items2 = kvp.Value[DatabaseFieldInfo.FieldSubCategory.Column2];
                            }
                            if (kvp.Value.ContainsKey(DatabaseFieldInfo.FieldSubCategory.Column3))
                            {
                                categoryPanels[kvp.Key].midPanel.Items3= kvp.Value[DatabaseFieldInfo.FieldSubCategory.Column3];
                            }
                            if (kvp.Value.ContainsKey(DatabaseFieldInfo.FieldSubCategory.None))
                            {
                                categoryPanels[kvp.Key].midPanel.Items4= kvp.Value[DatabaseFieldInfo.FieldSubCategory.None];
                            }
                            // TODO Something to fix the names of the categories in sidepanel, now that we're using enums
                            categoryPanels[kvp.Key].sidePanel.Fields = kvp.Value[DatabaseFieldInfo.FieldSubCategory.All].ToArray();
                        }
                    }


                    //retsMidPanel.Items1 = categorizedFieldInfos[DatabaseFieldInfo.FieldCategory.Rets]["Names"].ToArray();
                    //retsMidPanel.Items2 = categorizedFieldInfos[DatabaseFieldInfo.FieldCategory.Rets]["Kill"].ToArray();
                    //retsMidPanel.Items3 = categorizedFieldInfos[DatabaseFieldInfo.FieldCategory.Rets]["Position"].ToArray();
                    //retsMidPanel.Items4 = categorizedFieldInfos[DatabaseFieldInfo.FieldCategory.Rets]["Rest"].ToArray();

                    //retsSidePanel.Fields = categorizedFieldInfos[new Tuple<string, string>("Rets", "_all_")].ToArray();
                    //retsSidePanel.Fields = categorizedFieldInfos[DatabaseFieldInfo.FieldCategory.Rets][DatabaseFieldInfo.FieldSubCategory.All].ToArray();
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

        Dictionary<DatabaseFieldInfo.FieldCategory, SortingInfo> sortingInfos = new Dictionary<DatabaseFieldInfo.FieldCategory, SortingInfo>();

        //private string sortField = null;
        //private bool sortDescending = false;

        private string MakeSelectQuery(DatabaseFieldInfo.FieldCategory category, bool countQuery = false)
        {
            if (!categoryPanels.ContainsKey(category)) return null;

            StringBuilder sb = new StringBuilder();

            if (countQuery)
            {
                sb.Append($"SELECT COUNT(*) FROM {categoryPanels[category].tableName}");
            }
            else
            {
                sb.Append($"SELECT * FROM {categoryPanels[category].tableName}");
            }

            DatabaseFieldInfo[] activeFields = fieldMan.getActiveFields();

            bool oneWhereDone = false;
            foreach (DatabaseFieldInfo field in activeFields)
            {
                if (field.Active && field.Category == category)
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

            if (!countQuery)
            {
                string sortFieldLocal = sortingInfos.ContainsKey(category) ? sortingInfos[category].fieldName : null;
                if (sortFieldLocal != null)
                {
                    sb.Append(" ORDER BY ");
                    sb.Append(sortFieldLocal);
                    if (sortingInfos[category].descending)
                    {
                        sb.Append(" DESC");
                    }
                    else
                    {
                        sb.Append(" ASC");
                    }
                }
            }

            return sb.ToString();
        }

        private void UpdateLayoutDispatcher()
        {
            lock (dbMutex)
            {
                InitializeLayoutDispatcher();

                //sqlTableItemsSources[DatabaseFieldInfo.FieldCategory.Rets] = CreateSQLItemsSource<Ret>(DatabaseFieldInfo.FieldCategory.Rets);
                //sqlTableItemsSources[DatabaseFieldInfo.FieldCategory.Captures] = CreateSQLItemsSource<Capture>(DatabaseFieldInfo.FieldCategory.Captures);

                foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> categoryData in categoryPanels)
                {
                    sqlTableItemsSources[categoryData.Key] = (IDataVirtualizingCollection)typeof(DemoDatabaseExplorer).GetMethod("CreateSQLItemsSource",BindingFlags.NonPublic| BindingFlags.Instance).MakeGenericMethod(categoryData.Value.dataType).Invoke(this,new object[] { categoryData.Key });
                    if (sqlTableItemsSources.ContainsKey(categoryData.Key))
                    {
                        categoryData.Value.midPanel.TheGrid.ItemsSource = sqlTableItemsSources[categoryData.Key];
                    }
                }

            }
        }

        private IDataVirtualizingCollection<T> CreateSQLItemsSource<T>(DatabaseFieldInfo.FieldCategory category) where T : new()
        {
            return DataVirtualizingCollectionBuilder
                    .Build<T>(
                        pageSize: 30,
                        notificationScheduler: new SynchronizationContextScheduler(new DispatcherSynchronizationContext(Application.Current.Dispatcher))
                        )
                    .NonPreloading()
                    .LeastRecentlyUsed(10, 5)
                    .NonTaskBasedFetchers(
                        pageFetcher: (offset, pageSize, ct) =>
                        {
                            //List<Ret> res = dbConn.Query<Ret>($"SELECT * FROM rets LIMIT {offset},{pageSize}") as List<Ret>;
                            List<T> res = dbConn.Query<T>($"{MakeSelectQuery(category, false)} LIMIT {offset},{pageSize}") as List<T>;
                            return res.ToArray();
                        },
                        countFetcher: (ct) =>
                        {
                            //int res = dbConn.ExecuteScalar<int>($"SELECT COUNT(*) FROM rets");
                            int res = dbConn.ExecuteScalar<int>($"{MakeSelectQuery(category, true)}");
                            return res;
                        })
                    .ThrottledLifoPageRequests().AsyncIndexAccess((a, b) => {
                        return new T();//{ hash = "Loading, please wait." };
                    });
            //.SyncIndexAccess();
        }



        private void MidPanel_sortingChanged(object sender, SortingInfo e)
        {
            DatabaseExplorerElements.MidPanel panel = (DatabaseExplorerElements.MidPanel)sender;
            DatabaseFieldInfo.FieldCategory category = DatabaseFieldInfo.FieldCategory.None;
            foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> categoryData in categoryPanels)
            {
                if(panel == categoryData.Value.midPanel)
                {
                    category = categoryData.Key;
                }
            }
            if(category != DatabaseFieldInfo.FieldCategory.None)
            {
                sortingInfos[category] = e;
            }
            //sortField = e.fieldName;
            //sortDescending = e.descending;
            sqlTableItemsSources[category].Reset();
            //retsItemSource.Reset();
        }

        /*private void retsGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            sortField = e.Column.SortMemberPath; 
            e.Column.SortDirection = e.Column.SortDirection == System.ComponentModel.ListSortDirection.Ascending ? System.ComponentModel.ListSortDirection.Descending : System.ComponentModel.ListSortDirection.Ascending;
            sortDescending = e.Column.SortDirection == System.ComponentModel.ListSortDirection.Ascending;
            retsItemSource.Reset();
            e.Handled = true;
        }*/


        private void visibleColumnsPresetLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            (Dictionary<DatabaseFieldInfo.FieldCategory, HashSet<string>> visibleGridColumns, List<string> visibleGridColumnsConfig) = VisibleFieldsManager.GetVisibleGridColumnsAndPresets(visibleColumnsPresetComboBox.Text);

            bool visibleGridColumnsFound = visibleGridColumns != null;


            if (visibleGridColumnsFound)
            {
                foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, HashSet<string>> categoryVisibleColumns in visibleGridColumns)
                {
                    if (categoryPanels.ContainsKey(categoryVisibleColumns.Key))
                    {
                        foreach (DataGridColumn col in categoryPanels[categoryVisibleColumns.Key].midPanel.TheGrid.Columns)
                        {
                            col.Visibility = visibleGridColumns[categoryVisibleColumns.Key].Contains(col.Header) ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            } else
            {
                MessageBox.Show("Specified preset not found.");
            }

            visibleColumnsPresetComboBox.ItemsSource = visibleGridColumnsConfig.ToArray();
            visibleColumnsPresetComboBox.SelectedValue = "default";
        }

        private void visibleColumnsPresetSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<string, bool>> columnVisibility = new Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<string, bool>>();

            foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> categoryPanel in categoryPanels)
            {
                columnVisibility[categoryPanel.Key] = new Dictionary<string, bool>();
                foreach (var col in categoryPanel.Value.midPanel.TheGrid.Columns)
                {
                    columnVisibility[categoryPanel.Key][col.Header.ToString()] = col.Visibility == Visibility.Visible;
                }

            }

            (Dictionary<DatabaseFieldInfo.FieldCategory, HashSet<string>> visibleGridColumns, List<string> visibleGridColumnsConfig) = VisibleFieldsManager.SetVisibleGridColumnsAndPresets(visibleColumnsPresetComboBox.Text, columnVisibility);

            bool visibleGridColumnsFound = visibleGridColumns != null;


            visibleColumnsPresetComboBox.ItemsSource = visibleGridColumnsConfig.ToArray();
            visibleColumnsPresetComboBox.SelectedValue = "default";

        }
    }

}
