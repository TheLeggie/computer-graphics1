namespace PolygonEditor.UI;

/// <summary>
/// Okno z polem liczbowym do wpisania zadanej długości krawędzi
/// </summary>
public sealed class LengthDialog : Form
{
    private const decimal MinLength = 1m;
    private const decimal MaxLength = 10000m;

    private readonly NumericUpDown _lengthInput = new()
    {
        Minimum = MinLength,
        Maximum = MaxLength,
        DecimalPlaces = 1,
        Width = 120
    };

    public LengthDialog(float initialLength)
    {
        Text = "Zadana długość krawędzi";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        _lengthInput.Value = Math.Clamp((decimal)initialLength, MinLength, MaxLength);

        var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
        var cancelButton = new Button { Text = "Anuluj", DialogResult = DialogResult.Cancel, AutoSize = true };
        AcceptButton = okButton;
        CancelButton = cancelButton;

        var layout = new FlowLayoutPanel
        {
            AutoSize = true,
            Padding = new Padding(10),
            FlowDirection = FlowDirection.LeftToRight
        };
        layout.Controls.Add(new Label { Text = "Długość:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 3, 3) });
        layout.Controls.Add(_lengthInput);
        layout.Controls.Add(okButton);
        layout.Controls.Add(cancelButton);
        Controls.Add(layout);

        Shown += (_, _) => _lengthInput.Select(0, _lengthInput.Text.Length);
    }

    public float Length => (float)_lengthInput.Value;
}
