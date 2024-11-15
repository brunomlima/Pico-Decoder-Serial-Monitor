namespace VolumeControlApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // Mostra detalhes completos do erro
                MessageBox.Show($"Erro crítico: {ex.Message}\nDetalhes: {ex.StackTrace}", "Erro de Aplicação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
