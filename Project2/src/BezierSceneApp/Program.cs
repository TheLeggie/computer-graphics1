namespace BezierSceneApp
{
    internal static class Program
    {
        // plik danych kopiowany z ../../data do bin/.../data (patrz csproj)
        private const string DataFile = "surfaces.txt";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            List<BezierSurface> surfaces;
            try
            {
                surfaces = SurfaceFileReader.Load(Path.Combine(AppContext.BaseDirectory, "data", DataFile));
            }
            catch (Exception ex) when (ex is IOException or FormatException or UnauthorizedAccessException)
            {
                // brak pliku lub zly format: komunikat i zamkniecie aplikacji
                MessageBox.Show(ex.Message, "Błąd wczytywania powierzchni", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new MainForm(surfaces));
        }
    }
}
