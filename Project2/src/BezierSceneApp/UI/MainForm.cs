using System.Globalization;
using System.Numerics;

namespace BezierSceneApp
{
    public partial class MainForm : Form
    {
        private const string DefaultTextureFile = "texture.png";
        private const string DefaultNormalMapFile = "normalmap.png";
        // krok parametru t na tykniecie timera
        private const float LightAnimationStep = 0.005f;

        private Scene? scene;
        // blokuje zapis do wygladu powierzchni podczas odswiezania kontrolek
        private bool isSyncingUi;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(List<BezierSurface> surfaces) : this()
        {
            scene = new Scene(surfaces, divisionsTrackBar.Value);
            drawingPanel.Scene = scene;

            string dataDir = Path.Combine(AppContext.BaseDirectory, "data");
            TextureData? texture = LoadImage(Path.Combine(dataDir, DefaultTextureFile), false);
            foreach (SurfaceAppearance look in scene.Appearances)
                look.Texture = texture;
            scene.Appearances[1].UseTexture = true;
            scene.NormalMap = LoadImage(Path.Combine(dataDir, DefaultNormalMapFile), true);

            for (int i = 0; i < surfaces.Count; i++)
                surfaceComboBox.Items.Add($"Powierzchnia {i + 1}");
            surfaceComboBox.SelectedIndex = 0;

            drawingPanel.Light.Position.Z = lightZTrackBar.Value / 10f;
            drawingPanel.UpdateLightPosition();
            lightAnimationTimer.Start();
        }

        private SurfaceAppearance? SelectedAppearance =>
            scene != null && surfaceComboBox.SelectedIndex >= 0 ? scene.Appearances[surfaceComboBox.SelectedIndex] : null;

        private TextureData? LoadImage(string path, bool normalMap)
        {
            try
            {
                return normalMap ? TextureData.LoadNormalMap(path) : TextureData.LoadColor(path);
            }
            catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException
                                           or OutOfMemoryException or System.Runtime.InteropServices.ExternalException)
            {
                MessageBox.Show(this, $"Nie udało się wczytać obrazu:\n{path}\n\n{ex.Message}", "Błąd wczytywania obrazu",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private static Color ToColor(Vector3 c) =>
            Color.FromArgb((int)(c.X * 255f + 0.5f), (int)(c.Y * 255f + 0.5f), (int)(c.Z * 255f + 0.5f));

        private void surfaceComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SurfaceAppearance? look = SelectedAppearance;
            if (look == null)
                return;

            isSyncingUi = true;
            solidColorRadioButton.Checked = !look.UseTexture;
            textureRadioButton.Checked = look.UseTexture;
            objectColorButton.BackColor = ToColor(look.SolidColor);
            isSyncingUi = false;
        }

        private void textureRadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            SurfaceAppearance? look = SelectedAppearance;
            if (isSyncingUi || look == null)
                return;

            look.UseTexture = textureRadioButton.Checked;
            drawingPanel.Invalidate();
        }

        private void objectColorButton_Click(object? sender, EventArgs e)
        {
            SurfaceAppearance? look = SelectedAppearance;
            if (look == null)
                return;

            objectColorDialog.Color = ToColor(look.SolidColor);
            if (objectColorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            look.SolidColor = LightSettings.ToVector(objectColorDialog.Color);
            objectColorButton.BackColor = objectColorDialog.Color;
            drawingPanel.Invalidate();
        }

        private void loadTextureButton_Click(object? sender, EventArgs e)
        {
            SurfaceAppearance? look = SelectedAppearance;
            if (look == null || imageOpenFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            TextureData? texture = LoadImage(imageOpenFileDialog.FileName, false);
            if (texture == null)
                return;

            look.Texture = texture;
            textureRadioButton.Checked = true;
            drawingPanel.Invalidate();
        }

        private void useNormalMapCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (scene == null)
                return;

            scene.UseNormalMap = useNormalMapCheckBox.Checked;
            drawingPanel.Invalidate();
        }

        private void loadNormalMapButton_Click(object? sender, EventArgs e)
        {
            if (scene == null || imageOpenFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            TextureData? normalMap = LoadImage(imageOpenFileDialog.FileName, true);
            if (normalMap == null)
                return;

            scene.NormalMap = normalMap;
            useNormalMapCheckBox.Checked = true;
            drawingPanel.Invalidate();
        }

        private void divisionsTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            divisionsLabel.Text = $"Dokładność triangulacji: {divisionsTrackBar.Value}";
            scene?.SetDivisions(divisionsTrackBar.Value);
            drawingPanel.Invalidate();
        }

        private void showPolygonsCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            drawingPanel.ShowPolygons = showPolygonsCheckBox.Checked;
            drawingPanel.Invalidate();
        }

        private void showMeshCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            drawingPanel.ShowMesh = showMeshCheckBox.Checked;
            drawingPanel.Invalidate();
        }

