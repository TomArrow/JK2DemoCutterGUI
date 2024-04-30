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

    class MiniMapPoint {
        public Vector3 position;
        public bool main;
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

            double actualWidth = OpenTkControl.ActualWidth;
            

            if (string.IsNullOrWhiteSpace(map)) return;

            //if (!File.Exists())
            //{

            //}

            MiniMapMeta miniMapMeta = null;
            int textureHandle = GetMinimapTexture(map, ref miniMapMeta);

            if (textureHandle < 0 || miniMapMeta is null) return; // No minimap texture found

            GL.Enable(EnableCap.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, textureHandle);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(1.0, 1.0);
            GL.Vertex2(1.0, 1.0);

            GL.TexCoord2(1.0, 0.0);
            GL.Vertex2(1.0, -1.0);

            GL.TexCoord2(0.0, 0.0);
            GL.Vertex2(-1.0, -1.0);

            GL.TexCoord2(0.0, 1.0);
            GL.Vertex2(-1.0, 1.0);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Texture2D);

            MiniMapPoint[] points = items.ToArray();

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
