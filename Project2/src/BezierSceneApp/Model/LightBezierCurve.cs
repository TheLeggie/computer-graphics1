using System.Numerics;

namespace BezierSceneApp
{
    /// <summary>
    /// Zamknięta krzywa Béziera stopnia 6 (7 punktów kontrolnych V0..V6, V6 = V0) na płaszczyźnie xy
    /// </summary>
    public class LightBezierCurve
    {
        public const int PointCount = 7;
        // liczba niezależnych uchwytów do edycji (V6 jest zawsze sprzężone z V0)
        public const int EditablePointCount = PointCount - 1;

        private readonly Vector2[] points = new Vector2[PointCount];

        public LightBezierCurve()
        {
            // domyślna gładka pętla wokół obu powierzchni sceny, V6 = V0
            points[0] = new Vector2(0.00f, 0.60f);
            points[1] = new Vector2(-0.85f, 0.60f);
            points[2] = new Vector2(-0.95f, -0.55f);
            points[3] = new Vector2(0.00f, -0.85f);
            points[4] = new Vector2(0.95f, -0.55f);
            points[5] = new Vector2(0.85f, 0.60f);
            points[6] = points[0];
        }

        public Vector2 this[int index] => points[index];

        public void SetPoint(int index, Vector2 value)
        {
            if ((uint)index >= PointCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            // zachowanie domknięcia krzywej: V6 = V0
            if (index == 0 || index == PointCount - 1)
            {
                points[0] = value;
                points[PointCount - 1] = value;
            }
            else
            {
                points[index] = value;
            }
        }

        // algorytm de Casteljau na stosie (0 alokacji sterty)
        public Vector2 Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            Span<Vector2> work = stackalloc Vector2[PointCount];
            points.AsSpan().CopyTo(work);

            float oneMinusT = 1f - t;
            for (int r = 1; r < PointCount; r++)
                for (int i = 0; i < PointCount - r; i++)
                    work[i] = oneMinusT * work[i] + t * work[i + 1];

            return work[0];
        }

        public Vector3 Evaluate3D(float t, float z)
        {
            Vector2 p = Evaluate(t);
            return new Vector3(p.X, p.Y, z);
        }
    }
}
