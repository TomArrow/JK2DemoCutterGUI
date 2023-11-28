using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DemoCutterGUI.Tools
{



    static class BSPToMiniMap
    {
        const int MAX_QPATH = 64;		// max length of a quake game pathname

        const int LUMP_ENTITIES = 0;
        const int LUMP_SHADERS = 1;
        const int LUMP_PLANES = 2;
        const int LUMP_NODES = 3;
        const int LUMP_LEAFS = 4;
        const int LUMP_LEAFSURFACES = 5;
        const int LUMP_LEAFBRUSHES = 6;
        const int LUMP_MODELS = 7;
        const int LUMP_BRUSHES = 8;
        const int LUMP_BRUSHSIDES = 9;
        const int LUMP_DRAWVERTS = 10;
        const int LUMP_DRAWINDEXES = 11;
        const int LUMP_FOGS = 12;
        const int LUMP_SURFACES = 13;
        const int LUMP_LIGHTMAPS = 14;
        const int LUMP_LIGHTGRID = 15;
        const int LUMP_VISIBILITY = 16;
        const int LUMP_LIGHTARRAY = 17;
        const int HEADER_LUMPS = 18;

        const int MAXLIGHTMAPS = 4;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct lump_t
        {
            public int fileofs, filelen;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dheader_t
        {
            public int ident;
            public int version;

            public unsafe fixed int lumps[HEADER_LUMPS*2]; // lump_t is really just 2 ints: fileofs, filelen

            public lump_t GetLump(int index)
            {
                return Helpers.ArrayBytesAsType<lump_t, dheader_t>(this, (int)Marshal.OffsetOf<dheader_t>("lumps") + Marshal.SizeOf(typeof(lump_t)) * index);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dshader_t
        {
            public unsafe fixed byte shader[MAX_QPATH];
            public int surfaceFlags;
            public int contentFlags;
            public unsafe string getShaderName()
            {
                fixed (byte* shaderPtr = shader)
                {
                    return Encoding.ASCII.GetString(shaderPtr, MAX_QPATH).TrimEnd((Char)0);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct mapVert_t
        {
            public unsafe fixed float xyz[3];
            public unsafe fixed float st[2];
            public unsafe fixed float lightmap[MAXLIGHTMAPS * 2];
            public unsafe fixed float normal[3];
            public unsafe fixed byte color[MAXLIGHTMAPS * 4];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dsurface_t
        {
            public int shaderNum;
            public int fogNum;
            public int surfaceType;

            public int firstVert;
            public int numVerts;

            public int firstIndex;
            public int numIndexes;

            public unsafe fixed byte lightmapStyles[MAXLIGHTMAPS], vertexStyles[MAXLIGHTMAPS];
            public unsafe fixed int lightmapNum[MAXLIGHTMAPS];
            public unsafe fixed int lightmapX[MAXLIGHTMAPS], lightmapY[MAXLIGHTMAPS];
            public int lightmapWidth, lightmapHeight;

            public unsafe fixed float lightmapOrigin[3];
            public unsafe fixed float lightmapVecs[9]; // for patches, [0] and [1] are lodbounds

            public int patchWidth;
            public int patchHeight;
        }

        struct EzAccessTriangle
        {
            public Vector3[] points;
            public float minX, maxX, minY, maxY;
        }


        enum ShaderType {
            NORMAL,
            SYSTEM,
            SKY,
        }


        public static unsafe void MakeMiniMap(string bspPath, float pixelsPerUnit=0.1f, int maxWidth = 4000, int maxHeight = 4000, int extraBorderUnits = 100)
        {
            using(FileStream fs = new FileStream(bspPath, FileMode.Open,FileAccess.Read))
            {
                using(BinaryReader br = new BinaryReader(fs))
                {
                    dheader_t header = Helpers.ReadBytesAsType<dheader_t>(br);
                    if(header.version != 1)
                    {
                        throw new Exception("BSP header version is not 1");
                    }

                    lump_t shadersLump = header.GetLump(LUMP_SHADERS);

                    int shaderCount = shadersLump.filelen / Marshal.SizeOf(typeof(dshader_t));

                    // Make look up table that quickly tells us what kind of shader a particular shader index is. Is it a system shader or sky shader? So we quickly see whether a surface should be considered as "walkable"
                    ShaderType[] shaderTypeLUT = new ShaderType[shaderCount];
                    br.BaseStream.Seek(shadersLump.fileofs,SeekOrigin.Begin);
                    for(int i = 0; i < shaderCount; i++)
                    {
                        dshader_t shaderHere = Helpers.ReadBytesAsType<dshader_t>(br);
                        string shaderName = shaderHere.getShaderName();
                        if (shaderName.StartsWith("textures/system/"))
                        {
                            shaderTypeLUT[i] = ShaderType.SYSTEM;
                        }
                        else if (shaderName.StartsWith("textures/skies/"))
                        {
                            shaderTypeLUT[i] = ShaderType.SKY;
                        }
                        else
                        {
                            shaderTypeLUT[i] = ShaderType.NORMAL;
                        }
                    }

                    // Read surfaces, indices and verts into arrays and figure out total map dimensions
                    float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
                    float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
                    float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;

                    lump_t surfacesLump = header.GetLump(LUMP_SURFACES);
                    lump_t vertsLump = header.GetLump(LUMP_DRAWVERTS);
                    lump_t indexLump = header.GetLump(LUMP_DRAWINDEXES);

                    int surfacesCount = surfacesLump.filelen / Marshal.SizeOf(typeof(dsurface_t));
                    int vertsCount = vertsLump.filelen / Marshal.SizeOf(typeof(mapVert_t));
                    int indexCount = indexLump.filelen / sizeof(int);

                    dsurface_t[] surfaces = new dsurface_t[surfacesCount];
                    mapVert_t[] verts = new mapVert_t[vertsCount];
                    int[] indices = new int[indexCount];

                    // Read verts
                    br.BaseStream.Seek(vertsLump.fileofs, SeekOrigin.Begin);
                    for (int i = 0; i < vertsCount; i++)
                    {
                        verts[i] = Helpers.ReadBytesAsType<mapVert_t>(br);
                    }
                    // Read indices
                    br.BaseStream.Seek(indexLump.fileofs, SeekOrigin.Begin);
                    for (int i = 0; i < indexCount; i++)
                    {
                        indices[i] = br.ReadInt32();
                    }

                    List<EzAccessTriangle> triangles = new List<EzAccessTriangle>();

                    // Read surfaces and do some processing
                    br.BaseStream.Seek(surfacesLump.fileofs, SeekOrigin.Begin);
                    for (int i = 0; i < surfacesCount; i++)
                    {
                        dsurface_t surf = surfaces[i] = Helpers.ReadBytesAsType<dsurface_t>(br);
                        if((surf.numIndexes % 3) > 0)
                        {
                            throw new Exception("(surf.numIndexes % 3) > 0");
                        }
                        if(shaderTypeLUT[surf.shaderNum] == ShaderType.NORMAL)
                        {
                            for(int v = 0; v< surf.numVerts; v++)
                            {
                                mapVert_t vert = verts[surf.firstVert + v];
                                minX = Math.Min(vert.xyz[0], minX);
                                maxX = Math.Max(vert.xyz[0], maxX);
                                minY = Math.Min(vert.xyz[1], minY);
                                maxY = Math.Max(vert.xyz[1], maxY);
                                minZ = Math.Min(vert.xyz[2], minZ);
                                maxZ = Math.Max(vert.xyz[2], maxZ);
                            }
                            for(int index = 0; index < surf.numIndexes; index+=3)
                            {

                                mapVert_t vert1 = verts[surf.firstVert + indices[surf.firstIndex + index]];
                                mapVert_t vert2 = verts[surf.firstVert + indices[surf.firstIndex + index+1]];
                                mapVert_t vert3 = verts[surf.firstVert + indices[surf.firstIndex + index+2]];

                                EzAccessTriangle triangle = new EzAccessTriangle()
                                {
                                    points = new Vector3[] {
                                        new Vector3() { X = vert1.xyz[0], Y = vert1.xyz[1], Z = vert1.xyz[2] },
                                        new Vector3() { X = vert2.xyz[0], Y = vert2.xyz[1], Z = vert2.xyz[2] },
                                        new Vector3() { X = vert3.xyz[0], Y = vert3.xyz[1], Z = vert3.xyz[2] },
                                    },
                                    minX = Math.Min(vert1.xyz[0], Math.Min(vert2.xyz[0], vert3.xyz[0])),
                                    minY = Math.Min(vert1.xyz[1], Math.Min(vert2.xyz[1], vert3.xyz[1])),
                                    maxX = Math.Max(vert1.xyz[0], Math.Max(vert2.xyz[0], vert3.xyz[0])),
                                    maxY = Math.Max(vert1.xyz[1], Math.Max(vert2.xyz[1], vert3.xyz[1])),
                                };

                                // Calculate normal
                                Vector3 normal = Vector3.Normalize(Vector3.Cross(triangle.points[2]- triangle.points[1], triangle.points[2] - triangle.points[0]));
                                float angle = 360.0f * (float)Math.Acos(normal.Z) / (float)Math.PI / 2.0f;

                                // Just roughly consider walkable surfaces
                                if(angle < 45.0f)
                                {
                                    triangles.Add(triangle);
                                }

                            }
                        }
                    }

                    minX -= extraBorderUnits;
                    maxX += extraBorderUnits;
                    minY -= extraBorderUnits;
                    maxY += extraBorderUnits;

                    float xRange = maxX - minX;
                    float yRange = maxY - minY;
                    int xRes = (int)(Math.Ceiling(maxX - minX) * pixelsPerUnit);
                    int yRes = (int)(Math.Ceiling(maxY - minY) * pixelsPerUnit);
                    if (xRes > maxWidth)
                    {
                        yRes = yRes * maxWidth / xRes;
                        xRes = maxWidth;
                    }
                    if(yRes > maxHeight)
                    {
                        xRes = xRes * maxHeight / yRes;
                        yRes = maxHeight;
                    }

                    float[] pixelData = new float[xRes * yRes];
                    float[] pixelDataDivider = new float[xRes * yRes];

                    float zValueScale = 1.0f / (maxZ-minZ);
                    float zValueOffset = -minZ;

                    Parallel.For(0, yRes, new ParallelOptions() {MaxDegreeOfParallelism= Environment.ProcessorCount/2 }, (y) =>
                    {
                    //for(int y = 0; y < yRes; y++)
                    //{
                        for(int x = 0; x < xRes; x++)
                        {
                            Vector3 pixelWorldCoordinate = new Vector3() { X = (float)(xRes-x-1)/(float)xRes*xRange+minX, Y= (float)y / (float)yRes * yRange + minY };
                            for(int t = 0; t < triangles.Count; t++)
                            {
                                if (pixelWorldCoordinate.X < triangles[t].minX || pixelWorldCoordinate.Y < triangles[t].minY
                                    || pixelWorldCoordinate.X > triangles[t].maxX || pixelWorldCoordinate.Y > triangles[t].maxY
                                    )
                                {
                                    continue; // Quick skip triangles that obviously don't apply.
                                }
                                if(Helpers.pointInTriangle2D(ref pixelWorldCoordinate,ref triangles[t].points[0],ref triangles[t].points[1],ref triangles[t].points[2]))
                                {
                                    float Z = zValueScale*(zValueOffset + (triangles[t].points[0].Z+ triangles[t].points[1].Z+ triangles[t].points[2].Z)/3f);// dumb but should work good enough
                                    pixelData[y * xRes + x] += 1.0f + Z;
                                    pixelDataDivider[y * xRes + x]++;
                                }
                            }
                        }
                    //}
                    });

                    //float[] pixelDiffData = new float[xRes * yRes];



                    Bitmap bitmap = new Bitmap(xRes, yRes, PixelFormat.Format32bppArgb);
                    ByteImage img = Helpers.BitmapToByteArray(bitmap);
                    bitmap.Dispose();

                    // Kinda edge detection
                    for (int y = 0; y < yRes; y++)
                    {
                        for (int x = 0; x < xRes; x++)
                        {
                            if(pixelData[y * xRes + x] > 0)
                            {
                                byte color = (byte)Math.Clamp(255.0f * (pixelData[y * xRes + x]/pixelDataDivider[y * xRes + x] - 1.0f), 0, 255);
                                img.imageData[y * img.stride + x * 4] = Math.Max(img.imageData[y * img.stride + x * 4], color);
                                img.imageData[y * img.stride + x * 4 + 1] = Math.Max(img.imageData[y * img.stride + x * 4 + 1], color);
                                img.imageData[y * img.stride + x * 4 + 2] = Math.Max(img.imageData[y * img.stride + x * 4 + 2], color);
                                img.imageData[y * img.stride + x * 4 + 3] = 255;
                            }
                            for(int yJitter = Math.Max(0,y-1);yJitter < Math.Min(y + 1, yRes); yJitter++)
                            {
                                for (int xJitter = Math.Max(0, x - 1); xJitter < Math.Min(x + 1, xRes); xJitter++)
                                {
                                    float diff = pixelData[yJitter * xRes + xJitter]/ Math.Max(1f,pixelDataDivider[yJitter * xRes + xJitter]) - pixelData[y * xRes + x]/ Math.Max(1f, pixelDataDivider[y * xRes + x]);
                                    if (diff != 0)
                                    {
                                        byte delta = (byte)Math.Clamp(255.0f*Math.Pow(Math.Abs(diff),0.22f),0,255);
                                        //pixelDiffData[y * xRes + x] = 1.0f;
                                        img.imageData[y * img.stride + x*4] = Math.Max(img.imageData[y * img.stride + x * 4], delta);
                                        img.imageData[y * img.stride + x*4+1] = Math.Max(img.imageData[y * img.stride + x * 4 + 1], delta);
                                        img.imageData[y * img.stride + x*4+2] = Math.Max(img.imageData[y * img.stride + x * 4 + 2], delta);
                                        img.imageData[y * img.stride + x*4+3] = 255;
                                    }
                                }
                            }

                        }
                    }

                    bitmap = Helpers.ByteArrayToBitmap(img);
                    bitmap.Save($"{bspPath}.png");
                    bitmap.Dispose();

                    Console.WriteLine("hello");
                }
            }
        }



    }



    class ByteImage
    {
        public byte[] imageData;
        public int stride;
        public int width, height;
        public PixelFormat pixelFormat;

        public ByteImage(byte[] imageDataA, int strideA, int widthA, int heightA, PixelFormat pixelFormatA)
        {
            imageData = imageDataA;
            stride = strideA;
            width = widthA;
            height = heightA;
            pixelFormat = pixelFormatA;
        }

        public int Length
        {
            get { return imageData.Length; }
        }

        public byte this[int index]
        {
            get
            {
                return imageData[index];
            }

            set
            {
                imageData[index] = value;
            }
        }
    }
}
