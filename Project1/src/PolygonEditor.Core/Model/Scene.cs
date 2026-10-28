using PolygonEditor.Core.Constraints;

namespace PolygonEditor.Core.Model;

// Rekordy potrzebne do zwracania wierzchołka lub krawędzi - od razu zwracamy takze do którego wielokąta należą
public sealed record VertexHit(Polygon Polygon, Vertex Vertex);

public sealed record EdgeHit(Polygon Polygon, Edge Edge);

public class Scene
{
    public const float DefaultHitTolerance = 8f;

    // Lista wielokątów - im wyższy indeks tym wyżej rysowany na canvas jest wielokąt
    private readonly List<Polygon> _polygons = new();
    public IReadOnlyList<Polygon> Polygons => _polygons;

    private readonly List<IConstraint> _constraints = new();
    public IReadOnlyList<IConstraint> Constraints => _constraints;

    public Polygon AddPolygon(params (float X, float Y)[] points)
    {
        var polygon = new Polygon(points);
        _polygons.Add(polygon);
        return polygon;
    }
    
    public bool HideEdge(Polygon polygon, Edge edge)
    {
        if (!polygon.CanHideEdge || !polygon.Edges.Contains(edge))
            return false;

        RemoveConstraintsOn(edge);
        
        
        edge.StartVertex.ContinuityType = ContinuityType.G0;
        edge.StartVertex.IsContinuityExplicitlySet = false;
        edge.EndVertex.ContinuityType = ContinuityType.G0;
        edge.EndVertex.IsContinuityExplicitlySet = false;

        edge.ConvertToLine();
        edge.IsHidden = true;

        return true;
    }
    public void ShowAllHiddenEdges()
    {
        foreach (var polygon in _polygons)
            polygon.ShowHiddenEdges();
    }

    public void ShowBsplines()
    {
        
    }
    public void AddPolygon(Polygon polygon) => _polygons.Add(polygon);

    public bool RemovePolygon(Polygon polygon)
    {
        if (!_polygons.Remove(polygon))
            return false;

        foreach (var edge in polygon.Edges)
            RemoveConstraintsOn(edge);
        return true;
    }

    /// <summary>
    /// Dodaje ograniczenie, od razu je wymusza i dopasowuje do niego resztę sceny
    /// </summary>
    public void AddConstraint(IConstraint constraint)
    {
        _constraints.Add(constraint);
        foreach (var edge in constraint.Edges)
            edge.AddConstraint(constraint);

        constraint.Enforce(new HashSet<Vertex>());
        var moved = ConstraintSolver.Solve(constraint.Vertices);
        ContinuityEnforcer.RestoreAround(constraint.Vertices.Concat(moved));
    }

    public void RemoveConstraint(IConstraint constraint)
    {
        if (!_constraints.Remove(constraint))
            return;

        foreach (var edge in constraint.Edges)
            edge.RemoveConstraint(constraint);
    }

    public void RemoveConstraintsOn(Edge edge)
    {
        foreach (var constraint in edge.Constraints.ToList())
            RemoveConstraint(constraint);
    }

    /// <summary>
    /// Przeciągnięcie wierzchołka: wierzchołek idzie dokładnie za kursorem, resztę dopasowuje solver i ciągłość
    /// </summary>
    public void MoveVertex(Vertex vertex, float x, float y)
    {
        vertex.MoveWithControlPoints(x, y);
        var moved = ConstraintSolver.Solve(new[] { vertex });
        ContinuityEnforcer.RestoreAround(moved.Prepend(vertex));
    }

    /// <summary>
    /// Przeciągnięcie punktu kontrolnego. Wierzchołki jego krzywej są zablokowane, więc jeśli ciągłość
    /// obróciła sąsiedni odcinek, solver może go jedynie dopasować (np. przywrócić zadaną długość) i przenieść zmiany dalej.
    /// </summary>
    public void MoveControlPoint(ControlPoint controlPoint, float x, float y)
    {
        var movedVertex = ContinuityEnforcer.MoveControlPoint(controlPoint, x, y);
        if (movedVertex is null)
            return;

        var edge = controlPoint.ParentEdge;
        var moved = ConstraintSolver.Solve(new[] { edge.StartVertex, edge.EndVertex });
        ContinuityEnforcer.RestoreAround(moved.Prepend(movedVertex), frozenVertex: controlPoint.AnchorVertex);

        // Jeśli ograniczenia nie pozwoliły przeskalować odcinka, C1 przywraca długość ramienia punktu kontrolnego
        ContinuityEnforcer.EnforceAt(controlPoint.AnchorVertex);
    }

