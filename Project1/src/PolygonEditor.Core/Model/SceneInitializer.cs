using PolygonEditor.Core.Constraints;

namespace PolygonEditor.Core.Model;

public static class SceneInitializer
{
    public static Scene CreateDemoScene()
    {
        var scene = new Scene();

        var pentagon = scene.AddPolygon(
            (120, 120),
            (360, 100),
            (420, 300),
            (250, 380),
            (100, 280));
        // Oba sąsiednie wierzchołki dostają domyślną ciągłość C1, która ustawia punkty kontrolne na przedłużeniu odcinków
        pentagon.Edges[1].ConvertToBezier();

        var square = scene.AddPolygon(
            (520, 160),
            (760, 160),
            (760, 400),
            (520, 400));
        scene.AddConstraint(new FixedLengthConstraint(square.Edges[0], 240));
        // Relacja między różnymi wielokątami, a górna krawędź kwadratu ma jednocześnie dwa ograniczenia
        scene.AddConstraint(new PerpendicularConstraint(square.Edges[0], pentagon.Edges[4]));
        // Dolna krawędź kwadratu jako G1, drugi koniec łuku zostaje G0
        scene.ConvertEdgeToArc(square.Edges[2]);
        ContinuityEnforcer.SetContinuity(square.Vertices[2], ContinuityType.G1);

        return scene;
    }
}
