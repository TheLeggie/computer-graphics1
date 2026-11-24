using System.Numerics;

namespace BezierSceneApp
{
    /// <summary>
    /// Klasa macierzy 3x3 z potrzebnymi operacjami macierzowymi
    /// Konwencaj wektorów kolumnowych, P' = M*V, M - macierz, V - wektor jako kolumna 3x1
    /// </summary>
    public readonly struct Matrix3x3
    {
        public readonly float M11, M12, M13;
        public readonly float M21, M22, M23;
        public readonly float M31, M32, M33;

        public Matrix3x3(float m11, float m12, float m13,
                         float m21, float m22, float m23,
                         float m31, float m32, float m33)
        {
            M11 = m11; M12 = m12; M13 = m13;
            M21 = m21; M22 = m22; M23 = m23;
            M31 = m31; M32 = m32; M33 = m33;
        }

        public static Matrix3x3 Identity => new(1, 0, 0, 0, 1, 0, 0, 0, 1);

        // buduje macierz bazy z trzech wektorow jako kolumn, M = [c0 | c1 | c2]
        public static Matrix3x3 FromColumns(Vector3 c0, Vector3 c1, Vector3 c2) => new(
            c0.X, c1.X, c2.X,
            c0.Y, c1.Y, c2.Y,
            c0.Z, c1.Z, c2.Z);

        // macierz obrotu o kat angle [rad] wokol osi X (Y -> Z)
        public static Matrix3x3 CreateRotationX(float angle)
        {
            float c = MathF.Cos(angle), s = MathF.Sin(angle);
            return new(1, 0, 0,
                       0, c, -s,
                       0, s, c);
        }

        // macierz obrotu o kat angle [rad] wokol osi Y (Z -> X)
        public static Matrix3x3 CreateRotationY(float angle)
        {
            float c = MathF.Cos(angle), s = MathF.Sin(angle);
            return new(c, 0, s,
                       0, 1, 0,
                       -s, 0, c);
        }

        // macierz obrotu o kat angle [rad] wokol osi Z (X -> Y)
        public static Matrix3x3 CreateRotationZ(float angle)
        {
            float c = MathF.Cos(angle), s = MathF.Sin(angle);
            return new(c, -s, 0,
                       s, c, 0,
                       0, 0, 1);
        }

        // skladanie macierzy A * B (w konwencji kolumnowej najpierw dziala B, potem A)
        public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b) => new(
            a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
            a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
            a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
            a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
            a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
            a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
            a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
            a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
            a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33);

        // przeksztalcenie wektora kolumnowego: p' = M * p (iloczyn skalarny wierszy M z p)
        public Vector3 Transform(Vector3 p) => new(
            M11 * p.X + M12 * p.Y + M13 * p.Z,
            M21 * p.X + M22 * p.Y + M23 * p.Z,
            M31 * p.X + M32 * p.Y + M33 * p.Z);
    }
}