    /// <summary>
    /// Podział krawędzi usuwa nałożone na nią ograniczenia
    /// </summary>
    public Vertex InsertVertexOnEdge(Polygon polygon, Edge edge)
    {
        RemoveConstraintsOn(edge);
        return polygon.InsertVertexOnEdge(edge);
    }

    /// <summary>
    /// Usunięcie wierzchołka usuwa ograniczenia z obu krawędzi, które zostaną zastąpione nową krawędzią
    /// </summary>
    public bool RemoveVertex(Polygon polygon, Vertex vertex)
    {
        if (!polygon.CanRemoveVertex || !polygon.Vertices.Contains(vertex))
            return false;

        foreach (var edge in vertex.AdjacentEdges.ToList())
            RemoveConstraintsOn(edge);
        return polygon.RemoveVertex(vertex);
    }

    /// <summary>
    /// Ograniczenia liniowe nie mają sensu dla krzywej, więc są zdejmowane przy zamianie na krzywą Béziera
    /// </summary>
    public void ConvertEdgeToBezier(Edge edge)
    {
        RemoveConstraintsOn(edge);
        edge.ConvertToBezier();
    }

    /// <summary>
    /// Tak samo jak przy krzywej Béziera, ograniczenia liniowe są zdejmowane przy zamianie na łuk
    /// </summary>
    public void ConvertEdgeToArc(Edge edge)
    {
        RemoveConstraintsOn(edge);
        edge.ConvertToArc();
    }
    
    /// <summary>
    /// Szuka najbliższego wierzchołka, sprawdzamy wielokąty od góry (narysowane najwyżej)
    /// </summary>
    public VertexHit? FindVertexAt(float x, float y, float tolerance = DefaultHitTolerance)
    {
        VertexHit? best = null;
        float bestDistance = tolerance;

        for (int i = _polygons.Count - 1; i >= 0; i--)
        {
            foreach (var vertex in _polygons[i].Vertices)
            {
                float distance = vertex.DistanceTo(x, y);
                if (distance <= bestDistance && (best is null || distance < bestDistance))
                {
                    best = new VertexHit(_polygons[i], vertex);
                    bestDistance = distance;
                }
            }
        }
        return best;
    }

    /// <summary>
    /// Szuka najbliższego punktu kontrolnego, sprawdzamy wielokąty od góry (narysowane najwyżej)
    /// </summary>
    public ControlPoint? FindControlPointAt(float x, float y, float tolerance = DefaultHitTolerance)
    {
        ControlPoint? best = null;
        float bestDistance = tolerance;

        for (int i = _polygons.Count - 1; i >= 0; i--)
        {
            foreach (var edge in _polygons[i].Edges)
            {
                if(edge.IsHidden) continue;
                
                foreach (var controlPoint in edge.ControlPoints)
                {
                    float distance = controlPoint.DistanceTo(x, y);
                    if (distance <= bestDistance && (best is null || distance < bestDistance))
                    {
                        best = controlPoint;
                        bestDistance = distance;
                    }
                }
            }
        }
        return best;
    }

    /// <summary>
    /// Szuka najbliższej krawędzi, sprawdzamy wielokąty od góry (narysowane najwyżej)
    /// </summary>
    public EdgeHit? FindEdgeAt(float x, float y, float tolerance = DefaultHitTolerance)
    {
        EdgeHit? best = null;
        float bestDistance = tolerance;

        for (int i = _polygons.Count - 1; i >= 0; i--)
        {
            foreach (var edge in _polygons[i].Edges)
            {
                if(edge.IsHidden) continue;
                float distance = edge.DistanceTo(x, y);
                if (distance <= bestDistance && (best is null || distance < bestDistance))
                {
                    best = new EdgeHit(_polygons[i], edge);
                    bestDistance = distance;
                }
            }
        }
        return best;
    }

    /// <summary>
    /// Szuka wielokąta, w którego wnętrzu leży punkt
    /// </summary>
    public Polygon? FindPolygonAt(float x, float y)
    {
        for (int i = _polygons.Count - 1; i >= 0; i--)
        {
            if (_polygons[i].ContainsPoint(x, y))
                return _polygons[i];
        }
        return null;
    }
}
