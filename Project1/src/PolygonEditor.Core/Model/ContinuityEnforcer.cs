using System.Numerics;

namespace PolygonEditor.Core.Model;

/// <summary>
/// Utrzymuje klasy ciągłości (G0, G1, C1) w wierzchołkach sąsiadujących z krzywymi Béziera.
/// Wektor styczny krawędzi w wierzchołku W: 3(V - W) dla punktu kontrolnego V krzywej Béziera, (B - W) dla odcinka WB.
/// G1 wymaga przeciwnych kierunków tych wektorów, C1 dodatkowo równych długości.
/// 
/// Wierzchołki są przesuwane tylko wtedy, gdy użytkownik ciągnie punkt kontrolny, a sąsiadem jest odcinek.
/// Przesuwaniem wierzchołków razem z ograniczeniami koordynuje "Scene.cs">.
///
/// Łuki obsługują tylko G0 i G1, a G1 może mieć tylko jeden koniec łuku. Łuk nie ma punktów kontrolnych -
/// jego kształt jest wyliczany na bieżąco z sąsiedniej krawędzi (Edge.ToArc), więc tutaj niczego się dla niego nie poprawia.
/// </summary>
public static class ContinuityEnforcer
{
    private const float Epsilon = 1e-4f;

    /// <summary>
    /// Przy łuku C1 zamienia się na G1, a G1 na drugim końcu łuku jest wyłączane (zmienia się na G0).
    /// </summary>
    public static void SetContinuity(Vertex vertex, ContinuityType type)
    {
        vertex.ContinuityType = type;
        vertex.IsContinuityExplicitlySet = true;
        foreach (var arc in vertex.AdjacentEdges.Where(e => e.IsArc).ToList())
            NormalizeArcEnds(arc, preferredVertex: vertex);
        EnforceAt(vertex);
    }

    /// <summary>
    /// Na końcach nowej krzywej ustawia domyślną ciągłość C1 (o ile użytkownik nie ustawił jej jawnie)
    /// i dopasowuje punkty kontrolne tej krzywej do sąsiednich krawędzi.
    /// Gdy po drugiej stronie wierzchołka jest łuk, C1 staje się G1 (albo G0, jeśli drugi koniec łuku ma już G1).
    /// </summary>
    public static void ApplyDefaultContinuity(Edge bezierEdge)
    {
        foreach (var vertex in new[] { bezierEdge.StartVertex, bezierEdge.EndVertex })
        {
            if (!vertex.IsContinuityExplicitlySet)
                vertex.ContinuityType = ContinuityType.C1;

            var otherEdge = OtherEdge(vertex, bezierEdge);
            if (otherEdge is { IsArc: true })
                NormalizeArcEnds(otherEdge, preferredVertex: OppositeVertex(otherEdge, vertex));
            EnforceAt(vertex, bezierEdge);
        }
    }

    /// <summary>
    /// Pilnuje zasad łuku po zamianie krawędzi na łuk: C1 na końcach zamienia na G1, a jeśli oba końce mają G1,
    /// to zostaje ono tylko na początku łuku.
    /// </summary>
    public static void ApplyArcContinuity(Edge arc) => NormalizeArcEnds(arc, preferredVertex: arc.StartVertex);

    private static void NormalizeArcEnds(Edge arc, Vertex preferredVertex)
    {
        foreach (var vertex in new[] { arc.StartVertex, arc.EndVertex })
        {
            if (vertex.ContinuityType == ContinuityType.C1)
                vertex.ContinuityType = ContinuityType.G1;
        }

        var other = OppositeVertex(arc, preferredVertex);
        if (preferredVertex.ContinuityType == ContinuityType.G1 && other.ContinuityType == ContinuityType.G1)
            other.ContinuityType = ContinuityType.G0;
    }

    /// <summary>
    /// Przesuwa punkt kontrolny i dopasowuje krawędź po drugiej stronie wierzchołka:
    /// punkt kontrolny sąsiedniej krzywej albo drugi koniec sąsiedniego odcinka (obrót przy G1, obrót i skalowanie przy C1).
    /// Zwraca przesunięty koniec odcinka - wołający musi potem przywrócić ograniczenia i wywołać "RestoreAround">.
    /// </summary>
    public static Vertex? MoveControlPoint(ControlPoint controlPoint, float x, float y)
    {
        controlPoint.MoveTo(x, y);

        var vertex = controlPoint.AnchorVertex;
        var otherEdge = OtherEdge(vertex, controlPoint.ParentEdge);
        // łuk sam dopasowuje się do nowej stycznej
        if (otherEdge is null || otherEdge.IsArc || vertex.ContinuityType == ContinuityType.G0)
            return null;
        return Align(vertex, controlPoint.ParentEdge, otherEdge);
    }

    /// <summary>
    /// Poprawia punkty kontrolne po przesunięciu wierzchołków (każdy przesunięty razem ze swoimi punktami kontrolnymi).
    /// Wierzchołek "frozenVertex" jest pomijany, bo jego punkt kontrolny trzyma użytkownik - w przeciwnym przypadku
    /// pozycja punktu kontrolnego który trzyma użytkownik by się zmieniła.
    /// </summary>
    public static void RestoreAround(IEnumerable<Vertex> movedVertices, Vertex? frozenVertex = null)
    {
        var moved = movedVertices.ToList();
        foreach (var vertex in moved)
        {
            if (vertex != frozenVertex)
                EnforceAt(vertex);
        }

        // przemieszczony wierzchołek zmienia kąt nachylenia odcinków z którymi graniczy, dlatego poprawiamy ciągłość 
        // na drugim końcu danego odcinka
        foreach (var vertex in moved)
        {
            foreach (var edge in vertex.AdjacentEdges)
            {
                var opposite = OppositeVertex(edge, vertex);
                if (!edge.IsBezier && opposite != frozenVertex)
                    EnforceAt(opposite);
            }
        }
    }

