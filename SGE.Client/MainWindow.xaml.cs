using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SGE.Client
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            // Timer para atualizar data/hora ao vivo
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateDateTime();

            // Página inicial
            MainFrame.Navigate(new Pages.HomePage());
        }

        // ===== BARRA DE TÍTULO =====

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Duplo clique maximiza/restaura
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
                return;
            }

            DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ===== RELÓGIO =====

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            TxtDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        // ===== NAVEGAÇÃO =====

        private void BtnAlunos_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "ALUNOS";
            MainFrame.Navigate(new Pages.StudentsPage());
        }

        private void BtnProfessores_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "PROFESSORES";
            MainFrame.Navigate(new Pages.ProfessorsPage());
        }

        private void BtnDisciplinas_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "DISCIPLINAS";
            MainFrame.Navigate(new Pages.CoursesPage());
        }

        private void BtnTurmas_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "TURMAS";
            MainFrame.Navigate(new Pages.GroupsPage());
        }

        private void BtnAvaliacoes_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "AVALIAÇÕES";
            MainFrame.Navigate(new Pages.GradesPage());
        }

        private void BtnRelatorios_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "RELATÓRIOS";
            MainFrame.Navigate(new Pages.ReportsPage());
        }

        private void BtnEventos_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "EVENTOS";
            MainFrame.Navigate(new Pages.EventsPage());
        }

        private void BtnConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "CONFIGURAÇÕES";
            MainFrame.Navigate(new Pages.SettingsPage());
        }

        private void BtnCreditos_Click(object sender, RoutedEventArgs e)
        {
            TxtPageTitle.Text = "CRÉDITOS";
            MainFrame.Navigate(new Pages.CreditsPage());
        }
    }
}