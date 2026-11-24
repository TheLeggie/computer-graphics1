using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BezierSceneApp;

// obraz zdekodowany raz do plaskiej tablicy Vector3 (bez GetPixel w petli renderowania)
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
        // GDI+ zglasza brak pliku jako nieczytelne "Parameter is not valid"
        if (!File.Exists(path))
            throw new FileNotFoundException("Nie znaleziono pliku.", path);

        int w, h;
        int[] raw;
        using (var bmp = new Bitmap(path))
        {
            w = bmp.Width;
            h = bmp.Height;
            raw = new int[w * h];
            // dla 32bpp stride = width * 4, wiec jedna kopia calej tablicy
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

    // nearest-neighbor; u -> kolumna (od lewej), v -> wiersz liczony od dolu obrazu (ty = (1 - v) * (H - 1))
    public Vector3 Sample(float u, float v)
    {
        u = Math.Clamp(u, 0f, 1f);
        v = Math.Clamp(v, 0f, 1f);
        int tx = Math.Clamp((int)(u * (Width - 1) + 0.5f), 0, Width - 1);
        int ty = Math.Clamp((int)((1f - v) * (Height - 1) + 0.5f), 0, Height - 1);
        return pixels[ty * Width + tx];
    }
}
