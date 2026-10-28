using PolygonEditor.Core.Constraints;
using PolygonEditor.Core.Model;

namespace PolygonEditor.UI;

/// <summary>
/// Zamienia akcje myszy na płótnie na edycję sceny:
/// LPM na wierzchołku lub punkcie kontrolnym Béziera przeciąga ten punkt, LPM na krawędzi lub we wnętrzu wielokąta
/// przeciąga cały wielokąt, PPM otwiera menu kontekstowe wierzchołka/krawędzi pod kursorem.
/// Gdy relacja czeka na drugą krawędź, LPM na krawędzi ją tworzy, a PPM / klik w puste miejsce anuluje wybór.
/// </summary>
public sealed class InteractionController : IDisposable
{
    private readonly Scene _scene;
    private readonly Control _canvas;

    private readonly ContextMenuStrip _contextMenu = new();
    private readonly ToolStripMenuItem _addVertexItem = new("Dodaj wierzchołek w środku krawędzi");
    private readonly ToolStripMenuItem _toLineItem = new("Zamień na odcinek");
    private readonly ToolStripMenuItem _toBezierItem = new("Zamień na krzywą Béziera");
    private readonly ToolStripMenuItem _toArcItem = new("Zamień na łuk okręgu");
    private readonly ToolStripMenuItem _fixedLengthItem = new("Zadana długość krawędzi...");
    private readonly ToolStripMenuItem _perpendicularItem = new("Prostopadłość z inną krawędzią...");
    private readonly ToolStripMenuItem _removeConstraintItem = new("Usuń ograniczenie");
    private readonly ToolStripMenuItem _removeVertexItem = new("Usuń wierzchołek");
    private readonly ToolStripMenuItem _continuityItem = new("Ciągłość w wierzchołku");
    private readonly Dictionary<ContinuityType, ToolStripMenuItem> _continuityTypeItems = new();

    private Vertex? _draggedVertex;
    private ControlPoint? _draggedControlPoint;
    private Polygon? _draggedPolygon;
    private PointF _lastMousePosition;

    private VertexHit? _contextVertex;
    private EdgeHit? _contextEdge;

    /// <summary>
    /// Pierwsza krawędź relacji prostopadłości, czekająca na wskazanie drugiej
    /// </summary>
    public Edge? PendingPerpendicularEdge { get; private set; }

    public event EventHandler? PendingSelectionChanged;

    public InteractionController(Scene scene, Control canvas)
    {
        _scene = scene;
        _canvas = canvas;

        _addVertexItem.Click += AddVertexItem_Click;
        _toLineItem.Click += (_, _) => ConvertContextEdge(EdgeType.Line);
        _toBezierItem.Click += (_, _) => ConvertContextEdge(EdgeType.Bezier);
        _toArcItem.Click += (_, _) => ConvertContextEdge(EdgeType.Arc);
        _fixedLengthItem.Click += FixedLengthItem_Click;
        _perpendicularItem.Click += PerpendicularItem_Click;
        _removeVertexItem.Click += RemoveVertexItem_Click;
        foreach (var type in Enum.GetValues<ContinuityType>())
        {
            var item = new ToolStripMenuItem(type.ToString());
            item.Click += (_, _) => SetContinuity(type);
            _continuityTypeItems[type] = item;
            _continuityItem.DropDownItems.Add(item);
        }

        _contextMenu.Items.AddRange(new ToolStripItem[]
        {
            _addVertexItem, _toLineItem, _toBezierItem, _toArcItem, _fixedLengthItem, _perpendicularItem, _removeConstraintItem,
            _removeVertexItem, _continuityItem
        });

        _canvas.MouseDown += Canvas_MouseDown;
        _canvas.MouseMove += Canvas_MouseMove;
        _canvas.MouseUp += Canvas_MouseUp;
    }

    private void Canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (PendingPerpendicularEdge is not null)
        {
            if (e.Button == MouseButtons.Left)
                CompletePerpendicular(e.X, e.Y);
            else
                CancelPendingSelection();
            return;
        }

