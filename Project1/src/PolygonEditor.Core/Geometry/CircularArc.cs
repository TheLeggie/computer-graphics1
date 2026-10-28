using System.Numerics;

namespace PolygonEditor.Core.Geometry;

/// <summary>
/// Łuk okręgu o środku Center i promieniu Radius, zaczynający się pod kątem StartAngle i zakreślający kąt SweepAngle (radiany).
/// Kąty liczone jak w Atan2 we współrzędnych ekranu (oś Y w dół), czyli tak samo jak w Graphics.DrawArc -
/// rosnący kąt to ruch zgodny z ruchem wskazówek zegara na ekranie.
/// Łuk zdegenerowany (punkt końcowy leży na prostej stycznej - nieskończony promień) to odcinek Start-End.
/// </summary>
public readonly struct CircularArc
{
    public const int MinSegmentCount = 16;
    public const int MaxSegmentCount = 2000;

    private const float Epsilon = 1e-4f;
    private const float MaxRadius = 1e5f;

    public Vector2 Start { get; }
    public Vector2 End { get; }
    public Vector2 Center { get; }
    public float Radius { get; }
    public float StartAngle { get; }
    public float SweepAngle { get; }
    public bool IsDegenerate { get; }

    private CircularArc(Vector2 start, Vector2 end, Vector2 center, float radius, float startAngle, float sweepAngle,
        bool isDegenerate)
    {
        Start = start;
        End = end;
        Center = center;
        Radius = radius;
        StartAngle = startAngle;
        SweepAngle = sweepAngle;
        IsDegenerate = isDegenerate;
    }

    private static CircularArc Line(Vector2 a, Vector2 b) => new(a, b, (a + b) / 2f, 0f, 0f, 0f, isDegenerate: true);

    /// <summary>
    /// Oba końce G0: półokrąg o środku S = (A + B) / 2 i promieniu R = ||B - A|| / 2
    /// </summary>
    public static CircularArc Semicircle(Vector2 a, Vector2 b)
    {
        var center = (a + b) / 2f;
        float radius = Vector2.Distance(a, b) / 2f;
        if (radius < Epsilon)
            return Line(a, b);

        float startAngle = MathF.Atan2(a.Y - center.Y, a.X - center.X);
        return new CircularArc(a, b, center, radius, startAngle, MathF.PI, isDegenerate: false);
    }

    /// <summary>
    /// Łuk od A do B, którego wektor styczny w A ma kierunek "tangent" (ciągłość G1 w A).
    /// Środek S = A + u N leży na prostej prostopadłej do stycznej w A i na symetralnej cięciwy AB:
    /// u = ((M - A) · (B - A)) / (N · (B - A)) = ||B - A||^2 / (2 N · (B - A)).
    /// </summary>
    public static CircularArc Tangent(Vector2 a, Vector2 b, Vector2 tangent)
    {
        float tangentLength = tangent.Length();
        if (tangentLength < Epsilon)
            return Semicircle(a, b);

        var t = tangent / tangentLength;
        var normal = new Vector2(-t.Y, t.X);
        var chord = b - a;
        float chordLengthSquared = chord.LengthSquared();
        float denominator = Vector2.Dot(normal, chord);
        // B leży (prawie) na prostej stycznej - promień dąży do nieskończoności
        if (chordLengthSquared < Epsilon || MathF.Abs(denominator) * 2f * MaxRadius <= chordLengthSquared)
            return Line(a, b);

        float u = chordLengthSquared / (2f * denominator);
        var center = a + u * normal;
        float startAngle = MathF.Atan2(a.Y - center.Y, a.X - center.X);
        float endAngle = MathF.Atan2(b.Y - center.Y, b.X - center.X);

        // Przy u > 0 ruch w kierunku +T odpowiada rosnącemu kątowi, przy u < 0 malejącemu
        float sweep = endAngle - startAngle;
        if (u > 0f)
        {
            while (sweep <= 0f)
                sweep += 2f * MathF.PI;
        }
        else
        {
            while (sweep >= 0f)
                sweep -= 2f * MathF.PI;
        }

        return new CircularArc(a, b, center, MathF.Abs(u), startAngle, sweep, isDegenerate: false);
    }

    /// <summary>
    /// Ten sam łuk przechodzony w przeciwną stronę (od End do Start)
    /// </summary>
    public CircularArc Reverse() =>
        new(End, Start, Center, Radius, StartAngle + SweepAngle, -SweepAngle, IsDegenerate);

    public Vector2 Evaluate(float t)
    {
        if (IsDegenerate)
            return Vector2.Lerp(Start, End, t);

        float angle = StartAngle + t * SweepAngle;
        return Center + Radius * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    /// <summary>
    /// Jednostkowy wektor styczny w punkcie początkowym, zgodny z kierunkiem przechodzenia łuku
    /// </summary>
    public Vector2 TangentAtStart => DirectionAt(StartAngle);

    /// <summary>
    /// Jednostkowy wektor styczny w punkcie końcowym, zgodny z kierunkiem przechodzenia łuku
    /// </summary>
    public Vector2 TangentAtEnd => DirectionAt(StartAngle + SweepAngle);

    private Vector2 DirectionAt(float angle)
    {
        if (IsDegenerate)
        {
            var chord = End - Start;
            float length = chord.Length();
            return length < Epsilon ? Vector2.Zero : chord / length;
        }

        float sign = MathF.Sign(SweepAngle);
        return sign * new Vector2(-MathF.Sin(angle), MathF.Cos(angle));
    }

    /// <summary>
    /// Punkty łuku od Start do End. Kolejne punkty powstają przez obrót wektora promienia o stały kąt,
    /// więc w pętli nie ma wywołań funkcji trygonometrycznych.
    /// </summary>
    public Vector2[] ComputePoints()
    {
        if (IsDegenerate)
            return new[] { Start, End };

        int n = Math.Clamp((int)MathF.Ceiling(Radius * MathF.Abs(SweepAngle)), MinSegmentCount, MaxSegmentCount);
        float step = SweepAngle / n;
        float cos = MathF.Cos(step);
        float sin = MathF.Sin(step);

        var radius = Start - Center;
        var points = new Vector2[n + 1];
        points[0] = Start;
        for (int i = 1; i <= n; i++)
        {
            radius = new Vector2(radius.X * cos - radius.Y * sin, radius.X * sin + radius.Y * cos);
            points[i] = Center + radius;
        }

        points[n] = End;
        return points;
    }
}
