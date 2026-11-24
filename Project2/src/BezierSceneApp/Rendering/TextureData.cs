using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BezierSceneApp;

/// <summary>
/// Wczytanie tekstury lub mapy normalnej do pamięci i odczytywanie wartości dla współrzędnych (u,v)
/// </summary>
public class TextureData
{
    private readonly Vector3[] pixels;

    public int Width { get; }
    public int Height { get; }

    private TextureData(int width, int height, Vector3[] pixels)
    {
        Width = width;
        Height = height;
        this.pixels = pixels;
    }

    // kolor I_O: kanaly R, G, B -> [0, 1]
    public static TextureData LoadColor(string path) => Load(path, false);

    // wersor normalnej: kanaly R, G, B -> [-1, 1], znormalizowany przy ladowaniu
    public static TextureData LoadNormalMap(string path) => Load(path, true);

    private static TextureData Load(string path, bool normalMap)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Nie znaleziono pliku.", path);

        int w, h;
        int[] raw;
        using (var bmp = new Bitmap(path))
        {
            w = bmp.Width;
            h = bmp.Height;
            raw = new int[w * h];
            // 32 bitowe ARGB, 4 bajty na piksel, bez paddingu na końcu wiersza
            // wczytanie całosci do talbicy int[] raw
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                Marshal.Copy(data.Scan0, raw, 0, raw.Length);
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }
        // odczytanie kanałów rgb
        var pixels = new Vector3[raw.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            int argb = raw[i];
            var c = new Vector3((argb >> 16) & 0xFF, (argb >> 8) & 0xFF, argb & 0xFF);
            if (!normalMap)
            {
                pixels[i] = c / 255f;
                continue;
            }

            Vector3 n = c * (2f / 255f) - Vector3.One;
            float lenSq = n.LengthSquared();
            pixels[i] = lenSq < 1e-12f ? Vector3.UnitZ : n / MathF.Sqrt(lenSq);
        }
        return new TextureData(w, h, pixels);
    }

    // odczytanie wartosc dla wspolrzednych (u, v)
    // u - kolumna (od lewej), v - wiersz (od dolu obrazu) (ty = (1 - v) * (H - 1))
    public Vector3 Sample(float u, float v)
    {
        u = Math.Clamp(u, 0f, 1f);
        v = Math.Clamp(v, 0f, 1f);
        int tx = Math.Clamp((int)(u * (Width - 1) + 0.5f), 0, Width - 1);
        int ty = Math.Clamp((int)((1f - v) * (Height - 1) + 0.5f), 0, Height - 1);
        return pixels[ty * Width + tx];
    }
}
