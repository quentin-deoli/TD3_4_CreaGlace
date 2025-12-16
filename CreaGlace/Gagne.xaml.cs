using System.Windows;

namespace CreaGlace
{
    public partial class Gagne : Window
    {
        // Constructeur qui accepte deux paramètres : le score (int) et le temps (string)
        public Gagne(int scoreFinal, string tempsFinal)
        {
            InitializeComponent();

            // On affiche les informations reçues dans les TextBlocks
            txtScore.Text = scoreFinal.ToString();
            txtTime.Text = tempsFinal;
        }

        // Bouton pour revenir au menu principal
        private void BoutonMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow accueil = new MainWindow(); // On prépare la fenêtre d'accueil
            accueil.Show(); // On l'affiche
            this.Close();   // On ferme la fenêtre de victoire
        }

        // Bouton pour fermer complètement l'application
        private void BoutonQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // Arrête tout le programme
        }
    }
}