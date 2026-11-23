using System.Numerics;

namespace BezierSceneApp
{
    // powierzchnia beziera 3. stopnia: siatka 4x4 punktow kontrolnych
    public class BezierSurface
    {
        public const int Size = 4;

        // ControlPoints[i, j]: i - wiersz, j - kolumna
        public Vector3[,] ControlPoints { get; } = new Vector3[Size, Size];

        // punkty kontrolne po obrocie (do rysowania wieloboku)
        public Vector3[,] RotatedControlPoints { get; } = new Vector3[Size, Size];

        public void Rotate(Matrix3x3 r)
        {
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    RotatedControlPoints[i, j] = r.Transform(ControlPoints[i, j]);
        }

        // siatka k x k oczek, (k + 1) x (k + 1) wierzcholkow; u - indeks i, v - indeks j
        public Mesh Triangulate(int k)
        {
            var mesh = new Mesh();
            var grid = new Vertex[k + 1, k + 1];
            for (int r = 0; r <= k; r++)
            {
                for (int c = 0; c <= k; c++)
                {
                    Vertex v = Evaluate((float)r / k, (float)c / k);
                    grid[r, c] = v;
                    mesh.Vertices.Add(v);
                }
            }

            // przekatna (r, c)-(r + 1, c + 1)
            for (int r = 0; r < k; r++)
            {
                for (int c = 0; c < k; c++)
                {
                    mesh.Triangles.Add(new Triangle(grid[r, c], grid[r + 1, c], grid[r + 1, c + 1]));
                    mesh.Triangles.Add(new Triangle(grid[r, c], grid[r + 1, c + 1], grid[r, c + 1]));
                }
            }
            return mesh;
        }

        private Vertex Evaluate(float u, float v)
        {
            float[] bu3 = Bernstein3(u), bv3 = Bernstein3(v);
            float[] bu2 = Bernstein2(u), bv2 = Bernstein2(v);
            Vector3 p = Vector3.Zero, pu = Vector3.Zero, pv = Vector3.Zero;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    p += ControlPoints[i, j] * (bu3[i] * bv3[j]);
                    if (i < 3) pu += (ControlPoints[i + 1, j] - ControlPoints[i, j]) * (bu2[i] * bv3[j]);
                    if (j < 3) pv += (ControlPoints[i, j + 1] - ControlPoints[i, j]) * (bu3[i] * bv2[j]);
                }
            }
            pu *= 3f;
            pv *= 3f;

            // zdegenerowane naroze (zerowa styczna, np. sklejone punkty kontrolne) -> normalna wzdluz z
            Vector3 cross = Vector3.Cross(pu, pv);
            Vector3 n = cross.LengthSquared() > 1e-12f ? Vector3.Normalize(cross) : Vector3.UnitZ;

            return new Vertex { P = p, Pu = pu, Pv = pv, N = n, U = u, V = v };
        }

        private static float[] Bernstein3(float t)
        {
            float s = 1f - t;
            return new[] { s * s * s, 3f * t * s * s, 3f * t * t * s, t * t * t };
        }

        private static float[] Bernstein2(float t)
        {
            float s = 1f - t;
            return new[] { s * s, 2f * t * s, t * t };
        }
    }
}
