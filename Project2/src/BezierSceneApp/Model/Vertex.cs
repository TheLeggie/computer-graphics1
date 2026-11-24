using System.Numerics;

namespace BezierSceneApp;

/// <summary>
/// Wierzcholek siatki: wartosci przed obrotem (P, Pu, Pv, N) i po obrocie (Rot)
/// </summary>
public class Vertex
{
    public Vector3 P, Pu, Pv, N;
    public Vector3 RotP, RotPu, RotPv, RotN;
    public float U, V;

    public void Rotate(Matrix3x3 r)
    {
        RotP = r.Transform(P);
        RotPu = r.Transform(Pu);
        RotPv = r.Transform(Pv);
        RotN = Vector3.Normalize(r.Transform(N));
    }
}