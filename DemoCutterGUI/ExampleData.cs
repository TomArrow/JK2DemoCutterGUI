using DemoCutterGUI.TableMappings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace DemoCutterGUI
{
    public class ExampleData
    {
        public ObservableCollection<DemoLinePoint> DemoLinePoints { get; set; }
        public ObservableCollection<Demo> Demos { get; set; }
        public ExampleData()
        {
            DemoLinePoints = new ObservableCollection<DemoLinePoint>() {new DemoLinePoint() {time=10,demoTime=10 },
                            new DemoLinePoint() {time=11,demoTime=11 },
                            new DemoLinePoint() {time=200,demoTime=100 },
                            new DemoLinePoint() {time=202,demoTime=101 },
                            new DemoLinePoint() {time=400,demoTime=200 },
                            new DemoLinePoint() {time=402,demoTime=201 },
                            new DemoLinePoint() {time=404,demoTime=203 },
                            new DemoLinePoint() {time=406,demoTime=205 },
                            new DemoLinePoint() {time=606,demoTime=405 },
                            new DemoLinePoint() {time=608,demoTime=407 } };
            Demos = new ObservableCollection<Demo>() {new Demo() { highlightDemoTime = 5000, highlightOffset = 10000 },
                new Demo(new AdditionalHighlights(){ 5000,7000, 12000, 12500, 14000, 15000}) { highlightDemoTime = 7000, highlightOffset = 10000, name="demo1", },
                new Demo() { highlightDemoTime = 7500, highlightOffset = 10000, name="demo2" },
                new Demo() { highlightDemoTime = 8000, highlightOffset = 10000, name="demo3" },
                new Demo() { highlightDemoTime = 12000, highlightOffset = 10000, name="demo4" },
                new Demo() { highlightDemoTime = 15000, highlightOffset = 10000, name="demo5_abcdefg_235235HD_blahblahblah___ERBEREBERBERBEBERBER_1234325ups_blahblahbalh.dm_15" },
                new Demo() { highlightDemoTime = 15500, highlightOffset = 10000, name="demo6" },
                new Demo() { highlightDemoTime = 15700, highlightOffset = 10000, name="demo7" },
                new Demo() { highlightDemoTime = 17000, highlightOffset = 10000, name="demo8" },
            };

            InitKillDatabaseFields();
        }



        public ObservableCollection<DatabaseFieldInfo> KillDatabaseFieldsKillNames { get; set; } = new ObservableCollection<DatabaseFieldInfo>();
        public ObservableCollection<DatabaseFieldInfo> KillDatabaseFieldsKillKill { get; set; } = new ObservableCollection<DatabaseFieldInfo>();
        public ObservableCollection<DatabaseFieldInfo> KillDatabaseFieldsKillPosition { get; set; } = new ObservableCollection<DatabaseFieldInfo>();
        public ObservableCollection<DatabaseFieldInfo> KillDatabaseFieldsKillRest { get; set; } = new ObservableCollection<DatabaseFieldInfo>();
        public ObservableCollection<DatabaseFieldInfo> KillDatabaseFieldsKillAll { get; set; } = new ObservableCollection<DatabaseFieldInfo>();
        //public ObservableCollection<DataGridTextColumn> KillDatabaseFieldsGrid { get; set; } = new ObservableCollection<DataGridTextColumn>();
        public ObservableCollection<Ret> KillDatabaseExampleKills { get; set; } = new ObservableCollection<Ret>();
        void InitKillDatabaseFields()
        {
            var allFields = DatabaseFieldInfo.GetDatabaseFieldInfos();
            foreach (var field in allFields)
            {
                //KillDatabaseFieldsGrid.Add(new DataGridTextColumn() { Header=field.FieldName, Binding=new Binding(field.FieldName) });
                if((field.Category=="Kills" || field.Category == "KillAngles"))
                {
                    switch (field.SubCategory)
                    {
                        case "Names":
                            KillDatabaseFieldsKillNames.Add(field);
                            break;
                        case "Kill":
                            KillDatabaseFieldsKillKill.Add(field);
                            break;
                        case "Position":
                            KillDatabaseFieldsKillPosition.Add(field);
                            break;
                        default:
                            KillDatabaseFieldsKillRest.Add(field);
                            break;
                    }
                    KillDatabaseFieldsKillAll.Add(field);
                }
            }

            KillDatabaseExampleKills = new ObservableCollection<Ret>()
            {
                new Ret(){ hash="abc", killerName="James",victimName="Peter" },
                new Ret(){ hash="def",killerName="Jeffrey",victimName="Boink" },
                new Ret(){ hash="ghi",killerName="Dingold",victimName="Porp" },
                new Ret(){ hash="jkl",killerName="Master",victimName="Slave" },
                new Ret(){ hash="mno",killerName="Jeremiah",victimName="Horndrung" },
            };

        }
    }
}
