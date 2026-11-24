using System.Numerics;

namespace BezierSceneApp;

/// <summary>
/// Parametry oświetlenia
/// </summary>
public class LightSettings
{
    private const float DegenerateEpsilon = 1e-12f;

    public float Kd = 0.7f;
    public float Ks = 0.5f;
    public float M = 20f;
    // pozycja swiatla w jednostkach sceny (ta sama przestrzen co RotP)
    public Vector3 Position = new(0f, 0f, 2f);
    public Vector3 LightColor = Vector3.One;

    // p: punkt na powierzchni, n: znormalizowany wektor normalny, objectColor: I_O; wynik w ARGB
    public int Shade(Vector3 p, Vector3 n, Vector3 objectColor)
    {
        // normalna zawsze zwrocona w strone obserwatora V = (0, 0, 1)
        if (n.Z < 0f)
            n = -n;

        Vector3 toLight = Position - p;
        float lenSq = toLight.LengthSquared();
        Vector3 l = lenSq < DegenerateEpsilon ? Vector3.UnitZ : toLight / MathF.Sqrt(lenSq);

        float cosNL = Vector3.Dot(n, l);
        if (cosNL <= 0f) // swiatlo za widoczna strona powierzchni
            return unchecked((int)0xFF000000);

        // R = 2<N,L>N - L (wersor przy znormalizowanych N i L); <V,R> = R.z
        float cosVR = 2f * cosNL * n.Z - l.Z;
        float spec = cosVR > 0f ? MathF.Pow(cosVR, M) : 0f;

        Vector3 color = LightColor * objectColor * (Kd * cosNL + Ks * spec);
        int r = Math.Clamp((int)(color.X * 255f + 0.5f), 0, 255);
        int g = Math.Clamp((int)(color.Y * 255f + 0.5f), 0, 255);
        int b = Math.Clamp((int)(color.Z * 255f + 0.5f), 0, 255);
        return (255 << 24) | (r << 16) | (g << 8) | b;
    }

    public static Vector3 ToVector(Color c) => new(c.R / 255f, c.G / 255f, c.B / 255f);
}
