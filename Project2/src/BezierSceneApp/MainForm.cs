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
    }
}
