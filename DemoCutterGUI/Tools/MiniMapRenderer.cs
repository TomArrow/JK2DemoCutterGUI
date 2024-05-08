using DemoCutterGUI.TableMappings;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using StbImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DemoCutterGUI.Tools
{

    class MiniMapSubSquare
    {
        Vector2[] _quadCorners;
        Vector2 topLeftCorner, bottomRightCorner;
        Vector2 topLeftPosition, bottomRightPosition;
        float totalWidth, totalHeight;

        Vector2[] corners;
        Vector2[] textureCoords;
        int _xIndex;
        int _yIndex;
        MiniMapMeta _miniMapMeta;
        public bool isLocked = false; // Just a note to the renderer to not automatically adjust area on this based on selected items, for example when we zoomed or moved around or sth.
        public MiniMapSubSquare(Vector2[] quadCorners, Vector3 centerA, int xIndex, int yIndex, float range, float totalWidthA, float totalHeightA, MiniMapMeta miniMapMeta)
        {
            _quadCorners = quadCorners;
            topLeftCorner = quadCorners[3];
            bottomRightCorner = quadCorners[1];
            totalWidth = totalWidthA;
            totalHeight = totalHeightA;
            
            _xIndex = xIndex;
            _yIndex = yIndex;
            _miniMapMeta = miniMapMeta;

            setArea(centerA, range);
        }
        public void setMap(MiniMapMeta miniMapMeta)
        {
            _miniMapMeta = miniMapMeta;
            CalculateTextureCoords();
        }

        public void setArea(Vector3 centerA, float range)
        {

            float halfRange = range * 0.5f;
            Vector2 rangeHalfVec = Vector2.One * halfRange;
            Vector2 center = new Vector2 { X = centerA[_xIndex], Y = centerA[_yIndex] };
            corners = new Vector2[] {
                center + rangeHalfVec, // top right
                center + new Vector2() { X = halfRange, Y = -halfRange },// bottom right
                center- rangeHalfVec, // bottom left
                center + new Vector2() { X = -halfRange, Y = halfRange } // top left
            };
            topLeftPosition = corners[3];
            bottomRightPosition = corners[1];
            CalculateTextureCoords();
        }
        public void setArea(Vector2[] cornersA)
        {

            corners = (Vector2[])cornersA.Clone();
            topLeftPosition = corners[3];
            bottomRightPosition = corners[1];
            CalculateTextureCoords();
        }

        private void CalculateTextureCoords()
        {

            textureCoords = new Vector2[] {
                _miniMapMeta.GetTexturePosition(corners[0],_xIndex,_yIndex),
                _miniMapMeta.GetTexturePosition(corners[1],_xIndex,_yIndex),
                _miniMapMeta.GetTexturePosition(corners[2],_xIndex,_yIndex),
                _miniMapMeta.GetTexturePosition(corners[3],_xIndex,_yIndex),
            };
        }

        /*public Vector2 GetSquarePositionXY(Vector3 position)
        {
            Vector2 proportionalPosition = new Vector2() { X = (position.X - topLeftPosition.X) / (bottomRightPosition.X - topLeftPosition.X), Y = (position.Y - bottomRightPosition.Y) / (topLeftPosition.Y - bottomRightPosition.Y) };
            Vector2 finalPos = new Vector2() { X = topLeftCorner.X + proportionalPosition.X*(bottomRightCorner.X - topLeftCorner.X), Y = bottomRightCorner.Y+proportionalPosition.Y *(topLeftCorner.Y - bottomRightCorner.Y) };
            return finalPos;
        }*/
        public Vector2 GetSquarePosition(Vector2 position)
        {
            Vector2 proportionalPosition = new Vector2() { X = (position.X - topLeftPosition.X) / (bottomRightPosition.X - topLeftPosition.X), Y = (position.Y - bottomRightPosition.Y) / (topLeftPosition.Y - bottomRightPosition.Y) };
            Vector2 finalPos = new Vector2() { X = topLeftCorner.X + proportionalPosition.X*(bottomRightCorner.X - topLeftCorner.X), Y = bottomRightCorner.Y+proportionalPosition.Y *(topLeftCorner.Y - bottomRightCorner.Y) };
            return finalPos;
        }
        public Vector2 GetSquarePosition(Vector3 position, int xIndex, int yIndex)
        {
            Vector2 proportionalPosition = new Vector2() { X = (position[xIndex] - topLeftPosition.X) / (bottomRightPosition.X - topLeftPosition.X), Y = (position[yIndex] - bottomRightPosition.Y) / (topLeftPosition.Y - bottomRightPosition.Y) };
            Vector2 finalPos = new Vector2() { X = topLeftCorner.X + proportionalPosition.X*(bottomRightCorner.X - topLeftCorner.X), Y = bottomRightCorner.Y+proportionalPosition.Y *(topLeftCorner.Y - bottomRightCorner.Y) };
            return finalPos;
        }
        public Vector2 GetSquarePosition(Vector3 position, bool clamp=false)
        {
            Vector2 proportionalPosition = new Vector2() { X = (position[_xIndex] - topLeftPosition.X) / (bottomRightPosition.X - topLeftPosition.X), Y = (position[_yIndex] - bottomRightPosition.Y) / (topLeftPosition.Y - bottomRightPosition.Y) };
            Vector2 finalPos = new Vector2() { X = topLeftCorner.X + proportionalPosition.X*(bottomRightCorner.X - topLeftCorner.X), Y = bottomRightCorner.Y+proportionalPosition.Y *(topLeftCorner.Y - bottomRightCorner.Y) };
            if (clamp)
            {
                finalPos.X = Math.Clamp(finalPos.X,topLeftCorner.X,bottomRightCorner.X);
                finalPos.Y = Math.Clamp(finalPos.Y, bottomRightCorner.Y, topLeftCorner.Y);
            }
            return finalPos;
        }

        public Vector2 getUnitVec()
        {
            return new Vector2() {
                X= 2.0f/ totalWidth,
                Y= 2.0f / totalHeight
            };

        }

        //static readonly Vector2 glToTexOffset = new Vector2(-1.0f, -1.0f);
        public Vector3 positionFromOpenGLCoords(Vector2 position)
        {
            Vector2 texPos = new Vector2() {
                X = (position.X - _quadCorners[2].X) / (_quadCorners[0].X - _quadCorners[2].X),
                Y = (position.Y - _quadCorners[2].Y) / (_quadCorners[0].Y - _quadCorners[2].Y),
            };
            //Vector2 texPos = (position - glToTexOffset) / 2.0f;
            Vector3 retVal = new Vector3();
            Vector2 range = corners[0] - corners[2];
            retVal[_xIndex] = corners[2].X + texPos.X * range.X;
            retVal[_yIndex] = corners[2].Y + texPos.Y * range.Y;
            return retVal;
            //return _miniMapMeta.GetPositionFromTexturePosition(texPos,_xIndex,_yIndex);
        }
        
        public Vector2 positionFromOpenGLCoords2D(Vector2 position)
        {
            Vector2 texPos = new Vector2() {
                X = (position.X - _quadCorners[2].X) / (_quadCorners[0].X - _quadCorners[2].X),
                Y = (position.Y - _quadCorners[2].Y) / (_quadCorners[0].Y - _quadCorners[2].Y),
            };
            //Vector2 texPos = (position - glToTexOffset) / 2.0f;
            Vector2 retVal = new Vector2();
            Vector2 range = corners[0] - corners[2];
            retVal.X = corners[2].X + texPos.X * range.X;
            retVal.Y = corners[2].Y + texPos.Y * range.Y;
            return retVal;
            //return _miniMapMeta.GetPositionFromTexturePosition(texPos,_xIndex,_yIndex);
        }

        public Vector2[] getCornerTextureCoords()
        {
            return textureCoords;
        }
        public Vector2[] getCorners()
        {
            return (Vector2[])corners.Clone();
        }

        public void DrawBorder(float lineThickness=2, Vector4? color = null )
        {
            GL.LineWidth(lineThickness);
            if (color.HasValue)
            {
                GL.Color4(color.Value); // Line color
            }
            else
            {
                GL.Color4(1f, 1f, 1f, 1f); // Line color
            }
            GL.Begin(PrimitiveType.LineStrip);

            GL.Vertex2(_quadCorners[0]);
            GL.Vertex2(_quadCorners[1]);
            GL.Vertex2(_quadCorners[2]);
            GL.Vertex2(_quadCorners[3]);
            GL.Vertex2(_quadCorners[0]);
            GL.End();
        }

        public void DrawMiniMap(int textureHandle)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);

            GL.Color4(1f, 1f, 1f, 1f); // Line color
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindTexture(TextureTarget.Texture2D, textureHandle);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(textureCoords[0]);
            GL.Vertex2(_quadCorners[0]);

            GL.TexCoord2(textureCoords[1]);
            GL.Vertex2(_quadCorners[1]);

            GL.TexCoord2(textureCoords[2]);
            GL.Vertex2(_quadCorners[2]);

            GL.TexCoord2(textureCoords[3]);
            GL.Vertex2(_quadCorners[3]);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }

    }

    class MiniMapPoint {
        public Vector3 position;
        public bool main;

        public class Bounds {
            public Vector3 mins;
            public Vector3 maxs;
            public Vector3 GetCenter()
            {
                return (mins + maxs) / 2.0f;
            }
        }

        public static Bounds GetBounds(MiniMapPoint[] points, bool onlyMain = true)
        {
            if (points is null || points.Length == 0) return null;
            Vector3 maxs = new Vector3() { X=float.NegativeInfinity,Y=float.NegativeInfinity,Z=float.NegativeInfinity };
            Vector3 mins = new Vector3() { X=float.PositiveInfinity,Y=float.PositiveInfinity, Z=float.PositiveInfinity };
            foreach(MiniMapPoint point in points)
            {
                if (onlyMain && !point.main) continue;
                maxs = Vector3.ComponentMax(maxs, point.position);
                mins = Vector3.ComponentMin(mins, point.position);
            }
            return new Bounds() { mins = mins, maxs=maxs }; 
        }
    }

    class OpenGLPerfectRectangle
    {
        public Vector2[] corners { get; private set; } = new Vector2[4];
        public OpenGLPerfectRectangle(Vector2[] cornersA)
        {
            if(cornersA.Length != 4)
            {
                throw new InvalidOperationException("Can only initialize OpenGLPerfectRectangle with four corners (array of 4 Vector2)");
            }
            if(
                cornersA[0].X != cornersA[1].X
                || cornersA[2].X != cornersA[3].X
                || cornersA[0].Y != cornersA[3].Y
                || cornersA[1].Y != cornersA[2].Y
                || cornersA[0].Y <= cornersA[1].Y
                || cornersA[0].X <= cornersA[3].X
                )
            {
                // Bit of sanity check for our use case.
                throw new InvalidOperationException("Corners provided to OpenGLPerfectRectangle must form an actual perfect unrotated rectangle, starting at top right corner, with values rising towards the top right corner.");
            }
            corners = cornersA;
        }
        // Perfectly on border does not count as inside.
        public bool InRectangle(Vector2 position)
        {
            return position.Y < corners[0].Y
            && position.Y > corners[1].Y
            && position.X < corners[0].X
            && position.X > corners[3].X;
        } 
    }

    class MiniMapRenderer
    {

        public event Action prerender;

        public ConcurrentBag<MiniMapPoint> items = new ConcurrentBag<MiniMapPoint>();
        public string map = null;

        GLWpfControl OpenTkControl = null;
        public MiniMapRenderer(GLWpfControl control)
        {
            OpenTkControl = control;
            OpenTkControl.Render += OpenTkControl_Render;
            InitOpenTK();
            //OpenTkControl.Cursor = Cursors.SizeWE;
            //System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.SizeNS;
        }

        ~MiniMapRenderer()
        {
            UnloadOpenTK();
            ClearMiniMapTextures();
        }




        OpenGLPerfectRectangle xyQuadCorners = new OpenGLPerfectRectangle(new Vector2[]
        {
            new Vector2(1.0f,1.0f),
            new Vector2(1.0f,-0.3333333f),
            new Vector2(-1.0f,-0.3333333f),
            new Vector2(-1.0f,1.0f),
        });

        OpenGLPerfectRectangle xzQuadCorners = new OpenGLPerfectRectangle(new Vector2[]
        {
            new Vector2(0.0f,-0.3333333f),
            new Vector2(0.0f,-1f),
            new Vector2(-1.0f,-1f),
            new Vector2(-1.0f,-0.3333333f),
        });
        OpenGLPerfectRectangle yzQuadCorners = new OpenGLPerfectRectangle(new Vector2[]
        {
            new Vector2(1.0f,-0.3333333f),
            new Vector2(1.0f,-1f),
            new Vector2(0.0f,-1f),
            new Vector2(0.0f,-0.3333333f),
        });

        MiniMapSubSquare xySquare = null;
        MiniMapSubSquare xzSquare = null;
        MiniMapSubSquare yzSquare = null;

        void InitOpenTK()
        {
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 2,
                MinorVersion = 1,
                RenderContinuously = false,

            };
            OpenTkControl.Loaded += OpenTkControl_Loaded;
            OpenTkControl.MouseLeftButtonDown += OpenTkControl_MouseDown;
            OpenTkControl.MouseLeftButtonUp += OpenTkControl_MouseUp;
            OpenTkControl.MouseRightButtonDown += OpenTkControl_MouseDownRight;
            OpenTkControl.MouseRightButtonUp += OpenTkControl_MouseUpRight;
            OpenTkControl.MouseDown += OpenTkControl_MouseDown1;
            OpenTkControl.MouseUp += OpenTkControl_MouseUp1;
            OpenTkControl.MouseLeave += OpenTkControl_MouseLeave;
            OpenTkControl.MouseMove += OpenTkControl_MouseMove;
            OpenTkControl.MouseWheel += OpenTkControl_MouseWheel;
            OpenTkControl.Start(settings);
        }

        void UnloadOpenTK()
        {
            OpenTkControl.Loaded -= OpenTkControl_Loaded;
            OpenTkControl.MouseLeftButtonDown -= OpenTkControl_MouseDown;
            OpenTkControl.MouseLeftButtonUp -= OpenTkControl_MouseUp;
            OpenTkControl.MouseRightButtonDown -= OpenTkControl_MouseDownRight;
            OpenTkControl.MouseRightButtonUp -= OpenTkControl_MouseUpRight;
            OpenTkControl.MouseDown -= OpenTkControl_MouseDown1;
            OpenTkControl.MouseUp -= OpenTkControl_MouseUp1;
            OpenTkControl.MouseLeave -= OpenTkControl_MouseLeave;
            OpenTkControl.MouseMove -= OpenTkControl_MouseMove;
            OpenTkControl.MouseWheel -= OpenTkControl_MouseWheel;
        }

        private void OpenTkControl_MouseUp1(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void OpenTkControl_MouseDown1(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
            if(e.ChangedButton == MouseButton.Middle) // middle button resets view
            {
                lock (dragLock)
                {
                    OpenGLPerfectRectangle mousePositionRectangle = getContainingRectangle(mousePositionRelative);

                    if (!(xySquare is null || xzSquare is null || yzSquare is null || mousePositionRectangle is null))
                    {
                        MiniMapSubSquare subSquare = null;

                        if (mousePositionRectangle == xyQuadCorners)
                        {
                            subSquare = xySquare;
                        }
                        else if (mousePositionRectangle == xzQuadCorners)
                        {
                            subSquare = xzSquare;
                        }
                        else if (mousePositionRectangle == yzQuadCorners)
                        {
                            subSquare = yzSquare;
                        }

                        subSquare.isLocked = false;
                        OpenTkControl.InvalidateVisual();
                    }
                }
            }
        }

        private void OpenTkControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            float multiplier = 1.1f;
            if(e.Delta > 0)
            {
                multiplier = 1f / multiplier;
            }
            lock (dragLock)
            {
                OpenGLPerfectRectangle mousePositionRectangle = getContainingRectangle(mousePositionRelative);

                if (!(xySquare is null || xzSquare is null || yzSquare is null || mousePositionRectangle is null))
                {
                    MiniMapSubSquare subSquare = null;

                    if (mousePositionRectangle == xyQuadCorners)
                    {
                        subSquare = xySquare;
                    }
                    else if (mousePositionRectangle == xzQuadCorners)
                    {
                        subSquare = xzSquare;
                    }
                    else if (mousePositionRectangle == yzQuadCorners)
                    {
                        subSquare = yzSquare;
                    }

                    subSquare.isLocked = true;
                    Vector2[] corners = subSquare.getCorners();
                    Vector2 ourPos = subSquare.positionFromOpenGLCoords2D(mousePositionRelative);
                    Vector2 newTopRight = corners[0];
                    Vector2 newBottomLeft = corners[2];
                    Vector2 topRightDelta = newTopRight - ourPos;
                    Vector2 bottomLeftDelta = ourPos- newBottomLeft;
                    topRightDelta *= multiplier;
                    bottomLeftDelta *= multiplier;
                    Vector2[] newCorners = new Vector2[]
                    {
                        ourPos+topRightDelta,
                        new Vector2(ourPos.X+topRightDelta.X,ourPos.Y-bottomLeftDelta.Y),
                        ourPos-bottomLeftDelta,
                        new Vector2(ourPos.X-bottomLeftDelta.X,ourPos.Y+topRightDelta.Y),
                    };
                    subSquare.setArea(newCorners);
                    OpenTkControl.InvalidateVisual();
                }
            }
            //Debug.WriteLine(e.Delta);
        }

        Vector2 mousePositionRelative = new Vector2();
        Vector2 mousePositionRelativeDragStart = new Vector2();
        Vector2 mousePositionRelativeDragEnd = new Vector2();
        object dragLock = new object();
        bool isDragging = false;
        bool dragRectangleValid = false;

        Vector2 positionDragRelativeMousePositionStartPos = new Vector2();
        Vector2 positionDragStartPos = new Vector2();
        bool isPositionDragging = false;

        static readonly Vector3 dragMinsDefault = new Vector3() { X=float.NegativeInfinity, Y = float.NegativeInfinity, Z = float.NegativeInfinity };
        static readonly Vector3 dragMaxsDefault = new Vector3() { X = float.PositiveInfinity, Y = float.PositiveInfinity, Z = float.PositiveInfinity };
        Vector3 dragMins = dragMinsDefault;
        Vector3 dragMaxs = dragMaxsDefault;
        bool dragMinMaxesSet = false;

        enum DragStartMode {
            Default,
            NorthEast,
            SouthEast,
            SouthWest,
            NorthWest
        }

        DragStartMode dragStartMode = DragStartMode.Default;
        Vector2 dragStartPos = new Vector2();

        private void UpdateMousePosition(System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(OpenTkControl);
            mousePositionRelative.X = (float)(2.0*position.X / OpenTkControl.ActualWidth-1.0);
            mousePositionRelative.Y = -(float)(2.0*position.Y / OpenTkControl.ActualHeight-1.0);
        }

        private void setPositionDragging(bool dragging)
        {
            lock (dragLock)
            {
                if(dragging != isPositionDragging)
                {
                    isPositionDragging = dragging;
                    OpenTkControl.InvalidateVisual();
                }
            }
        }
        private void setDragging(bool dragging)
        {
            lock (dragLock)
            {
                if(dragging != isDragging)
                {
                    isDragging = dragging;
                    OpenTkControl.InvalidateVisual();
                }
            }
        }
        private void resetDragMinMax()
        {
            lock (dragLock)
            {
                dragMins = dragMinsDefault; 
                dragMaxs = dragMaxsDefault;
            }
        }
        private void setPositionDraggingPositionStart(Vector2 position)
        {
            lock (dragLock)
            {
                if (isPositionDragging)
                {
                    if (positionDragRelativeMousePositionStartPos != position)
                    {
                        positionDragRelativeMousePositionStartPos = position;
                        OpenGLPerfectRectangle dragStartRectangle = getContainingRectangle(positionDragRelativeMousePositionStartPos);
                        if (!(xySquare is null || xzSquare is null || yzSquare is null || dragStartRectangle is null))
                        {
                            MiniMapSubSquare subSquare = null;

                            if (dragStartRectangle == xyQuadCorners)
                            {
                                subSquare = xySquare;
                            }
                            else if (dragStartRectangle == xzQuadCorners)
                            {
                                subSquare = xzSquare;
                            }
                            else if (dragStartRectangle == yzQuadCorners)
                            {
                                subSquare = yzSquare;
                            }

                            subSquare.isLocked = true;
                            positionDragStartPos = subSquare.positionFromOpenGLCoords2D(mousePositionRelative);
                        }
                    }
                }
            }
        }
        private void setDraggingPositionStart(Vector2 position)
        {
            lock (dragLock)
            {
                if (isDragging)
                {
                    if (mousePositionRelativeDragStart != position)
                    {
                        mousePositionRelativeDragStart = position;
                        OpenTkControl.InvalidateVisual();
                    }
                }
            }
        }
        private void setDraggingPositionEnd(Vector2 position)
        {
            lock (dragLock)
            {
                if (isDragging)
                {
                    if (mousePositionRelativeDragEnd != position)
                    {
                        mousePositionRelativeDragEnd = position;
                        OpenTkControl.InvalidateVisual();
                    }
                }
            }
        }
        OpenGLPerfectRectangle draggingRectangle = null;
        private OpenGLPerfectRectangle getContainingRectangle(Vector2 position)
        {
            if (xyQuadCorners.InRectangle(position))
            {
                return xyQuadCorners;
            }
            else if (xzQuadCorners.InRectangle(position))
            {
                return xzQuadCorners;
            }
            else if (yzQuadCorners.InRectangle(position))
            {
                return yzQuadCorners;
            }
            else
            {
                return null;
            }
        }

        // Unit being 1 pixel
        private Vector2 getUnitVec()
        {
            return new Vector2((float)(2.0/OpenTkControl.ActualWidth),(float)(2.0/OpenTkControl.ActualHeight));
        }

        /// <summary>
        /// multiply with this to convert relative vector from opengl coords to pixels
        /// </summary>
        /// <returns></returns>
        private Vector2 getInverseUnitVec() 
        {
            return new Vector2((float)(OpenTkControl.ActualWidth/ 2.0),(float)(OpenTkControl.ActualHeight/ 2.0));
        }

        private float openglDistanceInPixels(Vector2 a,Vector2 b)
        {
            return ((a-b)*getInverseUnitVec()).Length; // does the multiplication thing work? uh
        }

        private void handleMouseCursor()
        {
            lock (dragLock)
            {
                if (isDragging)
                {
                    dragStartMode = DragStartMode.Default;
                    OpenTkControl.Cursor = default;
                    return;
                }

                OpenGLPerfectRectangle mousePositionRectangle = getContainingRectangle(mousePositionRelative);

                if (dragMinMaxesSet && !(xySquare is null || xzSquare is null || yzSquare is null || mousePositionRectangle is null))
                {
                    Vector2[][] dragMinMax = getDragMinMaxsRectangles();
                    Vector2[] rectangle = null;
                    dragMinMaxesSet = true;

                    if(!(dragMinMax is null))
                    {

                        if (mousePositionRectangle == xyQuadCorners)
                        {
                            rectangle = dragMinMax[0];
                        }
                        else if (mousePositionRectangle == xzQuadCorners)
                        {
                            rectangle = dragMinMax[1];
                        }
                        else if (mousePositionRectangle == yzQuadCorners)
                        {
                            rectangle = dragMinMax[2];
                        }
                    }

                    if(!(rectangle is null))
                    {
                        Vector2 cornerTopLeft = new Vector2(rectangle[0].X,rectangle[1].Y);
                        Vector2 cornerBottomRight = new Vector2(rectangle[1].X,rectangle[0].Y);
                        if (openglDistanceInPixels(mousePositionRelative, rectangle[0]) < 5f)
                        {
                            OpenTkControl.Cursor = Cursors.ScrollSW;
                            dragStartMode = DragStartMode.SouthWest;
                            dragStartPos = rectangle[1];
                        } else if (openglDistanceInPixels(mousePositionRelative, rectangle[1]) < 5f)
                        {
                            OpenTkControl.Cursor = Cursors.ScrollNE;
                            dragStartMode = DragStartMode.NorthEast;
                            dragStartPos = rectangle[0];
                        }  else if (openglDistanceInPixels(mousePositionRelative, cornerTopLeft) < 5f)
                        {
                            OpenTkControl.Cursor = Cursors.ScrollNW;
                            dragStartMode = DragStartMode.NorthWest;
                            dragStartPos = cornerBottomRight;
                        }  else if (openglDistanceInPixels(mousePositionRelative, cornerBottomRight) < 5f)
                        {
                            OpenTkControl.Cursor = Cursors.ScrollSE;
                            dragStartMode = DragStartMode.SouthEast;
                            dragStartPos = cornerTopLeft;
                        } else
                        {
                            dragStartMode = DragStartMode.Default;
                            OpenTkControl.Cursor = default;
                        }
                    } else
                    {

                        dragStartMode = DragStartMode.Default;
                        OpenTkControl.Cursor = default;
                    }
                }
                else
                {
                    dragStartMode = DragStartMode.Default;
                    OpenTkControl.Cursor = default;
                }
            }
        }
        
        private void updateDrag(bool isStarting = false)
        {
            lock (dragLock)
            {
                if (isDragging)
                {
                    OpenGLPerfectRectangle dragStartRectangle = getContainingRectangle(mousePositionRelativeDragStart);
                    OpenGLPerfectRectangle dragEndRectangle = getContainingRectangle(mousePositionRelativeDragEnd);
                    if (dragStartRectangle == dragEndRectangle)
                    {
                        draggingRectangle = dragStartRectangle;
                        dragRectangleValid = true;
                        OpenTkControl.InvalidateVisual();
                    } else
                    {
                        // Our drag start and end is not in the same sub rectangle
                        draggingRectangle = null;
                        setDragging(false);
                        dragRectangleValid = false;
                        OpenTkControl.InvalidateVisual();
                    }
                    if (isDragging && !(xySquare is null || xzSquare is null || yzSquare is null || draggingRectangle is null))
                    {
                        dragMinMaxesSet = true;
                        if (draggingRectangle == xyQuadCorners)
                        {
                            Vector3 dragStartRealPos = xySquare.positionFromOpenGLCoords(mousePositionRelativeDragStart);
                            Vector3 dragEndRealPos = xySquare.positionFromOpenGLCoords(mousePositionRelativeDragEnd);
                            dragMins.X = Math.Min(dragStartRealPos.X, dragEndRealPos.X);
                            dragMins.Y = Math.Min(dragStartRealPos.Y, dragEndRealPos.Y);
                            dragMaxs.X = Math.Max(dragStartRealPos.X, dragEndRealPos.X);
                            dragMaxs.Y = Math.Max(dragStartRealPos.Y, dragEndRealPos.Y);
                        }
                        else if(draggingRectangle == xzQuadCorners)
                        {
                            Vector3 dragStartRealPos = xzSquare.positionFromOpenGLCoords(mousePositionRelativeDragStart);
                            Vector3 dragEndRealPos = xzSquare.positionFromOpenGLCoords(mousePositionRelativeDragEnd);
                            dragMins.X = Math.Min(dragStartRealPos.X, dragEndRealPos.X);
                            dragMins.Z = Math.Min(dragStartRealPos.Z, dragEndRealPos.Z);
                            dragMaxs.X = Math.Max(dragStartRealPos.X, dragEndRealPos.X);
                            dragMaxs.Z = Math.Max(dragStartRealPos.Z, dragEndRealPos.Z);
                        }
                        else if(draggingRectangle == yzQuadCorners)
                        {
                            Vector3 dragStartRealPos = yzSquare.positionFromOpenGLCoords(mousePositionRelativeDragStart);
                            Vector3 dragEndRealPos = yzSquare.positionFromOpenGLCoords(mousePositionRelativeDragEnd);
                            dragMins.Y = Math.Min(dragStartRealPos.Y, dragEndRealPos.Y);
                            dragMins.Z = Math.Min(dragStartRealPos.Z, dragEndRealPos.Z);
                            dragMaxs.Y = Math.Max(dragStartRealPos.Y, dragEndRealPos.Y);
                            dragMaxs.Z = Math.Max(dragStartRealPos.Z, dragEndRealPos.Z);
                        } else
                        {
                            dragMinMaxesSet = false;
                        }
                    }
                }
                else
                {
                    draggingRectangle = null;
                    setDragging(false);
                }
            }
        }
        
        public void ResetView()
        {
            lock (dragLock)
            {
                if (!isPositionDragging)
                {
                    if (!(xySquare is null || xzSquare is null || yzSquare is null))
                    {
                        xySquare.isLocked = false;
                        xzSquare.isLocked = false;
                        yzSquare.isLocked = false;
                        OpenTkControl.InvalidateVisual();
                    }
                }
            }
        }

        private void updatePositionDrag()
        {
            lock (dragLock)
            {
                if (isPositionDragging)
                {
                    OpenGLPerfectRectangle dragStartRectangle = getContainingRectangle(positionDragRelativeMousePositionStartPos);
                    if (!(xySquare is null || xzSquare is null || yzSquare is null || dragStartRectangle is null))
                    {
                        MiniMapSubSquare subSquare = null;

                        if (dragStartRectangle == xyQuadCorners)
                        {
                            subSquare = xySquare;
                        }
                        else if (dragStartRectangle == xzQuadCorners)
                        {
                            subSquare = xzSquare;
                        }
                        else if (dragStartRectangle == yzQuadCorners)
                        {
                            subSquare = yzSquare;
                        }


                        subSquare.isLocked = true;
                        Vector2 ourPos = subSquare.positionFromOpenGLCoords2D(mousePositionRelative);

                        Vector2 delta = ourPos - positionDragStartPos;


                        Vector2[] corners = subSquare.getCorners();
                        for (int i = 0; i < 4; i++)
                        {
                            corners[i] -= delta;
                        }

                        //positionDragStartPos = ourPos;

                        subSquare.setArea(corners);
                        OpenTkControl.InvalidateVisual();
                    }
                }
            }
        }

        private Vector2[] getDragRectangle()
        {
            lock (dragLock)
            {
                if (dragRectangleValid)
                {
                    return new Vector2[] {mousePositionRelativeDragStart,mousePositionRelativeDragEnd };
                }
                else
                {
                    return null;
                }
            }
        }
        public Vector3[] getDragMinMaxs()
        {
            lock (dragLock)
            {
                if (dragMinMaxesSet)
                {
                    return new Vector3[] {dragMins,dragMaxs };
                } else
                {
                    return null;
                }
            }
        }

        // in opengl coords
        private Vector2[][] getDragMinMaxsRectangles()
        {
            if (xySquare is null || xzSquare is null || yzSquare is null/* || draggingRectangle is null*/)
            {
                return null;
            }

            Vector3[] dragMinMax = getDragMinMaxs();

            if (dragMinMax is null)
            {
                return null;
            }

            List<Vector2[]> rects = new List<Vector2[]>();

            Vector2[] dragRectangle = new Vector2[] { xySquare.GetSquarePosition(dragMinMax[0], true), xySquare.GetSquarePosition(dragMinMax[1], true) };
            for (int i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 1:
                        dragRectangle = new Vector2[] { xzSquare.GetSquarePosition(dragMinMax[0], true), xzSquare.GetSquarePosition(dragMinMax[1], true) };
                        break;
                    case 2:
                        dragRectangle = new Vector2[] { yzSquare.GetSquarePosition(dragMinMax[0], true), yzSquare.GetSquarePosition(dragMinMax[1], true) };
                        break;
                }
                rects.Add(dragRectangle);
            }
            return rects.ToArray();
        }

        private void OpenTkControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateMousePosition(e);
            setDragging(false);
        }
        private void OpenTkControl_MouseUpRight(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateMousePosition(e);
            setPositionDragging(false);
        }

        private void OpenTkControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateMousePosition(e);
            handleMouseCursor();
            updatePositionDrag();
            updateDrag();
            setDraggingPositionEnd(mousePositionRelative);
            updateDrag(false);
        }

        private void OpenTkControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateMousePosition(e);
            setDragging(false);
        }

        private void OpenTkControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateMousePosition(e);
            setDragging(true);
            if(dragStartMode == DragStartMode.Default)
            {
                setDraggingPositionStart(mousePositionRelative);
            }
            else
            {
                setDraggingPositionStart(dragStartPos);
            }
            setDraggingPositionEnd(mousePositionRelative);
            updateDrag(true);
        }
        private void OpenTkControl_MouseDownRight(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateMousePosition(e);
            setPositionDragging(true);
            setPositionDraggingPositionStart(mousePositionRelative);
            updatePositionDrag();
        }

        Dictionary<string, Tuple<int[], MiniMapMeta>> mapMinimapTextures = new Dictionary<string, Tuple<int[], MiniMapMeta>>();

        bool isEnded = false;
        void ClearMiniMapTextures()
        {
            lock (mapMinimapTextures)
            {
                isEnded = true;
                foreach (var kvp in mapMinimapTextures)
                {
                    if(!(kvp.Value.Item1 is null))
                    {
                        foreach(int textureHandle in kvp.Value.Item1)
                        {
                            GL.DeleteTexture(textureHandle);
                        }
                    }
                }
                mapMinimapTextures.Clear();
                OpenTkControl.Render -= OpenTkControl_Render;
            }
        }

        int MakeMiniMapTexture(string filename)
        {

            int handle = GL.GenTexture();

            if (handle == (int)ErrorCode.InvalidValue)
            {
                Debug.WriteLine($"ErrorCode.InvalidValue gotten on GL.GenTexture for {filename} minimap texture generation.");
                return -1;
            }


            GL.BindTexture(TextureTarget.Texture2D, handle);
            StbImage.stbi_set_flip_vertically_on_load(1);

            ImageResult img = ImageResult.FromStream(File.OpenRead(filename), ColorComponents.RedGreenBlueAlpha);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);


            return handle;
        }

        int[] GetMinimapTextures(string mapname, ref MiniMapMeta miniMapMeta)
        {
            lock (mapMinimapTextures) { 
                if (mapMinimapTextures.ContainsKey(mapname))
                {
                    miniMapMeta = mapMinimapTextures[mapname].Item2;
                    return mapMinimapTextures[mapname].Item1;
                }

                string miniMapPath = Path.Combine(Tools.BSPToMiniMap.minimapsPath, mapname.ToLowerInvariant());
                string miniMapMetaFile = Path.Combine(miniMapPath, "meta.json");
                string miniMapImageXY = Path.Combine(miniMapPath, "xy.png");
                string miniMapImageXZ = Path.Combine(miniMapPath, "xz.png");
                string miniMapImageYZ = Path.Combine(miniMapPath, "yz.png");

                if (!File.Exists(miniMapMetaFile) || !File.Exists(miniMapImageXY) || !File.Exists(miniMapImageXZ) || !File.Exists(miniMapImageYZ))
                {
                    Debug.WriteLine($"Minimap meta or image not found for {mapname} minimap texture generation.");
                    miniMapMeta = null;
                    return (mapMinimapTextures[mapname] = new Tuple<int[], MiniMapMeta>(null, null)).Item1;
                }

                miniMapMeta = BSPToMiniMap.DecodeMiniMapMeta(File.ReadAllText(miniMapMetaFile));

                if (miniMapMeta is null)
                {
                    Debug.WriteLine($"Failed decoding metadata for {mapname} minimap texture generation.");
                    miniMapMeta = null;
                    return (mapMinimapTextures[mapname] = new Tuple<int[], MiniMapMeta>(null, null)).Item1;
                }

                int[] textureHandles = new int[]
                {
                    MakeMiniMapTexture(miniMapImageXY),
                    MakeMiniMapTexture(miniMapImageXZ),
                    MakeMiniMapTexture(miniMapImageYZ),
                };

                if (textureHandles[0] == -1 || textureHandles[1] == -1 || textureHandles[2] == -1)
                {
                    textureHandles = null;
                    miniMapMeta = null;
                }

                return (mapMinimapTextures[mapname] = new Tuple<int[], MiniMapMeta>(textureHandles, miniMapMeta)).Item1;

            }
        }


        const double maxFps = 165;
        const double minTimeDelta = 1000.0 / maxFps;
        DateTime lastUpdate = DateTime.Now;

        private void OpenTkControl_Render(TimeSpan obj)
        {
            lock (mapMinimapTextures)
            {
                if (isEnded)
                {
                    return;
                }
            }
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            double timeSinceLast = (DateTime.Now - lastUpdate).TotalMilliseconds;
            //if (timeSinceLast < minTimeDelta) System.Threading.Thread.Sleep((int)(minTimeDelta- timeSinceLast));
            if (timeSinceLast > minTimeDelta) ; //OpenTkControl.InvalidateVisual();
            else return;

            prerender?.Invoke();

            GL.ClearColor(Color4.White);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            

            if (string.IsNullOrWhiteSpace(map)) return;

            double actualWidth = OpenTkControl.ActualWidth;
            double actualHeight = OpenTkControl.ActualHeight;

            //if (!File.Exists())
            //{

            //}

            MiniMapMeta miniMapMeta = null;
            int[] textureHandles = GetMinimapTextures(map, ref miniMapMeta);

            if (textureHandles is null || miniMapMeta is null) return; // No minimap texture found

            MiniMapPoint[] points = items.ToArray();

            MiniMapPoint.Bounds bounds = MiniMapPoint.GetBounds(points);
            Vector3 center = bounds.GetCenter();

            Vector3 boundsRanges = bounds.maxs - bounds.mins;
            float xyRange = Math.Max(Math.Max(boundsRanges.X,boundsRanges.Y) + 100.0f, 2000.0f);
            float xzRange = Math.Max(Math.Max(boundsRanges.X,boundsRanges.Z) + 100.0f, 500.0f);
            float yzRange = Math.Max(Math.Max(boundsRanges.Y,boundsRanges.Z) + 100.0f, 500.0f);


            Vector3 xyRangeHalfVec = Vector3.One * xyRange * 0.5f;
            float xyRangeHalf = xyRange * 0.5f;

            if(xySquare is null || !xySquare.isLocked)
            {
                xySquare = new MiniMapSubSquare(xyQuadCorners.corners, center, 0, 1, xyRange, (float)actualWidth, (float)actualHeight, miniMapMeta);
            } else
            {
                xySquare.setMap(miniMapMeta);
            }
            if(xzSquare is null || !xzSquare.isLocked)
            {
                xzSquare = new MiniMapSubSquare(xzQuadCorners.corners, center, 0, 2, xzRange, (float)actualWidth, (float)actualHeight, miniMapMeta);
            }
            else
            {
                xzSquare.setMap(miniMapMeta);
            }
            if (yzSquare is null || !yzSquare.isLocked)
            {
                yzSquare = new MiniMapSubSquare(yzQuadCorners.corners, center, 1, 2, yzRange, (float)actualWidth, (float)actualHeight, miniMapMeta);
            }
            else
            {
                yzSquare.setMap(miniMapMeta);
            }


            xySquare.DrawMiniMap(textureHandles[0]);
            xzSquare.DrawMiniMap(textureHandles[1]);
            yzSquare.DrawMiniMap(textureHandles[2]);

            xySquare.DrawBorder(default, xySquare.isLocked ? new Vector4(0, 0, 1, 1) : default);
            xzSquare.DrawBorder(default, xzSquare.isLocked ? new Vector4(0, 0, 1, 1) : default);
            yzSquare.DrawBorder(default, yzSquare.isLocked ? new Vector4(0, 0, 1, 1) : default);

            GL.LineWidth(2);
            GL.Begin(PrimitiveType.Lines);

            Vector2 crossSize = xySquare.getUnitVec()*10.0f;
            foreach (var point in points)
            {
                if (point.main)
                {

                    GL.Color4(1f, 0f, 0f, 1f); // Line color
                }
                else
                {
                    GL.Color4(1f, 1f, 0f, 1f); // Line color
                }
                Vector2 position = xySquare.GetSquarePosition(point.position);

                if (xyQuadCorners.InRectangle(position))
                {
                    GL.Vertex3(position.X, position.Y - crossSize.Y, 0);
                    GL.Vertex3(position.X, position.Y + crossSize.Y, 0);
                    GL.Vertex3(position.X - crossSize.X, position.Y, 0);
                    GL.Vertex3(position.X + crossSize.X, position.Y, 0);
                }

                position = xzSquare.GetSquarePosition(point.position);
                if (xzQuadCorners.InRectangle(position))
                {
                    GL.Vertex3(position.X, position.Y - crossSize.Y, 0);
                    GL.Vertex3(position.X, position.Y + crossSize.Y, 0);
                    GL.Vertex3(position.X - crossSize.X, position.Y, 0);
                    GL.Vertex3(position.X + crossSize.X, position.Y, 0);
                }

                position = yzSquare.GetSquarePosition(point.position);
                if (yzQuadCorners.InRectangle(position))
                {
                    GL.Vertex3(position.X, position.Y - crossSize.Y, 0);
                    GL.Vertex3(position.X, position.Y + crossSize.Y, 0);
                    GL.Vertex3(position.X - crossSize.X, position.Y, 0);
                    GL.Vertex3(position.X + crossSize.X, position.Y, 0);
                }
            }
            GL.End();
            
            Vector2[][] dragMinMax = getDragMinMaxsRectangles();
            if(dragMinMax != null)
            {
                GL.LineWidth(2);
                GL.Color4(0f, 1f, 0f, 1f); // Line color

                for (int i = 0; i < 3; i++)
                {
                    Vector2[] dragRectangle = dragMinMax[i];
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex3(dragRectangle[0].X, dragRectangle[0].Y, 0);
                    GL.Vertex3(dragRectangle[1].X, dragRectangle[0].Y, 0);
                    GL.Vertex3(dragRectangle[1].X, dragRectangle[1].Y, 0);
                    GL.Vertex3(dragRectangle[0].X, dragRectangle[1].Y, 0);
                    GL.Vertex3(dragRectangle[0].X, dragRectangle[0].Y, 0);
                    GL.End();
                }
            }

            lastUpdate = DateTime.Now;

        }

        private void OpenTkControl_Loaded(object sender, RoutedEventArgs e)
        {
            // var ifc = new InstalledFontCollection();


        }

        public void Update()
        {

            OpenTkControl.InvalidateVisual();
        }
    }
}
