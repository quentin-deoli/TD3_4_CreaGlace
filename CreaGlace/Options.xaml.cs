using System.Windows;

namespace CreaGlace
{
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            SliderMusique.Value = App.VolumeMusique * 100;
            SliderSFX.Value = App.VolumeSFX * 100;

            txtMusiqueValeur.Text = $"{SliderMusique.Value:F0}%";
            txtSFXValeur.Text = $"{SliderSFX.Value:F0}%";
        }

        private void SliderMusique_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtMusiqueValeur != null)
            {
                txtMusiqueValeur.Text = $"{SliderMusique.Value:F0}%";
            }
            App.VolumeMusique = SliderMusique.Value / 100.0;
        }

        private void SliderSFX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtSFXValeur != null)
            {
                txtSFXValeur.Text = $"{SliderSFX.Value:F0}%";
            }

            App.VolumeSFX = SliderSFX.Value / 100.0;
        }

        private void BoutonRetour_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}