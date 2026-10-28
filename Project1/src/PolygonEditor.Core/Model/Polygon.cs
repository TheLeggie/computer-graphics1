using System.Numerics;

namespace PolygonEditor.Core.Model;

/// <summary>
/// Edges[i] łączy Vertices[i] z Vertices[(i + 1) % Count].
/// </summary>
public class Polygon
{
    public const int MinVertexCount = 3;

    private readonly List<Vertex> _vertices = new();
    private readonly List<Edge> _edges = new();

    public IReadOnlyList<Vertex> Vertices => _vertices;
    public IReadOnlyList<Edge> Edges => _edges;

    public bool CanRemoveVertex => _vertices.Count > MinVertexCount;
    public bool CanHideEdge => !_edges.Any(e => e.IsHidden);
    public Polygon(IEnumerable<(float X, float Y)> points)
    {
        foreach (var (x, y) in points)
            _vertices.Add(new Vertex(x, y));

        if (_vertices.Count < MinVertexCount)
            throw new ArgumentException($"A polygon needs at least {MinVertexCount} vertices.", nameof(points));

        RebuildEdges();
    }
    
    public bool HideEdge(Edge edge)
    {
        if (!CanHideEdge || !_edges.Contains(edge))
            return false;

        edge.IsHidden = true;
        return true;
    }
    
    public void ShowHiddenEdges()
    {
        foreach (var edge in _edges)
            edge.IsHidden = false;
    }
	/// <summary>
	/// Przesuwa cały wielokąt o wektor (dx, dy)
    /// </summary>
    public void Translate(float dx, float dy)
    {
        foreach (var vertex in _vertices)
            vertex.MoveBy(dx, dy);

        foreach (var edge in _edges)
        {
            foreach (var controlPoint in edge.ControlPoints)
                controlPoint.MoveBy(dx, dy);
        }
    }

    /// <summary>
    /// Podział krawędzi na pół
    /// </summary>
    public Vertex InsertVertexOnEdge(Edge edge)
    {
        int index = _edges.IndexOf(edge);
        if (index < 0)
            throw new ArgumentException("The edge does not belong to this polygon.", nameof(edge));

        var (mx, my) = edge.Midpoint;
        var vertex = new Vertex(mx, my);

        var first = new Edge(edge.StartVertex, vertex);
        var second = new Edge(vertex, edge.EndVertex);
        edge.StartVertex.OutgoingEdge = first;
        vertex.IncomingEdge = first;
        vertex.OutgoingEdge = second;
        edge.EndVertex.IncomingEdge = second;

        _vertices.Insert(index + 1, vertex);
        _edges[index] = first;
        _edges.Insert(index + 1, second);

        ContinuityEnforcer.EnforceAt(first.StartVertex);
        ContinuityEnforcer.EnforceAt(second.EndVertex);
        return vertex;
    }

    /// <summary>
    /// Usuwa wierzchołek i łączy jego dwóch sąsiadów nową krawędzią
    /// </summary>
    public bool RemoveVertex(Vertex vertex)
    {
        int index = _vertices.IndexOf(vertex);
        if (index < 0 || !CanRemoveVertex)
            return false;

        int incomingIndex = (index - 1 + _vertices.Count) % _vertices.Count;
        
        var incomingEdge = _edges[incomingIndex];
        var outgoingEdge = _edges[index];

        var previous = incomingEdge.StartVertex;
        var next = outgoingEdge.EndVertex;
        
        bool wasAdjacentHidden = incomingEdge.IsHidden || outgoingEdge.IsHidden;

        var merged = new Edge(previous, next)
        {
            IsHidden = wasAdjacentHidden
        };
        
        if (wasAdjacentHidden)
        {
            previous.ContinuityType = ContinuityType.G0;
            previous.IsContinuityExplicitlySet = false;
            next.ContinuityType = ContinuityType.G0;
            next.IsContinuityExplicitlySet = false;
        }
        
        previous.OutgoingEdge = merged;
        next.IncomingEdge = merged;

        _edges[incomingIndex] = merged;
        _edges.RemoveAt(index);
        _vertices.RemoveAt(index);

        vertex.IncomingEdge = null;
        vertex.OutgoingEdge = null;

        ContinuityEnforcer.EnforceAt(previous);
        ContinuityEnforcer.EnforceAt(next);
        return true;
    }

    /// <summary>
    /// Even-odd rule - pozwala rozstrzygnąć czy jesteśmy w środku czy na zewnątrz danego wielokąta
    /// Świecimy umownym laserem z naszego punkty poziomo w prawo do nieskończoności
    /// Dla każdej krawędzi sprawdzamy czy ten laser ją przecina, jeśli liczba przecięć jest parzysta to jesteśmy
	/// poza danym wielokątem, jeśli jest nieparzysta to jesteśmy w środku
	/// Krzywe Béziera i łuki wchodzą do konturu jako łamana z punktów krzywej, więc liczy się też obszar pod krzywą
    /// </summary>
    public bool ContainsPoint(float x, float y)
    {
        var outline = GetOutline();
        bool inside = false;
        for (int i = 0, j = outline.Count - 1; i < outline.Count; j = i++)
        {
            var a = outline[i];
            var b = outline[j];
			// dwa warunki, czy prosta zawiera się między współrzędnymi na osi Y dwóch wierzchołków
			// oraz czy jest po prawo od naszego punktu
            if ((a.Y > y) != (b.Y > y) && x < (b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X)
                inside = !inside;
        }
        return inside;
    }

	/// <summary>
	/// Kontur wielokąta: wierzchołki, a dla krzywych także punkty pośrednie krzywej
	/// </summary>
    private List<Vector2> GetOutline()
    {
        var outline = new List<Vector2>();
        foreach (var edge in _edges)
        {
            // ostatni punkt krawędzi jest pierwszym punktem następnej
            var points = edge.ComputePoints();
            for (int i = 0; i < points.Length - 1; i++)
                outline.Add(points[i]);
        }
        return outline;
    }

	/// <summary>
    /// Buduje nowo powstający wielokąt, wywołanie tylko w konstruktorze
    /// </summary>
    private void RebuildEdges()
    {
        _edges.Clear();
        for (int i = 0; i < _vertices.Count; i++)
        {
            var start = _vertices[i];
            var end = _vertices[(i + 1) % _vertices.Count];
            var edge = new Edge(start, end);
            start.OutgoingEdge = edge;
            end.IncomingEdge = edge;
            _edges.Add(edge);
        }
    }
}
