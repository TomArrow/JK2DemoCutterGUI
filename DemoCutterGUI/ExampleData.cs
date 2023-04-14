using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoCutterGUI
{
    public class ExampleData
    {
        public ObservableCollection<DemoLinePoint> DemoLinePoints { get; set; }
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
        }
    }
}