        if (e.Button == MouseButtons.Left)
            BeginDrag(e.X, e.Y);
        else if (e.Button == MouseButtons.Right && !IsDragging)
            ShowContextMenu(e.Location);
    }

    private void Canvas_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_draggedVertex is not null)
        {
            _scene.MoveVertex(_draggedVertex, e.X, e.Y);
            _canvas.Invalidate();
        }
        else if (_draggedControlPoint is not null)
        {
            _scene.MoveControlPoint(_draggedControlPoint, e.X, e.Y);
            _canvas.Invalidate();
        }
        else if (_draggedPolygon is not null)
        {
            _draggedPolygon.Translate(e.X - _lastMousePosition.X, e.Y - _lastMousePosition.Y);
            _lastMousePosition = new PointF(e.X, e.Y);
            _canvas.Invalidate();
        }
        else
        {
            UpdateHoverCursor(e.X, e.Y);
        }
    }

    private void Canvas_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        _draggedVertex = null;
        _draggedControlPoint = null;
        _draggedPolygon = null;
        UpdateHoverCursor(e.X, e.Y);
    }

    private bool IsDragging => _draggedVertex is not null || _draggedControlPoint is not null || _draggedPolygon is not null;

    private void BeginDrag(float x, float y)
    {
        var vertexHit = _scene.FindVertexAt(x, y);
        if (vertexHit is not null)
        {
            _draggedVertex = vertexHit.Vertex;
            _scene.MoveVertex(_draggedVertex, x, y);
            _canvas.Invalidate();
            return;
        }

        var controlPoint = _scene.FindControlPointAt(x, y);
        if (controlPoint is not null)
        {
            _draggedControlPoint = controlPoint;
            _scene.MoveControlPoint(_draggedControlPoint, x, y);
            _canvas.Invalidate();
            return;
        }

        _draggedPolygon = _scene.FindEdgeAt(x, y)?.Polygon ?? _scene.FindPolygonAt(x, y);
        _lastMousePosition = new PointF(x, y);
    }

    private void UpdateHoverCursor(float x, float y)
    {
        if (PendingPerpendicularEdge is not null)
            _canvas.Cursor = Cursors.Cross;
        else if (_scene.FindVertexAt(x, y) is not null || _scene.FindControlPointAt(x, y) is not null)
            _canvas.Cursor = Cursors.Hand;
        else if (_scene.FindEdgeAt(x, y) is not null || _scene.FindPolygonAt(x, y) is not null)
            _canvas.Cursor = Cursors.SizeAll;
        else
            _canvas.Cursor = Cursors.Default;
    }

    private void ShowContextMenu(Point location)
    {
        _contextVertex = _scene.FindVertexAt(location.X, location.Y);
        _contextEdge = _contextVertex is null ? _scene.FindEdgeAt(location.X, location.Y) : null;

        if (_contextVertex is null && _contextEdge is null)
            return;

        _addVertexItem.Visible = _contextEdge is not null;
        var edgeType = _contextEdge?.Edge.EdgeType;
        _toLineItem.Visible = edgeType is not null and not EdgeType.Line;
        _toBezierItem.Visible = edgeType is not null and not EdgeType.Bezier;
        _toArcItem.Visible = edgeType is not null and not EdgeType.Arc;

        _fixedLengthItem.Visible = _contextEdge?.Edge.EdgeType == EdgeType.Line;
        _perpendicularItem.Visible = _contextEdge?.Edge.EdgeType == EdgeType.Line;
        RebuildRemoveConstraintItems();

        _removeVertexItem.Visible = _contextVertex is not null;
        _removeVertexItem.Enabled = _contextVertex?.Polygon.CanRemoveVertex ?? false;
        _removeVertexItem.ToolTipText = _removeVertexItem.Enabled
            ? null
            : $"Wielokąt musi mieć co najmniej {Polygon.MinVertexCount} wierzchołki.";

        _continuityItem.Visible = _contextVertex?.Vertex.HasAdjacentCurve == true;
        foreach (var (type, item) in _continuityTypeItems)
            item.Checked = _contextVertex?.Vertex.ContinuityType == type;

        // Łuk obsługuje tylko G0 i G1. Włączenie G1 przełącza drugi koniec łuku na G0.
        var c1Item = _continuityTypeItems[ContinuityType.C1];
        c1Item.Enabled = _contextVertex?.Vertex.HasAdjacentArc != true;
        c1Item.ToolTipText = c1Item.Enabled ? null : "Łuk okręgu obsługuje tylko ciągłość G0 i G1.";

        _contextMenu.Show(_canvas, location);
    }

    private void AddVertexItem_Click(object? sender, EventArgs e)
    {
        if (_contextEdge is null)
            return;

        _scene.InsertVertexOnEdge(_contextEdge.Polygon, _contextEdge.Edge);
        _contextEdge = null;
        _canvas.Invalidate();
    }

    /// <summary>
    /// Pozycja "Usuń ograniczenie" dostaje po jednym podpunkcie na każde ograniczenie krawędzi (opisany jego etykietą)
    /// </summary>
    private void RebuildRemoveConstraintItems()
    {
        var oldItems = _removeConstraintItem.DropDownItems.Cast<ToolStripItem>().ToArray();
        _removeConstraintItem.DropDownItems.Clear();
        foreach (var item in oldItems)
            item.Dispose();

        var constraints = _contextEdge?.Edge.Constraints ?? Array.Empty<IConstraint>();
        foreach (var constraint in constraints)
        {
            var item = new ToolStripMenuItem(constraint.ToString());
            item.Click += (_, _) => RemoveConstraint(constraint);
            _removeConstraintItem.DropDownItems.Add(item);
        }
        _removeConstraintItem.Visible = constraints.Count > 0;
    }

    private void FixedLengthItem_Click(object? sender, EventArgs e)
    {
        if (_contextEdge is null)
            return;

        var edge = _contextEdge.Edge;
        _contextEdge = null;

        using var dialog = new LengthDialog(edge.Length);
        if (dialog.ShowDialog(_canvas.FindForm()) != DialogResult.OK)
            return;

        foreach (var existing in edge.Constraints.OfType<FixedLengthConstraint>().ToList())
            _scene.RemoveConstraint(existing);
        _scene.AddConstraint(new FixedLengthConstraint(edge, dialog.Length));
        _canvas.Invalidate();
    }

    private void PerpendicularItem_Click(object? sender, EventArgs e)
    {
        if (_contextEdge is null)
            return;

        SetPendingPerpendicularEdge(_contextEdge.Edge);
        _contextEdge = null;
    }

    /// <summary>
    /// Kliknięcie w odcinek kończy wybór pary, kliknięcie w puste miejsce go anuluje.
    /// Krzywe, ta sama krawędź i już istniejąca para są ignorowane - można kliknąć inną krawędź.
    /// </summary>
    private void CompletePerpendicular(float x, float y)
    {
        var first = PendingPerpendicularEdge!;
        var second = _scene.FindEdgeAt(x, y)?.Edge;
        if (second is null)
        {
            CancelPendingSelection();
            return;
        }

        bool alreadyPaired = _scene.Constraints.OfType<PerpendicularConstraint>().Any(c => c.Involves(first, second));
        if (second == first || second.EdgeType != EdgeType.Line || alreadyPaired)
            return;

        SetPendingPerpendicularEdge(null);
        _scene.AddConstraint(new PerpendicularConstraint(first, second));
        _canvas.Invalidate();
    }

    public void CancelPendingSelection() => SetPendingPerpendicularEdge(null);

    private void SetPendingPerpendicularEdge(Edge? edge)
    {
        if (PendingPerpendicularEdge == edge)
            return;

        PendingPerpendicularEdge = edge;
        _canvas.Cursor = edge is null ? Cursors.Default : Cursors.Cross;
        PendingSelectionChanged?.Invoke(this, EventArgs.Empty);
        _canvas.Invalidate();
    }

    private void RemoveConstraint(IConstraint constraint)
    {
        _scene.RemoveConstraint(constraint);
        _contextEdge = null;
        _canvas.Invalidate();
    }

    private void ConvertContextEdge(EdgeType type)
    {
        if (_contextEdge is null)
            return;

        var edge = _contextEdge.Edge;
        switch (type)
        {
            case EdgeType.Line:
                edge.ConvertToLine();
                break;
            case EdgeType.Bezier:
                _scene.ConvertEdgeToBezier(edge);
                break;
            case EdgeType.Arc:
                _scene.ConvertEdgeToArc(edge);
                break;
        }

        _contextEdge = null;
        _canvas.Invalidate();
    }

    private void RemoveVertexItem_Click(object? sender, EventArgs e)
    {
        if (_contextVertex is null)
            return;

        _scene.RemoveVertex(_contextVertex.Polygon, _contextVertex.Vertex);
        _contextVertex = null;
        _canvas.Invalidate();
    }

    private void SetContinuity(ContinuityType type)
    {
        if (_contextVertex is null)
            return;

        ContinuityEnforcer.SetContinuity(_contextVertex.Vertex, type);
        _contextVertex = null;
        _canvas.Invalidate();
    }

    public void Dispose()
    {
        _canvas.MouseDown -= Canvas_MouseDown;
        _canvas.MouseMove -= Canvas_MouseMove;
        _canvas.MouseUp -= Canvas_MouseUp;
        _contextMenu.Dispose();
    }
}
