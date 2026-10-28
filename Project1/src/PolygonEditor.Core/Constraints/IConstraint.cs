using PolygonEditor.Core.Model;

namespace PolygonEditor.Core.Constraints;

/// <summary>
/// Ograniczenie geometryczne nałożone na jedną lub kilka krawędzi.
/// Nowy rodzaj ograniczenia to nowa klasa implementująca ten interfejs - solver nie zna konkretnych typów.
/// </summary>
public interface IConstraint
{
    IReadOnlyList<Edge> Edges { get; }

    /// <summary>
    /// Wierzchołki, których położenie może zmienić "Enforce".
    /// </summary>
    IReadOnlyList<Vertex> Vertices { get; }

    /// <summary>
    /// Krótka etykieta rysowana przy środku krawędzi
    /// </summary>
    string IconLabel { get; }

    bool Involves(Edge edge);

    bool IsSatisfied(float epsilon);

    /// <summary>
    /// Przesuwa niezablokowane wierzchołki tak, aby ograniczenie było spełnione.
    /// Zwraca false, gdy nie da się tego zrobić bez ruszania zablokowanych wierzchołków.
    /// </summary>
    bool Enforce(IReadOnlySet<Vertex> lockedVertices);
}
