using System.Windows;

namespace CreaGlace
{
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();

            // À l'ouverture, on récupère les valeurs stockées dans App
            SliderMusique.Value = App.VolumeMusique * 100;
            SliderSFX.Value = App.VolumeSFX * 100;

            // Mise à jour visuelle des textes (ex: "50%")
            // "F0" veut dire "Fixed point, 0 décimale" -> ça arrondit le chiffre
            txtMusiqueValeur.Text = $"{SliderMusique.Value:F0}%";
            txtSFXValeur.Text = $"{SliderSFX.Value:F0}%";
        }

        private void SliderMusique_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Sécurité : au tout début du chargement, le texte peut ne pas encore exister
            if (txtMusiqueValeur != null)
            {
                txtMusiqueValeur.Text = $"{SliderMusique.Value:F0}%";
            }

            // On stocke la nouvelle valeur dans la variable globale
            // On divise par 100 car le slider est sur 100, mais le volume WPF est de 0.0 à 1.0
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
            // Close() ferme juste cette fenêtre, on revient donc au Menu qui était en "ShowDialog"
            this.Close();
        }
    }
}