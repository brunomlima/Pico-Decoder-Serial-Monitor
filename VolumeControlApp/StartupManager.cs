using Microsoft.Win32;

public class StartupManager
{
    private const string StartupKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string appName;
    private readonly string executablePath;

    public StartupManager()
    {
        // Nome do aplicativo e caminho completo do executável
        appName = "VolumeControlApp";  // Pode ser qualquer nome descritivo
        executablePath = Application.ExecutablePath;
    }

    // Adiciona o aplicativo à inicialização
    public void AddApplicationToStartup()
    {
        try
        {
            // Tenta abrir a chave de inicialização para o usuário atual
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, true))
            {
                if (key != null) // Verifica se a chave foi aberta com sucesso
                {
                    // Adiciona aspas ao redor do caminho do executável
                    string executablePathWithQuotes = $"\"{executablePath}\"";
                    key.SetValue(appName, executablePathWithQuotes);
                    MessageBox.Show("Aplicativo adicionado à inicialização.");
                }
                else
                {
                    MessageBox.Show("Não foi possível acessar a chave de registro de inicialização.");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao adicionar à inicialização: {ex.Message}");
        }
    }



    // Remove o aplicativo da inicialização
    public void RemoveApplicationFromStartup()
    {
        try
        {
            // Tenta abrir a chave de inicialização para o usuário atual
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, true))
            {
                if (key != null) // Verifica se a chave foi aberta com sucesso
                {
                    // Verifica se o valor correspondente ao nome do aplicativo existe
                    if (key.GetValue(appName) != null)
                    {
                        key.DeleteValue(appName);
                        MessageBox.Show("Aplicativo removido da inicialização.");
                    }
                    else
                    {
                        MessageBox.Show("O aplicativo não está configurado para iniciar automaticamente.");
                    }
                }
                else
                {
                    MessageBox.Show("Não foi possível acessar a chave de registro de inicialização.");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao remover da inicialização: {ex.Message}");
        }
    }

}
