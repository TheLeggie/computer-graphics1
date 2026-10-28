using System.Numerics;
using PolygonEditor.Core.Constraints;
using PolygonEditor.Core.Geometry;

namespace PolygonEditor.Core.Model;

public enum EdgeType
{
    Line,
    Bezier,
    Arc
}

public class Edge
{
    public Vertex StartVertex { get; internal set; }
    public Vertex EndVertex { get; internal set; }

    public EdgeType EdgeType { get; private set; } = EdgeType.Line;

    // punkt kontrolny v1 lub null 
    public ControlPoint? ControlPoint1 { get; private set; }

    // punkt kontrolny v2 lub null 
    public ControlPoint? ControlPoint2 { get; private set; }

    // Ograniczenia dodaje i usuwa wyłącznie Scene, która trzyma też ich globalną listę
    private readonly List<IConstraint> _constraints = new();
    public IReadOnlyList<IConstraint> Constraints => _constraints;

    public Edge(Vertex startVertex, Vertex endVertex)
    {
        StartVertex = startVertex;
        EndVertex = endVertex;
    }

    public bool IsBezier => EdgeType == EdgeType.Bezier;

    public bool IsArc => EdgeType == EdgeType.Arc;

    public IEnumerable<ControlPoint> ControlPoints
    {
        get
        {
            if (ControlPoint1 is not null)
                yield return ControlPoint1;
            if (ControlPoint2 is not null)
                yield return ControlPoint2;
        }
    }

    public float Length => StartVertex.DistanceTo(EndVertex.X, EndVertex.Y);

    internal void AddConstraint(IConstraint constraint) => _constraints.Add(constraint);

    internal void RemoveConstraint(IConstraint constraint) => _constraints.Remove(constraint);

	/// Wylicza punkt środkowy w zależności od rodzaju krzywej, dla krzywej Beziera i łuku bierze punkt dla t = 0.5
    public (float X, float Y) Midpoint
    {
        get
        {
            if (IsBezier)
            {
                var p = ToBezier().Evaluate(0.5f);
                return (p.X, p.Y);
            }
            if (IsArc)
            {
                var p = ToArc().Evaluate(0.5f);
                return (p.X, p.Y);
            }
            return ((StartVertex.X + EndVertex.X) / 2f, (StartVertex.Y + EndVertex.Y) / 2f);
        }
    }

    /// <summary>
    /// Zmienia krawędź na krzywą Beziera 3 stopnia z punktami kontrolnymi w 1/3 i 2/3 długości 
    /// </summary>
    public void ConvertToBezier()
    {
        float dx = EndVertex.X - StartVertex.X;
        float dy = EndVertex.Y - StartVertex.Y;
        float nx = -dy * 0.25f;
        float ny = dx * 0.25f;

        ConvertToBezier(
            (StartVertex.X + dx / 3f + nx, StartVertex.Y + dy / 3f + ny),
            (StartVertex.X + 2f * dx / 3f + nx, StartVertex.Y + 2f * dy / 3f + ny));
    }

    public void ConvertToBezier((float X, float Y) controlPoint1, (float X, float Y) controlPoint2)
    {
        EdgeType = EdgeType.Bezier;
        ControlPoint1 = new ControlPoint(this, ControlPointAnchor.Start, controlPoint1.X, controlPoint1.Y);
        ControlPoint2 = new ControlPoint(this, ControlPointAnchor.End, controlPoint2.X, controlPoint2.Y);
        ContinuityEnforcer.ApplyDefaultContinuity(this);
    }

    public void ConvertToLine()
    {
        EdgeType = EdgeType.Line;
        ControlPoint1 = null;
        ControlPoint2 = null;

        // Sąsiednie krzywe muszą się dopasować do kierunku nowego odcinka
        ContinuityEnforcer.EnforceAt(StartVertex);
        ContinuityEnforcer.EnforceAt(EndVertex);
    }

    /// <summary>
    /// Zmienia krawędź na łuk okręgu. Kształt łuku nie jest zapamiętywany - wynika z położenia końców
    /// i ciągłości w nich.
    /// </summary>
    public void ConvertToArc()
    {
        EdgeType = EdgeType.Arc;
        ControlPoint1 = null;
        ControlPoint2 = null;
        ContinuityEnforcer.ApplyArcContinuity(this);
    }

