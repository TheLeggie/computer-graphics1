using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace BezierSceneApp
{
    // panel rysujacy: (0, 0) na srodku, os Y w gore, skala zalezna od rozmiaru
    public class DrawingPanel : Panel
    {
        private static readonly Color PolygonColor = Color.Gray;
        private static readonly Color MeshColor = Color.LimeGreen;
        private static readonly Color LightCurveColor = Color.DarkOrange;
        private static readonly Color LightHandleActiveColor = Color.OrangeRed;
        private const float ScaleFactor = 0.45f;
        private const float PointRadius = 3f;
        private const float LightHandleRadius = 5f;
        private const float LightHandleHitRadius = 10f;
        private const float LightMarkerRadius = 7f;
        private const int LightCurveSegments = 80;
        // czulosc obrotu myszy [rad / px]
        private const float RotationSensitivity = 0.01f;

        private readonly TriangleRasterizer rasterizer = new();
        private readonly PointF[] lightCurveScreenPoints = new PointF[LightCurveSegments + 1];
        private readonly PointF[] lightPolygonScreenPoints = new PointF[LightBezierCurve.PointCount];
        private Point lastMouse;
        private int draggedControlPoint = -1;
        private bool showLightCurve = true;

        public DrawingPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
            UpdateLightPosition();
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
        public bool ShowLightCurve
        {
            get => showLightCurve;
            set
            {
                showLightCurve = value;
                if (!value)
                {
                    draggedControlPoint = -1;
                    Cursor = Cursors.Default;
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LightSettings Light { get; } = new();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LightBezierCurve LightCurve { get; } = new();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float LightT { get; set; }

        public void UpdateLightPosition()
        {
            Light.Position = LightCurve.Evaluate3D(LightT, Light.Position.Z);
        }

        // warstwy: tło - wypelnione trojkaty - siatka - wieloboki - tor i zrodlo swiatla
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            if (Scene != null && ShowFilled)
                DrawFilled(g, Scene);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Scene != null)
            {
                if (ShowMesh)
                    foreach (Mesh m in Scene.Meshes)
                        DrawMesh(g, m);
                if (ShowPolygons)
                    foreach (BezierSurface s in Scene.Surfaces)
                        DrawControlPolygon(g, s);
            }

            if (ShowLightCurve)
                DrawLightCurve(g);
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
            if (e.Button == MouseButtons.Left)
            {
                draggedControlPoint = HitTestLightHandle(e.Location);
                if (draggedControlPoint >= 0)
                    Invalidate();
            }
        }

        // LPM na uchwycie: edycja punktu krzywej swiatla; LPM poza uchwytem: os Y i os X; PPM: os Z
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (draggedControlPoint >= 0)
            {
                if ((e.Button & MouseButtons.Left) == 0)
                {
                    draggedControlPoint = -1;
                    return;
                }

                int margin = (int)LightHandleRadius;
                int clampedX = Math.Clamp(e.X, margin, Math.Max(margin, ClientSize.Width - margin));
                int clampedY = Math.Clamp(e.Y, margin, Math.Max(margin, ClientSize.Height - margin));
                LightCurve.SetPoint(draggedControlPoint, FromScreen(new Point(clampedX, clampedY)));
                UpdateLightPosition();
                Invalidate();
                return;
            }

            if (e.Button == MouseButtons.None)
            {
                Cursor = HitTestLightHandle(e.Location) >= 0 ? Cursors.Hand : Cursors.Default;
                return;
            }

            if (Scene == null || (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right))
                return;

            float dx = (e.X - lastMouse.X) * RotationSensitivity;
            float dy = (e.Y - lastMouse.Y) * RotationSensitivity;
            lastMouse = e.Location;
            if (dx == 0f && dy == 0f)
                return;

            // dy ekranowe rosnie w dol: dodatni kąt wokol X przesuwa przod sceny w dol
            Matrix3x3 increment = e.Button == MouseButtons.Left
                ? Matrix3x3.CreateRotationX(dy) * Matrix3x3.CreateRotationY(dx)
                : Matrix3x3.CreateRotationZ(-dx); // ruch w prawo -> obrot zgodny z ruchem wskazowek
            Scene.Rotate(increment);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && draggedControlPoint >= 0)
            {
                draggedControlPoint = -1;
                Invalidate();
            }
        }

        // rzut prostokatny na plaszczyzne xy, odwrocenie Y wzgledem ekranu
        public PointF ToScreen(Vector3 p)
        {
            float scale = ScaleFactor * Math.Min(ClientSize.Width, ClientSize.Height);
            return new PointF(ClientSize.Width / 2f + p.X * scale, ClientSize.Height / 2f - p.Y * scale);
        }

        private PointF ToScreen(Vector2 p) => ToScreen(new Vector3(p.X, p.Y, 0f));

        // odwrotnosc ToScreen na plaszczyznie xy
        public Vector2 FromScreen(Point p)
        {
            float scale = ScaleFactor * Math.Min(ClientSize.Width, ClientSize.Height);
            if (scale <= 0f)
                return Vector2.Zero;
            return new Vector2((p.X - ClientSize.Width / 2f) / scale, (ClientSize.Height / 2f - p.Y) / scale);
        }

        private int HitTestLightHandle(Point mouse)
        {
            if (!ShowLightCurve)
                return -1;

            int best = -1;
            float bestDistSq = LightHandleHitRadius * LightHandleHitRadius;
            for (int i = 0; i < LightBezierCurve.EditablePointCount; i++)
            {
                PointF s = ToScreen(LightCurve[i]);
                float dx = mouse.X - s.X;
                float dy = mouse.Y - s.Y;
                float distSq = dx * dx + dy * dy;
                if (distSq <= bestDistSq)
                {
                    bestDistSq = distSq;
                    best = i;
                }
            }
            return best;
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

        private void DrawLightCurve(Graphics g)
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            for (int i = 0; i < LightBezierCurve.PointCount; i++)
                lightPolygonScreenPoints[i] = ToScreen(LightCurve[i]);
            using (var polyPen = new Pen(Color.FromArgb(140, LightCurveColor), 1f) { DashStyle = DashStyle.Dash })
                g.DrawLines(polyPen, lightPolygonScreenPoints);

            for (int i = 0; i <= LightCurveSegments; i++)
                lightCurveScreenPoints[i] = ToScreen(LightCurve.Evaluate((float)i / LightCurveSegments));
            using (var curvePen = new Pen(LightCurveColor, 2f))
                g.DrawLines(curvePen, lightCurveScreenPoints);

            using var handleBrush = new SolidBrush(LightCurveColor);
            using var activeBrush = new SolidBrush(LightHandleActiveColor);
            using var handleInnerPen = new Pen(Color.White, 1.5f);
            using var handleOuterPen = new Pen(Color.SaddleBrown, 1f);
            for (int i = 0; i < LightBezierCurve.EditablePointCount; i++)
            {
                PointF p = lightPolygonScreenPoints[i];
                float r = i == draggedControlPoint ? LightHandleRadius + 1.5f : LightHandleRadius;
                Brush fill = i == draggedControlPoint ? activeBrush : handleBrush;
                g.FillEllipse(fill, p.X - r, p.Y - r, 2 * r, 2 * r);
                g.DrawEllipse(handleInnerPen, p.X - r, p.Y - r, 2 * r, 2 * r);
                g.DrawEllipse(handleOuterPen, p.X - r - 1f, p.Y - r - 1f, 2 * (r + 1f), 2 * (r + 1f));
            }

            // znacznik aktualnego polozenia zrodla swiatla na krzywej
            PointF lp = ToScreen(Light.Position);
            Color lightFill = Color.FromArgb(
                Math.Clamp((int)(Light.LightColor.X * 255f + 0.5f), 0, 255),
                Math.Clamp((int)(Light.LightColor.Y * 255f + 0.5f), 0, 255),
                Math.Clamp((int)(Light.LightColor.Z * 255f + 0.5f), 0, 255));
            using var haloPen = new Pen(Color.Gold, 2f);
            using var markerBrush = new SolidBrush(lightFill);
            using var markerBorderPen = new Pen(Color.Black, 1.8f);
            float mr = LightMarkerRadius;
            g.DrawEllipse(haloPen, lp.X - mr - 2f, lp.Y - mr - 2f, 2 * (mr + 2f), 2 * (mr + 2f));
            g.FillEllipse(markerBrush, lp.X - mr, lp.Y - mr, 2 * mr, 2 * mr);
            g.DrawEllipse(markerBorderPen, lp.X - mr, lp.Y - mr, 2 * mr, 2 * mr);
        }
    }
}
