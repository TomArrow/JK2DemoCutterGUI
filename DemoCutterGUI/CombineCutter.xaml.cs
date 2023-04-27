using BitmapFontLibrary;
using BitmapFontLibrary.Model;
using Ninject;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
using System.Text;
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

    public class ScrubControl:INotifyPropertyChanged { 
        public double scrubPosition { get; set; }
        public double absoluteMin { get; set; } = -100; // set automatically so that all times and demos are visible
        public double absoluteMax { get; set; } = 10000; // set automatically so that all times and demos are visible

        public double currentMin { get; set; }
        public double currentMax { get; set; }

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


        BitmapFont font = null;

        public CombineCutter()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 2,
                MinorVersion = 1,
                RenderContinuously = false
            };
            OpenTkControl.Loaded += OpenTkControl_Loaded;
            OpenTkControl.Start(settings);

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

            points.bindListView(demoLinePointsView);
            points.bindCutterWindow(this);
            points.Updated += Points_Updated;
            scrubSlider.DataContext = scrubControl;
            rangeSlider.DataContext = scrubControl;
            //inversionTestValueControl.DataContext = scrubControl;
            //inversionTestValueVerifyControl.DataContext = scrubControl;
            debuggingTab.DataContext = scrubControl;


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
            if (timeSinceLast > minTimeDelta) OpenTkControl.InvalidateVisual();
            else return;
            GL.ClearColor(Color4.White);

            double width = OpenTkControl.ActualWidth;
            int maxValue = (int)Math.Max(10, width); // Picked random minimum number
            float divider = (float)maxValue / 2.0f;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Reference line at 1x
            GL.Color4(0.5, 0.9, 0.5, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(-1, 1f/5f-1f);
            GL.Vertex2(1, 1f / 5f - 1f);
            GL.End();

            // Reference line at 0.5x
            GL.Color4(0.75, 0.75, 0.95, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(-1, 0.5f/5f-1f);
            GL.Vertex2(1, 0.5f / 5f - 1f);
            GL.End();

            // Reference line at 2x
            GL.Color4(0.75, 0.75, 0.95, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(-1, 2f/5f-1f);
            GL.Vertex2(1, 2f / 5f - 1f);
            GL.End();

            // Scrub line
            float scrubPosition = (float)scrubControl.scrubPosition;
            GL.Color4(0.75, 0.75, 0.95, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(scrubPosition, -1f);
            GL.Vertex2(scrubPosition, 1f);
            GL.End();


            // Inverse lineat test
            if (scrubControl.inversionTest)
            {
                float inverseTestPosition = points.lineAtInverse((float)scrubControl.inversionTestValue);
                float relativeInverseTestPosition = (float)scrubControl.relativeFromAbsolute(inverseTestPosition);
                float blah = 0;
                scrubControl.verifyTestPositionValue = points.lineAtSimple(inverseTestPosition, ref blah);
                GL.Color4(0.95, 0.75, 0.75, 1);
                GL.LineWidth(1);
                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex2(relativeInverseTestPosition, -1f);
                GL.Vertex2(relativeInverseTestPosition, 1f);
                GL.End();
            }


            // Actual speed graph
            GL.Color4(0,0,0,1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            double actualWidth = OpenTkControl.ActualWidth;
            double from = scrubControl.currentMin;
            double to = scrubControl.currentMax;
            double step = (to - from)/actualWidth;
            double openGLStep = 2.0 / actualWidth;
            if((to-from) > 1.0 && actualWidth > 1.0) // Avoid endless loop if we're not yet
            {
                for (double i = from, openGLLoc = -1.0; i <= to; i += step, openGLLoc += openGLStep)
                {
                    int demoTime = 0;
                    float demoTimeFraction = 0;
                    float demoSpeed = 0;
                    int snappedTime = (int)i;
                    float fraction = (float)((double)i - (double)snappedTime);
                    points.lineAt(snappedTime, fraction, ref demoTime, ref demoTimeFraction, ref demoSpeed);
                    //GL.Vertex2((float)i/ divider - 1f, (demoSpeed / 5f - 1f));
                    GL.Vertex2(openGLLoc, (demoSpeed / 5f - 1f));
                }
            }
            GL.End();


            // Time markers. Simple way for now.
            //double minUnitBase = 1000; // Milliseconds
            double desiredTimeMarkersCount = 5;
            double timeMarkerStepBase = 10;
            double timeMarkerStep = (to - from) / desiredTimeMarkersCount;
            double baseAlignedTimeMarkerStep = Math.Pow(timeMarkerStepBase, Math.Round(Math.Log(timeMarkerStep) / Math.Log(timeMarkerStepBase))); // Basically make it a power of timeMarkerStepBase
            List<TimeMarker> timeMarkers = new List<TimeMarker>();
            double startPoint = baseAlignedTimeMarkerStep*Math.Ceiling(from/ baseAlignedTimeMarkerStep)- baseAlignedTimeMarkerStep;
            while (startPoint < to)
            {
                startPoint += baseAlignedTimeMarkerStep;
                double x = 2.0*(startPoint-from)/(to-from)-1.0;
                timeMarkers.Add(new TimeMarker() { time=startPoint,x=x });
            }


            GL.Color4(0, 0, 0, 1);
            GL.LineWidth(1);
            double actualHeight = OpenTkControl.ActualHeight;
            double desiredTimeMarkerLineHeight = 10;
            double relativeHeight = 2.0*desiredTimeMarkerLineHeight / actualHeight;
            double fontHeight = 20;
            double relativeFontHeight = 2.0* fontHeight/actualHeight;
            foreach (TimeMarker timeMarker in timeMarkers)
            {
                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex2(timeMarker.x, -1f+ relativeFontHeight);
                GL.Vertex2(timeMarker.x, -1f+ relativeHeight+ relativeFontHeight);
                GL.End();
            }


            // Time marker texts
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            double verticalScale = 1000.0;
            double horizontalScale = 1000.0*actualWidth / actualHeight;
            GL.Ortho(-horizontalScale, horizontalScale, -1000.0, 1000.0, -1.0, 1.0);

            /*font.Draw("My text", 0.0f, -500f, 0.0f, new TextConfiguration()
            {
                SizeInPixels = (uint)(relativeFontHeight*1000.0),
                MaximalWidth = 1000,
                Alignment = BitmapFontLibrary.Model.TextAlignment.LeftAligned
            });*/
            foreach (TimeMarker timeMarker in timeMarkers)
            {
                font.Draw(Math.Round(timeMarker.time).ToString(), (float)timeMarker.x*(float)horizontalScale-(float)horizontalScale/2f, 1000.0f*(-1f+(float)relativeFontHeight), 0.0f, new TextConfiguration
                {
                    SizeInPixels = (uint)(relativeFontHeight * 1000.0),
                    MaximalWidth = (float)horizontalScale,
                    Alignment = BitmapFontLibrary.Model.TextAlignment.Centered
                });
            }
            GL.Disable(EnableCap.Blend);

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

        }

        private void inversionTestValueControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {

            OpenTkControl.InvalidateVisual();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            OpenTkControl.InvalidateVisual();
        }
    }
}
