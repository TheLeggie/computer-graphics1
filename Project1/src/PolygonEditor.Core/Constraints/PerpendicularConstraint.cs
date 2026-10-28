using System.Numerics;
using PolygonEditor.Core.Model;

namespace PolygonEditor.Core.Constraints;

/// <summary>
/// Prostopadłość dwóch odcinków (także z różnych wielokątów): u · v = 0, gdzie u = B - A, v = D - C.
/// Wymuszana obrotem jednej krawędzi z zachowaniem jej długości, więc nie koliduje z zadaną długością.
/// </summary>
public sealed class PerpendicularConstraint : IConstraint
{
    private const float DegenerateLength = 1e-4f;

    // Numer pary wyświetlany przy obu krawędziach, żeby było widać, które są ze sobą powiązane
    private static int _nextId = 1;

    private readonly Edge _first;
    private readonly Edge _second;

    public PerpendicularConstraint(Edge first, Edge second)
    {
        if (first == second)
            throw new ArgumentException("An edge cannot be perpendicular to itself.", nameof(second));
        if (first.EdgeType != EdgeType.Line || second.EdgeType != EdgeType.Line)
            throw new ArgumentException("Perpendicularity can only be set on straight edges.");

        _first = first;
        _second = second;
        Id = _nextId++;
        Edges = new[] { first, second };
        Vertices = new[] { first.StartVertex, first.EndVertex, second.StartVertex, second.EndVertex }
            .Distinct()
            .ToArray();
    }

    public int Id { get; }

    public IReadOnlyList<Edge> Edges { get; }
    public IReadOnlyList<Vertex> Vertices { get; }

    public string IconLabel => $"⊥{Id}";

    public bool Involves(Edge edge) => edge == _first || edge == _second;

    public bool Involves(Edge first, Edge second) =>
        (first == _first && second == _second) || (first == _second && second == _first);

    /// <summary>
    /// Odchylenie mierzone w pikselach: o ile przesunąłby się koniec dłuższej krawędzi przy obrocie do prostopadłości
    /// </summary>
    public bool IsSatisfied(float epsilon)
    {
        var u = Direction(_first);
        var v = Direction(_second);
        float lu = u.Length();
        float lv = v.Length();
        if (lu < DegenerateLength || lv < DegenerateLength)
            return true;

		// wartość bezwzględna cosinusa kąta między wektorami
        float cosine = MathF.Abs(Vector2.Dot(u, v)) / (lu * lv);
        return cosine * MathF.Max(lu, lv) <= epsilon;
    }

    /// <summary>
    /// Obraca krawędź z mniejszą liczbą zablokowanych wierzchołków (przy remisie drugą) do prostopadłości względem pierwszej
    /// </summary>
    public bool Enforce(IReadOnlySet<Vertex> lockedVertices)
    {
        int firstLocked = LockedCount(_first, lockedVertices);
        int secondLocked = LockedCount(_second, lockedVertices);
        if (firstLocked == 2 && secondLocked == 2)
            return false;

        if (secondLocked <= firstLocked)
            RotatePerpendicular(_second, _first, lockedVertices);
        else
            RotatePerpendicular(_first, _second, lockedVertices);
        return true;
    }

    public override string ToString() => $"Prostopadłość {IconLabel}";

    /// <summary>
    /// Ustawia krawędź "target" prostopadle do krawędzi "reference", wybierając bliższy z dwóch zwrotów.
    /// Obrót następuje wokół zablokowanego końca, wspólnego wierzchołka (żeby nie zmienić kierunku drugiej krawędzi)
    /// albo wokół środka krawędzi.
    /// </summary>
    private void RotatePerpendicular(Edge target, Edge reference, IReadOnlySet<Vertex> lockedVertices)
    {
        var u = Direction(reference);
        var v = Direction(target);
        float lu = u.Length();
        float lv = v.Length();
        if (lu < DegenerateLength || lv < DegenerateLength)
            return;

        var normal = new Vector2(-u.Y, u.X) / lu;
        if (Vector2.Dot(normal, v) < 0f)
            normal = -normal;
        var newVector = normal * lv;

        var start = target.StartVertex;
        var end = target.EndVertex;
		// wybór punktu obrotu
        var pivot = lockedVertices.Contains(start) ? start
            : lockedVertices.Contains(end) ? end
            : IsShared(start, reference) ? start
            : IsShared(end, reference) ? end
            : null;

        if (pivot == start)
        {
            MoveTo(end, Position(start) + newVector);
        }
        else if (pivot == end)
        {
            MoveTo(start, Position(end) - newVector);
        }
        else
        {
			// obrót wokół środka krawędzi - ostateczna opcja gdy nie możemy obrócić tylko 1 krawędzią aby przywrócić ograniczenie
            var middle = (Position(start) + Position(end)) / 2f;
            MoveTo(start, middle - newVector / 2f);
            MoveTo(end, middle + newVector / 2f);
        }
    }

    private static bool IsShared(Vertex vertex, Edge edge) => edge.StartVertex == vertex || edge.EndVertex == vertex;

    private static int LockedCount(Edge edge, IReadOnlySet<Vertex> lockedVertices) =>
        (lockedVertices.Contains(edge.StartVertex) ? 1 : 0) + (lockedVertices.Contains(edge.EndVertex) ? 1 : 0);

    private static Vector2 Direction(Edge edge) => Position(edge.EndVertex) - Position(edge.StartVertex);

    private static Vector2 Position(Vertex vertex) => new(vertex.X, vertex.Y);

    private static void MoveTo(Vertex vertex, Vector2 position) => vertex.MoveWithControlPoints(position.X, position.Y);
}
