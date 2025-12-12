using System.Windows;

namespace CreaGlace
{
    public partial class GameOver : Window
    {
        public GameOver()
        {
            InitializeComponent();
        }

        public GameOver(int score, string temps) : this()
        {
            txtScore.Text = score.ToString();
            txtTime.Text = temps;
        }

        private void ButtonReplay_Click(object sender, RoutedEventArgs e)
        {
            // Ouvrir la fenêtre de choix du cône
            ChoixCone choixCone = new ChoixCone();
            choixCone.Show();

            // Fermer la fenêtre GameOver
            this.Close();
        }

        private void ButtonQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
