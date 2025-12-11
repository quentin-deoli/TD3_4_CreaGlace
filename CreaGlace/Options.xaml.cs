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

            // Initialiser sliders avec valeurs globales
            SliderMusique.Value = AudioManager.GetMusicVolume() * 100;
            txtMusiqueValeur.Text = $"{SliderMusique.Value:0}%";

            SliderSFX.Value = AudioManager.GetSFXVolume() * 100;
            txtSFXValeur.Text = $"{SliderSFX.Value:0}%";
        }

        private void BoutonRetour_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BoutonSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            AudioManager.SetMusicVolume(SliderMusique.Value / 100.0);
            AudioManager.SetSFXVolume(SliderSFX.Value / 100.0);
            this.Close();
        }

        private void SliderMusique_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtMusiqueValeur.Text = $"{SliderMusique.Value:0}%";
            AudioManager.SetMusicVolume(SliderMusique.Value / 100.0);
        }

        private void SliderSFX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtSFXValeur.Text = $"{SliderSFX.Value:0}%";
            AudioManager.SetSFXVolume(SliderSFX.Value / 100.0);
        }
    }
}
