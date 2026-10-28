using System.Numerics;
using PolygonEditor.Core.Model;

namespace PolygonEditor.Core.Geometry;

public readonly record struct BezierControlPoints(Vector2 V0, Vector2 V1, Vector2 V2, Vector2 V3)
{
    public CubicBezier ToCubicBezier() => new(V0, V1, V2, V3);
}

public static class BSplineLogic
{
    /// <summary>
    /// Metoda dla zamkniętego wielokątu
    /// Bok dzielony na 3 części, odcinki pomocnicze w narożnikach dzielone na pół.
    /// </summary>
    public static List<BezierControlPoints> GetBezierSegments(Polygon polygon)
    {
        int n = polygon.Vertices.Count;
        var p = new Vector2[n];
        for (int i = 0; i < n; i++)
            p[i] = new Vector2(polygon.Vertices[i].X, polygon.Vertices[i].Y);

        var firstThird = new Vector2[n];
        var secondThird = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            var a = p[i];
            var b = p[(i + 1) % n];
            firstThird[i] = Vector2.Lerp(a, b, 1f / 3f);
            secondThird[i] = Vector2.Lerp(a, b, 2f / 3f);
        }

        var junction = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            int prevEdge = (i - 1 + n) % n;
            junction[i] = (secondThird[prevEdge] + firstThird[i]) * 0.5f;
        }

        var segments = new List<BezierControlPoints>(n);
        for (int i = 0; i < n; i++)
        {
            int nextVertex = (i + 1) % n;
            segments.Add(new BezierControlPoints(
                junction[i],
                firstThird[i],
                secondThird[i],
                junction[nextVertex]));
        }

        return segments;
    }
}