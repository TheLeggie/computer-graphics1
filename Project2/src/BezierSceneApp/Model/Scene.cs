namespace BezierSceneApp
{
    // stan sceny: powierzchnie, ich siatki, wyglad i narastajaca orientacja R
    public class Scene
    {
        // poczatkowe jednolite kolory I_O, cyklicznie dla kolejnych powierzchni
        private static readonly Color[] DefaultColors = { Color.Gold, Color.SteelBlue, Color.IndianRed, Color.MediumSeaGreen };

        public List<BezierSurface> Surfaces { get; }
        public List<Mesh> Meshes { get; } = new();
        // Appearances[i] dotyczy Surfaces[i] i Meshes[i]
        public List<SurfaceAppearance> Appearances { get; } = new();
        public Matrix3x3 Orientation { get; private set; } = Matrix3x3.Identity;

        public bool UseNormalMap { get; set; }
        public TextureData? NormalMap { get; set; }

        public Scene(List<BezierSurface> surfaces, int divisions)
        {
            Surfaces = surfaces;
            for (int i = 0; i < surfaces.Count; i++)
                Appearances.Add(new SurfaceAppearance(LightSettings.ToVector(DefaultColors[i % DefaultColors.Length])));
            SetDivisions(divisions);
        }

        // nowa siatka z zachowaniem aktualnego obrotu
        public void SetDivisions(int k)
        {
            Meshes.Clear();
            foreach (BezierSurface s in Surfaces)
                Meshes.Add(s.Triangulate(k));
            ApplyOrientation();
        }

        // R_nowa = R_przyrost * R_aktualna
        public void Rotate(Matrix3x3 increment)
        {
            Orientation = increment * Orientation;
            ApplyOrientation();
        }

        // obracane sa zawsze oryginalne wartosci (bez kumulacji bledow w punktach)
        private void ApplyOrientation()
        {
            foreach (BezierSurface s in Surfaces)
                s.Rotate(Orientation);
            foreach (Mesh m in Meshes)
                m.Rotate(Orientation);
        }
    }
}
