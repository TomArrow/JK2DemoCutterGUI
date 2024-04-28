using BitmapFontLibrary;
using Ninject;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;

namespace DemoCutterGUI
{

    public class ScrubControl:INotifyPropertyChanged { 
        public double scrubPosition { get; set; }
        public double absoluteMin { get; set; } = -100; // set automatically so that all times and demos are visible
        public double absoluteMax { get; set; } = 10000; // set automatically so that all times and demos are visible

        public double currentMin { get; set; }
        public double currentMax { get; set; }

        public double absoluteMinVert { get; set; } = 0.0; 
        public double absoluteMaxVert { get; set; } = 10.0;
        public double currentMinVert { get; set; } = 0.0;
        public double currentMaxVert { get; set; } = 3.0;

        public event PropertyChangedEventHandler PropertyChanged;


        // Just fun for debugging
        public bool inversionTest { get; set; } = false;
        public double inversionTestValue { get; set; } = 1000;
        public double verifyTestPositionValue { get; set; } = 1000;

        //public double getAbsolutePosition(double rangeStart,double rangeEnd)
        //{

        //}

        public double relativeFromAbsolute(double absolute)
        {
            double delta = currentMax - currentMin;
            return (absolute - currentMin) / delta * 2.0 - 1.0;
        }
    }



    /// <summary>
    /// Interaction logic for CombineCutter.xaml
    /// </summary>
    public partial class CombineCutter : Window
    {



        ScrubControl scrubControl = new ScrubControl();


        public bool speedPreservationMode { get; private set; } = false;
        public bool speedChangeDemoTimeMode { get; private set; } = true;

        JommeTimePoints points = new JommeTimePoints();
        Demos demos = new Demos();


        BitmapFont font = null;

        public CombineCutter()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 2,
                MinorVersion = 1,
                RenderContinuously = false,

            };
            OpenTkControl.Loaded += OpenTkControl_Loaded;
            OpenTkControl.Start(settings);

            points.Updated += Points_Updated;
            demos.Updated += Demos_Updated;

            //points.addPoint(new DemoLinePoint() {time=10,demoTime=10 });
            //points.addPoint(new DemoLinePoint() {time=200,demoTime=100 });
            //points.addPoint(new DemoLinePoint() {time=400,demoTime=200 });
            //points.addPoint(new DemoLinePoint() {time=600,demoTime=600 });
            points.addPoint(new DemoLinePoint() {time=10,demoTime=10 });
            points.addPoint(new DemoLinePoint() {time=11,demoTime=11 });
            points.addPoint(new DemoLinePoint() {time=200,demoTime=100 });
            points.addPoint(new DemoLinePoint() {time=202,demoTime=101 });
            points.addPoint(new DemoLinePoint() {time=400,demoTime=200 });
            points.addPoint(new DemoLinePoint() {time=402,demoTime=201 });
            points.addPoint(new DemoLinePoint() {time=404,demoTime=203 });
            points.addPoint(new DemoLinePoint() {time=406,demoTime=205 });
            points.addPoint(new DemoLinePoint() {time=606,demoTime=405 });
            points.addPoint(new DemoLinePoint() {time=608,demoTime=407 });
            points.addPoint(new DemoLinePoint() {time=50000,demoTime=50000 });

            demos.Add(new Demo() { highlightDemoTime = 5000, highlightOffset = 10000 });
            demos.Add(new Demo(new AdditionalHighlights() { 5000, 7000, 12000 }) { highlightDemoTime = 7000, highlightOffset = 10000, name = "demo1" });
            demos.Add(new Demo() { highlightDemoTime = 7500, highlightOffset = 10000, name = "demo2" });
            demos.Add(new Demo() { highlightDemoTime = 8000, highlightOffset = 10000, name = "demo3" });
            demos.Add(new Demo() { highlightDemoTime = 12000, highlightOffset = 10000, name = "demo4" });
            demos.Add(new Demo() { highlightDemoTime = 15000, highlightOffset = 10000, name = "demo5_abcdefg_235235HD_blahblahblah___ERBEREBERBERBEBERBER_1234325ups_blahblahbalh.dm_15" });
            demos.Add(new Demo() { highlightDemoTime = 15500, highlightOffset = 10000, name = "demo6" });
            demos.Add(new Demo() { highlightDemoTime = 15700, highlightOffset = 10000, name = "demo7" });
            demos.Add(new Demo() { highlightDemoTime = 17000, highlightOffset = 10000, name = "demo8" });

