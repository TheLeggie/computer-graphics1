using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PolygonEditor.Rendering;

/// <summary>
/// Bitmapa oparta na tablicy int[] (jedna wartość ARGB na piksel), dzięki czemu piksele można
/// zapisywać bezpośrednio, bez Bitmap.SetPixel, a GDI+ nadal może rysować w tej samej pamięci.
/// </summary>
public sealed class DirectBitmap : IDisposable
{
    private GCHandle _bitsHandle;

    public int Width { get; }
    public int Height { get; }
    public int[] Bits { get; }
    public Bitmap Bitmap { get; }
	/// <summary>
    /// Bufor 
    /// </summary>
    public DirectBitmap(int width, int height)
    {
        Width = width;
        Height = height;
        Bits = new int[width * height];
        _bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, _bitsHandle.AddrOfPinnedObject());
    }

    public void Clear(Color color) => Array.Fill(Bits, color.ToArgb());

    public void SetPixel(int x, int y, Color color)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            return;
        Bits[y * Width + x] = color.ToArgb();
    }

    public void Dispose()
    {
        Bitmap.Dispose();
        if (_bitsHandle.IsAllocated)
            _bitsHandle.Free();
    }
}
