using SharpDX;

namespace GraphicsEngine
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target   { get; set; }

        public Camera()
        {
            Position = new Vector3(0f, 0f, 10.0f);
            Target = Vector3.Zero;
        }

        public Camera(Vector3 Position, Vector3 Target)
        {
            this.Position = Position;
            this.Target = Target;
        }
    }
}