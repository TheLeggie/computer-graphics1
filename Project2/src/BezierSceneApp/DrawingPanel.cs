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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawAxes(g);
            if (Scene == null)
                return;

            if (ShowMesh)
                foreach (Mesh m in Scene.Meshes)
                    DrawMesh(g, m);
            if (ShowPolygons)
                foreach (BezierSurface s in Scene.Surfaces)
                    DrawControlPolygon(g, s);
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
