using System.Windows;

namespace CreaGlace
{
    public partial class GameOver : Window
    {
        // Constructeur par défaut (utile pour le designer, mais on peut le laisser vide)
        public GameOver()
        {
            InitializeComponent();
        }

        // Constructeur principal utilisé par la fenêtre Game
        public GameOver(int score, string temps)
        {
            InitializeComponent();

            // On affiche les données reçues
            txtScore.Text = score.ToString();
            txtTime.Text = temps;
        }

        private void ButtonReplay_Click(object sender, RoutedEventArgs e)
        {
            // Logique : Le joueur veut rejouer -> Retour au choix du cône
            ChoixCone choixCone = new ChoixCone();
            choixCone.Show();

            // Fermeture de la fenêtre actuelle
            this.Close();
        }

        private void ButtonQuit_Click(object sender, RoutedEventArgs e)
        {
            // Logique : Arrêt complet de l'application
            Application.Current.Shutdown();
        }
    }
}