        private void showFilledCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            drawingPanel.ShowFilled = showFilledCheckBox.Checked;
            drawingPanel.Invalidate();
        }

        // wspolny handler suwakow kd, ks, m, punkt światła zachowuje położenie na krzywej
        private void lightingTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            LightSettings light = drawingPanel.Light;
            light.Kd = kdTrackBar.Value / 100f;
            light.Ks = ksTrackBar.Value / 100f;
            light.M = mTrackBar.Value;
            light.Position.Z = lightZTrackBar.Value / 10f;
            drawingPanel.UpdateLightPosition();

            kdLabel.Text = string.Format(CultureInfo.InvariantCulture, "kd: {0:0.00}", light.Kd);
            ksLabel.Text = string.Format(CultureInfo.InvariantCulture, "ks: {0:0.00}", light.Ks);
            mLabel.Text = $"m: {mTrackBar.Value}";
            lightZLabel.Text = string.Format(CultureInfo.InvariantCulture, "z światła: {0:0.0}", light.Position.Z);
            drawingPanel.Invalidate();
        }

        private void lightAnimationTimer_Tick(object? sender, EventArgs e)
        {
            drawingPanel.LightT = (drawingPanel.LightT + LightAnimationStep) % 1f;
            drawingPanel.UpdateLightPosition();
            drawingPanel.Invalidate();
        }

        private void animateLightCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            lightAnimationTimer.Enabled = animateLightCheckBox.Checked;
        }

        private void showLightCurveCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            drawingPanel.ShowLightCurve = showLightCurveCheckBox.Checked;
            drawingPanel.Invalidate();
        }

        private void lightColorButton_Click(object? sender, EventArgs e)
        {
            if (lightColorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            lightColorButton.BackColor = lightColorDialog.Color;
            drawingPanel.Light.LightColor = LightSettings.ToVector(lightColorDialog.Color);
            drawingPanel.Invalidate();
        }

        private void helpMenuItem_Click(object? sender, EventArgs e)
        {
            const string helpText =
                "STEROWANIE MYSZĄ\r\n" +
                "• LPM (poza uchwytami krzywej) — obrót sceny wokół osi X i Y.\r\n" +
                "• PPM — obrót sceny wokół osi Z.\r\n" +
                "• LPM na uchwycie krzywej światła (V0..V5) — przeciąganie punktu kontrolnego (V6 = V0).\r\n\r\n" +
                "PRAWY PANEL STEROWANIA\r\n" +
                "• Dokładność triangulacji — podział k ∈ [4, 40] (2k² trójkątów na powierzchnię).\r\n" +
                "• Wieloboki Béziera / Siatka trójkątów / Wypełnione trójkąty — widoczność warstw.\r\n" +
                "• kd, ks, m — współczynnik rozproszenia, odbicia zwierciadlanego i wykładnik połysku.\r\n" +
                "• z światła — wysokość płaszczyzny z = const źródła światła.\r\n" +
                "• Kolor światła — wybór barwy źródła światła IL.\r\n" +
                "• Animacja światła — start / pauza ruchu źródła światła po krzywej.\r\n" +
                "• Pokaż krzywą światła — widoczność i edycja toru oraz znacznika światła.\r\n" +
                "• Wybór powierzchni + Kolor obiektu — niezależny tryb (jednolity kolor / tekstura), wybór koloru i wczytanie tekstury IO.\r\n" +
                "• Mapa normalnych / Wczytaj NormalMap... — włączenie i wybór mapy normalnych.\r\n\r\n" +
                "KLUCZOWE ALGORYTMY I ZAŁOŻENIA\r\n" +
                "• Plik wejściowy i dane: plik data/surfaces.txt wczytywany jako domyślny, x y z z kropką dziesiętną, zakres [-1, 1]" +
                ", tekstura i normalMap wczytywane domyślnie z folderu data/\r\n" +
                "• Powierzchnie Béziera st. 3: siatka 4×4 punktów kontrolnych, po podziale na trójkąty dla każdego wierzchołka liczymy " +
                " Pu, Pv i N = Pu × Pv. ze wzorów Bernsteina w obszarze parametrów (u, v)\r\n" +
                "• Układ 3D i obrót: prawoskrętny (+Z do obserwatora, +X w prawo, +Y w górę), rzut prostokątny na xy; własna macierz 3×3 składana przyrostowo" +
                " i po każdym obrocie aplikowana do oryginalnych (nieobróconych) wierzchołków.\r\n" +
                "• Rasteryzacja, wypełnianie trójkątów i Z-buffer: skanlinia AET z sortowaniem wierzchołków do bufora int[] (przenoszonego na bitmapę)," +
                " interpolacja barycentryczna z, P3D, N, (u, v), Pu, Pv; test Z-buffera (większe z) - odrzucenie zasłoniętych punktów, dla nich nie trzeba liczyć oświetlenia.\r\n" +
                "• Oświetlenie i NormalMap: model Phonga bez ambient (kd·<N,L> + ks·<V,R>^m), dwustronność przez odwrócenie N przy Nz < 0 (tył płaszczyzny, dzięki temu dwie strony poprawnie oświetlone)" +
                ". Mapa normalnych modyfikuje kierunek normalnej w lokalnej bazie stycznej M = [Pu, Pv, Npow], N' = norm(M·Ntex). Kolor powierzchni jednolity lub pobierany z tekstury na podstawie (u, v).\r\n" +
                "• Tor światła: zamknięta krzywa Béziera st. 6 (7 pkt, V6 = V0) na płaszczyźnie z = const w układzie obserwatora, punkty na krzywej wyznaczane algorytmem de Casteljau.";

            using var dlg = new Form
            {
                Text = "Help — Bezier Scene",
                ClientSize = new Size(560, 440),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };
            using var box = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = SystemColors.Window,
                Font = new Font("Segoe UI", 9.5f),
                Text = helpText,
                SelectionStart = 0,
                SelectionLength = 0
            };
            dlg.Padding = new Padding(12);
            dlg.Controls.Add(box);
            dlg.ShowDialog(this);
        }
    }
}
