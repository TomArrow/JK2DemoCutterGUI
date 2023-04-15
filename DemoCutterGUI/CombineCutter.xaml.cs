﻿using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for CombineCutter.xaml
    /// </summary>
    public partial class CombineCutter : Window
    {

        public bool speedPreservationMode { get; private set; } = false;
        public bool speedChangeDemoTimeMode { get; private set; } = true;

        JommeTimePoints points = new JommeTimePoints();
        public CombineCutter()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 2,
                RenderContinuously = false
            };
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
        }

        private void Points_Updated(object sender, EventArgs e)
        {
            OpenTkControl.InvalidateVisual();
        }

        const double maxFps = 165;
        const double minTimeDelta = 1000.0 / maxFps;
        DateTime lastUpdate = DateTime.Now;


        private void OpenTkControl_OnRender(TimeSpan delta)
        {
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

            // Actual speed graph
            GL.Color4(0,0,0,1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            for(int i = 0; i < maxValue; i++)
            {
                int demoTime = 0;
                float demoTimeFraction = 0;
                float demoSpeed = 0;
                points.lineAt(i,0, ref demoTime, ref demoTimeFraction, ref demoSpeed);
                GL.Vertex2((float)i/ divider - 1f, (demoSpeed / 5f - 1f));
            }
            GL.End();

            lastUpdate =DateTime.Now;
        }

        private void UpdateSettingsFromGUI(object sender, RoutedEventArgs e)
        {
            speedPreservationMode = speedPreserveCheck.IsChecked.HasValue && speedPreserveCheck.IsChecked.Value;
            speedChangeDemoTimeMode = speedChangeDemoTimeModeCheck.IsChecked.HasValue && speedChangeDemoTimeModeCheck.IsChecked.Value;
        }
    }
}
