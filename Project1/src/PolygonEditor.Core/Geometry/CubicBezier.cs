using System.Numerics;

namespace PolygonEditor.Core.Geometry;

/// <summary>
/// Krzywa Beziera 3 stopnia zamieniona z postaci Bernsteina do bazy potęgowej
/// P(t) = A3 t^3 + A2 t^2 + A1 t + A0.
/// </summary>
public readonly struct CubicBezier
{
    public const int MinSegmentCount = 20;
    public const int MaxSegmentCount = 2000;

    public Vector2 A0 { get; }
    public Vector2 A1 { get; }
    public Vector2 A2 { get; }
    public Vector2 A3 { get; }

    public Vector2 End { get; }

    /// <summary>
    /// Ilość kroków N, wyliczona na podstawie rozmiaru wieloboku kontrolnego, w algorytmie d = 1/N
    /// </summary>
    public int SegmentCount { get; }

    public CubicBezier(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        // krok zerowy metody różnicowej
        A0 = v0;
        A1 = 3f * (v1 - v0);
        A2 = 3f * (v2 - 2f * v1 + v0);
        A3 = v3 - 3f * v2 + 3f * v1 - v0;
        End = v3;

        float controlPolygonLength = Vector2.Distance(v0, v1) + Vector2.Distance(v1, v2) + Vector2.Distance(v2, v3);
        SegmentCount = Math.Clamp((int)MathF.Ceiling(controlPolygonLength), MinSegmentCount, MaxSegmentCount);
    }

    /// <summary>
    /// Dla konkretnego miejsca t na krzywej zwraca współrzędne
    /// </summary>
    public Vector2 Evaluate(float t) => ((A3 * t + A2) * t + A1) * t + A0;

    /// <summary>
    /// Punkty P(0), P(d), ..., P(1) wyliczone metodą różnicową
    /// </summary>
    public Vector2[] ComputePoints()
    {
        int n = SegmentCount;
        float d = 1f / n;
        float d2 = d * d;
        float d3 = d2 * d;

        Vector2 p = A0;
        Vector2 deltaP = A3 * d3 + A2 * d2 + A1 * d;
        Vector2 delta2P = 6f * A3 * d3 + 2f * A2 * d2;
        Vector2 delta3P = 6f * A3 * d3;

        var points = new Vector2[n + 1];
        points[0] = p;
        
        // główna pętla algorytmu - dodawanie różnic
        for (int i = 1; i <= n; i++)
        {
            p += deltaP;
            deltaP += delta2P;
            delta2P += delta3P;
            points[i] = p;
        }

        // błędy zaokrągleń mogą przesunąć punkt - ostatni ustawiamy ręcznie
        points[n] = End;
        return points;
    }
}
