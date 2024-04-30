using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;

namespace DemoCutterGUI.Tools
{
    class MiniMapMeta
    {
        public float minX { get; set; }
        public float minY { get; set; }
        public float minZ { get; set; }
        public float maxX { get; set; }
        public float maxY { get; set; }
        public float maxZ { get; set; }

        public Vector2 GetTexturePositionXY(Vector3 position)
        {
            return new Vector2() { X = (position.X - minX) / (maxX - minX), Y = (position.Y - minY) / (maxY - minY) };
        }
    }
}
