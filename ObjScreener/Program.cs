using System.IO;
using System.Drawing;

using ObjScreener.Data;
using ObjScreener.Parser;
using ObjScreener.Renderer;

namespace ObjScreener
{
    public static class Program
    {
        public static void Main()
        {
            ObjParser parser = new ObjParser();

            string data = "";
            using (StreamReader str = new StreamReader("awp.obj"))
                data = str.ReadToEnd();

            Geometry geometry = parser.Parse(data);
            Bitmap texture = new Bitmap("texture.jpg");

            Mesh mesh = new Mesh(geometry, texture);

            MeshRenderer win = new MeshRenderer(1920, 1280, mesh);

            //settings go here
            win.CameraDistance += 10;
            win.AngleX += 5;

            win.SavePath = "test.jpg";
            win.CloseOnSaved = false; //will occupy a thread unil force finish

            win.Run(30);
        }
    }
}
