using System.Globalization;
using System.Numerics;

namespace BezierSceneApp
{
    public partial class MainForm : Form
    {
        private Scene? scene;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(List<BezierSurface> surfaces) : this()
        {
            scene = new Scene(surfaces, divisionsTrackBar.Value);
            drawingPanel.Scene = scene;
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
