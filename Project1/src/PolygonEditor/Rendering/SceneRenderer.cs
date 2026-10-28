using System.Drawing.Drawing2D;
using System.Numerics;
using PolygonEditor.Algorithms;
using PolygonEditor.Core.Model;

namespace PolygonEditor.Rendering;

public sealed class SceneRenderer : IDisposable
{
    private const float VertexRadius = 5f;
    private const float ControlPointHalfSize = 4f;
    private const int DashLength = 5;
    private const float ConstraintLabelOffset = 14f;

    private static readonly Color BackgroundColor = Color.White;
    private static readonly Color EdgeColor = Color.Black;
    private static readonly Color ControlPolygonColor = Color.Gray;

    private readonly Pen _edgePen = new(EdgeColor, 1f);
    private readonly Pen _controlPolygonPen = new(ControlPolygonColor, 1f)
    {
        DashPattern = new float[] { DashLength, DashLength }
    };
    private readonly Brush _vertexBrush = new SolidBrush(Color.RoyalBlue);
    private readonly Pen _vertexOutlinePen = new(Color.Navy, 1f);
    private readonly Brush _controlPointBrush = new SolidBrush(Color.Orange);
    private readonly Pen _controlPointOutlinePen = new(Color.DarkOrange, 1f);
    private readonly Font _labelFont = new("Segoe UI", 8f, FontStyle.Bold);
    private readonly Brush _continuityLabelBrush = new SolidBrush(Color.DarkGreen);
    private readonly Font _constraintLabelFont = new("Segoe UI Symbol", 8f, FontStyle.Bold);
    private readonly Pen _highlightPen = new(Color.FromArgb(140, Color.DeepSkyBlue), 6f);
    private readonly Brush _constraintLabelBrush = new SolidBrush(Color.Firebrick);

    private DirectBitmap? _buffer;

    public LineDrawingMode LineMode { get; set; } = LineDrawingMode.Library;

	/// <summary>
    /// Czyści bufor, rysuje wielokąty, kopiuje bitmapę na ekran.
    /// <paramref name="highlightedEdge"/> (np. pierwsza krawędź wybieranej relacji) jest podświetlana pod wielokątami.
    /// </summary>
    public void Render(Graphics target, Scene scene, int width, int height, Edge? highlightedEdge = null)
    {
        if (width <= 0 || height <= 0)
            return;

        var buffer = EnsureBuffer(width, height);
        buffer.Clear(BackgroundColor);

        using (var g = Graphics.FromImage(buffer.Bitmap))
        {
            if (highlightedEdge is not null)
            {
                g.DrawLine(_highlightPen, highlightedEdge.StartVertex.X, highlightedEdge.StartVertex.Y,
                    highlightedEdge.EndVertex.X, highlightedEdge.EndVertex.Y);
            }

            foreach (var polygon in scene.Polygons)
                DrawPolygon(g, polygon);
        }

        target.DrawImageUnscaled(buffer.Bitmap, 0, 0);
    }

	/// <summary>
    /// Kolejność rysowania: linie kontrolne(przerywane), krawędzie, etykiety ograniczeń, punkty kontrolne,
    /// wierzchołki z etykietami ciągłości
    /// </summary>
    private void DrawPolygon(Graphics g, Polygon polygon)
    {
        if (LineMode == LineDrawingMode.Bresenham)
            g.Flush(FlushIntention.Sync);

        foreach (var edge in polygon.Edges)
        {
            if (edge.IsBezier)
                DrawControlPolygon(g, edge);
        }

        foreach (var edge in polygon.Edges)
            DrawEdge(g, edge);

        foreach (var edge in polygon.Edges)
            DrawConstraintLabel(g, edge);

        foreach (var edge in polygon.Edges)
        {
            foreach (var controlPoint in edge.ControlPoints)
                DrawControlPoint(g, controlPoint);
        }

        foreach (var vertex in polygon.Vertices)
            DrawVertex(g, vertex);
    }

    private void DrawEdge(Graphics g, Edge edge)
    {
        if (edge.IsArc)
        {
            DrawArc(g, edge);
            return;
        }

        DrawPolyline(g, edge.ComputePoints());
    }

    /// <summary>
    /// W trybie bibliotecznym łuk rysuje Graphics.DrawArc, w trybie Bresenhama łamana z punktów łuku.
    /// </summary>
    private void DrawArc(Graphics g, Edge edge)
    {
        var arc = edge.ToArc();
        if (LineMode == LineDrawingMode.Bresenham || arc.IsDegenerate || arc.Radius < 1f)
        {
            DrawPolyline(g, arc.ComputePoints());
            return;
        }

        const float degreesPerRadian = 180f / MathF.PI;
        g.DrawArc(_edgePen,
            arc.Center.X - arc.Radius, arc.Center.Y - arc.Radius, 2f * arc.Radius, 2f * arc.Radius,
            arc.StartAngle * degreesPerRadian, arc.SweepAngle * degreesPerRadian);
    }