    public CubicBezier ToBezier()
    {
        if (ControlPoint1 is null || ControlPoint2 is null)
            throw new InvalidOperationException("The edge is not a Bézier segment.");

        return new CubicBezier(
            new Vector2(StartVertex.X, StartVertex.Y),
            new Vector2(ControlPoint1.X, ControlPoint1.Y),
            new Vector2(ControlPoint2.X, ControlPoint2.Y),
            new Vector2(EndVertex.X, EndVertex.Y));
    }

    /// <summary>
    /// Geometria łuku: półokrąg, gdy oba końce mają G0, albo łuk styczny do sąsiedniej krawędzi w końcu z G1.
    /// Gdyby oba końce miały G1, wygrywa początek. W wierzchołku G1 między dwoma łukami styczny do poprzedniego łuku
    /// jest łuk wychodzący, dzięki czemu wyliczenia nie zapętlają się.
    /// </summary>
    public CircularArc ToArc() => ComputeArc(followNeighbourTangents: true);

    private CircularArc ComputeArc(bool followNeighbourTangents)
    {
        if (!IsArc)
            throw new InvalidOperationException("The edge is not an arc.");

        var a = new Vector2(StartVertex.X, StartVertex.Y);
        var b = new Vector2(EndVertex.X, EndVertex.Y);
        if (followNeighbourTangents)
        {
            if (StartVertex.ContinuityType != ContinuityType.G0 && StartVertex.IncomingEdge is { } incoming)
                return CircularArc.Tangent(a, b, incoming.TangentAtEnd());

            if (EndVertex.ContinuityType != ContinuityType.G0 && EndVertex.OutgoingEdge is { IsArc: false } outgoing)
                return CircularArc.Tangent(b, a, -outgoing.TangentAtStart()).Reverse();
        }
        return CircularArc.Semicircle(a, b);
    }

    /// <summary>
    /// Kierunek wyjścia krawędzi z wierzchołka początkowego
    /// </summary>
    private Vector2 TangentAtStart()
    {
        var start = new Vector2(StartVertex.X, StartVertex.Y);
        return EdgeType switch
        {
            EdgeType.Bezier => new Vector2(ControlPoint1!.X, ControlPoint1.Y) - start,
            EdgeType.Arc => ComputeArc(followNeighbourTangents: false).TangentAtStart,
            _ => new Vector2(EndVertex.X, EndVertex.Y) - start
        };
    }

    /// <summary>
    /// Kierunek wejścia krawędzi do wierzchołka końcowego
    /// </summary>
    private Vector2 TangentAtEnd()
    {
        var end = new Vector2(EndVertex.X, EndVertex.Y);
        return EdgeType switch
        {
            EdgeType.Bezier => end - new Vector2(ControlPoint2!.X, ControlPoint2.Y),
            EdgeType.Arc => ComputeArc(followNeighbourTangents: false).TangentAtEnd,
            _ => end - new Vector2(StartVertex.X, StartVertex.Y)
        };
    }

    /// <summary>
    /// Kolejne punkty krawędzi od początku do końca: dla odcinka jego końce, dla krzywych przybliżenie łamaną
    /// </summary>
    public Vector2[] ComputePoints() => EdgeType switch
    {
        EdgeType.Bezier => ToBezier().ComputePoints(),
        EdgeType.Arc => ToArc().ComputePoints(),
        _ => new[] { new Vector2(StartVertex.X, StartVertex.Y), new Vector2(EndVertex.X, EndVertex.Y) }
    };

	/// <summary>
	/// Oblicza odległość punktu od danej krawędzi
	/// </summary>
    public float DistanceTo(float x, float y)
    {
        if (EdgeType == EdgeType.Line)
            return GeometryMath.DistanceToSegment(x, y, StartVertex.X, StartVertex.Y, EndVertex.X, EndVertex.Y);

		// dla krzywej odległość punktu od krzywej liczymy dzieląc krzywą na małe odcinki
		// i sprawdzamy odległość euklidesową - daje to bardzo mały błąd przybliżenia
        var points = ComputePoints();
        float best = float.MaxValue;
        for (int i = 1; i < points.Length; i++)
        {
            float distance = GeometryMath.DistanceToSegment(
                x, y, points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y);
            best = MathF.Min(best, distance);
        }
        return best;
    }

    public override string ToString() => $"{StartVertex} -> {EndVertex} [{EdgeType}]";
}
