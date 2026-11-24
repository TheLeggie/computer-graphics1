using System.Numerics;

namespace BezierSceneApp;
    
/// <summary>
/// Klasa siatki punktów kontrolnych
/// </summary>
public class Mesh
{
    public List<Vertex> Vertices { get; } = new();
    public List<Triangle> Triangles { get; } = new();

    public void Rotate(Matrix3x3 r)
    {
        foreach (Vertex v in Vertices)
            v.Rotate(r);
    }
}

