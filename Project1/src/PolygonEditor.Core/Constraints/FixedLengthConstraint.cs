using System.Globalization;
using System.Numerics;
using PolygonEditor.Core.Model;

namespace PolygonEditor.Core.Constraints;

/// <summary>
/// Zadana długość odcinka: ||B - A|| = TargetLength.
/// </summary>
public sealed class FixedLengthConstraint : IConstraint
{
    private const float DegenerateLength = 1e-4f;

    private readonly Edge _edge;

    public FixedLengthConstraint(Edge edge, float targetLength)
    {
        if (edge.EdgeType != EdgeType.Line)
            throw new ArgumentException("A fixed length can only be set on a straight edge.", nameof(edge));
        if (targetLength <= 0f)
            throw new ArgumentOutOfRangeException(nameof(targetLength), "The length must be positive.");

        _edge = edge;
        TargetLength = targetLength;
        Edges = new[] { edge };
        Vertices = new[] { edge.StartVertex, edge.EndVertex };
    }

    public float TargetLength { get; }

    public IReadOnlyList<Edge> Edges { get; }
    public IReadOnlyList<Vertex> Vertices { get; }

    public string IconLabel => TargetLength.ToString("0.#", CultureInfo.InvariantCulture);

    public bool Involves(Edge edge) => edge == _edge;

    public bool IsSatisfied(float epsilon) => MathF.Abs(_edge.Length - TargetLength) <= epsilon;

    /// <summary>
    /// Zachowuje kierunek odcinka i zmienia jego długość: przesuwa wolny koniec,
    /// a gdy oba końce są wolne - oba symetrycznie względem środka.
    /// </summary>
    public bool Enforce(IReadOnlySet<Vertex> lockedVertices)
    {
        var start = _edge.StartVertex;
        var end = _edge.EndVertex;
        bool startLocked = lockedVertices.Contains(start);
        bool endLocked = lockedVertices.Contains(end);
        if (startLocked && endLocked)
            return false;

        var a = new Vector2(start.X, start.Y);
        var b = new Vector2(end.X, end.Y);
        var delta = b - a;
        float length = delta.Length();
        var direction = length > DegenerateLength ? delta / length : Vector2.UnitX;
        var segment = direction * TargetLength;

        if (startLocked)
        {
            MoveTo(end, a + segment);
        }
        else if (endLocked)
        {
            MoveTo(start, b - segment);
        }
        else
        {
            var middle = (a + b) / 2f;
            MoveTo(start, middle - segment / 2f);
            MoveTo(end, middle + segment / 2f);
        }
        return true;
    }

    public override string ToString() => $"Długość {IconLabel}";

	/// <summary>
	/// Przesuwanie wierzchołka może zepsuć punkty kontrolne, dlatego one też są przesuwane
	/// </summary>
    private static void MoveTo(Vertex vertex, Vector2 position) => vertex.MoveWithControlPoints(position.X, position.Y);
}
