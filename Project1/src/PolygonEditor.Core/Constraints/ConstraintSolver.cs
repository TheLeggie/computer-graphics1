using PolygonEditor.Core.Model;

namespace PolygonEditor.Core.Constraints;

/// <summary>
/// Przywraca ograniczenia po przesunięciu wierzchołków.
/// </summary>
public static class ConstraintSolver
{
    public const int MaxIterations = 30;
    public const float Epsilon = 0.01f;

    /// <summary>
    /// Algorytm na podstawie Position Based Dynamics (PBD)
    /// Opiera się na dwóch pętlach
    /// 1. BFS od zablokowanych wierzchołków: każde niespełnione ograniczenie jest wymuszane względem
    ///    wierzchołków już ustalonych, a przesunięte wierzchołki stają się ustalone dla kolejnych ograniczeń
    /// 2. Relaksacja: ograniczenia, których nie dało się spełnić w kroku 1, są poprawiane iteracyjnie,
    ///    ale blokowane są już tylko wierzchołki wejściowe.
    /// Zwraca wierzchołki przesunięte przez solver.
    /// </summary>
    public static IReadOnlySet<Vertex> Solve(IEnumerable<Vertex> lockedVertices)
    {
        // zablokowane - trzymany przez użytkownika lub oba końce krzywej Beziera gdy uzytkownik przesuwa punkt kontrolny
        var locked = new HashSet<Vertex>(lockedVertices);
        // w pierwszej pętli (BFS) - te które mają spełnione swoje ograniczenia po przejściu pętli
        var settled = new HashSet<Vertex>(locked);
        // te które zostały ruszone (dla ponownego sprawdzenia ciągłości)
        var moved = new HashSet<Vertex>();
        // odwiedzone w BFS
        var visited = new HashSet<IConstraint>();
        // niespełnione ograniczenia
        var active = new List<IConstraint>();
        // kolejka BFS
        var queue = new Queue<Vertex>(locked);

        while (queue.Count > 0)
        {
            foreach (var constraint in ConstraintsAt(queue.Dequeue()))
            {
                if (!visited.Add(constraint))
                    continue;
                active.Add(constraint);

                if (constraint.IsSatisfied(Epsilon) || !constraint.Enforce(settled))
                    continue;

                foreach (var vertex in constraint.Vertices)
                {
                    if (settled.Add(vertex))
                    {
                        moved.Add(vertex);
                        queue.Enqueue(vertex);
                    }
                }
            }
        }

        for (int iteration = 0; iteration < MaxIterations; iteration++)
        {
            bool changed = false;
            for (int i = 0; i < active.Count; i++)
            {
                var constraint = active[i];
                if (constraint.IsSatisfied(Epsilon) || !constraint.Enforce(locked))
                    continue;

                changed = true;
                foreach (var vertex in constraint.Vertices.Where(v => !locked.Contains(v)))
                {
                    moved.Add(vertex);
                    foreach (var neighbour in ConstraintsAt(vertex))
                    {
                        if (visited.Add(neighbour))
                            active.Add(neighbour);
                    }
                }
            }
            if (!changed)
                break;
        }

        return moved;
    }

    private static IEnumerable<IConstraint> ConstraintsAt(Vertex vertex) =>
        vertex.AdjacentEdges.SelectMany(edge => edge.Constraints);
}
