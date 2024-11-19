using NAudio.CoreAudioApi;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;

namespace VolumeControlApp
{
    public partial class MainForm : Form
    {
        private SerialPort serialPort = null!;
        private string portName = null!;
        private int baudRate;
        private NotifyIcon notifyIcon = null!;

        private MMDeviceEnumerator deviceEnumerator = null!;
        private MMDevice audioDevice = null!;

        private bool isRaspberryDetected = false;

        private StartupManager startupManager = new StartupManager();


        public MainForm()
        {
            InitializeComponent();
            LoadConfiguration();
            SetupSerialPort();
            SetupAudioControl();
            SetupNotifyIcon();
            this.Resize += new EventHandler(MainForm_Resize);
            this.ShowInTaskbar = false;

            // Associa os eventos aos itens do menu
            menuMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;
            menuClose.Click += (s, e) => Close();
            menuActivateStartup.Click += (s, e) => startupManager.AddApplicationToStartup();
            menuDeactivateStartup.Click += (s, e) => startupManager.RemoveApplicationFromStartup();
            menuRefresh.Click += (s, e) => DetectAndConnectToPico(); // Botão de Refresh

            // Inicia minimizado e esconde a janela
            WindowState = FormWindowState.Minimized;
            Hide();
        }

        private void LoadConfiguration()
        {
            string picoPort = DetectRaspberryPiPico();
            if (!string.IsNullOrEmpty(picoPort))
            {
                portName = picoPort;
                baudRate = 115200;
                AppendToMonitor($"Utilizando a porta encontrada: {portName}\r\n");
            }            
        }

        private void SetupSerialPort()
        {
            try
            {
                serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                {
                    DtrEnable = true,
                    RtsEnable = true
                };
                serialPort.DataReceived += SerialPort_DataReceived;

                try
                {
                    serialPort.Open();
                    AppendToMonitor($"Porta Serial {portName} aberta a {baudRate} baud.\r\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir porta serial: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                AppendToMonitor($"Erro ao configurar porta serial: {ex.Message}\r\n");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine();
                Invoke(new Action(() => ProcessSerialCommand(data.Trim())));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => AppendToMonitor($"Erro na leitura: {ex.Message}\r\n")));
            }
        }

        private void ProcessSerialCommand(string command)
        {
            if (command == "vol_up")
            {
                IncreaseVolume();
            }
            else if (command == "vol_down")
            {
                DecreaseVolume();
            }
            else if (command == "mute")
            {
                ToggleMute();
            }
            else if (command.StartsWith("Modo:"))
            {
                // Exibe o aviso do Windows com o modo selecionado
                string modeMessage = command.Substring(5).Trim(); // Remove "Modo:" do início
                ShowBalloonNotification("Modo Selecionado", modeMessage, 1000); // Mostra por 1 segundo
                AppendToMonitor($"Modo selecionado: {modeMessage}\r\n"); // Opcional: Exibe também no monitor serial
            }
            else
            {
                string modeMessage = command.Trim(); 
                AppendToMonitor($"Comando desconhecido: {modeMessage}\r\n");
            }
        }

        private void ShowBalloonNotification(string title, string message, int durationMilliseconds)
        {  
            if (notifyIcon != null)
            {
                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = message;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.ShowBalloonTip(durationMilliseconds); // Exibe a notificação
            }
            else
            {
                AppendToMonitor("Erro: NotifyIcon não configurado.\r\n");
            }
        }


        private void SetupAudioControl()
        {
            try
            {
                deviceEnumerator = new MMDeviceEnumerator();
                audioDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }
            catch (Exception ex)
            {
                AppendToMonitor($"Erro ao configurar controle de áudio: {ex.Message}\r\n");
            }
        }

        private void IncreaseVolume()
        {
            if (audioDevice == null) return;

            if (audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar < 0.95f)
            {
                audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar += 0.05f;
                AppendToMonitor("Volume aumentado.\r\n");
            }
            else
            {
                audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;
                AppendToMonitor("Volume no máximo.\r\n");
            }
        }

        private void DecreaseVolume()
        {
            if (audioDevice == null) return;

            if (audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar > 0.05f)
            {
                audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar -= 0.05f;
                AppendToMonitor("Volume diminuído.\r\n");
            }
            else
            {
                audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 0.0f;
                AppendToMonitor("Volume no mínimo.\r\n");
            }
        }

