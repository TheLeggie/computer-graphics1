using System.Numerics;

namespace BezierSceneApp;

// zrodlo koloru obiektu I_O dla jednej powierzchni
public class SurfaceAppearance
{
    public bool UseTexture;
    public Vector3 SolidColor;
    public TextureData? Texture;

    public SurfaceAppearance(Vector3 solidColor)
    {
        SolidColor = solidColor;
    }
}
