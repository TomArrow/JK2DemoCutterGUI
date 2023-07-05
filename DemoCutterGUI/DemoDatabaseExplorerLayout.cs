using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using DemoCutterGUI.TableMappings;
using System;
using System.Collections.Generic;
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
    public partial class DemoDatabaseExplorer
    {


        bool layoutIsInitialized = false;
        DatabaseFieldInfo[] fieldInfoForSearch = null;


        IDataVirtualizingCollection<Ret> retsItemSource = null;
        private void InitializeLayoutDispatcher()
        {
            lock (dbMutex)
            {
                if (!layoutIsInitialized)
                {


                    fieldInfoForSearch = DatabaseFieldInfo.GetDatabaseFieldInfos();


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
                        { new Tuple<string,string>("Rets","Names"),new List<DatabaseFieldInfo>() },
                        { new Tuple<string,string>("Rets","Kill"),new List<DatabaseFieldInfo>() },
                        { new Tuple<string,string>("Rets","Position"),new List<DatabaseFieldInfo>() },
                        { new Tuple<string,string>("Rets","Rest"),new List<DatabaseFieldInfo>() },
                    };
                    //Dictionary<string, List<DataGridTextColumn>> dataGridCategorizedFieldInfos = new Dictionary<string, List<DataGridTextColumn>>()
                    //{
                    //    {"Rets",new List<DataGridTextColumn>() }
                    //};


                    displayKill.Children.Clear();
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
                                retsGrid.Columns.Add(new DataGridTextColumn() { Header = fieldInfo.FieldName, Binding = new Binding(fieldInfo.FieldName) });

                                if (!groupBoxes["Rets"].ContainsKey(fieldInfo.SubCategory))
                                {
                                    groupBoxes["Rets"][fieldInfo.SubCategory] = new GroupBox() { Header=fieldInfo.SubCategory, Content = new StackPanel() };
                                }
                                var newTextBox = new TextBox();
                                newTextBox.SetBinding(TextBox.TextProperty,new Binding(fieldInfo.FieldName));
                                (groupBoxes["Rets"][fieldInfo.SubCategory].Content as StackPanel).Children.Add(newTextBox);
                                break;
                            }

                        }
                    }

                    foreach(var box in groupBoxes["Rets"])
                    {
                        displayKill.Children.Add(box.Value);
                    }

                    // Apply search fields
                    listKillsNames.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Names")].ToArray();
                    listKillsKill.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Kill")].ToArray();
                    listKillsPosition.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Position")].ToArray();
                    listKillsRest.ItemsSource = categorizedFieldInfos[new Tuple<string, string>("Rets", "Rest")].ToArray();

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
                                List<Ret> res = dbConn.Query<Ret>($"SELECT * FROM rets LIMIT {offset},{pageSize}") as List<Ret>;
                                return res.ToArray();
                            },
                            countFetcher: (ct) =>
                            {
                                int res = dbConn.ExecuteScalar<int>($"SELECT COUNT(*) FROM rets");
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
            retsItemSource.Reset();
            e.Column.SortDirection = System.ComponentModel.ListSortDirection.Ascending;
            e.Handled = true;
        }
    }

}