    private void DrawPolyline(Graphics g, Vector2[] points)
    {
        for (int i = 1; i < points.Length; i++)
            DrawLine(g, points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y);
    }

    private void DrawControlPolygon(Graphics g, Edge edge)
    {
        var v0 = edge.StartVertex;
        var v1 = edge.ControlPoint1!;
        var v2 = edge.ControlPoint2!;
        var v3 = edge.EndVertex;

        DrawLine(g, v0.X, v0.Y, v1.X, v1.Y, dashed: true);
        DrawLine(g, v1.X, v1.Y, v2.X, v2.Y, dashed: true);
        DrawLine(g, v2.X, v2.Y, v3.X, v3.Y, dashed: true);
        DrawLine(g, v3.X, v3.Y, v0.X, v0.Y, dashed: true);
    }
	/// <summary>
    /// Rysowanie linii - Bresenham albo biblioteczny
	/// Argument dashed decyduje czy rysujemy linię przerywaną czy zwykłą
    /// </summary>
    private void DrawLine(Graphics g, float x1, float y1, float x2, float y2, bool dashed = false)
    {
        if (LineMode == LineDrawingMode.Bresenham && _buffer is not null)
        {
            BresenhamLine.Draw(
                _buffer,
                (int)MathF.Round(x1), (int)MathF.Round(y1),
                (int)MathF.Round(x2), (int)MathF.Round(y2),
                dashed ? ControlPolygonColor : EdgeColor,
                dashed ? DashLength : 0);
        }
        else
        {
            g.DrawLine(dashed ? _controlPolygonPen : _edgePen, x1, y1, x2, y2);
        }
    }

	/// <summary>
    /// Etykiety wszystkich ograniczeń krawędzi obok jej środka
    /// </summary>
    private void DrawConstraintLabel(Graphics g, Edge edge)
    {
        if (edge.Constraints.Count == 0)
            return;

        string label = string.Join(" ", edge.Constraints.Select(c => c.IconLabel));
        var size = g.MeasureString(label, _constraintLabelFont);

        var (mx, my) = edge.Midpoint;
        float dx = edge.EndVertex.X - edge.StartVertex.X;
        float dy = edge.EndVertex.Y - edge.StartVertex.Y;
        float length = MathF.Max(MathF.Sqrt(dx * dx + dy * dy), 1f);
        float cx = mx - dy / length * ConstraintLabelOffset;
        float cy = my + dx / length * ConstraintLabelOffset;

        g.DrawString(label, _constraintLabelFont, _constraintLabelBrush, cx - size.Width / 2f, cy - size.Height / 2f);
    }

    private void DrawVertex(Graphics g, Vertex vertex)
    {
        float x = vertex.X - VertexRadius;
        float y = vertex.Y - VertexRadius;
        float size = 2 * VertexRadius;
        g.FillEllipse(_vertexBrush, x, y, size, size);
        g.DrawEllipse(_vertexOutlinePen, x, y, size, size);

        if (vertex.HasAdjacentCurve)
        {
            g.DrawString(vertex.ContinuityType.ToString(), _labelFont, _continuityLabelBrush,
                vertex.X + VertexRadius + 2f, vertex.Y - VertexRadius - 14f);
        }
    }

    private void DrawControlPoint(Graphics g, ControlPoint controlPoint)
    {
        float x = controlPoint.X - ControlPointHalfSize;
        float y = controlPoint.Y - ControlPointHalfSize;
        float size = 2 * ControlPointHalfSize;
        g.FillRectangle(_controlPointBrush, x, y, size, size);
        g.DrawRectangle(_controlPointOutlinePen, x, y, size, size);
    }
	/// <summary>
    /// Gdy zachodzi zmiana rozmiaru odtwarza bufor
    /// </summary>
    private DirectBitmap EnsureBuffer(int width, int height)
    {
        if (_buffer is null || _buffer.Width != width || _buffer.Height != height)
        {
            _buffer?.Dispose();
            _buffer = new DirectBitmap(width, height);
        }
        return _buffer;
    }

    public void Dispose()
    {
        _buffer?.Dispose();
        _edgePen.Dispose();
        _controlPolygonPen.Dispose();
        _vertexBrush.Dispose();
        _vertexOutlinePen.Dispose();
        _controlPointBrush.Dispose();
        _controlPointOutlinePen.Dispose();
        _labelFont.Dispose();
        _continuityLabelBrush.Dispose();
        _constraintLabelBrush.Dispose();
        _constraintLabelFont.Dispose();
        _highlightPen.Dispose();
    }
}
