using System.ComponentModel;
using System.Numerics;

namespace BezierSceneApp
{
    // panel rysujacy: (0, 0) na srodku, os Y w gore, skala zalezna od rozmiaru
    public class DrawingPanel : Panel
    {
        private static readonly Color PolygonColor = Color.Gray;
        private static readonly Color MeshColor = Color.LimeGreen;
        private const float ScaleFactor = 0.45f;
        private const float PointRadius = 3f;
        // czulosc obrotu myszy [rad / px]
        private const float RotationSensitivity = 0.01f;

        private readonly TriangleRasterizer rasterizer = new();
        private Point lastMouse;

        public DrawingPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Scene? Scene { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowPolygons { get; set; } = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowMesh { get; set; } = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowFilled { get; set; } = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LightSettings Light { get; } = new();

        // warstwy: tlo -> wypelnione trojkaty -> siatka -> wieloboki -> osie
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            if (Scene != null)
            {
                if (ShowFilled)
                    DrawFilled(g, Scene);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                if (ShowMesh)
                    foreach (Mesh m in Scene.Meshes)
                        DrawMesh(g, m);
                if (ShowPolygons)
                    foreach (BezierSurface s in Scene.Surfaces)
                        DrawControlPolygon(g, s);
            }

            DrawAxes(g);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                rasterizer.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            lastMouse = e.Location;
        }

        // LPM: dx -> os Y, dy -> os X; PPM: dx -> os Z; obrot narastajacy, bez limitu katow
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Scene == null || (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right))
                return;

            float dx = (e.X - lastMouse.X) * RotationSensitivity;
            float dy = (e.Y - lastMouse.Y) * RotationSensitivity;
            lastMouse = e.Location;
            if (dx == 0f && dy == 0f)
                return;

            // dy ekranowe rosnie w dol: dodatni kat wokol X przesuwa przod sceny w dol
            Matrix3x3 increment = e.Button == MouseButtons.Left
                ? Matrix3x3.CreateRotationX(dy) * Matrix3x3.CreateRotationY(dx)
                : Matrix3x3.CreateRotationZ(-dx); // ruch w prawo -> obrot zgodny z ruchem wskazowek
            Scene.Rotate(increment);
            Invalidate();
        }

        // rzut prostokatny na plaszczyzne xy (z pomijane); odwrocenie Y wzgledem ekranu
        private PointF ToScreen(Vector3 p)
        {
            float scale = ScaleFactor * Math.Min(ClientSize.Width, ClientSize.Height);
            return new PointF(ClientSize.Width / 2f + p.X * scale, ClientSize.Height / 2f - p.Y * scale);
        }

        // pomocnicze osie do weryfikacji polozenia (0, 0) i kierunku Y
        private void DrawAxes(Graphics g)
        {
            PointF o = ToScreen(Vector3.Zero);
            using var pen = new Pen(Color.LightGray);
            g.DrawLine(pen, 0, o.Y, ClientSize.Width, o.Y);
            g.DrawLine(pen, o.X, 0, o.X, ClientSize.Height);
            g.DrawString("+X", Font, Brushes.Gray, ToScreen(new Vector3(1f, 0f, 0f)));
            g.DrawString("+Y", Font, Brushes.Gray, ToScreen(new Vector3(0f, 1f, 0f)));
        }

        private void DrawFilled(Graphics g, Scene scene)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w <= 0 || h <= 0) // np. zminimalizowane okno
                return;

            rasterizer.EnsureSize(w, h);
            rasterizer.Clear(BackColor.ToArgb());
            TextureData? normalMap = scene.UseNormalMap ? scene.NormalMap : null;
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                SurfaceAppearance look = scene.Appearances[i];
                TextureData? texture = look.UseTexture ? look.Texture : null;
                foreach (Triangle t in scene.Meshes[i].Triangles)
                    rasterizer.FillTriangle(ToScreenDepth(t.V0.RotP), ToScreenDepth(t.V1.RotP), ToScreenDepth(t.V2.RotP),
                        t.V0, t.V1, t.V2, look.SolidColor, texture, normalMap, Light);
            }
            rasterizer.CopyToBitmap();
            g.DrawImageUnscaled(rasterizer.Bitmap!, 0, 0);
        }

        // (x, y) ekranowe jak w ToScreen, z bez zmian (glebokosc do Z-buffera)
        private Vector3 ToScreenDepth(Vector3 p)
        {
            PointF s = ToScreen(p);
            return new Vector3(s.X, s.Y, p.Z);
        }

        private void DrawMesh(Graphics g, Mesh mesh)
        {
            using var pen = new Pen(MeshColor);
            var tri = new PointF[3];
            foreach (Triangle t in mesh.Triangles)
            {
                tri[0] = ToScreen(t.V0.RotP);
                tri[1] = ToScreen(t.V1.RotP);
                tri[2] = ToScreen(t.V2.RotP);
                g.DrawPolygon(pen, tri);
            }
        }

        private void DrawControlPolygon(Graphics g, BezierSurface surface)
        {
            const int n = BezierSurface.Size;
            var pts = new PointF[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    pts[i, j] = ToScreen(surface.RotatedControlPoints[i, j]);

            using var pen = new Pen(PolygonColor);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j + 1 < n) g.DrawLine(pen, pts[i, j], pts[i, j + 1]);
                    if (i + 1 < n) g.DrawLine(pen, pts[i, j], pts[i + 1, j]);
                }
            }

            using var brush = new SolidBrush(PolygonColor);
            foreach (PointF p in pts)
                g.FillEllipse(brush, p.X - PointRadius, p.Y - PointRadius, 2 * PointRadius, 2 * PointRadius);
        }
    }
}
