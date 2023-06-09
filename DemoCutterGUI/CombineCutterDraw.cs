﻿using BitmapFontLibrary;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float getXFromTime(float time)
        {
            return (float)((time - scrubControl.currentMin) / (scrubControl.currentMax - scrubControl.currentMin)) * 2f - 1f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double getRelativeSizeHorz(double size)
        {
            return size / OpenTkControl.ActualWidth * 2.0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double getRelativeSizeVert(double size)
        {
            return size / OpenTkControl.ActualHeight * 2.0;
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
            double scrubPositionX = (scrubControl.scrubPosition - scrubControl.currentMin)/(scrubControl.currentMax- scrubControl.currentMin)*2.0-1.0 ;
            GL.Color4(0.75, 0.75, 0.95, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(scrubPositionX, -1f);
            GL.Vertex2(scrubPositionX, 1f);
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

        class DemoWrapper {
            public Demo demo;
            public float timelineTimeStart = 0;
            public float timelineTimeEnd = 0;
        }

        static readonly double goldenRatio = (1.0 + Math.Sqrt(5.0)) / 2.0;
        static readonly float goldenRatioF = (float)goldenRatio;
        static readonly double goldenRatioMultiplier = goldenRatio / (goldenRatio+1.0);
        static readonly float goldenRatioMultiplierF = (float)goldenRatioMultiplier;
        static readonly double goldenRatioMultiplierInverse = 1.0- goldenRatioMultiplier;
        static readonly float goldenRatioMultiplierInverseF = (float)goldenRatioMultiplierInverse;

        public void DrawDemos(double actualWidth, double to, double from)
        {
            List<DemoWrapper> visibleDemos = new List<DemoWrapper>();
            demos.Foreach((in Demo demo)=> {
                float demoSpeed = 0;
                float demoHighlightTimeInCombinedDemoTime = points.lineAtSimple(demo.highlightDemoTime,ref demoSpeed);
                float combinedDemoStartTime = demoHighlightTimeInCombinedDemoTime - demo.highlightOffset;
                float timelineTimeStart = points.lineAtInverse(combinedDemoStartTime);
                float possibleEndTime = demoHighlightTimeInCombinedDemoTime + 10000; // Maybe make it configurable at some point.
                float timelineTimeEnd = points.lineAtInverse(possibleEndTime);

                // Overlap with current timeline section?
                if(timelineTimeStart < to && timelineTimeEnd > from)
                {
                    // It's visible
                    visibleDemos.Add(new DemoWrapper() { 
                        demo=demo,
                        timelineTimeStart=timelineTimeStart,
                        timelineTimeEnd = timelineTimeEnd,
                    });
                }

            });

            if (visibleDemos.Count == 0) return;


            // Preparations for text
            double actualHeight = OpenTkControl.ActualHeight;
            double fontHeight =13;
            double relativeFontHeight = 2.0 * fontHeight / actualHeight;
            double verticalScale = 1000.0;
            double horizontalScale = 1000.0 * actualWidth / actualHeight;

            // Now start drawing them
            double singleHeight = 2.0/(double)visibleDemos.Count;
            double singlePaddingSize = 2;
            double singleBorder = Math.Min(singleHeight * 0.5, 2.0*singlePaddingSize/actualHeight);
            double singleBorderHorz = 2.0*singlePaddingSize/actualWidth;
            double offset = 1.0;
            foreach (DemoWrapper demo in visibleDemos)
            {
                double xLeft = 2.0 * ((demo.timelineTimeStart - from) / (to - from)) - 1.0;
                double xRight = 2.0 * ((demo.timelineTimeEnd - from) / (to - from)) - 1.0;
                double xHighlight = 2.0 * ((demo.demo.highlightDemoTime - from) / (to - from)) - 1.0;
                double yTop = offset;
                double yBottom = offset - singleHeight;

                // Box
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Color4(1.0, 0.8, 0.8, 1);
                GL.Begin(PrimitiveType.Quads);

                GL.Vertex3(xLeft, yTop- singleBorder, 0);
                GL.Vertex3(xRight, yTop- singleBorder, 0);
                GL.Vertex3(xRight, yBottom+ singleBorder, 0);
                GL.Vertex3(xLeft, yBottom+ singleBorder, 0);
                GL.End();

                // Darker outline
                GL.Color4(0.7, 0.4, 0.4, 1);
                GL.LineWidth(1);
                GL.Begin(PrimitiveType.LineLoop);

                GL.Vertex3(xLeft, yTop - singleBorder, 0);
                GL.Vertex3(xRight, yTop - singleBorder, 0);
                GL.Vertex3(xRight, yBottom + singleBorder, 0);
                GL.Vertex3(xLeft, yBottom + singleBorder, 0);
                GL.End();


                // Highlight point
                GL.Color4(0f, 0f, 1f, 1f);
                GL.LineWidth(3);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(xHighlight, yTop, 0);
                GL.Vertex3(xHighlight, yBottom, 0);
                GL.End();

                // Additional highlight points
                double sizeMultiplierVert = 2.0 / actualHeight;
                double sizeMultiplierHorz = 2.0  / actualWidth;
                double iconSize = 7.0;
                foreach (AdditionalHighlight ah in demo.demo.additionalHighlights)
                {
                    float demoSpeed = 0;
                    float aHDemoTime = points.lineAtSimple(demo.demo.highlightDemoTime, ref demoSpeed) - demo.demo.highlightOffset + ah.time;
                    double aHTimelineTime = points.lineAtInverse(aHDemoTime);

                    double xAHighlight = 2.0 * ((aHTimelineTime - from) / (to - from)) - 1.0;
                    if(ah.type == AdditionalHighlight.Type.METAEVENT_CAPTURE || ah.type == AdditionalHighlight.Type.METAEVENT_TEAMCAPTURE || ah.type == AdditionalHighlight.Type.METAEVENT_ENEMYTEAMCAPTURE)
                    {
                        // Draw a shitty little "flag"

                        GL.LineWidth(1);
                        double yCenter = (yTop - yBottom) * goldenRatioMultiplier + yBottom;
                        double yFlagBottom = yCenter - iconSize * sizeMultiplierVert;
                        double yFlagTop = yCenter + iconSize * sizeMultiplierVert;
                        double xFlagLeft = xAHighlight - iconSize * sizeMultiplierHorz;
                        for (int i = 0; i < 2; i++)
                        {
                            // Two passes. Fill, then lines
                            if (i == 0)
                            {
                                switch (ah.type)
                                {
                                    case AdditionalHighlight.Type.METAEVENT_CAPTURE:
                                        GL.Color4(0f, 1f, 0f, 1f);
                                        break;
                                    case AdditionalHighlight.Type.METAEVENT_TEAMCAPTURE:
                                        GL.Color4(0f, 0f, 1f, 1f);
                                        break;
                                    case AdditionalHighlight.Type.METAEVENT_ENEMYTEAMCAPTURE:
                                        GL.Color4(1f, 0f, 0f, 1f);
                                        break;
                                    default:
                                        GL.Color4(1f, 1f, 1f, 1f); // Shouldn't really happen but this will show me if there's a bug here
                                        break;
                                }
                                GL.Begin(PrimitiveType.Quads);
                            } else
                            {
                                GL.Color4(0f, 0f, 0f, 1f); // Line color
                                GL.Begin(PrimitiveType.LineStrip);
                            }
                            GL.Vertex3(xAHighlight, i==0 ? yCenter: yFlagBottom, 0);
                            GL.Vertex3(xAHighlight, yFlagTop, 0);
                            GL.Vertex3(xFlagLeft, yFlagTop, 0);
                            GL.Vertex3(xFlagLeft, yCenter, 0);
                            if (i == 1)
                            {
                                GL.Vertex3(xAHighlight, yCenter, 0);
                            }
                            GL.End();
                        }
                        
                    } else if(ah.type == AdditionalHighlight.Type.METAEVENT_JUMP)
                    {
                        // Draw a little "arrow" pointing down

                        GL.LineWidth(1);
                        double yJumpBottom = yBottom + 2.0 * sizeMultiplierVert;
                        double yJumpTop = yJumpBottom + iconSize * sizeMultiplierVert;
                        double xJumpOffset = iconSize * sizeMultiplierHorz;
                         
                        GL.Color4(0.2f, 0.2f, 0f, 1f); // Line color
                        GL.Begin(PrimitiveType.LineStrip);
                        
                        GL.Vertex3(xAHighlight + xJumpOffset, yJumpTop, 0);
                        GL.Vertex3(xAHighlight, yJumpBottom, 0);
                        GL.Vertex3(xAHighlight - xJumpOffset, yJumpTop, 0);
                        GL.End();
                        
                    } else if(ah.type == AdditionalHighlight.Type.METAEVENT_SABERBLOCK || ah.type == AdditionalHighlight.Type.METAEVENT_SABERHIT || ah.type == AdditionalHighlight.Type.METAEVENT_EFFECT || ah.type == AdditionalHighlight.Type.METAEVENT_DEATH)
                    {


                        // Draw a cross
                        int passes = 1; // bright colors need second pass of darker underneath to become visible.
                        Color4 color = new Color4();
                        switch (ah.type)
                        {
                            default:
                            case AdditionalHighlight.Type.METAEVENT_SABERBLOCK:
                                color = new Color4(1f, 1f, 0f, 1f);
                                break;
                            case AdditionalHighlight.Type.METAEVENT_SABERHIT:
                                color = new Color4(1f, 0.75f, 0f, 1f);
                                passes = 2;
                                break;
                            case AdditionalHighlight.Type.METAEVENT_EFFECT:
                                color = new Color4(1f, 0f, 1f, 1f);
                                break;
                            case AdditionalHighlight.Type.METAEVENT_DEATH:
                                color = new Color4(1f, 0f, 0f, 1f);
                                break;
                        }

                        double yCrossCenter = (yTop - yBottom) * goldenRatioMultiplierInverse + yBottom;
                        double yCrossOffset = iconSize * sizeMultiplierVert;
                        double xCrossOffset = iconSize * sizeMultiplierHorz;
                        double passOffseHorz = 1.0 * sizeMultiplierHorz;
                        double passOffseVert = 1.0 * sizeMultiplierVert;

                        for (int i = 0; i < passes; i++)
                        {
                            double actualPassOffsetHorz = 0;
                            double actualPassOffsetVert = 0;
                            GL.LineWidth(2);

                            if (passes > 1 && i == 0)
                            {
                                GL.Color4(color.R / 1.3,color.G / 1.3, color.B / 1.3, color.A);
                                actualPassOffsetHorz = passOffseHorz;
                                actualPassOffsetVert = passOffseVert;
                                GL.LineWidth(4);
                            } else
                            {
                                GL.Color4(color);
                            }
                            GL.Begin(PrimitiveType.Lines);

                            GL.Vertex3(xAHighlight + xCrossOffset + actualPassOffsetHorz, yCrossCenter + yCrossOffset - actualPassOffsetVert, 0);
                            GL.Vertex3(xAHighlight - xCrossOffset + actualPassOffsetHorz, yCrossCenter - yCrossOffset - actualPassOffsetVert, 0);
                            GL.Vertex3(xAHighlight - xCrossOffset + actualPassOffsetHorz, yCrossCenter + yCrossOffset - actualPassOffsetVert, 0);
                            GL.Vertex3(xAHighlight + xCrossOffset + actualPassOffsetHorz, yCrossCenter - yCrossOffset - actualPassOffsetVert, 0);
                            GL.End();
                        }
                        
                    } else
                    {
                        GL.LineWidth(1);
                        switch (ah.type)
                        {
                            default:
                            case AdditionalHighlight.Type.METAEVENT_NONE:
                                GL.Color4(0f, 0f, 1f, 1f);
                                break;
                            case AdditionalHighlight.Type.METAEVENT_KILL:
                                GL.Color4(1f, 0f, 0f, 1f);
                                break;
                            case AdditionalHighlight.Type.METAEVENT_RETURN:
                                GL.Color4(0f, 0f, 0f, 1f);
                                GL.LineWidth(2);
                                break;
                        }
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex3(xAHighlight, yTop, 0);
                        GL.Vertex3(xAHighlight, yBottom, 0);
                        GL.End();
                    }
                }


                // Text
                GL.Enable(EnableCap.Blend);
                GL.Color4(1f, 1f, 1f, 1f);
                GL.BlendColor(0f, 0f, 0f, 1.0f);
                GL.BlendFunc(BlendingFactor.ConstantColor, BlendingFactor.OneMinusSrcColor);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(-horizontalScale, horizontalScale, -1000.0, 1000.0, -1.0, 1.0);
                double fontSize = relativeFontHeight * 1000.0;
                double textXLeft = Math.Max(-1.0 + singleBorderHorz, singleBorderHorz + xLeft);
                double maxWidth = Math.Max(0.001, (xRight-singleBorderHorz)-textXLeft);
                font.Draw(demo.demo.name, (float)textXLeft * (float)horizontalScale, 1000.0f * ((float)yTop - (float)singleBorder*2.0f), 0.0f, new TextConfiguration
                {
                    SizeInPixels = (uint)fontSize,
                    MaximalWidth = (float)(maxWidth*horizontalScale),
                    Alignment = BitmapFontLibrary.Model.TextAlignment.LeftAligned
                });
                GL.Disable(EnableCap.Blend);
                GL.LoadIdentity();


                offset = yBottom;
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);






        }



        public void DrawSpeedGraph(double actualWidth, double to, double from)
        {
            // Actual speed graph
            GL.Color4(0, 0, 0, 1);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            //GL.Color4(1f, 0f, 0f, 1f);
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

            // Line dots
            float lineDotSize =8;
            float lineDotSizeHorz = (float)getRelativeSizeHorz(lineDotSize);
            float lineDotSizeVert = (float)getRelativeSizeVert(lineDotSize);
            GL.Begin(PrimitiveType.Triangles);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            points.Foreach((in DemoLinePoint linePoint) =>
            {
                double speed = linePoint.effectiveSpeed;
                float y = getYFromDemoSpeed((float)speed);
                float x = getXFromTime(linePoint.time);
                GL.Color4(1f, 0f, 0f, 1f);
                GL.Vertex3(x,y,0);
                GL.Vertex3(x- lineDotSizeHorz/2.0f, y- lineDotSizeVert, 0);
                GL.Vertex3(x + lineDotSizeHorz / 2.0f, y- lineDotSizeVert, 0);
            });
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