        private void ToggleMute()
        {
            if (audioDevice == null) return;

            audioDevice.AudioEndpointVolume.Mute = !audioDevice.AudioEndpointVolume.Mute;
            AppendToMonitor("Volume mutado/desmutado.\r\n");
        }

        private void SetupNotifyIcon()
        {
            try
            {
                // Carrega o ícone embutido como recurso diretamente do assembly
                using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VolumeControlApp.Resources.VolumeControlApp.ico"))
                {
                    if (stream != null)
                    {
                        Icon embeddedIcon = new Icon(stream);
                        notifyIcon = new NotifyIcon
                        {
                            Icon = embeddedIcon,
                            Text = "Volume Control App",
                            Visible = true
                        };

                        notifyIcon.DoubleClick += (sender, e) =>
                        {
                            Show();
                            WindowState = FormWindowState.Normal;
                        };

                        FormClosing += (sender, e) =>
                        {
                            notifyIcon.Visible = false;
                            notifyIcon.Icon?.Dispose();
                        };
                    }
                    else
                    {
                        MessageBox.Show("Erro: Ícone embutido não encontrado. Verifique o nome do recurso.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar o ícone embutido: {ex.Message}");
            }
        }




        // Evento para ocultar na bandeja quando minimizado
        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();  // Oculta o formulário
                //notifyIcon.ShowBalloonTip(200, "Volume Control", "Aplicação minimizada para a bandeja", ToolTipIcon.Info);
            }
        }

        private void DetectAndConnectToPico()
        {
            string detectedPort = DetectRaspberryPiPico();
            if (!string.IsNullOrEmpty(detectedPort))
            {
                try
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                        Thread.Sleep(100); // Pausa após fechar antes de reabrir
                        AppendToMonitor("Porta serial anterior fechada para reconectar.\r\n");
                    }
                    serialPort.PortName = detectedPort;
                    serialPort.Open();
                    AppendToMonitor($"Reconectado à porta {detectedPort}.\r\n");
                }
                catch (Exception ex)
                {
                    AppendToMonitor($"Erro ao reconectar: {ex.Message}\r\n");
                }
            }
            else
            {
                AppendToMonitor("Nenhuma porta foi encontrada para reconectar ao Raspberry Pi Pico.\r\n");
            }
        }


        private string DetectRaspberryPiPico()
        {
            string[] ports = SerialPort.GetPortNames();
            AppendToMonitor("Iniciando a varredura das portas COM...\r\n");

            foreach (string port in ports)
            {
                if (serialPort != null && serialPort.IsOpen && serialPort.PortName == port)
                {
                    AppendToMonitor($"Porta {port} já está em uso e aberta, pulando...\r\n");
                    continue;
                }

                using (SerialPort tempSerialPort = new SerialPort(port, 115200, Parity.None, 8, StopBits.One))
                {
                    tempSerialPort.DtrEnable = true;
                    tempSerialPort.RtsEnable = true;
                    tempSerialPort.ReadTimeout = 500;
                    tempSerialPort.WriteTimeout = 500;

                    try
                    {
                        tempSerialPort.Open();
                        AppendToMonitor($"Testando a porta: {port}\r\n");
                        tempSerialPort.WriteLine("ping");

                        // Breve pausa para permitir a resposta
                        Thread.Sleep(200);
                        string response = tempSerialPort.ReadLine();

                        if (response.Trim() == "pong")
                        {
                            AppendToMonitor($"Raspberry Pi Pico encontrado na porta {port}\r\n");
                            isRaspberryDetected = true;
                            return port;
                        }
                    }
                    catch (TimeoutException)
                    {
                        AppendToMonitor($"Nenhuma resposta na porta: {port}\r\n");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        AppendToMonitor($"Porta {port} está em uso, ignorando.\r\n");
                    }
                    catch (Exception ex)
                    {
                        AppendToMonitor($"Erro ao testar a porta {port}: {ex.Message}\r\n");
                    }
                }
            }

            AppendToMonitor("Raspberry Pi Pico não encontrado.\r\n");
            isRaspberryDetected = false;
            return string.Empty;
        }

        private void AppendToMonitor(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendToMonitor(message)));
            }
            else
            {
                txtSerialMonitor.AppendText(message + Environment.NewLine); // Adiciona nova linha explicitamente
                txtSerialMonitor.ScrollToCaret(); // Garante que a mensagem seja visível
            }
        }


    }
}
