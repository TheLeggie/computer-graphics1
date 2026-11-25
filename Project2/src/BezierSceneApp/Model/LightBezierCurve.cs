using System.Numerics;

namespace BezierSceneApp
{
    /// <summary>
    /// Zamknięta krzywa Béziera stopnia 6 (7 punktów kontrolnych V0..V6, V6 = V0) na płaszczyźnie xy
    /// Punkty na przywej są wyznaczanie za pomocą algorytmu de Casteljau - interpolacja liniowa aż zostanie jeden punkt
    /// </summary>
    public class LightBezierCurve
    {
        public const int PointCount = 7;
        public const int EditablePointCount = PointCount - 1;

        private readonly Vector2[] points = new Vector2[PointCount];

        public LightBezierCurve()
        {
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

        // algorytm de Casteljau - wielokrotne dzielenie odcinków w proporcji t:(1-t)
        // Pętla dziala dopoki ze wszystkich punktów kontrolnych zostanie 1 - wynik, punkt na przywej Beziera
        public Vector2 Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            Span<Vector2> work = stackalloc Vector2[PointCount];
            points.AsSpan().CopyTo(work);

            for (int r = 1; r < PointCount; r++)
                for (int i = 0; i < PointCount - r; i++)
                    work[i] = (1f - t) * work[i] + t * work[i + 1];

            return work[0];
        }

        public Vector3 Evaluate3D(float t, float z)
        {
            Vector2 p = Evaluate(t);
            return new Vector3(p.X, p.Y, z);
        }
    }
}
