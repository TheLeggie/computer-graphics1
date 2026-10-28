using PolygonEditor.Core.Model;
using PolygonEditor.Rendering;

namespace PolygonEditor.UI;

public sealed class MainForm : Form
{
    private readonly Scene _scene;
    private readonly SceneRenderer _renderer = new();
    private readonly CanvasPanel _canvas;
    private readonly InteractionController _interaction;
    private readonly Label _statusLabel = new()
    {
        AutoSize = true,
        ForeColor = Color.DarkBlue,
        Margin = new Padding(20, 6, 3, 3)
    };

    public MainForm()
    {
        Text = "Edytor wielokątów";
        ClientSize = new Size(1000, 650);
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;
        KeyPreview = true;

        _scene = SceneInitializer.CreateDemoScene();

        _canvas = new CanvasPanel { Dock = DockStyle.Fill };
        _canvas.Paint += Canvas_Paint;
        Controls.Add(_canvas);
        Controls.Add(CreateToolbar());
        Controls.Add(CreateMenu());

        _interaction = new InteractionController(_scene, _canvas);
        _interaction.PendingSelectionChanged += (_, _) =>
            _statusLabel.Text = _interaction.PendingPerpendicularEdge is null
                ? string.Empty
                : "Kliknij drugi odcinek, który ma być prostopadły (prawy przycisk / Esc - anuluj)";
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            _interaction.CancelPendingSelection();
        else if (e.KeyCode == Keys.F1)
            ShowHelp();
        base.OnKeyDown(e);
    }
	/// <summary>
    /// Tworzy Toolbar służący zmienianiem rysowania - algorytm biblioteczny lub Bresenhama
    /// i obsługuje kliknięcia.
    /// </summary>
    private Control CreateToolbar()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(6, 4, 6, 4)
        };

        var libraryRadio = new RadioButton
        {
            Text = "Algorytm biblioteczny",
            AutoSize = true,
            Checked = _renderer.LineMode == LineDrawingMode.Library
        };
        var bresenhamRadio = new RadioButton
        {
            Text = "Algorytm Bresenhama",
            AutoSize = true,
            Checked = _renderer.LineMode == LineDrawingMode.Bresenham
        };

        libraryRadio.CheckedChanged += (_, _) =>
        {
            if (libraryRadio.Checked)
                SetLineMode(LineDrawingMode.Library);
        };
        bresenhamRadio.CheckedChanged += (_, _) =>
        {
            if (bresenhamRadio.Checked)
                SetLineMode(LineDrawingMode.Bresenham);
        };

        toolbar.Controls.Add(libraryRadio);
        toolbar.Controls.Add(bresenhamRadio);
        toolbar.Controls.Add(_statusLabel);
        return toolbar;
    }
    
    private MenuStrip CreateMenu()
    {
        var menu = new MenuStrip { Dock = DockStyle.Top };
        var helpItem = new ToolStripMenuItem("Pomoc") { ShortcutKeyDisplayString = "F1" };
        helpItem.Click += (_, _) => ShowHelp();
        menu.Items.Add(helpItem);
        MainMenuStrip = menu;
        return menu;
    }

    private void ShowHelp()
    {
        using var help = new HelpForm();
        help.ShowDialog(this);
    }

    private void SetLineMode(LineDrawingMode mode)
    {
        _renderer.LineMode = mode;
        _canvas.Invalidate();
    }
	// Rysowanie - wywołuje renderera
    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        _renderer.Render(e.Graphics, _scene, _canvas.ClientSize.Width, _canvas.ClientSize.Height,
            _interaction.PendingPerpendicularEdge);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _interaction.Dispose();
            _renderer.Dispose();
        }
        base.Dispose(disposing);
    }
}
