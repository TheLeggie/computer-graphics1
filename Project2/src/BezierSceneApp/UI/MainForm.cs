using System.Globalization;
using System.Numerics;

namespace BezierSceneApp
{
    public partial class MainForm : Form
    {
        private const string DefaultTextureFile = "texture.png";
        private const string DefaultNormalMapFile = "normalmap.png";

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
            // start: 1. powierzchnia jednolity kolor, 2. tekstura
            scene.Appearances[1].UseTexture = true;
            scene.NormalMap = LoadImage(Path.Combine(dataDir, DefaultNormalMapFile), true);

            for (int i = 0; i < surfaces.Count; i++)
                surfaceComboBox.Items.Add($"Powierzchnia {i + 1}");
            surfaceComboBox.SelectedIndex = 0;
        }

        private SurfaceAppearance? SelectedAppearance =>
            scene != null && surfaceComboBox.SelectedIndex >= 0 ? scene.Appearances[surfaceComboBox.SelectedIndex] : null;

        // blad pliku -> komunikat i null (wywolujacy zostawia dotychczasowy obraz)
        private TextureData? LoadImage(string path, bool normalMap)
        {
            try
            {
                return normalMap ? TextureData.LoadNormalMap(path) : TextureData.LoadColor(path);
            }
            catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException
                                           or OutOfMemoryException or System.Runtime.InteropServices.ExternalException)
            {
                // GDI+ zglasza uszkodzony / nieobslugiwany obraz jako ArgumentException lub OutOfMemoryException
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

        // udane wczytanie przelacza wybrana powierzchnie na teksture
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

        // udane wczytanie wlacza mape normalnych
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

        // wspolny handler suwakow kd, ks, m, z swiatla
        private void lightingTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            LightSettings light = drawingPanel.Light;
            light.Kd = kdTrackBar.Value / 100f;
            light.Ks = ksTrackBar.Value / 100f;
            light.M = mTrackBar.Value;
            light.Position = new Vector3(0f, 0f, lightZTrackBar.Value / 10f);

            kdLabel.Text = string.Format(CultureInfo.InvariantCulture, "kd: {0:0.00}", light.Kd);
            ksLabel.Text = string.Format(CultureInfo.InvariantCulture, "ks: {0:0.00}", light.Ks);
            mLabel.Text = $"m: {mTrackBar.Value}";
            lightZLabel.Text = string.Format(CultureInfo.InvariantCulture, "z światła: {0:0.0}", light.Position.Z);
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
    }
}
