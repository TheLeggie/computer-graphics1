using PolygonEditor.Rendering;

namespace PolygonEditor.Algorithms;

public static class BresenhamLine
{
    /// <summary>
    /// Algorytm Midpoint, uogólniony na wszystkie osiem oktantów.
    /// "dashLength" do linii przerywanej
    /// </summary>
    public static void Draw(DirectBitmap buffer, int x1, int y1, int x2, int y2, Color color, int dashLength = 0)
    {
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;

        int x = x1;
        int y = y1;
        int step = 0;
        PutPixel(buffer, x, y, color, step++, dashLength);

        if (dx >= dy)
        {
            int incrE = 2 * dy;
            int incrNE = 2 * (dy - dx);
            int d = 2 * dy - dx;

            while (x != x2)
            {
                if (d < 0)
                {
                    d += incrE;
                    x += sx;
                }
                else
                {
                    d += incrNE;
                    x += sx;
                    y += sy;
                }
                PutPixel(buffer, x, y, color, step++, dashLength);
            }
        }
        else
        {
            int incrE = 2 * dx;
            int incrNE = 2 * (dx - dy);
            int d = 2 * dx - dy;

            while (y != y2)
            {
                if (d < 0)
                {
                    d += incrE;
                    y += sy;
                }
                else
                {
                    d += incrNE;
                    y += sy;
                    x += sx;
                }
                PutPixel(buffer, x, y, color, step++, dashLength);
            }
        }
    }

    /// <summary>
    /// Zapis do pól tablicy bitmapy
    /// </summary>
    private static void PutPixel(DirectBitmap buffer, int x, int y, Color color, int step, int dashLength)
    {
        if (dashLength > 0 && (step / dashLength) % 2 == 1)
            return;
        buffer.SetPixel(x, y, color);
    }
}
