namespace PolygonEditor.Core.Geometry;

public static class GeometryMath
{
    /// <summary>
    /// Oblicza najkrótszą odległość euklidesową punktu (x, y) do odcinka o końcach a, b.
    /// </summary>
    public static float DistanceToSegment(float x, float y, float ax, float ay, float bx, float by)
    {
        float dx = bx - ax;
        float dy = by - ay;
        float lengthSquared = dx * dx + dy * dy;

        // iloczyn skalarny
        float t = lengthSquared > 0f ? ((x - ax) * dx + (y - ay) * dy) / lengthSquared : 0f;
        t = Math.Clamp(t, 0f, 1f);

        // wektor odległości
        float cx = ax + t * dx - x;
        float cy = ay + t * dy - y;
        // długość wektora odległości
        return MathF.Sqrt(cx * cx + cy * cy);
    }
}
