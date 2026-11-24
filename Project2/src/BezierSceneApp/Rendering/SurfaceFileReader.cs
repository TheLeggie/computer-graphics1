using System.Globalization;
using System.Numerics;

namespace BezierSceneApp
{
    // wczytuje powierzchnie beziera: kolejne grupy po 16 wierszy "x y z"
    public static class SurfaceFileReader
    {
        private const int PointsPerSurface = BezierSurface.Size * BezierSurface.Size;
        private const int MinSurfaces = 2;

        public static List<BezierSurface> Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Nie znaleziono pliku z powierzchniami:\n{path}");

            var points = new List<Vector3>();
            string[] lines = File.ReadAllLines(path);
            for (int lineNo = 0; lineNo < lines.Length; lineNo++)
            {
                string line = lines[lineNo].Trim();
                if (line.Length == 0)
                    continue; // puste linie pomijamy

                string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3
                    || !TryParse(parts[0], out float x)
                    || !TryParse(parts[1], out float y)
                    || !TryParse(parts[2], out float z))
                {
                    throw new FormatException($"Wiersz {lineNo + 1}: oczekiwano trzech liczb \"x y z\", otrzymano \"{line}\".");
                }
                points.Add(new Vector3(x, y, z));
            }

            if (points.Count % PointsPerSurface != 0)
                throw new FormatException($"Liczba punktów ({points.Count}) nie jest wielokrotnością {PointsPerSurface}.");

            int count = points.Count / PointsPerSurface;
            if (count < MinSurfaces)
                throw new FormatException($"Wczytano {count} powierzchni, wymagane są co najmniej {MinSurfaces}.");

            var surfaces = new List<BezierSurface>(count);
            for (int s = 0; s < count; s++)
            {
                var surface = new BezierSurface();
                // k-ty punkt grupy trafia do wiersza k / 4, kolumny k % 4
                for (int k = 0; k < PointsPerSurface; k++)
                    surface.ControlPoints[k / BezierSurface.Size, k % BezierSurface.Size] = points[s * PointsPerSurface + k];
                surfaces.Add(surface);
            }
            return surfaces;
        }

        private static bool TryParse(string s, out float value) =>
            float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
