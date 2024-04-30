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

namespace DemoCutterGUI.Tools
{

    class MiniMapSubSquare
    {
        Vector2[] _quadCorners;
        Vector2 topLeftCorner, bottomRightCorner;
        Vector2 topLeftPosition, bottomRightPosition;
        float totalWidth, totalHeight;
        /*public MiniMapSubSquare(Vector2 topLeftCornerA, Vector2 bottomRightCornerA, Vector3 topLeftPositionA, Vector3 bottomRightPositionA, float totalWidthA, float totalHeightA)
        {
            topLeftCorner = topLeftCornerA;
            bottomRightCorner = bottomRightCornerA;
            topLeftPosition = topLeftPositionA;
            bottomRightPosition = bottomRightPositionA;
            totalWidth = totalWidthA;
            totalHeight = totalHeightA;
        }*/

        Vector2[] corners;
        Vector2[] textureCoords;
        int _xIndex;
        int _yIndex;
        public MiniMapSubSquare(Vector2[] quadCorners, Vector3 centerA, int xIndex, int yIndex, float range, float totalWidthA, float totalHeightA, MiniMapMeta miniMapMeta)
        {
            _quadCorners = quadCorners;
            topLeftCorner = quadCorners[3];
            bottomRightCorner = quadCorners[1];
            totalWidth = totalWidthA;
            totalHeight = totalHeightA;
            float halfRange = range * 0.5f;
            Vector2 rangeHalfVec = Vector2.One * halfRange;
            Vector2 center = new Vector2 { X= centerA[xIndex], Y=centerA[yIndex] };
            corners = new Vector2[] {
                center + rangeHalfVec, // top right
                center + new Vector2() { X = halfRange, Y = -halfRange },// bottom right
                center- rangeHalfVec, // bottom left
                center + new Vector2() { X = -halfRange, Y = halfRange } // top left
            };
            textureCoords = new Vector2[] {
                miniMapMeta.GetTexturePosition(corners[0],xIndex,yIndex),
                miniMapMeta.GetTexturePosition(corners[1],xIndex,yIndex),
                miniMapMeta.GetTexturePosition(corners[2],xIndex,yIndex),
                miniMapMeta.GetTexturePosition(corners[3],xIndex,yIndex),
            };
            topLeftPosition = corners[3];
            bottomRightPosition = corners[1];
            _xIndex = xIndex;
            _yIndex = yIndex;
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
        public Vector2 GetSquarePosition(Vector3 position)
        {
            Vector2 proportionalPosition = new Vector2() { X = (position[_xIndex] - topLeftPosition.X) / (bottomRightPosition.X - topLeftPosition.X), Y = (position[_yIndex] - bottomRightPosition.Y) / (topLeftPosition.Y - bottomRightPosition.Y) };
            Vector2 finalPos = new Vector2() { X = topLeftCorner.X + proportionalPosition.X*(bottomRightCorner.X - topLeftCorner.X), Y = bottomRightCorner.Y+proportionalPosition.Y *(topLeftCorner.Y - bottomRightCorner.Y) };
            return finalPos;
        }

        public Vector2 getUnitVec()
        {
            return new Vector2() {
                X= 2.0f/ totalWidth,
                Y= 2.0f / totalHeight
            };

        }

        public Vector2[] getCornerTextureCoords()
        {
            return textureCoords;
        }
        public Vector2[] getCorners()
        {
            return corners;
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

        public static Bounds GetBounds(MiniMapPoint[] points)
        {
            if (points is null || points.Length == 0) return null;
            Vector3 maxs = new Vector3() { X=float.NegativeInfinity,Y=float.NegativeInfinity,Z=float.NegativeInfinity };
            Vector3 mins = new Vector3() { X=float.PositiveInfinity,Y=float.PositiveInfinity, Z=float.PositiveInfinity };
            foreach(MiniMapPoint point in points)
            {
                maxs = Vector3.ComponentMax(maxs, point.position);
                mins = Vector3.ComponentMin(mins, point.position);
            }
            return new Bounds() { mins = mins, maxs=maxs }; 
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
        }


        ~MiniMapRenderer()
        {
            ClearMiniMapTextures();
        }


        void InitOpenTK()
        {
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 2,
                MinorVersion = 1,
                RenderContinuously = false,

            };
            OpenTkControl.Loaded += OpenTkControl_Loaded;
            OpenTkControl.Start(settings);
        }

        Dictionary<string, Tuple<int, MiniMapMeta>> mapMinimapTextures = new Dictionary<string, Tuple<int, MiniMapMeta>>();

        bool isEnded = false;
        void ClearMiniMapTextures()
        {
            lock (mapMinimapTextures)
            {
                isEnded = true;
                foreach (var kvp in mapMinimapTextures)
                {
                    GL.DeleteTexture(kvp.Value.Item1);
                }
                mapMinimapTextures.Clear();
                OpenTkControl.Render -= OpenTkControl_Render;
            }
        }

        int GetMinimapTexture(string mapname, ref MiniMapMeta miniMapMeta)
        {
            lock (mapMinimapTextures) { 
                if (mapMinimapTextures.ContainsKey(mapname))
                {
                    miniMapMeta = mapMinimapTextures[mapname].Item2;
                    return mapMinimapTextures[mapname].Item1;
                }

                string miniMapPath = Path.Combine(Tools.BSPToMiniMap.minimapsPath, mapname.ToLowerInvariant());
                string miniMapMetaFile = Path.Combine(miniMapPath, "meta.json");
                string miniMapImage = Path.Combine(miniMapPath, "xy.png");

                if (!File.Exists(miniMapMetaFile) || !File.Exists(miniMapImage))
                {
                    Debug.WriteLine($"Minimap meta or image not found for {mapname} minimap texture generation.");
                    miniMapMeta = null;
                    return (mapMinimapTextures[mapname] = new Tuple<int, MiniMapMeta>(-1, null)).Item1;
                }

                int handle = GL.GenTexture();

                if (handle == (int)ErrorCode.InvalidValue)
                {
                    Debug.WriteLine($"ErrorCode.InvalidValue gotten on GL.GenTexture for {mapname} minimap texture generation.");
                    miniMapMeta = null;
                    return (mapMinimapTextures[mapname] = new Tuple<int, MiniMapMeta>(-1, null)).Item1;
                }

                miniMapMeta = BSPToMiniMap.DecodeMiniMapMeta(File.ReadAllText(miniMapMetaFile));

                if (miniMapMeta is null)
                {
                    Debug.WriteLine($"Failed decoding metadata for {mapname} minimap texture generation.");
                    miniMapMeta = null;
                    return (mapMinimapTextures[mapname] = new Tuple<int, MiniMapMeta>(-1, null)).Item1;
                }

                GL.BindTexture(TextureTarget.Texture2D, handle);
                StbImage.stbi_set_flip_vertically_on_load(1);

                ImageResult img = ImageResult.FromStream(File.OpenRead(miniMapImage), ColorComponents.RedGreenBlueAlpha);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                GL.BindTexture(TextureTarget.Texture2D, 0);

                return (mapMinimapTextures[mapname] = new Tuple<int, MiniMapMeta>(handle, miniMapMeta)).Item1;
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
            int textureHandle = GetMinimapTexture(map, ref miniMapMeta);

            if (textureHandle < 0 || miniMapMeta is null) return; // No minimap texture found

            MiniMapPoint[] points = items.ToArray();

            MiniMapPoint.Bounds bounds = MiniMapPoint.GetBounds(points);
            Vector3 center = bounds.GetCenter();

            Vector3 boundsRanges = bounds.maxs - bounds.mins;
            float xyRange = Math.Max(Math.Max(boundsRanges.X,boundsRanges.Y) + 100.0f, 2000.0f);
            float xzRange = Math.Max(Math.Max(boundsRanges.X,boundsRanges.Z) + 100.0f, 1000.0f);
            float yzRange = Math.Max(Math.Max(boundsRanges.Y,boundsRanges.Z) + 100.0f, 1000.0f);


            Vector2[] xyQuadCorners = new Vector2[]
            {
                new Vector2(1.0f,1.0f),
                new Vector2(1.0f,-0.3333333f),
                new Vector2(-1.0f,-0.3333333f),
                new Vector2(-1.0f,1.0f),
            };

            Vector2[] xzQuadCorners = new Vector2[]
            {
                new Vector2(0.0f,-0.3333333f),
                new Vector2(0.0f,-1f),
                new Vector2(-1.0f,-1f),
                new Vector2(-1.0f,-0.3333333f),
            };
            Vector2[] yzQuadCorners = new Vector2[]
            {
                new Vector2(1.0f,-0.3333333f),
                new Vector2(1.0f,-1f),
                new Vector2(0.0f,-1f),
                new Vector2(0.0f,-0.3333333f),
            };

            Vector3 xyRangeHalfVec = Vector3.One * xyRange * 0.5f;
            float xyRangeHalf = xyRange * 0.5f;
            /*
            Vector3[] xyCorners = new Vector3[] {
                center + xyRangeHalfVec, // top right
                center + new Vector3() { X = xyRangeHalf, Y = -xyRangeHalf },// bottom right
                center- xyRangeHalfVec, // bottom left
                center + new Vector3() { X = -xyRangeHalf, Y = xyRangeHalf } // top left
            };

            Vector2[] xyTextureCoords = new Vector2[] {
                miniMapMeta.GetTexturePositionXY(xyCorners[0]),
                miniMapMeta.GetTexturePositionXY(xyCorners[1]),
                miniMapMeta.GetTexturePositionXY(xyCorners[2]),
                miniMapMeta.GetTexturePositionXY(xyCorners[3]),
            };*/


            //MiniMapSubSquare xySquare = new MiniMapSubSquare(xyQuadCorners[3], xyQuadCorners[1], xyCorners[3], xyCorners[1],(float)actualWidth, (float)actualHeight);
            MiniMapSubSquare xySquare = new MiniMapSubSquare(xyQuadCorners, center,0,1, xyRange, (float)actualWidth, (float)actualHeight,miniMapMeta);
            //var xyTextureCoords = xySquare.getCornerTextureCoords();

            xySquare.DrawMiniMap(textureHandle);

            GL.LineWidth(2);
            GL.Color4(1f, 0f, 0f, 1f); // Line color
            GL.Begin(PrimitiveType.Lines);

            Vector2 crossSize = xySquare.getUnitVec()*10.0f;
            foreach (var point in points)
            {
                Vector2 position = xySquare.GetSquarePosition(point.position,0,1);

                GL.Vertex3(position.X, position.Y- crossSize.Y, 0);
                GL.Vertex3(position.X, position.Y+ crossSize.Y, 0);
                GL.Vertex3(position.X - crossSize.X, position.Y, 0);
                GL.Vertex3(position.X + crossSize.X, position.Y, 0);
            }
            GL.End();

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
