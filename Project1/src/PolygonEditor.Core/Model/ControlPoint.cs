namespace PolygonEditor.Core.Model;

public enum ControlPointAnchor
{
    Start,
    End
}

/// <summary>
/// Punkt kontrolny v1 lub v2 krzywej Beziera
/// </summary>
public class ControlPoint
{
    public float X { get; set; }
    public float Y { get; set; }
    // krawędź, która jest krzywą Beziera dla danego punktu kontrolnego
    public Edge ParentEdge { get; }
    // wierzchołek wielokąta przy punkcie kontrolnym
    public ControlPointAnchor Anchor { get; }

    public Vertex AnchorVertex => Anchor == ControlPointAnchor.Start ? ParentEdge.StartVertex : ParentEdge.EndVertex;

    public ControlPoint(Edge parentEdge, ControlPointAnchor anchor, float x, float y)
    {
        ParentEdge = parentEdge;
        Anchor = anchor;
        X = x;
        Y = y;
    }

    public void MoveTo(float x, float y)
    {
        X = x;
        Y = y;
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
