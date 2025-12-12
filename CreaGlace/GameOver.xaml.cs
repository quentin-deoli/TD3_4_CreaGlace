using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CreaGlace
{
    /// <summary>
    /// Logique d'interaction pour GameOver.xaml
    /// </summary>
    public partial class GameOver : Window
    {
        public GameOver()
        {
            InitializeComponent();
        }

        // Constructeur surchargé : permet d'envoyer le score et le temps depuis le jeu
        // Exemple d'appel : new GameOver(150, "02:30").Show();
        public GameOver(int score, string temps) : this()
        {
            // Mise à jour des TextBlock définis dans le XAML
            txtScore.Text = score.ToString();
            txtTime.Text = temps;
        }

        // Action du bouton "Rejouer"
        private void ButtonReplay_Click(object sender, RoutedEventArgs e)
        {
            // 1. Créer une nouvelle instance de votre fenêtre de jeu principale
            // Remplacez "MainWindow" par le nom de votre fenêtre de jeu (ex: GameWindow) si différent
            MainWindow nouvellePartie = new MainWindow();

            // 2. Afficher la nouvelle partie
            nouvellePartie.Show();

            // 3. Fermer la fenêtre de Game Over actuelle
            this.Close();
        }

        // Action du bouton "Quitter"
        private void ButtonQuit_Click(object sender, RoutedEventArgs e)
        {
            // Ferme complètement l'application
            Application.Current.Shutdown();
        }
    }
}
