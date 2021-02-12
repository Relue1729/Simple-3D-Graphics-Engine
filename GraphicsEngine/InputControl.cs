using SharpDX;
using System.Windows.Input;

namespace GraphicsEngine
{
    public class InputControl
    {
        System.Windows.Point previousPoint = new System.Windows.Point(-1, -1);

        public void MouseControl(Mesh mesh, MouseEventArgs e)
        {
            if (mesh is null) { return; }
            
            var currentPoint = e.GetPosition(null); 
            if (previousPoint.X == -1) { previousPoint = currentPoint; }

            float dx = (float)(previousPoint.X - currentPoint.X);
            float dy = (float)(previousPoint.Y - currentPoint.Y);
            float step = 0.003f;

            previousPoint = currentPoint;

            if (e.LeftButton == MouseButtonState.Pressed)   
            { 
                mesh.Rotation += new Vector3(dy * step, dx * step, 0); 
            }
            else if (e.RightButton == MouseButtonState.Pressed)  
            {
                mesh.Position += new Vector3(dx * step, dy * step, 0); 
            }
            else if (e.MiddleButton == MouseButtonState.Pressed) 
            { 
                mesh.Position += new Vector3(0, 0, (-dx + dy) * step); 
            }
        }

        public void Spin(Mesh mesh, double Xrate, double Yrate, double Zrate)
        {
            if (mesh is null) { return; }

            mesh.Rotation += new Vector3((float)Xrate, (float)Yrate, (float)Zrate);
        }
    }
}