namespace PolygonEditor.Core.Model;

public enum ContinuityType
{
    G0,
    G1,
    C1
}

public class Vertex
{
    public float X { get; set; }
    public float Y { get; set; }

    public Edge? IncomingEdge { get; internal set; }
    public Edge? OutgoingEdge { get; internal set; }

    public ContinuityType ContinuityType { get; set; } = ContinuityType.G0;
    public bool IsContinuityExplicitlySet { get; set; }

    public bool HasBothVisibleEdges =>
        IncomingEdge is { IsHidden: false } &&
        OutgoingEdge is { IsHidden: false };

    public bool HasAdjacentBezier =>
        HasBothVisibleEdges && (IncomingEdge!.IsBezier || OutgoingEdge!.IsBezier);

    public bool HasAdjacentArc =>
        HasBothVisibleEdges && (IncomingEdge!.IsArc || OutgoingEdge!.IsArc);

    // Tylko przy krzywych ciągłość ma znaczenie, więc tylko wtedy jest pokazywana i edytowalna
    public bool HasAdjacentCurve => HasAdjacentBezier || HasAdjacentArc;

    public IEnumerable<Edge> AdjacentEdges
    {
        get
        {
            if (IncomingEdge is not null)
                yield return IncomingEdge;
            if (OutgoingEdge is not null)
                yield return OutgoingEdge;
        }
    }

    public Vertex(float x, float y)
    {
        X = x;
        Y = y;
    }

    public void MoveTo(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Przesuwa wierzchołek razem z punktami kontrolnymi sąsiednich krzywych, które są przy nim zaczepione
    /// </summary>
    public void MoveWithControlPoints(float x, float y)
    {
        float dx = x - X;
        float dy = y - Y;
        MoveTo(x, y);
        IncomingEdge?.ControlPoint2?.MoveBy(dx, dy);
        OutgoingEdge?.ControlPoint1?.MoveBy(dx, dy);
    }

    public void MoveBy(float dx, float dy)
    {
        X += dx;
        Y += dy;
    }

    public float DistanceTo(float x, float y)
    {
        float dx = X - x;
        float dy = Y - y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public override string ToString() => $"({X:0.##}, {Y:0.##})";
}
