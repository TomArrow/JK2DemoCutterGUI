using OpenTK.Graphics.OpenGL;
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


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Color4(0,0,0,1);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(0.5, 0);
            GL.Vertex2(0.5, 1);
            GL.End();
            lastUpdate =DateTime.Now;
        }
    }
}
