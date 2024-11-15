namespace VolumeControlApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtSerialMonitor;
        private MenuStrip menuStrip;
        private ToolStripMenuItem menuOptions;
        private ToolStripMenuItem menuMinimize;
        private ToolStripMenuItem menuClose;
        private ToolStripMenuItem menuActivateStartup;
        private ToolStripMenuItem menuDeactivateStartup;
        private ToolStripMenuItem menuRefresh;  // Novo item de menu

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtSerialMonitor = new System.Windows.Forms.TextBox();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMinimize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuClose = new System.Windows.Forms.ToolStripMenuItem();
            this.menuActivateStartup = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDeactivateStartup = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRefresh = new System.Windows.Forms.ToolStripMenuItem();  // Configuração do novo item

            // Configuração do TextBox
            this.txtSerialMonitor.Location = new System.Drawing.Point(10, 35);
            this.txtSerialMonitor.Multiline = true;
            this.txtSerialMonitor.ScrollBars = ScrollBars.Vertical;
            this.txtSerialMonitor.ReadOnly = true;
            this.txtSerialMonitor.Size = new System.Drawing.Size(360, 200);
            this.txtSerialMonitor.Name = "txtSerialMonitor";

            // Configuração do MenuStrip
            this.menuStrip.Items.AddRange(new ToolStripItem[] { this.menuOptions });
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(380, 24);

            // Configuração das opções do menu
            this.menuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                this.menuMinimize,
                this.menuClose,
                this.menuActivateStartup,
                this.menuDeactivateStartup,
                this.menuRefresh  // Adiciona o item Refresh
            });
            this.menuOptions.Name = "menuOptions";
            this.menuOptions.Text = "Opções";

            // Configuração dos itens do menu
            this.menuMinimize.Name = "menuMinimize";
            this.menuMinimize.Text = "Minimizar";
            this.menuClose.Name = "menuClose";
            this.menuClose.Text = "Fechar";
            this.menuActivateStartup.Name = "menuActivateStartup";
            this.menuActivateStartup.Text = "Ativar Inicialização Automática";
            this.menuDeactivateStartup.Name = "menuDeactivateStartup";
            this.menuDeactivateStartup.Text = "Desativar Inicialização Automática";
            this.menuRefresh.Name = "menuRefresh";  // Configuração do item Refresh
            this.menuRefresh.Text = "Recarregar";  // Título do item de menu Refresh

            // Adicionar controles ao formulário
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.txtSerialMonitor);
            this.MainMenuStrip = this.menuStrip;

            // Configurações do formulário
            this.ClientSize = new System.Drawing.Size(380, 250);
            this.Name = "MainForm";
            this.Text = "Serial Monitor com Controle de Volume";
            this.ResumeLayout(false);
            this.PerformLayout();

            // Associa evento de clique ao item Refresh
            this.menuRefresh.Click += (s, e) => DetectAndConnectToPico();
        }
    }
}
