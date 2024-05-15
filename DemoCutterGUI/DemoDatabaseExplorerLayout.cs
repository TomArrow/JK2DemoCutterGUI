using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using DemoCutterGUI.DatabaseExplorerElements;
using DemoCutterGUI.TableMappings;
using OpenTK.Wpf;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Collections.Concurrent;
using StbImageSharp;
using DemoCutterGUI.Tools;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemoCutterGUI
{
    public class MiniMapPointLogical
    {
        public struct PersistVector3{
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
        }
        [JsonPropertyName("position")]
        public PersistVector3 persistPosition
        {
            get
            {
                return new PersistVector3() { X=position.X,Y=position.Y,Z=position.Z };
            }
            set
            {
                position = new Vector3() { X = value.X, Y = value.Y, Z = value.Z };

            }
        }
        public Vector3 position;
        public int index { get; set; }
        public string note { get; set; }
        public Ret ret { get; set; } = null;
        public object getObject()
        {
            return ret;
        }
    }
    public class DemoDatabaseProperties
    {
        public bool serverNameInKillAngles = false;
        public bool serverNameInKillSpree = false;
    }
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
            if(property == null)
            {
                return null;
            }
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

    public static class SavedPointsManager
    {

        public class SavedPoints
        {
            public MiniMapPointLogical[] points { get; set; }
        }

        static string savedPointsConfigPath = "configs/savedPoints";
        static object savedPointsConfigLock = new object();

        public static string[] GetListOfSavedPointsPresets()
        {
            if (!Directory.Exists(savedPointsConfigPath)) return new string[] { "default" };
            string[] files = Directory.GetFiles(savedPointsConfigPath, "*.json");
            List<string> presets = new List<string>();
            foreach (string file in files)
            {
                presets.Add(Path.GetFileNameWithoutExtension(file));
            }
            return presets.ToArray();
        }

        /*public static Vector3[] GetSavedPointsIni(string presetName = "default")
        {
            string filePath = Path.Combine(savedPointsConfigPath, presetName) + ".ini";
            List<Vector3> points = new List<Vector3>();
            lock (savedPointsConfigLock)
            {
                if (File.Exists(filePath))
                {
                    ConfigParser cfg = new ConfigParser(filePath);

                    string pointStringsString = cfg.GetValue("points", "points", "");
                    string[] pointStrings = pointStringsString.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (pointStrings is null || pointStrings.Length == 0) return null;
                    foreach (string pointString in pointStrings)
                    {
                        string[] floatParts = pointString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (floatParts.Length >= 3)
                        {
                            Vector3 position = new Vector3();
                            bool success = float.TryParse(floatParts[0], out position.X);
                            success = success && float.TryParse(floatParts[1], out position.Y);
                            success = success && float.TryParse(floatParts[2], out position.Z);
                            if (success)
                            {
                                points.Add(position);
                            }
                        }
                    }
                    if (points.Count > 0)
                    {
                        return points.ToArray();
                    }
                }
            }
            return null;
        }*/
        static JsonSerializerOptions opts = new JsonSerializerOptions()
        {NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals | System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString };

        public static MiniMapPointLogical[] GetSavedPoints(string presetName = "default")
        {
            string filePath = Path.Combine(savedPointsConfigPath, presetName)+".json";
            List<MiniMapPointLogical> points = new List<MiniMapPointLogical>();
            lock (savedPointsConfigLock)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        SavedPoints retVal = JsonSerializer.Deserialize<SavedPoints>(File.ReadAllText(filePath),opts);
                        if (retVal is null) return null;
                        return retVal.points;
                    } catch(Exception e)
                    {
                        // 
                        return null;
                    }
                }
            }
            return null;
        }
        public static void SetSavedPoints(string presetName, MiniMapPointLogical[] points)
        {
            if (points is null || points.Length == 0) return;

            SavedPoints savedPoints = null;

            string filePath = Path.Combine(savedPointsConfigPath, presetName) + ".json";

            lock (savedPointsConfigLock)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        savedPoints = JsonSerializer.Deserialize<SavedPoints>(File.ReadAllText(filePath), opts);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            if (savedPoints == null)
            {
                savedPoints = new SavedPoints();
            }

            savedPoints.points = points;


            lock (savedPointsConfigLock)
            {
                Directory.CreateDirectory(savedPointsConfigPath);
                try
                {
                    string jsonText = JsonSerializer.Serialize(savedPoints, opts);
                    File.WriteAllText(filePath, jsonText);
                }catch(Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

        }
        /*
        public static void SetSavedPointsIni(string presetName, Vector3[] points)
        {
            if (points is null || points.Length == 0) return;

            ConfigParser cfg = null;

            string filePath = Path.Combine(savedPointsConfigPath, presetName) + ".ini";

            lock (savedPointsConfigLock)
            {
                if (File.Exists(filePath))
                {
                    cfg = new ConfigParser(filePath);
                }
            }

            if (cfg == null)
            {
                cfg = new ConfigParser();
            }

            List<string> pointStrings = new List<string>();

            foreach(var point in points)
            {
                pointStrings.Add($"{point.X.ToString()};{point.Y.ToString()};{point.Z.ToString()}");
            }

            string pointStringsString = string.Join('|', pointStrings);

            cfg.SetValue("points","points", pointStringsString);

            lock (savedPointsConfigLock)
            {
                Directory.CreateDirectory(savedPointsConfigPath);
                cfg.Save(filePath);
            }

        }*/
    }


    public struct SortingInfo
    {
        public string fieldName;
        public bool descending;
    }


    public partial class DemoDatabaseExplorer
    {

        DemoDatabaseProperties dbProperties = new DemoDatabaseProperties();

        bool layoutIsInitialized = false;
        DatabaseFieldInfo[] fieldInfoForSearch = DatabaseFieldInfo.GetDatabaseFieldInfos();


        Dictionary<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> categoryPanels = null;
        Dictionary<DatabaseFieldInfo.FieldCategory, CategorySQLQueryInfo> categorySQLQueryData = new Dictionary<DatabaseFieldInfo.FieldCategory, CategorySQLQueryInfo>();

        partial void Constructor()
        {
            InitMiniMap();
            categoryPanels = new Dictionary<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection>()
            {
                { DatabaseFieldInfo.FieldCategory.Rets, new CategoryInfoCollection(){  midPanel=retsMidPanel, sidePanel=retsSidePanel, tableName="rets", dataType=typeof(Ret)} },
                { DatabaseFieldInfo.FieldCategory.Captures, new CategoryInfoCollection(){  midPanel=capsMidPanel, sidePanel=capsSidePanel, tableName="captures", dataType=typeof(TableMappings.Capture)} },
                { DatabaseFieldInfo.FieldCategory.KillSprees, new CategoryInfoCollection(){  midPanel=killSpreesMidPanel, sidePanel=killSpreesSidePanel, tableName="killSprees", dataType=typeof(KillSpree)} },
                { DatabaseFieldInfo.FieldCategory.DefragRuns, new CategoryInfoCollection(){  midPanel=defragMidPanel, sidePanel=defragSidePanel, tableName="defragRuns", dataType=typeof(DefragRun)} },
                { DatabaseFieldInfo.FieldCategory.Laughs, new CategoryInfoCollection(){  midPanel=laughsMidPanel, sidePanel=laughsSidePanel, tableName="laughs", dataType=typeof(Laughs)} }
            };
            
            foreach(KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> kvp in categoryPanels)
            {
                categorySQLQueryData[kvp.Key] = new CategorySQLQueryInfo();
                //kvp.Value.midPanel.TheGrid.Loaded += TheGrid_Loaded;
            }

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

        MiniMapRenderer miniMapRenderer = null;

        ConcurrentDictionary<MiniMapRendererWindow,int> miniMapWindows = new ConcurrentDictionary<MiniMapRendererWindow,int>(); // using dictionary as it allows us to easily remove stuff again which concurrentbag doesnt. dumb, ik.

        void InitMiniMap()
        {
            miniMapRenderer = new MiniMapRenderer(OpenTkControl);

        }


        private void newMiniMapWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            MiniMapRendererWindow wnd = new MiniMapRendererWindow();
            miniMapWindows.TryAdd(wnd,1);
            wnd.Closed += Wnd_Closed;
            wnd.RangeAppliedButtonPressed += Wnd_RangeAppliedButtonPressed;
            wnd.MiniMapPointEdited += Wnd_MiniMapPointEdited;
            wnd.Show();
        }

        private void Wnd_MiniMapPointEdited(object sender, EventArgs e)
        {
            UpdateMiniMap();
        }

        private void Wnd_RangeAppliedButtonPressed(object sender, EventArgs e)
        {
            MiniMapRendererWindow wnd = sender as MiniMapRendererWindow;
            if (wnd is null) return;
            Vector3[] minMaxs = wnd.miniMapRenderer.getDragMinMaxs();
            ApplyMinMaxsRange(minMaxs);
        }

        private void Wnd_Closed(object sender, EventArgs e)
        {
            MiniMapRendererWindow wnd = sender as MiniMapRendererWindow;
            if (wnd is null) return;
            miniMapWindows.TryRemove(wnd, out _); 
            wnd.Closed -= Wnd_Closed;
            wnd.RangeAppliedButtonPressed -= Wnd_RangeAppliedButtonPressed;
            wnd.MiniMapPointEdited -= Wnd_MiniMapPointEdited;
        }

        partial void Destructor()
        {
            fieldMan.fieldInfoChanged -= FieldMan_fieldInfoChanged;

            foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> panelSet in categoryPanels)
            {
                panelSet.Value.midPanel.sortingChanged -= MidPanel_sortingChanged;
            }
        }
        
        private void updateMinimapBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateMiniMap();
        }


        private void resetMinimapBtn_Click(object sender, RoutedEventArgs e)
        {

            miniMapRenderer.ResetView();
        }

        private void applyMinimapRangeBtn_Click(object sender, RoutedEventArgs e)
        {
            Vector3[] minMaxs = miniMapRenderer.getDragMinMaxs();
            ApplyMinMaxsRange(minMaxs);
        }

        private void ApplyMinMaxsRange(Vector3[] minMaxs)
        {
            DatabaseFieldInfo.FieldCategory? category = GetActiveTabCategory();
            if (category != DatabaseFieldInfo.FieldCategory.Rets || minMaxs is null) return;
            //
            foreach (var fieldInfo in fieldInfoForSearch)
            {
                if (fieldInfo.Category == category && fieldInfo.FieldName == "positionX")
                {
                    if (!float.IsInfinity(minMaxs[0].X) && !float.IsInfinity(minMaxs[1].X) && !float.IsNaN(minMaxs[0].X) && !float.IsNaN(minMaxs[1].X))
                    {
                        fieldInfo.Content = minMaxs[0].X.ToString("0.##") + "-" + minMaxs[1].X.ToString("0.##");
                        fieldInfo.Active = true;
                    }
                }
                else if (fieldInfo.Category == category && fieldInfo.FieldName == "positionY")
                {

                    if (!float.IsInfinity(minMaxs[0].Y) && !float.IsInfinity(minMaxs[1].Y) && !float.IsNaN(minMaxs[0].Y) && !float.IsNaN(minMaxs[1].Y))
                    {
                        fieldInfo.Content = minMaxs[0].Y.ToString("0.##") + "-" + minMaxs[1].Y.ToString("0.##");
                        fieldInfo.Active = true;
                    }
                }
                else if (fieldInfo.Category == category && fieldInfo.FieldName == "positionZ")
                {

                    if (!float.IsInfinity(minMaxs[0].Z) && !float.IsInfinity(minMaxs[1].Z) && !float.IsNaN(minMaxs[0].Z) && !float.IsNaN(minMaxs[1].Z))
                    {
                        fieldInfo.Content = minMaxs[0].Z.ToString("0.##") + "-" + minMaxs[1].Z.ToString("0.##");
                        fieldInfo.Active = true;
                    }
                }
            }
        }

        List<MiniMapPointLogical> currentPoints = new List<MiniMapPointLogical>();

        void UpdateMiniMap()
        {
            
            DatabaseFieldInfo.FieldCategory? category = GetActiveTabCategory();
            if (category != DatabaseFieldInfo.FieldCategory.Rets && category != DatabaseFieldInfo.FieldCategory.Captures) return;

            System.Collections.IList selectedItems = categoryPanels[category.Value].midPanel.TheGrid.SelectedItems;

            List<MiniMapPointLogical> positions = new List<MiniMapPointLogical>();

            string map = null; // we will simply draw the map of the first item. if other kills are from other maps, too bad!

            foreach (object selectedItem in selectedItems)
            {
                if (selectedItem is Ret)
                {
                    Ret ret = selectedItem as Ret;
                    if (!ret.positionX.HasValue || !ret.positionY.HasValue || !ret.positionZ.HasValue) continue;
                    positions.Add(new MiniMapPointLogical() { 
                        position= new Vector3() { X = (float)ret.positionX.Value, Y = (float)ret.positionY.Value, Z = (float)ret.positionZ.Value },
                        ret = ret,
                    });
                    if (map is null && !string.IsNullOrWhiteSpace(ret.map))
                    {
                        map = ret.map;
                    }
                }
                /*else if (selectedItem is TableMappings.Capture)
                {
                    TableMappings.Capture cap = selectedItem as TableMappings.Capture;
                    if (!cap.positionX.HasValue || !cap.positionY.HasValue || !cap.positionZ.HasValue) continue;
                    positions.Add(new Vector3() { X = (float)cap.positionX.Value, Y = (float)cap.positionY.Value, Z = (float)cap.positionZ.Value });
                    if (cap is null && !string.IsNullOrWhiteSpace(cap.map))
                    {
                        map = cap.map;
                    }
                }*/
            }

            miniMapRenderer.items.Clear();
            foreach(var wnd in miniMapWindows)
            {
                wnd.Key.miniMapRenderer.items.Clear();
            }

            int maxIndex = -1;
            lock (savedPoints)
            {
                foreach (MiniMapPointLogical position in savedPoints)
                {
                    miniMapRenderer.items.Add(new MiniMapPoint() { main = false, position = position.position, index= position.index, note=position.note, callbackReferenceObject=position,clickedCallback=
                        (object o) => {
                            editMiniMapPointNote(o as MiniMapPointLogical);
                        }
                        });

                    foreach (var wnd in miniMapWindows)
                    {
                        MiniMapRendererWindow thisWndLocal = wnd.Key;
                        wnd.Key.miniMapRenderer.items.Add(new MiniMapPoint() { main = false, position = position.position, index= position.index, note=position.note, callbackReferenceObject=position,clickedCallback=
                        (object o) => {
                            thisWndLocal.editMiniMapPointNote(o as MiniMapPointLogical);
                        }
                        });
                    }
                    maxIndex = Math.Max(maxIndex, position.index);
                }
            }
            int index = maxIndex+1;
            lock (currentPoints)
            {
                currentPoints.Clear();
                foreach (MiniMapPointLogical position in positions)
                {
                    miniMapRenderer.items.Add(new MiniMapPoint() { main = true, position = position.position, index = index });
                    foreach (var wnd in miniMapWindows)
                    {
                        wnd.Key.miniMapRenderer.items.Add(new MiniMapPoint() { main = true, position = position.position, index = index });
                    }
                    currentPoints.Add(new MiniMapPointLogical()
                    {
                        position = position.position,
                        ret = position.ret,
                        index = index
                    });
                    index++;
                }
            }

            miniMapRenderer.map = map;
            miniMapRenderer.Update();
            foreach (var wnd in miniMapWindows)
            {
                wnd.Key.miniMapRenderer.map = map;
                wnd.Key.miniMapRenderer.Update();
            }
        }

        MiniMapPointLogical miniMapPointEditorPoint = null;
        private void editMiniMapPointNote(MiniMapPointLogical position)
        {
            miniMapPointEditorPoint = position;
            Dispatcher.Invoke(()=> {
                miniMapPointEditor.Visibility = Visibility.Visible;
                miniMapPointEditorNoteTxt.Text = miniMapPointEditorPoint.note ?? "";
            });
        }

        private void miniMapPointEditorOkBtn_Click(object sender, RoutedEventArgs e)
        {
            MiniMapPointLogical position = miniMapPointEditorPoint;
            miniMapPointEditorPoint = null;
            miniMapPointEditor.Visibility = Visibility.Collapsed;
            string editedNote = miniMapPointEditorNoteTxt.Text;
            miniMapPointEditorNoteTxt.Text = "";
            if (position is null) return;
            position.note = editedNote;

            UpdateMiniMap();
        }

        private void FieldMan_fieldInfoChanged(object sender, DatabaseFieldInfo e)
        {
            //UpdateLayout();
            if (sqlTableItemsSources.ContainsKey(e.Category))
            {
                RequestQueryReset(e.Category);
                //sqlTableItemsSources[e.Category]?.Reset();
                //retsItemSource?.Reset();
            }
        }


        struct CategoryInfoCollection {
            public DatabaseExplorerElements.MidPanel midPanel;
            public DatabaseExplorerElements.SidePanel sidePanel;
            public string tableName;
            public Type dataType;
        }

        class CategorySQLQueryInfo
        {
            public bool gridIsLoading = false; // When we reset the items source, we set this to true and it remains true until the grid has finished loading, so we don't reset again earlier, lest we cause issues.
            public bool resetRequested = false; // If we cannot reset at a given time, just set this to true.
            public object gridLoadingLock = new object();
            public string currentQuery; // We reset this once a grid has finished loading. We don't wanna have dynamic queries that change while a grid is loading.
            public string currentCountQuery;
        }

        Dictionary<DatabaseFieldInfo.FieldCategory, IDataVirtualizingCollection> sqlTableItemsSources = new Dictionary<DatabaseFieldInfo.FieldCategory, IDataVirtualizingCollection>();
        Dictionary<DatabaseFieldInfo.FieldCategory, Func<object[]>> sqlTableSyncDataFetchers = new Dictionary<DatabaseFieldInfo.FieldCategory, Func<object[]>>();

        Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLColumnInfo>> sqlColumns = new Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLColumnInfo>>();
        Dictionary<DatabaseFieldInfo.FieldCategory, string> sqlPrimaryKeys = new Dictionary<DatabaseFieldInfo.FieldCategory, string>();
        //Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<string,SQLite.SQLiteConnection.ColumnInfo>> sqlColumnsMapped = new Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<string, SQLite.SQLiteConnection.ColumnInfo>>();

        Dictionary<string, DemoMeta> demoMetaCache = new Dictionary<string, DemoMeta>();

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

                    savedPointsCombo.ItemsSource = SavedPointsManager.GetListOfSavedPointsPresets();
                    savedPointsCombo.SelectedValue = "default";


                    // Kills first.
                    //Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLite.SQLiteConnection.ColumnInfo>> sqlColumns = new Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLite.SQLiteConnection.ColumnInfo>>();
                    sqlColumns.Clear(); 
                    dbProperties = new DemoDatabaseProperties();

                    //var retColumns = dbConn.GetTableInfo("rets");
                    try
                    {
                        List<DemoDatabaseProperty> propsRes = dbConn.Query<DemoDatabaseProperty>($"SELECT ROWID,* FROM demoDatabaseProperties") as List<DemoDatabaseProperty>;
                        if (propsRes != null)
                        {
                            foreach (DemoDatabaseProperty prop in propsRes)
                            {
                                switch (prop.propertyName)
                                {
                                    case "serverNameInKillAngles":
                                        dbProperties.serverNameInKillAngles = prop.value == "1";
                                        break;
                                    case "serverNameInKillSpree":
                                        dbProperties.serverNameInKillSpree = prop.value == "1";
                                        break;
                                }
                            }
                        }
                    } catch(SQLite.SQLiteException e)
                    {
                        // Ignore. Older versions of DemoHighlightFinder don't create this table.
                        Debug.Print("Database has no demoDatabaseProperties table.");
                    }

                    demoMetaCache.Clear();
                    
                    try
                    {
                        List<DemoMeta> propsRes = dbConn.Query<DemoMeta>($"SELECT ROWID,* FROM demoMeta") as List<DemoMeta>;
                        if (propsRes != null)
                        {
                            foreach (DemoMeta demoMeta in propsRes)
                            {
                                demoMetaCache[demoMeta.demoPath] = demoMeta;
                            }
                        }
                    } catch(SQLite.SQLiteException e)
                    {
                        // Ignore. Older versions of DemoHighlightFinder don't create this table.
                        Debug.Print("Database has no demoMeta table.");
                    }


                    Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<DatabaseFieldInfo.FieldSubCategory, List<DatabaseFieldInfo>>> categorizedFieldInfos = new Dictionary<DatabaseFieldInfo.FieldCategory, Dictionary<DatabaseFieldInfo.FieldSubCategory, List<DatabaseFieldInfo>>>();
                    Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLColumnInfo[]>> unmatchedSQLColumns = new Dictionary<DatabaseFieldInfo.FieldCategory, List<SQLColumnInfo[]>>();

                    int unmatchedSQLColumnCount = 0;

                    sqlPrimaryKeys.Clear();
                    unmatchedSQLColumns.Clear();
                    sqlColumns.Clear();

                    foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> associatedPanel in categoryPanels)
                    {
                        associatedPanel.Value.midPanel.TheGrid.Columns.Clear();
                        sqlColumns[associatedPanel.Key] =  dbConn.GetTableInfoMore(associatedPanel.Value.tableName);
                        unmatchedSQLColumns[associatedPanel.Key] = new List<SQLColumnInfo[]>();

                        SQLColumnInfo lastColumn = null;

                        foreach (SQLColumnInfo sqlColumn in sqlColumns[associatedPanel.Key])
                        {
                            if (sqlColumn.pk > 0)
                            {
                                sqlPrimaryKeys[associatedPanel.Key] = sqlColumn.Name; 
                            }

                            if (sqlColumn.Name.EndsWith(":1") || sqlColumn.Name.EndsWith(":2") || sqlColumn.Name.EndsWith(":3")) continue; // Lazy workaround.

                            bool matchFound = false;

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

                                    matchFound = true;
                                    break;
                                }

                            }

                            if (!matchFound)
                            {
                                unmatchedSQLColumnCount++;
                                unmatchedSQLColumns[associatedPanel.Key].Add(new SQLColumnInfo[] { sqlColumn, lastColumn });
                            }

                            lastColumn = sqlColumn;
                        }
                    }

                    if(unmatchedSQLColumnCount > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Unmatched SQL columns:\n");
                        sb.Append("Would you like to copy this error message to the clipboard? Click Yes to do so.\n");
                        SQLColumnInfo lastColumn = null;
                        foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> associatedPanel in categoryPanels)
                        {
                            sb.Append($"\n\nCategory {associatedPanel.Key}:\n");
                            foreach (SQLColumnInfo[] sqlColumn in unmatchedSQLColumns[associatedPanel.Key])
                            {
                                if (sqlColumn[1] == null)
                                {
                                    sb.Append($"\tAt start: \n");
                                } 
                                else if(sqlColumn[1] != lastColumn) { 

                                    sb.Append($"\tAfter: ");
                                    sb.Append($"{sqlColumn[1].Name}");
                                    if (sqlColumn[1].notnull)
                                    {
                                        sb.Append($" NOT NULL");
                                    }
                                    sb.Append($"\n");
                                }
                                sb.Append($"{sqlColumn[0].Name}");
                                if (sqlColumn[0].notnull)
                                {
                                    sb.Append($" NOT NULL");
                                }
                                sb.Append($"\n");
                                lastColumn = sqlColumn[0];
                            }
                        }
                        string message = sb.ToString();
                        if (MessageBox.Show(message, "Unmatched SQL columns, update tool",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Clipboard.SetText(message);
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

                    UpdateQueries();
                }
                layoutIsInitialized = true;
            }
        }

        private void RequestQueryReset(DatabaseFieldInfo.FieldCategory category)
        {
            lock (categorySQLQueryData[category].gridLoadingLock)
            {
                if (categorySQLQueryData[category].gridIsLoading)
                {
                    categorySQLQueryData[category].resetRequested = true;
                    AwaitDispatcherIdleness();
                } else
                {
                    UpdateQuery(category);
                }
            }
        }
        private void AwaitDispatcherIdleness()
        {
            Dispatcher.InvokeAsync(()=> { DispatcherIdleHandler(); },DispatcherPriority.ApplicationIdle);
        }

        private void DispatcherIdleHandler()
        {
            // All data loading on grids is finished (WHAT AN UGLY WAY TO DO THIS!!!)
            foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> kvp in categoryPanels)
            {
                bool doReset = false;
                lock (categorySQLQueryData[kvp.Key].gridLoadingLock)
                {
                    categorySQLQueryData[kvp.Key].gridIsLoading = false;
                    if (categorySQLQueryData[kvp.Key].resetRequested)
                    {
                        categorySQLQueryData[kvp.Key].resetRequested = false;
                        doReset = true;
                    }
                }
                if (doReset)
                {
                    UpdateQuery(kvp.Key);
                }
            }
        }

        private void TheGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid == null) return;
            foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> kvp in categoryPanels)
            {
                if(kvp.Value.midPanel.TheGrid == grid)
                {
                    bool doReset = false;
                    lock (categorySQLQueryData[kvp.Key].gridLoadingLock)
                    {
                        categorySQLQueryData[kvp.Key].gridIsLoading = false;
                        if (categorySQLQueryData[kvp.Key].resetRequested)
                        {
                            categorySQLQueryData[kvp.Key].resetRequested = false;
                            doReset = true;
                        }
                    }
                    if (doReset)
                    {
                        UpdateQuery(kvp.Key);
                    }
                    break;
                }
            }
        }

        private void UpdateQueries()
        {
            foreach (KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> kvp in categoryPanels)
            {
                categorySQLQueryData[kvp.Key].currentQuery = MakeSelectQuery(kvp.Key, false);
                categorySQLQueryData[kvp.Key].currentCountQuery = MakeSelectQuery(kvp.Key, true);
            }
        }
        private void UpdateQuery(DatabaseFieldInfo.FieldCategory category)
        {
            categorySQLQueryData[category].currentQuery = MakeSelectQuery(category, false);
            categorySQLQueryData[category].currentCountQuery = MakeSelectQuery(category, true);
            sqlTableItemsSources[category].Reset();
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

        private Regex numericCompareSearchRegex = new Regex(@"^\s*(?<operator>(?:>=)|(?:<=)|<|>)\s*(?<number>(?:-?\d*(?:[\.,]\d+))|-?\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private Regex numericRangeSearchRegex = new Regex(@"^\s*(?<number1>(?:-?\d*(?:[\.,]\d+))|-?\d+)\s*-\s*(?<number2>(?:-?\d*(?:[\.,]\d+))|-?\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private Regex numericSearchRegex = new Regex(@"^\s*(?<number>(?:-?\d*(?:[\.,]\d+))|-?\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled); // Not using atm

        private string MakeSelectQuery(DatabaseFieldInfo.FieldCategory category, bool countQuery = false)
        {
            if (!categoryPanels.ContainsKey(category)) return null;

            StringBuilder sb = new StringBuilder();

            string primaryKeyASPrefix = "";
            if (sqlPrimaryKeys.ContainsKey(category))
            {
                primaryKeyASPrefix = $"`{sqlPrimaryKeys[category]}` AS ";
            }

            if (countQuery)
            {
                sb.Append($"SELECT COUNT(*) FROM {categoryPanels[category].tableName}");
            }
            else
            {
                sb.Append($"SELECT {primaryKeyASPrefix}ROWID,* FROM {categoryPanels[category].tableName}");
            }

            DatabaseFieldInfo[] activeFields = fieldMan.getActiveFields();

            bool oneWhereDone = false;
            Match match;
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
                    else if (field.Numeric && (match=numericCompareSearchRegex.Match(field.Content)).Success)
                    {
                        string op = match.Groups["operator"].Value.Trim();
                        string numberString = match.Groups["number"].Value.Replace(',', '.');

                        if (op == ">=" || op == "<=" || op == "<" || op == ">")
                        {
                            if (decimal.TryParse(numberString.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal number))
                            {
                                sb.Append($"{field.FieldName} ");
                                sb.Append(op);
                                sb.Append(" ");
                                sb.Append(number.ToString("F99").TrimEnd('0'));
                            }
                        }
                    }
                    else if (field.Numeric && (match=numericRangeSearchRegex.Match(field.Content)).Success)
                    {
                        string op = match.Groups["operator"].Value.Trim();
                        string number1String = match.Groups["number1"].Value.Replace(',', '.');
                        string number2String = match.Groups["number2"].Value.Replace(',', '.');

                        if (decimal.TryParse(number1String.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal number1))
                        {
                            if (decimal.TryParse(number2String.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal number2))
                            {
                                decimal from = Math.Min(number1, number2);
                                decimal to = Math.Max(number1, number2);
                                sb.Append($"{field.FieldName} ");
                                sb.Append(" >= ");
                                sb.Append(from.ToString("F99").TrimEnd('0'));
                                sb.Append($" AND  ");
                                sb.Append($"{field.FieldName} ");
                                sb.Append("<= ");
                                sb.Append(to.ToString("F99").TrimEnd('0'));
                            }
                        }
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
                    sqlTableSyncDataFetchers[categoryData.Key] = (Func<object[]>)typeof(DemoDatabaseExplorer).GetMethod("CreateSQLItemsSyncDataFetcher",BindingFlags.NonPublic| BindingFlags.Instance).MakeGenericMethod(categoryData.Value.dataType).Invoke(this,new object[] { categoryData.Key });
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
                            lock (categorySQLQueryData[category].gridLoadingLock)
                            {
                                categorySQLQueryData[category].gridIsLoading = true;
                            }
                            string query = categorySQLQueryData[category].currentQuery;
                            //List<Ret> res = dbConn.Query<Ret>($"SELECT * FROM rets LIMIT {offset},{pageSize}") as List<Ret>;
                            //List<T> res = dbConn.Query<T>($"{MakeSelectQuery(category, false)} LIMIT {offset},{pageSize}") as List<T>;
                            List<T> res = dbConn.Query<T>($"{query} LIMIT {offset},{pageSize}") as List<T>;
                            return res.ToArray();
                        },
                        countFetcher: (ct) =>
                        {
                            lock (categorySQLQueryData[category].gridLoadingLock)
                            {
                                categorySQLQueryData[category].gridIsLoading = true;
                            }
                            string countQuery = categorySQLQueryData[category].currentCountQuery;
                            //int res = dbConn.ExecuteScalar<int>($"SELECT COUNT(*) FROM rets");
                            //int res = dbConn.ExecuteScalar<int>($"{MakeSelectQuery(category, true)}");
                            int res = dbConn.ExecuteScalar<int>($"{countQuery}");
                            return res;
                        })
                    .ThrottledLifoPageRequests().AsyncIndexAccess((a, b) => {
                        return new T();//{ hash = "Loading, please wait." };
                    });
            //.SyncIndexAccess();
        }
        private Func<T[]> CreateSQLItemsSyncDataFetcher<T>(DatabaseFieldInfo.FieldCategory category) where T : new()
        {
            return () => {
                string query = categorySQLQueryData[category].currentQuery;
                //List<T> res = dbConn.Query<T>($"{MakeSelectQuery(category, false)}") as List<T>;
                List<T> res = dbConn.Query<T>(query) as List<T>;
                return res.ToArray();
            };
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
            RequestQueryReset(category);
            //sqlTableItemsSources[category].Reset();
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


        private DatabaseFieldInfo.FieldCategory? GetActiveTabCategory()
        {
            TabItem item = midSectionTabs.SelectedItem as TabItem;
            if (item == null) return null;

            MidPanel midPanel = item.Content as MidPanel;
            if (midPanel == null)
            {
                midPanel = item.GetChildOfType<MidPanel>() as MidPanel; // future proofing a bit?
            }
            if (midPanel == null) return null;

            foreach(KeyValuePair<DatabaseFieldInfo.FieldCategory, CategoryInfoCollection> categoryData in categoryPanels)
            {
                if(categoryData.Value.midPanel == midPanel)
                {
                    return categoryData.Key;
                }
            }
            return null;
        }

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

        List<MiniMapPointLogical> savedPoints = new List<MiniMapPointLogical>();
        private void savedPointsLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            lock (savedPoints) { 
                if(savedPoints.Count > 0)
                {
                    if (MessageBox.Show("Loading preset, but points exist. Replace?","Existing points",MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                MiniMapPointLogical[] result = SavedPointsManager.GetSavedPoints(savedPointsCombo.Text);

                if(result is null || result.Length == 0)
                {
                    MessageBox.Show("Preset contains no points.");
                    return;
                }
                savedPoints.Clear();
                savedPoints.AddRange(result);
                savedPointsCombo.ItemsSource = SavedPointsManager.GetListOfSavedPointsPresets();
                savedPointsCombo.SelectedValue = "default";
                UpdateMiniMap();
            }
        }

        private void savedPointsSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            lock (savedPoints)
            {
                if (savedPoints.Count == 0)
                {
                    if (MessageBox.Show("Saving preset, but no saved points. Save empty preset?", "No point(s)", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                SavedPointsManager.SetSavedPoints(savedPointsCombo.Text, savedPoints.ToArray());

                savedPointsCombo.ItemsSource = SavedPointsManager.GetListOfSavedPointsPresets();
                savedPointsCombo.SelectedValue = "default";
            }
        }


        private void saveVisiblePointsBtn_Click(object sender, RoutedEventArgs e)
        {
            lock (savedPoints)
            {
                //MiniMapRenderer renderer = miniMapRenderer;
                //if (renderer is null) return;

                //MiniMapPoint[] points = renderer.items.ToArray();

                //Array.Sort(points, (a, b) => { return a.index.CompareTo(b.index); });

                lock (currentPoints)
                {
                    //if (currentPoints is null || currentPoints.Count == 0) return;
                    //savedPoints.Clear();
                    savedPoints.AddRange(currentPoints);
                }
                UpdateMiniMap();
            }
        }


        private void resetSavedPointsBtn_Click(object sender, RoutedEventArgs e)
        {
            lock (savedPoints)
            {
                savedPoints.Clear();
                UpdateMiniMap();
            }
        }
    }

}
