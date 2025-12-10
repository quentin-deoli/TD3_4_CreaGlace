using System.Windows;

namespace CreaGlace
{
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();

            BoutonRetour.Click += BoutonRetour_Click;
            BoutonSauvegarder.Click += BoutonSauvegarder_Click;

            // Initialiser les valeurs des TextBlock au démarrage
            txtMusiqueValeur.Text = $"{SliderMusique.Value:0}%";
            txtSFXValeur.Text = $"{SlidersSFX.Value:0}%";
        }

        private void BoutonRetour_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BoutonSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            // Mettre à jour les paramètres
            GameSettings.MusicVolume = SliderMusique.Value;
            GameSettings.SFXVolume = SlidersSFX.Value;

            this.Close();
        }

        // Mise à jour en temps réel du pourcentage musique
        private void SliderMusique_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtMusiqueValeur.Text = $"{SliderMusique.Value:0}%";
        }

        // Mise à jour en temps réel du pourcentage SFX
        private void SlidersSFX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtSFXValeur.Text = $"{SlidersSFX.Value:0}%";
        }
    }

    public static class GameSettings
    {
        public static double MusicVolume { get; set; } = 100;
        public static double SFXVolume { get; set; } = 100;
    }
}
