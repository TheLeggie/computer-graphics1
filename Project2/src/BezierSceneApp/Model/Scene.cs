namespace BezierSceneApp
{
    // stan sceny: powierzchnie, ich siatki i narastajaca orientacja R
    public class Scene
    {
        public List<BezierSurface> Surfaces { get; }
        public List<Mesh> Meshes { get; } = new();
        public Matrix3x3 Orientation { get; private set; } = Matrix3x3.Identity;

        public Scene(List<BezierSurface> surfaces, int divisions)
        {
            Surfaces = surfaces;
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