    /// <summary>
    /// Przywraca ciągłość w wierzchołku, poprawiając wyłącznie punkt kontrolny (nigdy wierzchołek).
    /// Przy dwóch krzywych poprawiana jest prefferedTarget, a domyślnie krzywa wychodząca.
    /// Używana gdy:
    /// 1. Zmiana ciągłości w danym wierchołku
    /// 2. Zmiana rodzaju krawędzi
    /// 3. Usunięcie lub dodanie wierzchołka
    /// 4. Poruszanie wierzchołkiem (wtedy używana też dla sąsiadujących wierzchołków)
    /// 5. Zmiana pozycji wierzchołka przez wymagania ograniczeń
    /// </summary>
    public static void EnforceAt(Vertex vertex, Edge? preferredTarget = null)
    {
        if (vertex.ContinuityType == ContinuityType.G0)
            return;

        var incoming = vertex.IncomingEdge;
        var outgoing = vertex.OutgoingEdge;
        // przy łuku styczność zapewnia sam łuk, wyliczany z kierunku sąsiedniej krawędzi
        if (incoming is null || outgoing is null || incoming.IsArc || outgoing.IsArc)
            return;
        
        if (incoming.IsBezier && outgoing.IsBezier)
        {
            var target = preferredTarget == incoming ? incoming : outgoing;
            Align(vertex, OtherEdge(vertex, target)!, target);
        }
        else if (incoming.IsBezier)
        {
            Align(vertex, outgoing, incoming);
        }
        else if (outgoing.IsBezier)
        {
            Align(vertex, incoming, outgoing);
        }
    }

    /// <summary>
    /// Ustawia styczną krawędzi "target" w wierzchołku przeciwnie do stycznej krawędzi "source" i przeprowadza skalowanie
    /// w zaleznosci od kombinacji typów krawędzi source i target.
    /// Czym jest "source" i "target":
    /// 1. Jeśli Bezier i odcinek to "source" to odcinek
    /// 2. Jeśli dwie krzywe Beziera to decyduje o tym metoda EnforceAt, domyślnie ustawia jedną krawędź jako "source"
    /// 3. Jeśli ruszamy punktem kontrolnym to "source" to krzywa Beziera, a "target" to odcinek
    /// Zwraca wierzchołek, jeśli trzeba było przesunąć drugi koniec odcinka "target">.
    /// </summary>
    private static Vertex? Align(Vertex vertex, Edge source, Edge target)
    {
        var w = new Vector2(vertex.X, vertex.Y);
        // wektor zrodlowy - według niego dostosowujemy wektor odcinka po drugiej stronie wierzchołka
        var sourceVector = AwayPoint(source, vertex) - w;
        float sourceLength = sourceVector.Length();
        if (sourceLength < Epsilon)
            return null;
        // normalizacja i odwrocenie kierunku
        var direction = -sourceVector / sourceLength;
        // współczynnik skalowania w zależności od typów krawędzi ( dla kombinacji Bezier odcinek skalowanie * 3)
        float c1Length = sourceLength * DerivativeScale(source) / DerivativeScale(target);
        float targetLength = vertex.ContinuityType == ContinuityType.C1
            ? c1Length
            : (AwayPoint(target, vertex) - w).Length();
        if (targetLength < Epsilon)
            targetLength = c1Length;
        // zaktualizowane położenie zmienianego wierzchołka lub punktu kontrolnego
        var p = w + direction * targetLength;
        // sprawdza czy target jest krzywą Beziera i ma punkty kontrolne
        var handle = HandleAt(target, vertex);
        if (handle is not null)
        {
            handle.MoveTo(p.X, p.Y);
            return null;
        }
        // sytuacja gdy ruszamy punktem kontrolnym i target to odcinek
        var opposite = OppositeVertex(target, vertex);
        opposite.MoveWithControlPoints(p.X, p.Y);
        return opposite;
    }

    private static float DerivativeScale(Edge edge) => edge.IsBezier ? 3f : 1f;

    /// <summary>
    /// Drugi koniec odcinka, którego wektor styczny jest wyznaczany
    /// </summary>
    private static Vector2 AwayPoint(Edge edge, Vertex vertex)
    {
        var handle = HandleAt(edge, vertex);
        if (handle is not null)
            return new Vector2(handle.X, handle.Y);

        var opposite = OppositeVertex(edge, vertex);
        return new Vector2(opposite.X, opposite.Y);
    }

    private static ControlPoint? HandleAt(Edge edge, Vertex vertex) =>
        edge.StartVertex == vertex ? edge.ControlPoint1 : edge.ControlPoint2;

    private static Vertex OppositeVertex(Edge edge, Vertex vertex) =>
        edge.StartVertex == vertex ? edge.EndVertex : edge.StartVertex;

    private static Edge? OtherEdge(Vertex vertex, Edge edge) =>
        vertex.IncomingEdge == edge ? vertex.OutgoingEdge : vertex.IncomingEdge;
}
