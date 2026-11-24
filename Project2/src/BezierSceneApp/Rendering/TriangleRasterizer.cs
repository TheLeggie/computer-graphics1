using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BezierSceneApp
{
    // wypelnianie trojkatow (sortowanie wierzcholkow + tablica aktywnych krawedzi AET) do bufora int[] z Z-bufferem
    public class TriangleRasterizer : IDisposable
    {
        private const float Epsilon = 1e-6f;

        private struct ActiveEdge
        {
            public int Start;
            public int End;
            public float X;
            public float InvM;
        }

        private int[] colorBuffer = Array.Empty<int>();
        private float[] zBuffer = Array.Empty<float>();

        // reuzywane bufory pomocnicze (bez alokacji per trojkat); AET miesci max 3 krawedzie przejsciowo
        private readonly float[] px = new float[3];
        private readonly int[] py = new int[3];
        private readonly int[] ind = new int[3];
        private readonly ActiveEdge[] aet = new ActiveEdge[4];
        private int aetCount;

        // atrybuty biezacego trojkata (obrocone P, N, Pu, Pv oraz u, v wierzcholkow) i parametry cieniowania
        private Vector3 pA, pB, pC, nA, nB, nC, objectColor;
        private Vector3 puA, puB, puC, pvA, pvB, pvC;
        private float uA, uB, uC, vA, vB, vC;
        private TextureData? texture, normalMap;
        private LightSettings light = new();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Bitmap? Bitmap { get; private set; }

        // realokacja buforow i bitmapy tylko przy zmianie rozmiaru
        public void EnsureSize(int width, int height)
        {
            if (width == Width && height == Height && Bitmap != null)
                return;

            Bitmap?.Dispose();
            Width = width;
            Height = height;
            colorBuffer = new int[width * height];
            zBuffer = new float[width * height];
            Bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        }

        public void Clear(int backgroundArgb)
        {
            Array.Fill(colorBuffer, backgroundArgb);
            Array.Fill(zBuffer, float.NegativeInfinity);
        }

        // a, b, c: (x, y) ekranowe (y w dol), z = RotP.Z (+Z do obserwatora, wieksze z = blizej)
        // va, vb, vc: odpowiadajace wierzcholki (RotP, RotN do oswietlenia); color: I_O w [0, 1]
        // tex: tekstura I_O zamiast color (null = jednolity kolor); normals: NormalMap (null = wylaczona)
        public void FillTriangle(Vector3 a, Vector3 b, Vector3 c, Vertex va, Vertex vb, Vertex vc,
            Vector3 color, TextureData? tex, TextureData? normals, LightSettings lightSettings)
        {
            // wyznacznik barycentryczny liczony na oryginalnych ekranowych float (bez psucia interpolacji)
            float d = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            if (MathF.Abs(d) < Epsilon)
                return;

            // wierzcholki P[0..2] z zaokraglonym calkowitym y na potrzeby scanlinii
            px[0] = a.X; py[0] = (int)MathF.Round(a.Y);
            px[1] = b.X; py[1] = (int)MathF.Round(b.Y);
            px[2] = c.X; py[2] = (int)MathF.Round(c.Y);

            // sortowanie tablicy indeksow ind[0..2] rosnaco wedlug py
            ind[0] = 0; ind[1] = 1; ind[2] = 2;
            if (py[ind[0]] > py[ind[1]]) (ind[0], ind[1]) = (ind[1], ind[0]);
            if (py[ind[1]] > py[ind[2]]) (ind[1], ind[2]) = (ind[2], ind[1]);
            if (py[ind[0]] > py[ind[1]]) (ind[0], ind[1]) = (ind[1], ind[0]);

            int yMin = py[ind[0]];
            int yMax = py[ind[2]];
            if (yMin == yMax || yMax < 0 || yMin >= Height)
                return;

            // stale barycentryczne liczone raz na trojkat
            float invD = 1f / d;
            float bcY = b.Y - c.Y;
            float cbX = c.X - b.X;
            float caY = c.Y - a.Y;
            float acX = a.X - c.X;

            pA = va.RotP; 
            pB = vb.RotP; 
            pC = vc.RotP;
            nA = va.RotN; 
            nB = vb.RotN; 
            nC = vc.RotN;
            puA = va.RotPu;
            puB = vb.RotPu;
            puC = vc.RotPu;
            pvA = va.RotPv;
            pvB = vb.RotPv;
            pvC = vc.RotPv;
            uA = va.U; uB = vb.U; uC = vc.U;
            vA = va.V; vB = vb.V; vC = vc.V;
            objectColor = color;
            texture = tex;
            normalMap = normals;
            light = lightSettings;

            aetCount = 0;
            int k = 0;
            int yEnd = Math.Min(yMax, Height - 1);

            // aktualizacja AET rowniez dla y < 0; rysowanie pikseli tylko dla 0 <= y < Height
            for (int y = yMin; y <= yEnd; y++)
            {
                while (k < 3 && py[ind[k]] == y - 1)
                {
                    int i = ind[k++];
                    int prev = (i + 2) % 3;
                    int next = (i + 1) % 3;
                    UpdateAetEdge(i, prev);
                    UpdateAetEdge(i, next);
                }

                // sortowanie AET rosnaco wedlug biezacego x (insertion sort - bez alokacji)
                for (int i = 1; i < aetCount; i++)
                {
                    ActiveEdge key = aet[i];
                    int j = i - 1;
                    while (j >= 0 && aet[j].X > key.X)
                    {
                        aet[j + 1] = aet[j];
                        j--;
                    }
                    aet[j + 1] = key;
                }

                if (y >= 0)
                {
                    for (int e = 0; e + 1 < aetCount; e += 2)
                        FillRow(y, aet[e].X, aet[e + 1].X, a, b, c, invD, bcY, cbX, caY, acX);
                }

                for (int e = 0; e < aetCount; e++)
                    aet[e].X += aet[e].InvM;
            }
        }

        private void UpdateAetEdge(int i, int neighbor)
        {
            int dy = py[neighbor] - py[i];
            if (dy > 0)
            {
                // ostro > 0: pomijamy krawedzie poziome; x startuje na linii y = py[i] + 1
                float invM = (px[neighbor] - px[i]) / (float)dy;
                aet[aetCount++] = new ActiveEdge { Start = i, End = neighbor, X = px[i] + invM, InvM = invM };
            }
            else if (dy < 0)
            {
                // krawedz zakonczona w P[i]: usun (neighbor, i) z AET
                for (int e = 0; e < aetCount; e++)
                {
                    if (aet[e].Start == neighbor && aet[e].End == i)
                    {
                        aet[e] = aet[--aetCount];
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Wypełnia jedną scanlinię na wysokości "y" pomiędzy lewą (xLeft)
        /// i prawą (xRight) krawędzią trójkąta
        /// </summary>
        private void FillRow(int y, float xLeft, float xRight, Vector3 a, Vector3 b, Vector3 c,
            float invD, float bcY, float cbX, float caY, float acX)
        {
            int xFrom = Math.Max(0, (int)MathF.Ceiling(xLeft));
            int xTo = Math.Min(Width - 1, (int)MathF.Floor(xRight));
            float dy = y - c.Y;
            int row = y * Width;
            // obliczanie wag dla kazdego piksela x
            for (int x = xFrom; x <= xTo; x++)
            {
                float dx = x - c.X;
                float alpha = (bcY * dx + cbX * dy) * invD;
                float beta = (caY * dx + acX * dy) * invD;
                float gamma = 1f - alpha - beta;
                float z = alpha * a.Z + beta * b.Z + gamma * c.Z;

                // test glebokosci przed interpolacja atrybutow i oswietleniem
                int idx = row + x;
                if (z <= zBuffer[idx])
                    continue;
                zBuffer[idx] = z;

                Vector3 p = alpha * pA + beta * pB + gamma * pC;
                Vector3 n = alpha * nA + beta * nB + gamma * nC;
                float nLenSq = n.LengthSquared();
                n = nLenSq < 1e-12f ? Vector3.UnitZ : n / MathF.Sqrt(nLenSq);

                Vector3 color = objectColor;
                if (texture != null || normalMap != null)
                {
                    float u = Math.Clamp(alpha * uA + beta * uB + gamma * uC, 0f, 1f);
                    float v = Math.Clamp(alpha * vA + beta * vB + gamma * vC, 0f, 1f);
                    if (texture != null)
                        color = texture.Sample(u, v);
                    if (normalMap != null)
                        n = PerturbNormal(n, alpha, beta, gamma, normalMap.Sample(u, v));
                }

                colorBuffer[idx] = light.Shade(p, n, color);
            }
        }

        // N = normalize(M * N_tekstury), M = [Pu, Pv, N_powierzchni]; RotPu i RotPv to pochodne (dlugosc != 1)
        private Vector3 PerturbNormal(Vector3 nSurf, float alpha, float beta, float gamma, Vector3 nTex)
        {
            Vector3 pu = alpha * puA + beta * puB + gamma * puC;
            Vector3 pv = alpha * pvA + beta * pvB + gamma * pvC;
            float puLenSq = pu.LengthSquared();
            float pvLenSq = pv.LengthSquared();
            if (puLenSq < 1e-12f || pvLenSq < 1e-12f)
                return nSurf;

            Matrix3x3 m = Matrix3x3.FromColumns(pu / MathF.Sqrt(puLenSq), pv / MathF.Sqrt(pvLenSq), nSurf);
            Vector3 n = m.Transform(nTex);
            float nLenSq = n.LengthSquared();
            return nLenSq < 1e-12f ? nSurf : n / MathF.Sqrt(nLenSq);
        }

        // kopiowanie bufora do bitmapy; dla 32bpp stride = width * 4, wiec jedna kopia calej tablicy
        public void CopyToBitmap()
        {
            if (Bitmap == null)
                return;

            BitmapData data = Bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                Marshal.Copy(colorBuffer, 0, data.Scan0, colorBuffer.Length);
            }
            finally
            {
                Bitmap.UnlockBits(data);
            }
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
            Bitmap = null;
        }
    }
}
