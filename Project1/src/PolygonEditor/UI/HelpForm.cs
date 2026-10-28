namespace PolygonEditor.UI;

/// <summary>
/// Okno pomocy: instrukcja obsługi, przyjęte założenia i opis trudniejszych algorytmów.
/// Model danych jest opisany w pliku README.md.
/// </summary>
public sealed class HelpForm : Form
{
    private const string HelpText =
        "Instrukcja obsługi:\r\n" +
        "• Przesuwanie wierzchołka lub punktu kontrolnego Béziera: naciśnij LPM na punkcie i przesuń kursor.\r\n" +
        "• Przesuwanie wielokąta: naciśnij LPM na krawędzi lub we wnętrzu wielokąta i przesuń kursor.\r\n" +
        "• PPM na wierzchołku - menu: usuń wierzchołek, ciągłość G0/G1/C1 (tylko przy krzywej lub łuku).\r\n" +
        "• PPM na krawędzi - menu: dodaj wierzchołek w środku, zamień na odcinek/Béziera/łuk, " +
        "zadana długość, prostopadłość, usuń ograniczenie.\r\n" +
        "• Prostopadłość: po wybraniu z menu kliknij LPM drugi odcinek. PPM, kliknięcie w puste miejsce " +
        "lub Esc anuluje wybór.\r\n" +
        "• F1 - pomoc. Przyciski radiowe - wybór algorytmu rysowania odcinków.\r\n" +
        "\r\n" +
        "Algorytm relacji (ConstraintSolver):\r\n" +
        "1. Przeciągany wierzchołek (przy ruchu punktu kontrolnego - oba końce krzywej) jest zablokowany.\r\n" +
        "2. BFS po grafie ograniczeń od zablokowanych wierzchołków: niespełnione ograniczenie jest wymuszane " +
        "ruchem tylko nieustalonych jeszcze wierzchołków, które potem stają się ustalone i trafiają do kolejki.\r\n" +
        "3. Relaksacja: ograniczenia niespełnione po BFS są poprawiane iteracyjnie (zablokowane są już tylko " +
        "wierzchołki wejściowe) aż do braku zmian, maks. 30 iteracji, tolerancja 0.01 px.\r\n" +
        "4. Na koniec przywracana jest ciągłość w przesuniętych wierzchołkach.\r\n" +
        "• Długość: wolny koniec przesuwany wzdłuż kierunku odcinka (oba wolne - symetrycznie od środka).\r\n" +
        "• Prostopadłość: obracana jest krawędź z mniejszą liczbą zablokowanych końców, z zachowaniem długości, " +
        "wokół zablokowanego lub wspólnego końca (inaczej wokół środka), do bliższego z dwóch zwrotów.\r\n" +
        "\r\n" +
        "Ciągłość (ContinuityEnforcer):\r\n" +
        "• Wektor styczny w W: 3(V - W) dla punktu kontrolnego V Béziera, (B - W) dla odcinka WB. " +
        "G1 - wektory przeciwnie skierowane, C1 - dodatkowo równej długości.\r\n" +
        "• Poprawiany jest punkt kontrolny. Gdy użytkownik ciągnie punkt kontrolny obok odcinka, odcinek jest " +
        "obracany (G1) lub obracany i skalowany (C1), a następnie uruchamiany jest solver relacji.\r\n" +
        "• Łuk z G1 w A (styczna sąsiedniej krawędzi T, normalna N): środek S = A + uN, " +
        "u = |B - A|² / (2 N·(B - A)).\r\n" +
        "\r\n" +
        "Model danych - w README.md.";

    public HelpForm()
    {
        Text = "Pomoc";
        ClientSize = new Size(560, 620);
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        KeyPreview = true;

        var textBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Window,
            Font = new Font("Segoe UI", 10f),
            Text = HelpText,
            TabStop = false
        };

        var container = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = SystemColors.Window };
        container.Controls.Add(textBox);
        Controls.Add(container);

        Shown += (_, _) => textBox.Select(0, 0);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            Close();
        base.OnKeyDown(e);
    }
}
