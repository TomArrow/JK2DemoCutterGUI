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
using System.Runtime.CompilerServices;
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

    public partial class CombineCutter : Window
    {


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float getYFromDemoSpeed(float demoSpeed)
        {
            return (float)((demoSpeed - scrubControl.currentMinVert) / (scrubControl.currentMaxVert - scrubControl.currentMinVert)) * 2f - 1f;
        }
        public void DrawVariousLines()
        {
            // Reference line at 1x
            float lineat1x = getYFromDemoSpeed(1);
            GL.Color4(0.5, 0.9, 0.5, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(-1, lineat1x);
            GL.Vertex2(1, lineat1x);
            GL.End();

            // Reference line at 0.5x
            float lineat05x = getYFromDemoSpeed(0.5f);
            GL.Color4(0.75, 0.75, 0.95, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(-1, lineat05x);
            GL.Vertex2(1, lineat05x);
            GL.End();

            // Reference line at 2x
            float lineat2x = getYFromDemoSpeed(2);
            GL.Color4(0.75, 0.75, 0.95, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(-1, lineat2x);
            GL.Vertex2(1, lineat2x);
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
        }

        public void DrawSpeedGraph(double actualWidth, double to, double from)
        {
            // Actual speed graph
            GL.Color4(0, 0, 0, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            double step = (to - from) / actualWidth;
            double openGLStep = 2.0 / actualWidth;
            if ((to - from) > 1.0 && actualWidth > 1.0) // Avoid endless loop if we're not yet
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
                    //GL.Vertex2(openGLLoc, (demoSpeed / 5f - 1f));
                    GL.Vertex2(openGLLoc, getYFromDemoSpeed(demoSpeed));
                }
            }
            GL.End();
        }

        public void DrawTimeMarkers(double actualWidth, double to, double from)
        {
            // Time markers. Simple way for now.
            //double minUnitBase = 1000; // Milliseconds
            double desiredTimeMarkersCount = actualWidth / 150;
            double timeMarkerStepBase = 10;
            int[] subdiv = new int[] { 0, 2, 4, 5, 6, 8 };
            double timeMarkerStep = Math.Max(0.01,(to - from) / desiredTimeMarkersCount); // Prevent infinitely tiny steps thaat will fill up memory
            double baseAlignedTimeMarkerStep = Math.Pow(timeMarkerStepBase, Math.Round(Math.Log(timeMarkerStep) / Math.Log(timeMarkerStepBase))); // Basically make it a power of timeMarkerStepBase
            int markersCountWithoutSubdiv = (int)(Math.Ceiling((to - from) / baseAlignedTimeMarkerStep) + 0.5);
            int markersCountWithSubdiv = markersCountWithoutSubdiv;
            int subdivIndex = 0;
            while (markersCountWithSubdiv < desiredTimeMarkersCount && subdivIndex < subdiv.Length - 1)
            {
                subdivIndex++;
                markersCountWithSubdiv = markersCountWithoutSubdiv * (subdiv[subdivIndex]);
            }
            int subDivCount = subdiv[subdivIndex];
            double subDivOffset = baseAlignedTimeMarkerStep / ((double)subDivCount);
            List<TimeMarker> timeMarkers = new List<TimeMarker>();
            double startPoint = baseAlignedTimeMarkerStep * Math.Floor(from / baseAlignedTimeMarkerStep) - baseAlignedTimeMarkerStep;
            while (startPoint < to)
            {
                startPoint += baseAlignedTimeMarkerStep;
                double x = 2.0 * (startPoint - from) / (to - from) - 1.0;
                timeMarkers.Add(new TimeMarker() { time = startPoint, x = x });
                // do subdiv:
                for (int i = 1; i < subDivCount; i++)
                {
                    double timeHere = startPoint + (double)i * subDivOffset;
                    x = 2.0 * (timeHere - from) / (to - from) - 1.0;
                    timeMarkers.Add(new TimeMarker() { time = timeHere, x = x, isSubDivMarker = true });
                }
            }



            GL.LineWidth(1);
            double actualHeight = OpenTkControl.ActualHeight;
            double desiredTimeMarkerLineHeight = 10;
            double relativeHeight = 2.0 * desiredTimeMarkerLineHeight / actualHeight;
            double fontHeight = 20;
            double relativeFontHeight = 2.0 * fontHeight / actualHeight;
            foreach (TimeMarker timeMarker in timeMarkers)
            {
                if (timeMarker.isSubDivMarker)
                {
                    GL.Color4(0.8, 0.8, 0.8, 1);
                }
                else
                {
                    GL.Color4(0, 0, 0, 1);
                }
                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex2(timeMarker.x, -1f + relativeFontHeight);
                GL.Vertex2(timeMarker.x, -1f + relativeHeight + relativeFontHeight);
                GL.End();
            }


            // Time marker texts
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.ConstantColor, BlendingFactor.OneMinusSrcColor);
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            double verticalScale = 1000.0;
            double horizontalScale = 1000.0 * actualWidth / actualHeight;
            GL.Ortho(-horizontalScale, horizontalScale, -1000.0, 1000.0, -1.0, 1.0);

            /*font.Draw("My text", 0.0f, -500f, 0.0f, new TextConfiguration()
            {
                SizeInPixels = (uint)(relativeFontHeight*1000.0),
                MaximalWidth = 1000,
                Alignment = BitmapFontLibrary.Model.TextAlignment.LeftAligned
            });*/
            foreach (TimeMarker timeMarker in timeMarkers)
            {
                double fontSize = relativeFontHeight * 1000.0;
                if (timeMarker.isSubDivMarker)
                {
                    fontSize *= 0.75;
                    GL.BlendColor(0.75f, 0.75f, 0.75f, 1.0f);
                }
                else
                {

                    fontSize *= 0.9;
                    GL.BlendColor(0f, 0f, 0f, 1.0f);
                }
                font.Draw(timeMarker.time.ToString("0.##"), (float)timeMarker.x * (float)horizontalScale - (float)horizontalScale / 2f, 1000.0f * (-1f + (float)relativeFontHeight), 0.0f, new TextConfiguration
                {
                    SizeInPixels = (uint)fontSize,
                    MaximalWidth = (float)horizontalScale,
                    Alignment = BitmapFontLibrary.Model.TextAlignment.Centered
                });
            }
            GL.Disable(EnableCap.Blend);
        }
    }
}
