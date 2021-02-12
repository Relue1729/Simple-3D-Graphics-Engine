using Newtonsoft.Json;
using SharpDX;
using System;
using System.IO;

namespace GraphicsEngine
{
    public class Mesh
    {
        public string    Name     { get; set; }
        public Vector3[] Vertices { get; set; }
        public int[]     Faces    { get; set; }
         
        public Vector3   Position { get; set; }
        public Vector3   Rotation { get; set; }

        public Color Color { get; set; } = ColorPalette.Frost2;

        public static Mesh LoadFromJson(string path)
        {
            try
            {
                var file = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Mesh>(file);
            }
            catch { throw new ArgumentException($"Mesh file {path} is missing or corrupted"); }
        }
    }
}