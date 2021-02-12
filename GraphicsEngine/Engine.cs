using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GraphicsEngine
{
    public class Engine
    {
        byte[]       backBuffer;
        float[]      depthBuffer; //Z-буфер
        readonly int pixelWidth;
        readonly int pixelHeight;
        readonly int bmpStride;
        Color        backgroundColor;

        public bool DrawMeshLines { get; set; }
        public bool DrawModel     { get; set; } = true;
        public bool isSolidColor  { get; set; }

        public Engine(int resolutionX = 1280, int resolutionY = 720)
        {
            pixelWidth  = resolutionX;
            pixelHeight = resolutionY;

            backBuffer  = new byte[pixelWidth * pixelHeight * 4];
            depthBuffer = new float[pixelWidth * pixelHeight];

            bmpStride = (pixelWidth * 32 + 7) / 8;
            backgroundColor = ColorPalette.PolarNight2;
        }

        public void Clear()
        {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                backBuffer[index]     = backgroundColor.B;
                backBuffer[index + 1] = backgroundColor.G;
                backBuffer[index + 2] = backgroundColor.R;
                backBuffer[index + 3] = backgroundColor.A;
            }

            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
            }
        }

        public WriteableBitmap GetFrontBuffer()
        {
            BitmapSource bitmap = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, 
                                                      System.Windows.Media.PixelFormats.Bgr32, 
                                                      null, backBuffer, bmpStride);
            return new WriteableBitmap(bitmap);
        }

        #region Draw Methods
        public void DrawPoint(Vector3 point, Color color) 
        {
            int depthIndex = (int)(point.X + point.Y * pixelWidth);
            int index = depthIndex * 4;

            bool isInFrame = point.X >= 0 && point.Y >= 0 && point.X < pixelWidth && point.Y < pixelHeight;
            bool isVisible = isInFrame && point.Z < depthBuffer[depthIndex];

            if (isInFrame && isVisible)
            {
                depthBuffer[depthIndex] = point.Z;

                backBuffer[index]     = color.B;
                backBuffer[index + 1] = color.G;
                backBuffer[index + 2] = color.R;
                backBuffer[index + 3] = color.A;
            }
        }

        //Алгоритм Брезенхэма с интерполяцией Z значения
        public void DrawLine(Vector3 point0, Vector3 point1, Color color)
        {
            int x0 = (int)point0.X;
            int y0 = (int)point0.Y;
            int x1 = (int)point1.X;
            int y1 = (int)point1.Y;
            var z0 = point0.Z;
            var z1 = point1.Z;

            var dx = Math.Abs(x1 - x0);
            var dy = -Math.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            
            var err = dx + dy;

            while (true)
            {
                float gradientZ;

                if (dx > -dy) { gradientZ = (x0 - point0.X) / (x1 - point0.X); }
                else          { gradientZ = (y0 - point0.Y) / (y1 - point0.Y); }

                var z = Interpolate(z0, z1, gradientZ);

                //z - 0.00005 для того чтобы структура всегда была поверх самой модели
                DrawPoint(new Vector3(x0, y0, z - 0.00005f), color);

                if ((x0 == x1) && (y0 == y1)) break;

                var e2 = 2 * err;

                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        public void DrawMesh(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            Color color;

            if (DrawModel) { color = ColorPalette.Frost4; }
            else           { color = ColorPalette.Frost3; }

            DrawLine(point1, point2, color);
            DrawLine(point1, point3, color);
            DrawLine(point2, point3, color);
        }

        //Растеризация треугольника
        public void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color)
        {
            //Сортируем вершины в порядке v0, v1, v2 с верху вниз
            if (v0.Y > v1.Y) { var t = v1; v1 = v0; v0 = t; }
            if (v1.Y > v2.Y) { var t = v1; v1 = v2; v2 = t; }
            if (v0.Y > v1.Y) { var t = v1; v1 = v0; v0 = t; }

            float m0, m1;

            if (v1.Y == v0.Y) { m0 = 0; }
            else { m0 = (v1.X - v0.X) / (v1.Y - v0.Y); }

            if (v2.Y == v0.Y) { m1 = 0; }
            else { m1 = (v2.X - v0.X) / (v2.Y - v0.Y); }

            if (m0 > m1) //Если v1 справа от линии v0-v2
            {
                for (var y = (int)v0.Y; y <= (int)v2.Y; y++)
                {
                    if (y < v1.Y) { DrawScanLine(y, v0, v2, v0, v1, color); } //Скан линия выше v1
                    else          { DrawScanLine(y, v0, v2, v1, v2, color); } //Скан линия ниже v1
                }
            }
            else         //Если v1 слева от линии v0-v2
            {
                for (var y = (int)v0.Y; y <= (int)v2.Y; y++)
                {
                    if (y < v1.Y) { DrawScanLine(y, v0, v1, v0, v2, color); }
                    else          { DrawScanLine(y, v1, v2, v0, v2, color); }
                }
            }
        }

        //Интерполируем X/Z в скан линии на основе отсортированных точек
        //v0-v1 - Вертикальная линия 1, v2-v3 - Вертикальная линия 2
        void DrawScanLine(int y, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color color)
        {
            var gradient1 = v0.Y == v1.Y ? 1 : (y - v0.Y) / (v1.Y - v0.Y);
            var gradient2 = v2.Y == v3.Y ? 1 : (y - v2.Y) / (v3.Y - v2.Y);

            int sx = (int)Interpolate(v0.X, v1.X, gradient1);
            int ex = (int)Interpolate(v2.X, v3.X, gradient2);

            var z0 = Interpolate(v0.Z, v1.Z, gradient1);
            var z1 = Interpolate(v2.Z, v3.Z, gradient2);

            for (var x = sx; x < ex; x++)
            {
                var gradientZ = (float)(x - sx) / (ex - sx);
                var z = Interpolate(z0, z1, gradientZ);

                DrawPoint(new Vector3(x, y, z), color);
            }
        }
        #endregion

        public void Render(Camera camera, List<Mesh> meshes)
        {
            Clear();

            float aspectRatio    = (float) pixelWidth / pixelHeight;
            var viewMatrix       = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix.PerspectiveFovLH(0.8f, aspectRatio, 0.1f, 1.0f);

            foreach (Mesh mesh in meshes)
            {
                var rotationMatrix    = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);
                var translationMatrix = Matrix.Translation(mesh.Position);
                var worldMatrix       = rotationMatrix * translationMatrix;
                var transformMatrix   = worldMatrix * viewMatrix * projectionMatrix;

                Parallel.For(0, mesh.Faces.Length / 3, y =>
                {
                    int i = y * 3;

                    var vertex1 = mesh.Vertices[mesh.Faces[i]];
                    var vertex2 = mesh.Vertices[mesh.Faces[i + 1]];
                    var vertex3 = mesh.Vertices[mesh.Faces[i + 2]];

                    var point1 = Project(vertex1, transformMatrix);
                    var point2 = Project(vertex2, transformMatrix);
                    var point3 = Project(vertex3, transformMatrix);

                    var triangleColor = mesh.Color;

                    if (!isSolidColor) { triangleColor = Color.AdjustContrast(mesh.Color, 1f + (y % 6) * 0.1f); }

                    if (DrawMeshLines) { DrawMesh(point1, point2, point3); }
                    if (DrawModel)     { DrawTriangle(point1, point2, point3, triangleColor); }
                });
            }
        }

        Vector3 Project(Vector3 vertex, Matrix transformMatrix)
        {
            var point = Vector3.TransformCoordinate(vertex, transformMatrix);

            var x =  point.X * pixelWidth  + pixelWidth  / 2.0f;
            var y = -point.Y * pixelHeight + pixelHeight / 2.0f;

            return new Vector3(x, y, point.Z);
        }

        float Interpolate(float a, float b, float gradient) => a + (b - a) * gradient;
    }
}