            points.bindListView(demoLinePointsView);
            points.bindCutterWindow(this);
            scrubSlider.DataContext = scrubControl;
            rangeSlider.DataContext = scrubControl;
            rangeSliderVert.DataContext = scrubControl;
            //inversionTestValueControl.DataContext = scrubControl;
            //inversionTestValueVerifyControl.DataContext = scrubControl;
            debuggingTab.DataContext = scrubControl;

            demos.bindListView(demosView);

        }

        private void Demos_Updated(object sender, EventArgs e)
        {
            OpenTkControl.InvalidateVisual();
        }

        private void OpenTkControl_Loaded(object sender, RoutedEventArgs e)
        {
            // var ifc = new InstalledFontCollection();


        }

        private void Points_Updated(object sender, EventArgs e)
        {
            scrubControl.absoluteMin = Math.Min(0,points.lowestTime)-5000;
            scrubControl.absoluteMax = points.highestTime+10000;
            OpenTkControl.InvalidateVisual();
        }

        const double maxFps = 165;
        const double minTimeDelta = 1000.0 / maxFps;
        DateTime lastUpdate = DateTime.Now;

        struct TimeMarker {
            public double time;
            public double x;
            public bool isSubDivMarker;
        }
        private void OpenTkControl_OnRender(TimeSpan delta)
        {

            if(font == null)
            {

                IKernel kernel = new StandardKernel(new BitmapFontModule());
                font = kernel.Get<BitmapFont>();
                font.Initialize("data/mph-2b-damase128.fnt");
            }

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            double timeSinceLast = (DateTime.Now - lastUpdate).TotalMilliseconds;
            //if (timeSinceLast < minTimeDelta) System.Threading.Thread.Sleep((int)(minTimeDelta- timeSinceLast));
            if (timeSinceLast > minTimeDelta) ;//OpenTkControl.InvalidateVisual();
            else return;
            GL.ClearColor(Color4.White);

            double width = OpenTkControl.ActualWidth;
            int maxValue = (int)Math.Max(10, width); // Picked random minimum number
            float divider = (float)maxValue / 2.0f;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            /* // This just makes the lines fat and ugly?
            GL.ClearColor(1.0f, 1.0f, 1.0f, 0.0f);
            GL.LineWidth(1);
            GL.Enable(EnableCap.LineSmooth); 
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.DepthMask(false); 
            */

            double actualWidth = OpenTkControl.ActualWidth;
            double from = scrubControl.currentMin;
            double to = scrubControl.currentMax;

            this.DrawDemos(actualWidth, to, from);


            this.DrawVariousLines();

            this.DrawSpeedGraph(actualWidth, to, from);


            this.DrawTimeMarkers(actualWidth, to, from);

            lastUpdate =DateTime.Now;
        }

        private void UpdateSettingsFromGUI(object sender, RoutedEventArgs e)
        {
            speedPreservationMode = speedPreserveCheck.IsChecked.HasValue && speedPreserveCheck.IsChecked.Value;
            speedChangeDemoTimeMode = speedChangeDemoTimeModeCheck.IsChecked.HasValue && speedChangeDemoTimeModeCheck.IsChecked.Value;
        }

        private void scrubSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OpenTkControl.InvalidateVisual();
        }

        private void rangeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OpenTkControl.InvalidateVisual();
        }

        private void newTimePointBtn_Click(object sender, RoutedEventArgs e)
        {
            int newTime = (int)scrubControl.scrubPosition;
            float demoSpeed = 0;
            float demoTime = points.lineAtSimple(newTime,ref demoSpeed);
            points.addPoint(new DemoLinePoint() {time=newTime,demoTime= (int)demoTime });
        }

        private void inversionTestValueControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {

            OpenTkControl.InvalidateVisual();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            OpenTkControl.InvalidateVisual();
        }

        private void demoLinePointsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            delTimePointBtn.IsEnabled = demoLinePointsView.SelectedItems.Count > 0;


        }

        private void delTimePointBtn_Click(object sender, RoutedEventArgs e)
        {

            List<DemoLinePoint> selectedPoints = demoLinePointsView.SelectedItems.Cast<DemoLinePoint>().ToList();
            foreach(DemoLinePoint point in selectedPoints)
            {
                points.removePoint(point);
            }
        }

        private void newDemoBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Demo> selectedDemos = demosView.SelectedItems.Cast<Demo>().ToList();
            Demo after = selectedDemos.Count > 0 ? selectedDemos[0] : null;

            demos.Add(new Demo() { highlightDemoTime = after == null ? default : after.highlightDemoTime },after);
        }

        private void removeDemoBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Demo> selectedDemos = demosView.SelectedItems.Cast<Demo>().ToList();
            foreach (Demo demo in selectedDemos)
            {
                demos.Remove(demo);
            }
        }

        private void demosView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            removeDemoBtn.IsEnabled = demosView.SelectedItems.Count > 0;
            loadMetaForDemo.IsEnabled = demosView.SelectedItems.Count == 1;
        }
        /*
        private void AddSecondaryDemoHighlightOffset_Click(object sender, RoutedEventArgs e)
        {
            Demo demo = (Demo)((Button)sender).DataContext;
            demo.additionalHighlights.Add(10000,AdditionalHighlight.Type.METAEVENT_NONE,true);
        }

        private void RemoveSecondaryDemoHighlightOffset_Click(object sender, RoutedEventArgs e)
        {
            AdditionalHighlight highlight = (AdditionalHighlight)((Button)sender).DataContext;
            highlight.associatedDemo.additionalHighlights.Remove(highlight);
        }*/

        private void additionalHighlightsDataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            /*
            DataGrid dg = (sender as DataGrid);
            if (dg == null) return;
            Demo demo = (dg.DataContext as Demo);
            if (demo == null) return;
            AdditionalHighlight ah = e.NewItem as AdditionalHighlight;
            if (ah == null) return;

            ah.associatedDemo = demo;
            Console.WriteLine(sender);*/
        }

        private void additionalHighlightsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(removeAdditionalHighlight != null && additionalHighlightsDataGrid != null) // safety in case it fires early during window load?
            {
                foreach(var obj in additionalHighlightsDataGrid.SelectedItems)
                {
                    if(obj is AdditionalHighlight)
                    {
                        removeAdditionalHighlight.IsEnabled = true;
                        return;
                    }
                }
                removeAdditionalHighlight.IsEnabled = false;
            }
        }

        private void removeAdditionalHighlight_Click(object sender, RoutedEventArgs e)
        {
            Demo demo = additionalHighlightsDataGrid.DataContext as Demo;
            if (demo == null) return;
            List<AdditionalHighlight> itemsToRemove = new List<AdditionalHighlight>();
            foreach (var obj in additionalHighlightsDataGrid.SelectedItems)
            {
                if (obj is AdditionalHighlight) // We have to do it stupid like this because we allow the DataGrid to create new items and if we select that "new item row" it has the type Microsoft.Internal.Something idk I forgot. So cassting to a list like usual won't work.
                {
                    itemsToRemove.Add(obj as AdditionalHighlight);
                }
            }
            foreach(AdditionalHighlight ah in itemsToRemove)
            {
                demo.additionalHighlights.Remove(ah);
            }
        }
    }
}
